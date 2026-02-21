<%@ Page Title="Registro de órdenes de servicio" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ProyectoWeb._Default" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadTitle" runat="server">
    <title>Registro de órdenes de servicio</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0-alpha1/dist/css/bootstrap.min.css">
    <style>
        body { background-color: #f4f7fb; font-family: 'Roboto', sans-serif; color: #333; }
        main { background-color: white; border-radius: 10px; box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1); padding: 40px; max-width: 1000px; margin: auto; }
        h1, h3 { color: #1d1d1d; font-weight: 600; margin-bottom: 30px; }

        /* Estilos emulando Excel */
        .lbl-title { width: 100px; text-align: right; background-color: transparent !important; border: none !important; font-weight: bold; }
        .lbl-field { width: 120px; font-weight: 500; }
        .bg-orange-light { background-color: #fce4d6 !important; }
        .bg-yellow-light { background-color: #fff2cc !important; }
        .bg-green-light { background-color: #e2efda !important; }
        .bg-orange-dark { background-color: #ff9900 !important; color: white !important; font-weight: bold; }

        .table th { background-color: #e2efda; text-align: center; }
        .table td { text-align: center; vertical-align: middle; }
    </style>
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main>
        <h1 class="text-center">Registro de Órdenes de Servicio</h1>

        <form runat="server">
            
            <div class="row mb-4">
                <div class="col-md-5 offset-md-7">
                    <div class="input-group mb-1">
                        <span class="input-group-text lbl-title">FOLIO:</span>
                        <asp:TextBox ID="txtFolio" runat="server" CssClass="form-control bg-orange-light" ReadOnly="true"></asp:TextBox>
                    </div>
                    <div class="input-group">
                        <span class="input-group-text lbl-title">FECHA:</span>
                        <asp:TextBox ID="txtFecha" runat="server" CssClass="form-control bg-orange-light" TextMode="Date" ReadOnly="true"></asp:TextBox>
                    </div>
                </div>
            </div>

            <div class="row mb-4">
                <div class="col-md-10">
                    <label class="form-label fw-bold">Seleccionar Cliente:</label>
                    <asp:DropDownList ID="ddlClientes" runat="server" CssClass="form-select mb-3 bg-yellow-light" AutoPostBack="true" OnSelectedIndexChanged="ddlClientes_SelectedIndexChanged">
                    </asp:DropDownList>

                    <div class="input-group mb-1">
                        <span class="input-group-text lbl-title">CLIENTE:</span>
                        <span class="input-group-text lbl-field bg-yellow-light">RFC</span>
                        <asp:TextBox ID="txtRFC" runat="server" CssClass="form-control bg-yellow-light" ReadOnly="true"></asp:TextBox>
                    </div>
                    <div class="input-group">
                        <span class="input-group-text lbl-title"></span>
                        <span class="input-group-text lbl-field bg-yellow-light">NOMBRE</span>
                        <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control bg-yellow-light" ReadOnly="true"></asp:TextBox>
                    </div>
                </div>
            </div>

            <div class="row mb-4">
                <div class="col-md-10">
                    <label class="form-label fw-bold">Seleccionar Vehículo:</label>
                    <asp:DropDownList ID="ddlVehiculos" runat="server" CssClass="form-select mb-3 bg-green-light" AutoPostBack="true" OnSelectedIndexChanged="ddlVehiculos_SelectedIndexChanged">
                    </asp:DropDownList>

                    <div class="input-group mb-1">
                        <span class="input-group-text lbl-title">AUTO:</span>
                        <span class="input-group-text lbl-field bg-green-light">CLAVE</span>
                        <asp:TextBox ID="txtClaveAuto" runat="server" CssClass="form-control bg-green-light" ReadOnly="true"></asp:TextBox>
                    </div>
                    <div class="input-group">
                        <span class="input-group-text lbl-title"></span>
                        <span class="input-group-text lbl-field bg-green-light">DESCRIPCIÓN</span>
                        <asp:TextBox ID="txtDescAuto" runat="server" CssClass="form-control bg-green-light" ReadOnly="true"></asp:TextBox>
                    </div>
                </div>
            </div>

            <hr class="my-4" />

            <div class="row mb-4 align-items-end">
                <div class="col-md-6">
                    <label class="form-label fw-bold">Servicio:</label>
                    <asp:DropDownList ID="ddlServicios" runat="server" CssClass="form-select">
                        <asp:ListItem Text="Seleccione un servicio..." Value="0"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-md-2">
                    <label class="form-label fw-bold">Cantidad:</label>
                    <asp:TextBox ID="txtCantidad" runat="server" CssClass="form-control text-center" TextMode="Number" Text="1" min="1"></asp:TextBox>
                </div>
                <div class="col-md-4 text-end">
                    <asp:Button ID="btnAgregar" runat="server" Text="+ Agregar" CssClass="btn btn-primary w-100" OnClick="btnAgregar_Click" />
                </div>
            </div>

            <div class="table-responsive mb-3">
                <asp:GridView ID="gvDetalles" runat="server" CssClass="table table-bordered table-sm" AutoGenerateColumns="False" ShowHeaderWhenEmpty="True">
                    <Columns>
                        <asp:BoundField DataField="IdServicio" HeaderText="ID SERVICIO" />
                        <asp:BoundField DataField="Descripcion" HeaderText="DESCRIPCION" />
                        <asp:BoundField DataField="Cantidad" HeaderText="CANTIDAD" />
                        <asp:BoundField DataField="Precio" HeaderText="PRECIO" DataFormatString="{0:C}" />
                        <asp:BoundField DataField="Importe" HeaderText="IMPORTE" DataFormatString="{0:C}" />
                    </Columns>
                </asp:GridView>
            </div>

            <div class="row justify-content-end mb-4">
                <div class="col-md-5">
                    <div class="input-group mb-1">
                        <span class="input-group-text w-50 justify-content-end bg-white border-0 fw-bold">SUBTOTAL</span>
                        <asp:TextBox ID="txtSubtotal" runat="server" CssClass="form-control text-end bg-orange-dark" ReadOnly="true"></asp:TextBox>
                    </div>
                    <div class="input-group mb-1">
                        <span class="input-group-text w-50 justify-content-end bg-white border-0 fw-bold">IVA</span>
                        <asp:TextBox ID="txtIva" runat="server" CssClass="form-control text-end bg-orange-dark" ReadOnly="true"></asp:TextBox>
                    </div>
                    <div class="input-group">
                        <span class="input-group-text w-50 justify-content-end bg-white border-0 fw-bold">TOTAL</span>
                        <asp:TextBox ID="txtTotal" runat="server" CssClass="form-control text-end bg-orange-dark" ReadOnly="true"></asp:TextBox>
                    </div>
                </div>
            </div>

            <div class="text-center mt-4">
                <asp:Button ID="btnGuardar" runat="server" Text="Guardar Orden de Servicio" CssClass="btn btn-success btn-lg px-5" OnClick="btnGuardar_Click" />
            </div>

        </form>
    </main>
</asp:Content>