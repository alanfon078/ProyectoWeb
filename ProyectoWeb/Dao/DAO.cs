using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ProyectoWeb.Dao
{
    public class DAO
    {
        public DataTable ObtenerServicios()
        {
            DataTable dt = new DataTable();
            string connectionString = ConfigurationManager.ConnectionStrings["ConexionTaller"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT IdServicio, Descripcion, Precio FROM Servicios", conn))
                {
                    conn.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            return dt;
        }

        public int InsertarOrden(int idCliente, int idVehiculo, decimal subtotal, decimal iva, decimal total)
        {
            int folioGenerado = 0;
            string connectionString = ConfigurationManager.ConnectionStrings["ConexionTaller"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO OrdenesServicio (IdCliente, IdVehiculo, Subtotal, IVA, Total) OUTPUT INSERTED.Folio VALUES (@IdCliente, @IdVehiculo, @Subtotal, @IVA, @Total)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdCliente", idCliente);
                    cmd.Parameters.AddWithValue("@IdVehiculo", idVehiculo);
                    cmd.Parameters.AddWithValue("@Subtotal", subtotal);
                    cmd.Parameters.AddWithValue("@IVA", iva);
                    cmd.Parameters.AddWithValue("@Total", total);

                    conn.Open();
                    folioGenerado = (int)cmd.ExecuteScalar();
                }
            }

            return folioGenerado;
        }

        // Método que guarda la orden y sus detalles en una sola transacción
        public int GuardarOrdenCompleta(int idCliente, int idVehiculo, decimal subtotal, decimal iva, decimal total, DataTable detallesServicio)
        {
            int folioGenerado = 0;
            string connectionString = ConfigurationManager.ConnectionStrings["ConexionTaller"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // 1. Iniciar la transacción
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 2. Insertar la Cabecera (OrdenesServicio)
                        string queryCabecera = @"INSERT INTO OrdenesServicio (IdCliente, IdVehiculo, Subtotal, IVA, Total) 
                                                 OUTPUT INSERTED.Folio 
                                                 VALUES (@IdCliente, @IdVehiculo, @Subtotal, @IVA, @Total)";

                        using (SqlCommand cmdCabecera = new SqlCommand(queryCabecera, conn, transaction))
                        {
                            cmdCabecera.Parameters.AddWithValue("@IdCliente", idCliente);
                            cmdCabecera.Parameters.AddWithValue("@IdVehiculo", idVehiculo);
                            cmdCabecera.Parameters.AddWithValue("@Subtotal", subtotal);
                            cmdCabecera.Parameters.AddWithValue("@IVA", iva);
                            cmdCabecera.Parameters.AddWithValue("@Total", total);

                            // Ejecutar y obtener el Folio generado
                            folioGenerado = (int)cmdCabecera.ExecuteScalar();
                        }

                        // 3. Insertar los Detalles (DetallesOrden)
                        string queryDetalle = @"INSERT INTO DetallesOrden (Folio, IdServicio, Cantidad, Precio) 
                                                VALUES (@Folio, @IdServicio, @Cantidad, @Precio)";

                        using (SqlCommand cmdDetalle = new SqlCommand(queryDetalle, conn, transaction))
                        {
                            // Iterar sobre cada fila del DataTable de detalles
                            foreach (DataRow fila in detallesServicio.Rows)
                            {
                                cmdDetalle.Parameters.Clear(); // Limpiar parámetros en cada iteración
                                cmdDetalle.Parameters.AddWithValue("@Folio", folioGenerado);
                                cmdDetalle.Parameters.AddWithValue("@IdServicio", fila["IdServicio"]);
                                cmdDetalle.Parameters.AddWithValue("@Cantidad", fila["Cantidad"]);
                                cmdDetalle.Parameters.AddWithValue("@Precio", fila["Precio"]);

                                cmdDetalle.ExecuteNonQuery();
                            }
                        }

                        // 4. Confirmar la transacción si todo salió bien
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // 5. Revertir todo si hay un error
                        transaction.Rollback();
                        throw new Exception("Error al guardar la orden: " + ex.Message);
                    }
                }
            }

            return folioGenerado; // Retorna el folio para mostrarlo en pantalla
        }

    }
}