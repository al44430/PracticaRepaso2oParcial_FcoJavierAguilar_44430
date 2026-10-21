using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;     // Necesario para ingresar datos a las tablas
using System;
using System.Collections.Generic;   // Necesario para consultar datos de las tablas


namespace PracticaRepaso2oParcial_FcoJavierAguilar_44430.Pages
{
    public class IndexModel : PageModel
    {
        [TempData]
        public string MensajeSistema { get; set; } = "";

        public class Producto
        {
            public string Sku { get; set; }

            public string Nombre { get; set; } = "";

            public string Precio { get; set; } = "";

            public int Existencias { get; set; }

            public string Categoria { get; set; } = "";
        }

        public List<Producto> Productos { get; set; } = new List<Producto>();

        string cadenaConexion = "Server=localhost\\SQLEXPRESS01;Database=SupermercadoDB;Trusted_Connection=True;TrustServerCertificate=True;";

        static readonly Dictionary<string, string> Categorias = new Dictionary<string, string>
        {
            {"Abarrotes","Abarrotes" },
            {"Lacteos","Lacteos" },
            {"Limpieza","Limpieza" },
            {"frutas y Verduras","FrutasVerduras" }
        };


        public void OnGet()
        {
            CargarAlmacen();
        }

        void CargarAlmacen() 
        {
            CargarAlmacen().Clear();
            try
            {
                SELECT* FROM Abarrotes;
                SELECT* FROM Lacteos;
                SELECT* FROM FrutasVerduras;
                SELECT* FROM Limpieza;
            }
            catch (Exception ex)
            {

            }
        }
    }
}
