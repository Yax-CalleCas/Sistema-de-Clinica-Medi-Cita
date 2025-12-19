using Microsoft.Data.SqlClient;
using System.Data;
using MediCita.Web.Entidades;
using MediCita.Web.Servicios.Contrato;
using Microsoft.Extensions.Configuration;

namespace MediCita.Web.Servicios.Implementacion
{
    public class VentaService : IVentaService
    {
        private readonly string _cadena;

        public VentaService(IConfiguration config)
        {
            _cadena = config.GetConnectionString("CadenaSQL")
                ?? throw new Exception("CadenaSQL no encontrada en appsettings.json");
        }

        public async Task<Venta> Registrar(Venta venta)
        {
            if (venta?.Detalles == null || venta.Detalles.Count == 0)
                throw new ArgumentException("La venta debe contener al menos un detalle.");

            using var cn = new SqlConnection(_cadena);
            using var cmd = new SqlCommand("usp_RegistrarVentaCompleta", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = venta.IdUsuario;
            cmd.Parameters.Add("@MetodoPago", SqlDbType.VarChar, 30).Value = venta.MetodoPago ?? "EFECTIVO";

            SqlParameter tvpParam = cmd.Parameters.Add("@Detalles", SqlDbType.Structured);
            tvpParam.Value = CrearTablaDetalles(venta);
            tvpParam.TypeName = "TVP_DetalleVenta";

            await cn.OpenAsync();

            using var dr = await cmd.ExecuteReaderAsync();
            if (await dr.ReadAsync())
            {
                venta.IdVenta = Convert.ToInt32(dr["IdVenta"]);
                venta.SubTotal = Convert.ToDecimal(dr["SubTotal"]);
                venta.IGV = Convert.ToDecimal(dr["IGV"]);
                venta.Total = Convert.ToDecimal(dr["TotalFinal"]);
                venta.FechaVenta = DateTime.Now;
                return venta;
            }

            throw new Exception("Error al registrar la venta.");
        }

        public async Task<Venta> ObtenerPorId(int idVenta)
        {
            Venta? venta = null;
            using var cn = new SqlConnection(_cadena);
            using var cmd = new SqlCommand("usp_ObtenerVentaDetalle", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@IdVenta", idVenta);
            await cn.OpenAsync();

            using var dr = await cmd.ExecuteReaderAsync();

            // LECTURA DE CABECERA
            if (await dr.ReadAsync())
            {
                venta = new Venta
                {
                    IdVenta = Convert.ToInt32(dr["IdVenta"]),
                    IdUsuario = Convert.ToInt32(dr["IdUsuario"]),
                    FechaVenta = Convert.ToDateTime(dr["FechaVenta"]),
                    SubTotal = Convert.ToDecimal(dr["SubTotal"]),
                    IGV = Convert.ToDecimal(dr["IGV"]),
                    Total = Convert.ToDecimal(dr["Total"]),
                    MetodoPago = dr["MetodoPago"].ToString() ?? "EFECTIVO",
                    Detalles = new List<DetalleVenta>()
                };

                // LECTURA DE DETALLES (Segundo Select del SP)
                if (await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        venta.Detalles.Add(new DetalleVenta
                        {
                            IdMedicamento = Convert.ToInt32(dr["IdMedicamento"]),
                            NombreMedicamento = dr["NombreMedicamento"].ToString() ?? "",
                            Cantidad = Convert.ToInt32(dr["Cantidad"]),
                            PrecioUnitario = Convert.ToDecimal(dr["PrecioUnitario"]),
                            Importe = Convert.ToDecimal(dr["Importe"]),
                            Promocion = dr["Promocion"].ToString() ?? ""
                        });
                    }
                }
            }
            return venta ?? throw new Exception("Venta no encontrada.");
        }

        public async Task<List<DetalleVenta>> ObtenerCarritoPersistente(int idUsuario)
        {
            var lista = new List<DetalleVenta>();
            using var cn = new SqlConnection(_cadena);
            using var cmd = new SqlCommand("usp_ObtenerCarritoPersistente", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
            await cn.OpenAsync();

            using var dr = await cmd.ExecuteReaderAsync();
            while (await dr.ReadAsync())
            {
                lista.Add(new DetalleVenta
                {
                    IdMedicamento = Convert.ToInt32(dr["IdMedicamento"]),
                    NombreMedicamento = dr["NombreMedicamento"].ToString() ?? "",
                    PrecioUnitario = Convert.ToDecimal(dr["PrecioUnitario"]),
                    Cantidad = Convert.ToInt32(dr["Cantidad"]),
                    Importe = Convert.ToDecimal(dr["PrecioUnitario"]) * Convert.ToInt32(dr["Cantidad"])
                });
            }
            return lista;
        }

        public async Task<bool> SincronizarCarrito(int idUsuario, int idMedicamento, int cantidad)
        {
            try
            {
                using var cn = new SqlConnection(_cadena);
                using var cmd = new SqlCommand("usp_SincronizarCarrito", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                cmd.Parameters.AddWithValue("@IdMedicamento", idMedicamento);
                cmd.Parameters.AddWithValue("@Cantidad", cantidad);

                await cn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> QuitarDelCarrito(int idUsuario, int idMedicamento)
        {
            try
            {
                using var cn = new SqlConnection(_cadena);
                using var cmd = new SqlCommand("usp_QuitarDelCarrito", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                cmd.Parameters.AddWithValue("@IdMedicamento", idMedicamento);

                await cn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
                return true;
            }
            catch { return false; }
        }

        // --- AUXILIAR TVP ACTUALIZADO ---
        private static DataTable CrearTablaDetalles(Venta venta)
        {
            var dt = new DataTable();
            // ESTRICTAMENTE el orden y tipos definidos en SQL:
            // 1. IdMedicamento (INT), 2. Cantidad (INT), 3. Precio (DECIMAL), 
            // 4. Nombre (VARCHAR), 5. Imagen (VARCHAR), 6. Promocion (VARCHAR)
            dt.Columns.Add("IdMedicamento", typeof(int));
            dt.Columns.Add("Cantidad", typeof(int));
            dt.Columns.Add("Precio", typeof(decimal));
            dt.Columns.Add("Nombre", typeof(string));
            dt.Columns.Add("ImagenUrl", typeof(string));
            dt.Columns.Add("Promocion", typeof(string));

            foreach (var d in venta.Detalles)
            {
                dt.Rows.Add(
                    d.IdMedicamento,
                    d.Cantidad,
                    d.PrecioUnitario,
                    d.NombreMedicamento ?? "",
                    "", // ImagenUrl (vacío si no se requiere)
                    d.Promocion ?? ""
                );
            }
            return dt;
        }
    }
}