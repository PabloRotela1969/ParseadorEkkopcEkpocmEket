using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Xml;
using System.Text;
using System.Windows.Forms;


namespace ParseadorEkkopcEkpocmEket
{


    /// <summary>
    /// Esta clase se basa en el Demonio Dioxlog para procesar los 3 archivos, permite parsearlos, verificar cada dato leído a partir del archivo de preferencias
    /// y registrar en log de errores, si un dato de un renglón no pasa la validación, se registra y toda la celda no se ingresa, el proceso prosigue con la siguiente linea hasta leer completamente
    /// todos los archivos, primero a listas genéricas, luego se pasan a Datatables para usar en BulkInsert a las tablas BCP correspondientes para luego correr la secuencia de SP que el demonio
    /// original, procesando desde las tablas BCP a las definitivas
    /// Al final del proceso se mueven los archivos a la carpeta de procesados exitosamente concatenando datos del tiempo en que se procesaron
    /// Este proceso deja registros precisos de las celdas, el error por el que no pasaron la validación y su ubicación en numero de linea y archivo al que pertenecen, con el fin de poder
    /// corregirlos si procede y volver a procesarlos en particular
    /// las rutas por defecto se establecen en el archivo APP.CONFIG
    /// las características que se validan se encuentran en el archivo "Preferencies.xml" (tomado desde el demonio original y adaptado al formato de los últimos archivos contemporáneos)
    /// Para esto se implementaron clases que modelan celdas, renglones y archivos con listas genéricas tanto para recibir los datos de las preferencias como los datos desde los archivos
    /// además de una clase estática con métodos estáticos con utilidades que no hacen al modelo del problema
    /// y una clase DAO sencilla con métodos simples para acceder a DB 
    /// </summary>
    public partial class frmProcesamiento : Form
    {
        #region --------- objetos que se usan dentro de esta clase -----------
        Preferencias listaPreferencias = new Preferencias();
        DatosDesdeArchivos ListadoDeArchivos = new DatosDesdeArchivos();
        string ruta = utiles.leerArchivoConfiguración("rutaAlLogErrores");
        DAO enlaceAdatos;
        string logPasosExitosos;
        string pasosExitososContenido;
        #endregion

        #region -----------metodos del proceso general ----------------------
        public frmProcesamiento()
        {
            InitializeComponent();
            logPasosExitosos = utiles.leerArchivoConfiguración("rutaAlLogAExitos");
            
        }



        private void Proceso()
        {
            
            try
            {
                leerPreferencias(utiles.leerArchivoConfiguración("rutaApreferencias"));
                pasosExitososContenido = " carga de preferencias ";
                utiles.escribirArchivoTexto(logPasosExitosos, pasosExitososContenido);
                enlaceAdatos = new DAO(utiles.leerArchivoConfiguración("cadenaAtablas"));
                procesarArchivos();
                DataSet conjuntoTablasCargadas = cargarCadaDataTable(listaPreferencias.ListaDePreferencias, ListadoDeArchivos.listaDeArchivos);
            
                volcarDataTablesAtablas(conjuntoTablasCargadas);
                enlaceAdatos.ejecutarSecuenciaDeStoredsFinal();
                moverArchivos();
            }
            catch (Exception excepcion)
            {
                string texto;
                string ruta = utiles.leerArchivoConfiguración("rutaAlLogErrores");

                texto = " excepción " + excepcion.Message;
                utiles.escribirArchivoTexto(ruta, texto);
            }

        }

        

        /// <summary>
        /// El metodo se basa en el archivo XML de preferencias, va a buscar un archivo y lo lee desde la ruta 
        /// especificada, guardándolo dentro de una lista genérica 
        /// </summary>
        private void procesarArchivos()
        {
            string ruta = utiles.leerArchivoConfiguración("rutaLecturaArchivos"); 
            foreach (archivo archivo in listaPreferencias.ListaDePreferencias)
            {
                ListadoDeArchivos.listaDeArchivos.Add(extraerCamposDeCadaArchivoTexto(ruta, archivo));
            }
        }


