using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;
using System.Windows.Forms;

namespace ParseadorEkkopcEkpocmEket
{
    public static class utiles
    {
        /// <summary>
        /// metodo que recibe una ruta completa o sea, ruta y nombre del archivo a escribir y un contenido a cargar
        /// se usa para los logs, por eso es de solo APPEND
        /// </summary>
        /// <param name="rutaCompleta">ruta y nombre de archivo</param>
        /// <param name="contenido">renglón de log</param>
        public static void escribirArchivoTexto(string rutaCompleta, string contenido)
        {

            FileStream strim = new FileStream(rutaCompleta, FileMode.Append, FileAccess.Write);
            StreamWriter escritor = new StreamWriter(strim);
            contenido = System.DateTime.Now + " : " + contenido;
            escritor.WriteLine(contenido);
            escritor.Flush();
            escritor.Close();
            strim.Close();

        }

        /// <summary>
        /// permite leer una clave desde el APP.CONFIG 
        /// </summary>
        /// <param name="clave">clave a buscar</param>
        /// <returns>linea leída con esa clave desde el APP.CONFIG</returns>
        public static string leerArchivoConfiguración(string clave)
        {
            string devolver;
            devolver = System.Configuration.ConfigurationSettings.AppSettings[clave].ToString();
            

            return devolver;
        }

        /// <summary>
        /// metodo que manda al sistema operativo un archivo para que este gestione su carga
        /// con el ide apropiado a su extensión ( Ej. un archivo txt abrirá el block de notas y lo cargará para mostrar su contenido )
        /// </summary>
        /// <param name="rutaCompleta"></param>
        public static void cargarArchivo(string rutaCompleta)
        {
            try
            {
                System.Diagnostics.Process.Start(rutaCompleta);
            }
            catch (Exception e)
            {
                MessageBox.Show("No se encuentra");
            }
        }



    }
}
