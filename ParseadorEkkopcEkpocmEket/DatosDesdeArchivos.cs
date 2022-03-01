using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParseadorEkkopcEkpocmEket
{
    /// <summary>
    /// clase que contiene cada dato
    /// </summary>
    public class filasDatos
    {
        public List<string> celdas = new List<string>();
    }
    /// <summary>
    /// Clase que contiene conjuntos de datos formando filas, esto representa a un archivo
    /// con su nombre
    /// </summary>
    public class archivoDatos
    {
        public string nombre;
        public List<filasDatos> renglones = new List<filasDatos>();
        
    }

    /// <summary>
    /// Clase que contiene conjunto de archivos, o sea, a los tres enumerados en el archivo de preferencias xml
    /// </summary>
    class DatosDesdeArchivos
    {
        public List<archivoDatos> listaDeArchivos = new List<archivoDatos>();
    }
}