        /// <summary>
        /// Esta clase recibe data de un archivo de preferencia (desde archivo XML ) y data de un archivo de dato
        /// de modo que primero consulta la tabla BCP para traer su estructura, luego por cada fila del archivo de datos
        /// la parsea armando un array de celdas y las carga en una fila sacada desde el datatable con la estructura del archivo bCP
        /// Es necesario pasar de las listas genéricas a datatables para usar en los bulkinsert
        /// </summary>
        /// <param name="archivoPreferencia">se trata del nombre del archivo BCP del que extraer su estructura</param> 
        /// <param name="archivoCargado">es la estructura de archivo en particular correspondiente al archivo BCP</param>
        /// <returns>es el datatable cargado</returns>
        private DataTable deListaAdatatable(archivo archivoPreferencia , archivoDatos archivoCargado)
        {
            DataTable tabla = new DataTable();
            tabla.TableName = archivoPreferencia.bcp;
            bool titulos = true;
            tabla = enlaceAdatos.consultarTabla(archivoPreferencia.bcp);
            tabla.TableName = archivoPreferencia.bcp;
            tabla.Clear();
            foreach (filasDatos renglon in archivoCargado.renglones)
            {
                if (!titulos)
                {
                    DataRow fila = tabla.NewRow();
                    int columna = 0;
                    foreach (string celdas in renglon.celdas)
                    {
                        fila[columna] = celdas;
                        columna++;
                    }
                    tabla.Rows.Add(fila);
                }
                titulos = false;
            }
            return tabla;
        }

        /// <summary>
        /// Se itera por el listado de archivos desde las preferencias ( XML ) para pasar a Datatables para usar en el BulkInsert
        /// </summary>
        /// <param name="listaPreferencias">listado de archivos desde XML</param>
        /// <param name="listaDatos">lista de datos extraidos desde archivos a procesar</param>
        /// <returns>Dataset conteniendo datatables cargados con los datos de los archivos a procesar</returns>
        private DataSet cargarCadaDataTable(List<archivo> listaPreferencias, List<archivoDatos> listaDatos)
        {
            DataSet conjuntoDatatables = new DataSet();
            for (int i = 0; i < listaPreferencias.Count; i++)
            {
                conjuntoDatatables.Tables.Add( deListaAdatatable(listaPreferencias[i],listaDatos[i]));
            }

            return conjuntoDatatables;
        }

        /// <summary>
        /// Tomando el dataset con los datos cargados desde los archivos a procesar, primero se borra cada tabla BCP
        /// luego se hace el volcado o carga de esa tabla BCP correspondiente
        /// </summary>
        /// <param name="conjuntoDeTablas"></param>
        private void volcarDataTablesAtablas(DataSet conjuntoDeTablas)
        {
            foreach (DataTable tabla in conjuntoDeTablas.Tables)
            {
                enlaceAdatos.borrarTabla(tabla.TableName);
                enlaceAdatos.InserciónMasivaPorCadaTabla(tabla, tabla.TableName);
            }

        }


        /// <summary>
        /// Este método recibe la ruta del archivo a leer y las preferencias de ese archivo para poderlo parsear
        /// Se toma el archivo de Preferencias del archivo a leer y se extraen datos en función al largo de los titulos, de modo que si algún
        /// campo contiene un carácter de parseo ";" se ignora y se lee como parte del dato
        /// Si la celda pasa las validaciones, se ingresa a la lista de celdas correctas para formar un renglón
        /// Si la celda falla alguna validación, se deja log de error como constancia y se saltea el resto de las celdas que forman la línea
        /// Si el renglón no tuvo celdas erróneas se ingresa a la lista del "nuevoArchivoDeDatos", a continuación se lee la siguiente línea reptitiendo
        /// el proceso hasta terminar con todas las lineas de cada archivo a procesar
        /// </summary>
        /// <param name="rutaCarpetaLectura">la ruta a la carpeta donde se ingresan los 3 archivos a ser procesados</param>
        /// <param name="archivo_">es el objeto desde el archivo de preferencias de XML que contiene todos los datos para validar cada dato leído</param>
        /// <returns>lista de renglones que conforman un archivo en particular</returns>
        public archivoDatos extraerCamposDeCadaArchivoTexto(string rutaCarpetaLectura,archivo archivo_)
        {
            string linea;
            string datoParseadoDeUnaLinea;
            int numeroLineaLeida = 0;
            int numeroLineaErrada = 0;
            bool IngresarLinea = true;
            archivoDatos nuevoArchivoDatos;
            StreamReader lector;
      
            lector = new StreamReader(rutaCarpetaLectura + archivo_.nombre);
                
            nuevoArchivoDatos = new archivoDatos();
            nuevoArchivoDatos.nombre = archivo_.nombre;
            filasDatos nuevaFilaDeDatos;
            while ((linea = lector.ReadLine()) != null)
            {
                nuevaFilaDeDatos = new filasDatos();
                numeroLineaLeida++;
                IngresarLinea = true;
                if (numeroLineaLeida == 1)
                {
                    calcularMedidasDeCadaCelda(archivo_, linea); 
                }
                

                foreach (campo campoDePreferencias in archivo_.listaDeCampos)
                {

                    datoParseadoDeUnaLinea = linea.Substring(campoDePreferencias.desde, campoDePreferencias.largo);

                    if (numeroLineaLeida ==1 || esCeldaValida(datoParseadoDeUnaLinea, numeroLineaLeida, campoDePreferencias, archivo_.nombre, campoDePreferencias.nombre))
                    {
                        datoParseadoDeUnaLinea = transformarFormatoFechaAgeneral(datoParseadoDeUnaLinea);
                        nuevaFilaDeDatos.celdas.Add(datoParseadoDeUnaLinea);
                    }
                    else
                    {
                        IngresarLinea = false;
                        numeroLineaErrada++;
                        break;
                    }

                }
                if (IngresarLinea)
                {
                    nuevoArchivoDatos.renglones.Add(nuevaFilaDeDatos);

                }

                
            }
            pasosExitososContenido = " Del archivo " + archivo_.nombre + " se procesaron " + numeroLineaLeida.ToString() + "lineas sin problemas ";
            pasosExitososContenido = pasosExitososContenido + numeroLineaErrada.ToString() + " con errores ";
            utiles.escribirArchivoTexto(logPasosExitosos, pasosExitososContenido);

            lector.Close();
            lector.Dispose();
                
                
      
            return nuevoArchivoDatos;

        }

