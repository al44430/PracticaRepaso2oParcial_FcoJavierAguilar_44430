using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;     // Necesario para ingresar datos a las tablas
using System;
using System.Collections.Generic;   // Necesario para consultar datos de las tablas


namespace PracticaRepaso2oParcial_FcoJavierAguilar_44430.Pages
{
    public class IndexModel : PageModel
    {
        [TempData]    // conserva el mensaje despues del RedirectToPage
        public string MensajeSistema { get; set; } = "";

        public class Producto
        {
            public string SKU { get; set; } = "";
            public string Nombre { get; set; } = "";
            public decimal Precio { get; set; }
            public int Existencia { get; set; }
            public string Categoria { get; set; } = "";
        }

        public List<Producto> Almacen { get; set; } = new List<Producto>();

        string cadenaConexion = "Server=localhost\\SQLEXPRESS01;Database=SupermercadoDB;Trusted_Connection=True;TrustServerCertificate=True;";

        // Lista blanca: categoria -> tabla.
        // Nombre de tabla NO va por parámetro. Solo de esta lista. Así nadie inyecta SQL.
        static readonly Dictionary<string, string> Tablas = new Dictionary<string, string>
        {
            { "Abarrotes", "Abarrotes" },
            { "Lacteos", "Lacteos" },
            { "Frutas y Verduras", "FrutasVerduras" },
            { "Limpieza", "Limpieza" }
        };

        public void OnGet()
        {
            CargarAlmacen();
        }

        // Lee las 4 tabla en una sola consulta
        void CargarAlmacen()
        {
            Almacen.Clear();
            try
            {
                using (SqlConnection conexion = new SqlConnection(cadenaConexion))
                {
                    conexion.Open();

                    string queryLectura = @"
                        SELECT SKU, Nombre, 'Abarrotes' AS Categoria, Precio, Existencia FROM Abarrotes
                        UNION ALL
                        SELECT SKU, Nombre, 'Lácteos', Precio, Existencia FROM Lacteos
                        UNION ALL
                        SELECT SKU, Nombre, 'Frutas y Verduras', Precio, Existencia FROM FrutasVerduras
                        UNION ALL
                        SELECT SKU, Nombre, 'Limpieza', Precio, Existencia FROM Limpieza
                        ORDER BY Categoria, Nombre";

                    using (SqlCommand comando = new SqlCommand(queryLectura, conexion))
                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            Almacen.Add(new Producto
                            {
                                SKU = lector.GetString(0),
                                Nombre = lector.GetString(1),
                                Categoria = lector.GetString(2),
                                Precio = lector.GetDecimal(3),
                                Existencia = lector.GetInt32(4)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MensajeSistema = "Error de conexión: " + ex.Message;
            }
        }

        public IActionResult OnPost(string nombre, string categoria, decimal precio, int existencia)
        {
            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrEmpty(categoria))
            {
                MensajeSistema = "Error: El nombre y la categoría son obligatorios.";
                CargarAlmacen();
                return Page();
            }

            if (precio < 0 || existencia < 0)
            {
                MensajeSistema = "Error: Precio y existencia no pueden ser negativos.";
                CargarAlmacen();
                return Page();
            }

            // Categoría rara? No hay tabla. Parar.
            if (!Tablas.TryGetValue(categoria, out string tabla))
            {
                MensajeSistema = "Error: Categoría no válida.";
                CargarAlmacen();
                return Page();
            }

            try
            {
                using (SqlConnection conexion = new SqlConnection(cadenaConexion))
                {
                    conexion.Open();

                    // Id y SKU los pone SQL. Aquí solo mandamos 3 campos.
                    // SCOPE_IDENTITY() = Id recién creado. Con él pedimos el SKU.
                    string consultaSql = $@"
                        INSERT INTO {tabla} (Nombre, Precio, Existencia)
                        VALUES (@nom, @pre, @exi);
                        SELECT SKU FROM {tabla} WHERE Id = SCOPE_IDENTITY();";

                    using (SqlCommand comando = new SqlCommand(consultaSql, conexion))
                    {
                        comando.Parameters.AddWithValue("@nom", nombre.Trim());
                        comando.Parameters.AddWithValue("@pre", precio);
                        comando.Parameters.AddWithValue("@exi", existencia);

                        string skuGenerado = (string)comando.ExecuteScalar();

                        MensajeSistema = $"¡Producto {nombre} registrado con el código automático: {skuGenerado}!";
                    }
                }

                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                MensajeSistema = "Falla al guardar en BD: " + ex.Message;
                CargarAlmacen();
                return Page();
            }
        }
    }
}