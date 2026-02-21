using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ProyectoWeb.Dao;

namespace ProyectoWeb
{
    public partial class _Default : Page
    {
        public class ClienteMock { public int Id { get; set; } public string RFC { get; set; } public string Nombre { get; set; } }
        public class AutoMock { public int Id { get; set; } public string Clave { get; set; } public string Descripcion { get; set; } }
        public class ServicioMock { public int Id { get; set; } public string Descripcion { get; set; } public decimal Precio { get; set; } }
        List<ClienteMock> dbClientes = new List<ClienteMock>
        {
            new ClienteMock { Id = 1, RFC = "XAXX010101000", Nombre = "Público en General" },
            new ClienteMock { Id = 2, RFC = "PEPJ800101XYZ", Nombre = "Juan Pérez" },
            new ClienteMock { Id = 3, RFC = "GOMA900215ABC", Nombre = "Ana Gómez" }
        };

        List<AutoMock> dbAutos = new List<AutoMock>
        {
            new AutoMock { Id = 1, Clave = "TOY-001", Descripcion = "Toyota Corolla 2015" },
            new AutoMock { Id = 2, Clave = "NIS-002", Descripcion = "Nissan Sentra 2018" },
            new AutoMock { Id = 3, Clave = "HON-003", Descripcion = "Honda Civic 2020" }
        };

        List<ServicioMock> dbServicios = new List<ServicioMock>
        {
            new ServicioMock { Id = 25, Descripcion = "CAMBIO DE ACEITE", Precio = 1500.00m },
            new ServicioMock { Id = 999, Descripcion = "REVISION DE FRENOS", Precio = 150.00m },
            new ServicioMock { Id = 888, Descripcion = "CAMBIO DE BALATAS", Precio = 400.00m }
        };

        // -------------------------------------------------------------
        // EVENTOS DE LA PÁGINA
        // -------------------------------------------------------------
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                txtFecha.Text = DateTime.Now.ToString("yyyy-MM-dd");