        /// <summary>
        /// Esta función es necesaria para igualar formatos regionales al general
        /// </summary>
        /// <param name="fecha"></param>
        /// <returns></returns>
        private string transformarFormatoFechaAgeneral(string fecha)
        {
            string salida = fecha;
            char barra = '/';

            if (fecha != "" && fecha.Contains(barra))
            {
                string[] date = fecha.Split(barra);
                if (date.Length == 3)
                {
                    string dia = date[0];
                    string mes = date[1];
                    string año = date[2];
                    salida = año + mes + dia;
                }

            }
            return salida;
        }

        /// <summary>
        /// Este método surge de la necesidad de usar los títulos como norma de ancho de columna, por lo que cargo en el objeto archivo
        /// los campos desde para saber desde donde calcular el substring desde las lineas con datos, y el largo lo sobreescribo puesto que 
        /// había sido cargado anteriormente, pero si cambian el ancho de los titulos, el archivo de Preferencias queda obsoleto en las longitudes
        /// En el metodo, parseo los titulos por ";" en un array de tramos, si el tramo es el contemplado en la posición traida del archivo
        /// Preferencias.xml, lleno el campo "desde" y sobreescribo el campo "largo" con el largo del tramo de título y busco la siguiente
        /// posición a considerar
        /// solo marca las posiciones
        /// </summary>
        /// <param name="Archivo">Cualquiera de los 3 archivos, Ekpocm, Ekkopc o Eket</param>
        /// <param name="lineaDeTitulo">Es el título de cualquiera de los 3 archivos mencionados arriba</param>
        private void calcularMedidasDeCadaCelda(archivo Archivo, string lineaDeTitulo)
        {
            lineaDeTitulo = lineaDeTitulo.Substring(0, lineaDeTitulo.Length - 1); // quito el último ";" del titulo antes de splitearlo
            string [] tramosDeTitulo = lineaDeTitulo.Split(';');
            List<campo> camposDesdeTitulos = new List<campo>();
            int columnaPosicion = 0;
            int longitudAcumulada = 0;
            foreach (string tramo in tramosDeTitulo)
            {
                campo columna = new campo();
                columna.nombre = tramo;
                columna.desde = longitudAcumulada;
                columna.largo = tramo.Length;
                columna.posicion = columnaPosicion;
                camposDesdeTitulos.Add(columna);
                longitudAcumulada += tramo.Length;
                longitudAcumulada += 1; // salteo el carácter ";" 
                columnaPosicion++;
            }

            for (int i = 0; i < Archivo.listaDeCampos.Count; i++)
            {
                for(int j = 0; j<camposDesdeTitulos.Count; j++)
                {
                    if (Archivo.listaDeCampos[i].posicion == camposDesdeTitulos[j].posicion)
                    {
                        Archivo.listaDeCampos[i].desde = camposDesdeTitulos[j].desde;
                        Archivo.listaDeCampos[i].largo = camposDesdeTitulos[j].largo;
                        break;
                    }
                }
            }
            

        }


