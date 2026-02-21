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
            DAO db = new DAO();

            // Cargar Clientes
            ddlClientes.DataSource = db.ObtenerClientes();
            ddlClientes.DataTextField = "Nombre";
            ddlClientes.DataValueField = "IdCliente"; // Asegúrate que coincida con la columna SQL
            ddlClientes.DataBind();
            ddlClientes.Items.Insert(0, new ListItem("-- Seleccione un Cliente --", "0"));

            // Cargar Vehículos
            ddlVehiculos.DataSource = db.ObtenerVehiculos();
            ddlVehiculos.DataTextField = "Descripcion";
            ddlVehiculos.DataValueField = "IdVehiculo"; // Asegúrate que coincida con la columna SQL
            ddlVehiculos.DataBind();
            ddlVehiculos.Items.Insert(0, new ListItem("-- Seleccione un Vehículo --", "0"));

            // Cargar Servicios
            ddlServicios.DataSource = db.ObtenerServicios();
            ddlServicios.DataTextField = "Descripcion";
            ddlServicios.DataValueField = "IdServicio"; // Coincide con tu SELECT de DAO.cs
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

        protected void ddlClientes_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idSeleccionado = int.Parse(ddlClientes.SelectedValue);
            if (idSeleccionado > 0)
            {
                DAO db = new DAO();
                DataRow cliente = db.ObtenerClientePorId(idSeleccionado);
                if (cliente != null)
                {
                    txtRFC.Text = cliente["RFC"].ToString();
                    txtNombre.Text = cliente["Nombre"].ToString();
                }
            }
            else
            {
                txtRFC.Text = string.Empty;
                txtNombre.Text = string.Empty;
            }
        }

        protected void ddlVehiculos_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idSeleccionado = int.Parse(ddlVehiculos.SelectedValue);
            if (idSeleccionado > 0)
            {
                DAO db = new DAO();
                DataRow auto = db.ObtenerVehiculoPorId(idSeleccionado);
                if (auto != null)
                {
                    txtClaveAuto.Text = auto["Clave"].ToString();
                    txtDescAuto.Text = auto["Descripcion"].ToString();
                }
            }
            else
            {
                txtClaveAuto.Text = string.Empty;
                txtDescAuto.Text = string.Empty;
            }
        }

        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            int idServicio = int.Parse(ddlServicios.SelectedValue);
            if (idServicio == 0) return;

            int cantidad = 0;
            if (!int.TryParse(txtCantidad.Text, out cantidad) || cantidad <= 0) return;

            DAO db = new DAO();
            DataRow servicio = db.ObtenerServicioPorId(idServicio);

            if (servicio != null)
            {
                string descripcion = servicio["Descripcion"].ToString();
                decimal precio = Convert.ToDecimal(servicio["Precio"]);
                decimal importe = precio * cantidad;

                DataTable dt = TablaDetalles;
                dt.Rows.Add(idServicio, descripcion, cantidad, precio, importe);
                TablaDetalles = dt;

                gvDetalles.DataSource = dt;
                gvDetalles.DataBind();

                CalcularTotales();

                ddlServicios.SelectedIndex = 0;
                txtCantidad.Text = "1";
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
                int idCliente = int.Parse(ddlClientes.SelectedValue);
                int idVehiculo = int.Parse(ddlVehiculos.SelectedValue);

                // Validar que seleccionaron opciones válidas
                if (idCliente == 0 || idVehiculo == 0) return;

                DataTable dtDetalles = this.TablaDetalles;

                if (dtDetalles != null && dtDetalles.Rows.Count > 0)
                {
                    DAO db = new DAO();

                    // Usar System.Globalization.NumberStyles.Currency para que ignore el signo de '$' y las comas
                    decimal subtotal = decimal.Parse(txtSubtotal.Text, System.Globalization.NumberStyles.Currency);
                    decimal iva = decimal.Parse(txtIva.Text, System.Globalization.NumberStyles.Currency);
                    decimal total = decimal.Parse(txtTotal.Text, System.Globalization.NumberStyles.Currency);

                    int folioGenerado = db.GuardarOrdenCompleta(idCliente, idVehiculo, subtotal, iva, total, dtDetalles);

                    txtFolio.Text = folioGenerado.ToString();

                    // MÁXIMA FUNCIONALIDAD: Limpiar todo para una nueva captura
                    TablaDetalles = null; // Borra el ViewState
                    gvDetalles.DataSource = null;
                    gvDetalles.DataBind();

                    txtSubtotal.Text = (0m).ToString("C");
                    txtIva.Text = (0m).ToString("C");
                    txtTotal.Text = (0m).ToString("C");

                    ddlClientes.SelectedIndex = 0;
                    ddlVehiculos.SelectedIndex = 0;
                    txtRFC.Text = string.Empty;
                    txtNombre.Text = string.Empty;
                    txtClaveAuto.Text = string.Empty;
                    txtDescAuto.Text = string.Empty;
                }
                else
                {
                    Console.WriteLine("Necesita agregar servicios");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ocurrio un error: " + ex.Message);
            }
        }

    }
}