                CargarCombos();
            }
        }
        private void CargarCombos()
        {
            // Cargar Clientes
            ddlClientes.DataSource = dbClientes;
            ddlClientes.DataTextField = "Nombre";
            ddlClientes.DataValueField = "Id";
            ddlClientes.DataBind();
            ddlClientes.Items.Insert(0, new ListItem("-- Seleccione un Cliente --", "0"));

            // Cargar Vehículos
            ddlVehiculos.DataSource = dbAutos;
            ddlVehiculos.DataTextField = "Descripcion";
            ddlVehiculos.DataValueField = "Id";
            ddlVehiculos.DataBind();
            ddlVehiculos.Items.Insert(0, new ListItem("-- Seleccione un Vehículo --", "0"));

            // Cargar Servicios
            ddlServicios.DataSource = dbServicios;
            ddlServicios.DataTextField = "Descripcion";
            ddlServicios.DataValueField = "Id";
            ddlServicios.DataBind();
            ddlServicios.Items.Insert(0, new ListItem("-- Seleccione un Servicio --", "0"));
        }
        private string GenerarNuevoFolio()
        {
            Random rnd = new Random();
            int nuevoFolio = rnd.Next(1000, 9999);

            return "F-" + nuevoFolio.ToString();
        }
        private DataTable TablaDetalles
        {
            get
            {
                if (ViewState["DetallesOrden"] == null)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("IdServicio", typeof(int));
                    dt.Columns.Add("Descripcion", typeof(string));
                    dt.Columns.Add("Cantidad", typeof(int));
                    dt.Columns.Add("Precio", typeof(decimal));
                    dt.Columns.Add("Importe", typeof(decimal));
                    ViewState["DetallesOrden"] = dt;
                }
                return (DataTable)ViewState["DetallesOrden"];
            }
            set
            {
                ViewState["DetallesOrden"] = value;
            }
        }

        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            // 1. Validar que seleccionó un servicio válido (no el "0")
            int idServicio = int.Parse(ddlServicios.SelectedValue);
            if (idServicio == 0) return; // No hace nada si no ha seleccionado uno

            // 2. Validar que la cantidad sea válida
            int cantidad = 0;
            if (!int.TryParse(txtCantidad.Text, out cantidad) || cantidad <= 0) return;

            // 3. Buscar los datos del servicio seleccionado en nuestra lista de prueba
            var servicio = dbServicios.FirstOrDefault(s => s.Id == idServicio);
            if (servicio != null)
            {
                // 4. Calcular el importe (Precio x Cantidad)
                decimal importe = servicio.Precio * cantidad;

                // 5. Traer la tabla de la memoria, agregarle la fila y volverla a guardar
                DataTable dt = TablaDetalles;
                dt.Rows.Add(servicio.Id, servicio.Descripcion, cantidad, servicio.Precio, importe);
                TablaDetalles = dt;

                // 6. Actualizar el GridView en la pantalla
                gvDetalles.DataSource = dt;
                gvDetalles.DataBind();

                // 7. Recalcular el Subtotal, IVA y Total
                CalcularTotales();

                // 8. Opcional: Reiniciar el combo de servicios y cantidad para agregar otro rápido
                ddlServicios.SelectedIndex = 0;
                txtCantidad.Text = "1";
            }
        }

        protected void ddlClientes_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idSeleccionado = int.Parse(ddlClientes.SelectedValue);
            var cliente = dbClientes.FirstOrDefault(c => c.Id == idSeleccionado);

            if (cliente != null)
            {
                txtRFC.Text = cliente.RFC;
                txtNombre.Text = cliente.Nombre;
            }
            else
            {
                txtRFC.Text = string.Empty;
                txtNombre.Text = string.Empty;
            }
        }

        protected void ddlVehiculos_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Obtenemos el ID del auto seleccionado (1, 2, o 3)
            int idSeleccionado = int.Parse(ddlVehiculos.SelectedValue);

            // Buscamos el auto en nuestra lista temporal
            var auto = dbAutos.FirstOrDefault(a => a.Id == idSeleccionado);

            // Si encontró el auto (es decir, no seleccionaron la opción "0")
            if (auto != null)
            {
                txtClaveAuto.Text = auto.Clave;
                txtDescAuto.Text = auto.Descripcion;
            }
            else
            {
                // Si seleccionan "Seleccione un Vehículo", limpiamos los campos
                txtClaveAuto.Text = string.Empty;
                txtDescAuto.Text = string.Empty;
            }
        }
        private void CalcularTotales()
        {
            DataTable dt = TablaDetalles;
            decimal subtotal = 0;

            // Sumar todos los importes de la tabla
            foreach (DataRow row in dt.Rows)
            {
                subtotal += Convert.ToDecimal(row["Importe"]);
            }

            // Calcular el IVA (16%)
            decimal iva = subtotal * 0.16m;

            // Calcular Total
            decimal total = subtotal + iva;

            // Mostrar en los TextBox con formato de moneda ("C")
            txtSubtotal.Text = subtotal.ToString("C");
            txtIva.Text = iva.ToString("C");
            txtTotal.Text = total.ToString("C");
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. Recolectar datos de los controles (Dropdowns y Textboxes)
                int idCliente = int.Parse(ddlClientes.SelectedValue);
                int idVehiculo = int.Parse(ddlVehiculos.SelectedValue);

                // 2. Obtener los detalles. 
                // En WebForms, la tabla temporal de la vista suele guardarse en una Sesión ("Session") de tipo DataTable
                DataTable dtDetalles = (DataTable)Session["TablaDetalles"];

                if (dtDetalles != null && dtDetalles.Rows.Count > 0)
                {
                    // 3. Instanciar clase de acceso a datos y ejecutar
                    DAO db = new DAO();
                    int folioGenerado = db.GuardarOrdenCompleta(idCliente, idVehiculo, Decimal.Parse(txtSubtotal.Text), Decimal.Parse(txtIva.Text), Decimal.Parse(txtTotal.Text), dtDetalles);

                    // 4. Mostrar éxito y asignar el folio autogenerado a la pantalla
                    txtFolio.Text = folioGenerado.ToString();
                    //txtMensajeExito.Text = "Orden registrada exitosamente con el folio: " + folioGenerado;

                    // Limpiar la pantalla y la sesión
                    Session["TablaDetalles"] = null;
                    // LlenarGridView(); // Tu método para limpiar la tabla
                }
                else
                {
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ocurrio un error: "+ex.Message);
            }
        }
    }
}