        /// <summary>
        /// Método que somete a validaciones a cada dato leído, registrando en LOG si el dato no pasa alguna validación, devolviendo FALSE como valor de retorno
        /// </summary>
        /// <param name="celda">ES EL DATO A VALIDAR llega genéricamente como string pero si se espera un numerico se trata de parsear, por ejemplo</param>
        /// <param name="linea">número de renglón leído desde el archivo (para el LOG)</param>
        /// <param name="campo">Estructura con los datos desde las preferencias, que debe pasar el dato </param>
        /// <param name="nombreArchivo">nombre del archivo al que pertenece el dato a validar (para el LOG)</param>
        /// <param name="nombreDeCampo">nombre de la columna al que pertenece la celda (para el LOG)</param>
        /// <returns></returns>
        private bool esCeldaValida(string celda, int linea, campo campo, string nombreArchivo, string nombreDeCampo)
        {
            bool respuesta = true;
            string texto;

            switch (campo.tipo)
            {
                case "decimal":
                    try
                    {
                        Convert.ToDecimal(celda.TrimEnd());
                    }
                    catch (Exception)
                    {
                        respuesta = false;
                        texto = "archivo: " + nombreArchivo + " linea:" + linea.ToString() + " campo: " + nombreDeCampo + " valor: " + celda + " no es tipo " + campo.tipo;
                        utiles.escribirArchivoTexto(ruta, texto);
                    }
                    break;
                case "varchar":
                    try
                    {
                        celda.ToString();
                    }
                    catch (Exception)
                    {
                        respuesta = false;
                        texto = "archivo: " + nombreArchivo + " linea:" + linea.ToString() + " campo: " + nombreDeCampo + " valor: " + celda + "  no es tipo " + campo.tipo;
                        utiles.escribirArchivoTexto(ruta, texto);
                    }

                    break;
            }

            return respuesta;
        }




        /// <summary>
        /// Se lee el archivo de preferencias XML y se vuelca a listas para poder ser usado para el parseo y la validación de datos
        /// </summary>
        /// <param name="rutaCompleta">es la ruta donde encontrar el XML</param>
        public void leerPreferencias(string rutaCompleta)
        {

            XmlDataDocument documento = new XmlDataDocument();
            
            documento.Load(rutaCompleta);
            foreach (XmlNode nodo in documento.DocumentElement.ChildNodes)
            {
                if (nodo.HasChildNodes)
                {
                    foreach (XmlNode nodo2 in nodo.ChildNodes)
                    {
 
                        archivo nuevoArchivo = new archivo();
                        nuevoArchivo.nombre = nodo2.Attributes[0].Value;
                        nuevoArchivo.bcp = nodo2.Attributes[1].Value;
                        if (nodo2.HasChildNodes)
                        {
                            foreach (XmlNode nodo3 in nodo2.ChildNodes)
                            {
                                campo nuevoCampo = new campo();

                                nuevoCampo.nombre = nodo3.Attributes[0].Value;
                                nuevoCampo.tipo = nodo3.Attributes[1].Value;
                                nuevoCampo.largo = Convert.ToInt32(nodo3.Attributes[2].Value);
                                nuevoCampo.posicion = Convert.ToInt32(nodo3.Attributes[3].Value);
                                nuevoArchivo.listaDeCampos.Add(nuevoCampo);
                            
                            }
                        }
                        listaPreferencias.ListaDePreferencias.Add(nuevoArchivo);
                

                    }
                }
            }
            
        }

        /// <summary>
        /// Una vez terminado de procesar todos los archivos, estos se mueven a la carpeta destino especificada en APP.CONFIG con el nombre del archivo 
        /// agregándole la fecha y hora de su procesamiento
        /// registrando esta operación en el log de pasos exitosos
        /// </summary>
        private void moverArchivos()
        {
            string desde = utiles.leerArchivoConfiguración("rutaLecturaArchivos");
            string hasta = utiles.leerArchivoConfiguración("rutaSalidaArchivos");
            DirectoryInfo directorio = new DirectoryInfo(desde);
            string destino;
            string fecha = System.DateTime.Now.ToString("yyyyMMdd_hhmmss");
            foreach (FileInfo archivo in directorio.GetFiles())
            {

                destino = hasta + archivo.Name.Replace(".txt", "_") + fecha + ".txt";
                pasosExitososContenido = " el archivo " + archivo + " se movió desde " + desde + " a " + hasta + " con el nombre " + destino;
                archivo.MoveTo(destino);
                utiles.escribirArchivoTexto(logPasosExitosos, pasosExitososContenido);
            }

        }

