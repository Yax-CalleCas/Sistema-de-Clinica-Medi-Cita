using Microsoft.Data.SqlClient;
using System.Data;
using MediCita.Web.Entidades;
using MediCita.Web.Servicios.Contrato;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace MediCita.Web.Servicios.Implementacion
{
    public class MedicamentoService : IMedicamentoService
    {
        private readonly IConfiguration _configuration;

        public MedicamentoService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // MÉTODO AUXILIAR
        // Normaliza la promoción:
        // - Solo permite formatos tipo: 5%, 10%, 25%
        // - Cualquier otro valor retorna NULL
        private string NormalizarPromocion(string promocion)
        {
            if (string.IsNullOrWhiteSpace(promocion))
                return null;

            promocion = promocion.Trim().ToUpper();

            // Acepta solo 1 o 2 dígitos seguidos de %
            if (Regex.IsMatch(promocion, @"^\d{1,2}%$"))
                return promocion;

            return null;
        }

        // 1. LISTAR
        public async Task<List<Medicamento>> Listar()
        {
            var lista = new List<Medicamento>();

            using var cn = new SqlConnection(_configuration.GetConnectionString("CadenaSQL"));
            using var cmd = new SqlCommand("usp_ListarMedicamentos", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            await cn.OpenAsync();
            using var dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                lista.Add(new Medicamento
                {
                    IdMedicamento = Convert.ToInt32(dr["IdMedicamento"]),
                    Nombre = dr["Nombre"].ToString(),
                    Laboratorio = dr["Laboratorio"].ToString(),
                    Precio = Convert.ToDecimal(dr["Precio"]),
                    Stock = Convert.ToInt32(dr["Stock"]),
                    ImagenUrl = dr["ImagenUrl"]?.ToString(),
                    Descripcion = dr["Descripcion"]?.ToString(),
                    Promocion = dr["Promocion"]?.ToString(),
                    Categoria = dr["Categoria"]?.ToString()
                });
            }

            return lista;
        }

        // 2. OBTENER
        public async Task<Medicamento> Obtener(int id)
        {
            Medicamento objeto = null;

            using var cn = new SqlConnection(_configuration.GetConnectionString("CadenaSQL"));
            using var cmd = new SqlCommand("usp_ObtenerMedicamento", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdMedicamento", id);

            await cn.OpenAsync();
            using var dr = await cmd.ExecuteReaderAsync();

            if (await dr.ReadAsync())
            {
                objeto = new Medicamento
                {
                    IdMedicamento = Convert.ToInt32(dr["IdMedicamento"]),
                    Nombre = dr["Nombre"].ToString(),
                    Laboratorio = dr["Laboratorio"].ToString(),
                    Precio = Convert.ToDecimal(dr["Precio"]),
                    Stock = Convert.ToInt32(dr["Stock"]),
                    ImagenUrl = dr["ImagenUrl"]?.ToString(),
                    Descripcion = dr["Descripcion"]?.ToString(),
                    Promocion = dr["Promocion"]?.ToString(),
                    Categoria = dr["Categoria"]?.ToString()
                };
            }

            return objeto;
        }

        // 3. GUARDAR
        public async Task<bool> Guardar(Medicamento modelo)
        {
            using var cn = new SqlConnection(_configuration.GetConnectionString("CadenaSQL"));
            using var cmd = new SqlCommand("usp_RegistrarMedicamento", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Nombre", modelo.Nombre);
            cmd.Parameters.AddWithValue("@Laboratorio", modelo.Laboratorio);
            cmd.Parameters.AddWithValue("@Precio", modelo.Precio);
            cmd.Parameters.AddWithValue("@Stock", modelo.Stock);
            cmd.Parameters.AddWithValue("@ImagenUrl", (object?)modelo.ImagenUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Descripcion", (object?)modelo.Descripcion ?? DBNull.Value);

            // Aplicar validación de promoción (solo %)
            cmd.Parameters.AddWithValue("@Promocion",
                (object?)NormalizarPromocion(modelo.Promocion) ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@Categoria", (object?)modelo.Categoria ?? DBNull.Value);

            await cn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        // 4. EDITAR
        public async Task<bool> Editar(Medicamento modelo)
        {
            using var cn = new SqlConnection(_configuration.GetConnectionString("CadenaSQL"));
            using var cmd = new SqlCommand("usp_EditarMedicamento", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@IdMedicamento", modelo.IdMedicamento);
            cmd.Parameters.AddWithValue("@Nombre", modelo.Nombre);
            cmd.Parameters.AddWithValue("@Laboratorio", modelo.Laboratorio);
            cmd.Parameters.AddWithValue("@Precio", modelo.Precio);
            cmd.Parameters.AddWithValue("@Stock", modelo.Stock);
            cmd.Parameters.AddWithValue("@ImagenUrl", (object?)modelo.ImagenUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Descripcion", (object?)modelo.Descripcion ?? DBNull.Value);

            // Aplicar validación de promoción (solo %)
            cmd.Parameters.AddWithValue("@Promocion",
                (object?)NormalizarPromocion(modelo.Promocion) ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@Categoria", (object?)modelo.Categoria ?? DBNull.Value);

            await cn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        // 5. ELIMINAR
        public async Task<bool> Eliminar(int id)
        {
            using var cn = new SqlConnection(_configuration.GetConnectionString("CadenaSQL"));
            using var cmd = new SqlCommand("usp_EliminarMedicamento", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdMedicamento", id);

            await cn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }
    }
}
