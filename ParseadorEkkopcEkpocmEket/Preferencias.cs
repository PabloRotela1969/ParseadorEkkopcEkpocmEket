using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParseadorEkkopcEkpocmEket
{
    public class campo
    {
        public string nombre;
        public string tipo;
        public int largo;
        public int posicion;
        public int desde;
    }
    public class archivo
    {
        public string nombre;
        public string bcp;
        public List<campo> listaDeCampos = new List<campo>();
    }
    class Preferencias
    {
        public List<archivo> ListaDePreferencias = new List<archivo>();


    }
}