        #endregion

        #region --------------- botones -------------------------------

        /// <summary>
        /// Permite mostrar el archivo MXL de preferencias para verlo o editarlo
        /// esta acción se registra en el LOG de pasos exitosos
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnArchivoConfiguracion_Click(object sender, EventArgs e)
        {
            string ruta = utiles.leerArchivoConfiguración("rutaApreferencias");
            utiles.cargarArchivo(ruta);
            pasosExitososContenido =  " se cargó el archivo " + ruta;
            utiles.escribirArchivoTexto(logPasosExitosos, pasosExitososContenido);
        }

        /// <summary>
        /// las rutas de las carpetas donde se procesarán los archivos se encuentran en el APP.CONFIG
        /// esta acción se registra en el LOG de pasos exitosos
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAbrirCarpetaLectura_Click(object sender, EventArgs e)
        {
            string ruta = utiles.leerArchivoConfiguración("rutaLecturaArchivos");
            utiles.cargarArchivo(ruta);
            pasosExitososContenido =  " se cargó el archivo " + ruta;
            utiles.escribirArchivoTexto(logPasosExitosos, pasosExitososContenido);
        }

        /// <summary>
        /// Permite mostrar el archivo de LOG de errores para verlo
        /// esta acción se registra en el LOG de pasos exitosos
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLog_Click(object sender, EventArgs e)
        {
            string ruta = utiles.leerArchivoConfiguración("rutaAlLogErrores");
            utiles.cargarArchivo(ruta);
            pasosExitososContenido = " se cargó el archivo " + ruta;
            utiles.escribirArchivoTexto(logPasosExitosos, pasosExitososContenido);
        }

        /// <summary>
        /// botón que desencadena el evento completo de procesar todos los archivos indicados en el XML logeando errores de encontrarlos
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnProcesar_Click(object sender, EventArgs e)
        {
            this.lblEstado.Text = "INICIADO";
            this.Text = "PROCESANDO ARCHIVOS";
            Proceso();
            this.lblEstado.Text = ".........TERMINADO";
            this.Text = "Proceso de archivos";
        }


        #endregion

        #region -------------------información al pasar el mouse sobre controles -------------------------
        private void btnProcesar_MouseHover(object sender, EventArgs e)
        {
            ttInformacion.Show("   Este botón inicia el procesamiento de los archivos  \n   eket_GIM.txt, ekpocm_GIM.txt y ekkopc_GIM.txt \n   al final notificará su terminación  ", this.btnProcesar,5000);
        }

        private void frmProcesamiento_MouseHover(object sender, EventArgs e)
        {
            ttInformacion.Show("   Tenga presente que los 3 archivos:\n   eket_GIM.txt, ekpocm_GIM.txt y ekkopc_GIM.txt \n   deben tener los títulos completos antes de procesar", this, 5000);
        }

        private void btnAbrirCarpetaLectura_MouseHover(object sender, EventArgs e)
        {
            ttInformacion.Show("   Este botón abre la carpeta donde deben depositarse los archivos  \n    eket_GIM.txt, ekpocm_GIM.txt y ekkopc_GIM.txt \n    para ser procesados", this.btnAbrirCarpetaLectura,8000);
        }

        private void btnArchivoConfiguracion_MouseHover(object sender, EventArgs e)
        {
            ttInformacion.Show("    Este botón muestra un archivo XML con la configuración de formato que deben cumplir los archivos \n    eket_GIM.txt, ekpocm_GIM.txt y ekkopc_GIM.txt \n   para poder ser procesados ", this.btnArchivoConfiguracion,8000);
        }

        private void btnLog_MouseHover(object sender, EventArgs e)
        {
            ttInformacion.Show("    Este botón abre en la aplicación que la máquina tenga asignada para ver archivos de texto, el registro de errores de la aplicación \n     si hubiera algún problema, genere una incidencia adjuntando este archivo a la misma para poder analizarlo ", this.btnLog,12000);
        }

        #endregion


    }
}
