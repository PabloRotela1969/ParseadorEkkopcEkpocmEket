using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;

namespace ParseadorEkkopcEkpocmEket
{
    class DAO
    {
        SqlConnection conexionAtablas;

        /// <summary>
        /// Constructor por defecto
        /// </summary>
        public DAO()
        {
        }

        /// <summary>
        /// constructor con connection string
        /// </summary>
        /// <param name="cadena"></param>
        public DAO(string cadena)
        {
            conexionAtablas = new SqlConnection(cadena);
        }

        /// <summary>
        /// metodo que envía el script de borrado a la tabla BCP que reciba por parámetro
        /// </summary>
        /// <param name="nombreTabla"></param>
        public void borrarTabla(string nombreTabla)
        {
            ejecutarScript("delete from " + nombreTabla);
        }

        /// <summary>
        /// secuencia de storeds a ser invocados luego del llenado de las tablas BCP
        /// </summary>
        public void ejecutarSecuenciaDeStoredsFinal()
        {
            ejecutarScript("WDC_Carga_Pedidos_SAP");
            ejecutarScript("WDC_Carga_WDC_PedidosPosItem_SAP");
            ejecutarScript("WDC_ActualizarPedidos_SAP");
            ejecutarScript("WDC_ActualizarPedidos_PosItem_SAP");
            ejecutarScript("WDC_EliminarPedidos_PosItem_SAP");
        }

        /// <summary>
        /// metodo para ejecutar un comando y relanzando una excepción si surgiera desde tablas
        /// </summary>
        /// <param name="script"></param>
        private void ejecutarScript(string script)
        {
            SqlCommand comando = new SqlCommand();
            comando.CommandText = script;
            comando.Connection = conexionAtablas;
            
            
            try
            {
                if (conexionAtablas.State == ConnectionState.Closed)
                {
                    conexionAtablas.Open();
                    comando.ExecuteNonQuery();
                }

            }
            catch (Exception e)
            {
                //string mensaje = e.Message;
                throw new Exception(e.Message);
            }
            finally
            {
                conexionAtablas.Close();
            }
        }

        /// <summary>
        /// método usado para crear Datatables con la estructura de la tabla BCP con el nombre de ésta recibido por parámetro
        /// </summary>
        /// <param name="nombreTabla"></param>
        /// <returns></returns>
        public DataTable  consultarTabla(string nombreTabla)
        {
            SqlCommand comando = new SqlCommand();
            comando.CommandText = "select top 1 * from " + nombreTabla;
            comando.Connection = conexionAtablas;
            DataTable tabla = new DataTable();
            SqlDataAdapter adaptador;
            try
            {
                if (conexionAtablas.State == ConnectionState.Closed)
                {
                    conexionAtablas.Open();
                    adaptador = new SqlDataAdapter(comando);
                    adaptador.Fill(tabla);
                }
                
            }
            catch (Exception e)
            {
                string mensaje = e.Message;
            }
            finally
            {
                conexionAtablas.Close();
            }
            return tabla;
        }

        /// <summary>
        /// método que permite ingresar todo un datatable directamente a una tabla BCP en vez de hacerlo línea por línea
        /// </summary>
        /// <param name="data"></param>
        /// <param name="tblToFill"></param>
        public void InserciónMasivaPorCadaTabla(DataTable data, string tblToFill)
        {
            try
            {

                if (conexionAtablas.State == ConnectionState.Closed)
                {
                    conexionAtablas.Open();
                    using (SqlBulkCopy bc = new SqlBulkCopy(conexionAtablas, SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.KeepNulls, null))
                    {
                        bc.DestinationTableName = tblToFill;
                        bc.BatchSize = data.Rows.Count;
                        bc.WriteToServer(data);
                        bc.Close();
                        data.Rows.Clear();
                    }

                }
            }
            catch (Exception e)
            {
            }
            finally
            {
                conexionAtablas.Close();
            }


        
        }
    }








}
