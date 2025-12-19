using MediCita.Web.Entidades;
using MediCita.Web.Servicios.Contrato;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MediCita.Web.Servicios.Implementacion
{
    public class UsuarioService : IUsuarioService
    {
        private readonly string _cadenaConexion;

        public UsuarioService(IConfiguration configuration)
        {
            _cadenaConexion = configuration.GetConnectionString("CadenaSQL")
                ?? throw new InvalidOperationException("Cadena de conexión 'CadenaSQL' no encontrada.");
        }

        // Validar usuario (login)
        public async Task<Usuario?> ValidarUsuario(string correo, string clave)
        {
            Usuario? usuarioEncontrado = null;

            using var cn = new SqlConnection(_cadenaConexion);
            using var cmd = new SqlCommand("usp_ValidarUsuario", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Correo", correo);
            cmd.Parameters.AddWithValue("@Clave", clave);

            try
            {
                await cn.OpenAsync();
                using var dr = await cmd.ExecuteReaderAsync();

                if (await dr.ReadAsync())
                {
                    usuarioEncontrado = new Usuario
                    {
                        IdUsuario = dr.GetInt32(dr.GetOrdinal("IdUsuario")),
                        NombreCompleto = dr.GetString(dr.GetOrdinal("NombreCompleto")),
                        Correo = dr.GetString(dr.GetOrdinal("Correo")),
                        IdRol = dr.GetInt32(dr.GetOrdinal("IdRol")),
                        NombreRol = dr.GetString(dr.GetOrdinal("NombreRol")),

                        // ⭐ SI ES MEDICO → viene IdMedico
                        IdMedico = dr.IsDBNull(dr.GetOrdinal("IdMedico"))
                                    ? 0
                                    : dr.GetInt32(dr.GetOrdinal("IdMedico"))
                    };
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al validar usuario en la BD", ex);
            }

            return usuarioEncontrado;
        }


        // Registrar nuevo usuario
        public async Task<int> RegistrarUsuario(Usuario usuario)
        {
            using var cn = new SqlConnection(_cadenaConexion);
            using var cmd = new SqlCommand("usp_RegistrarUsuario", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@NombreCompleto", usuario.NombreCompleto);

            cmd.Parameters.Add(new SqlParameter("@DNI", SqlDbType.VarChar, 15)
            {
                Value = usuario.DNI ?? (object)DBNull.Value
            });

            cmd.Parameters.AddWithValue("@Correo", usuario.Correo);
            cmd.Parameters.AddWithValue("@Clave", usuario.Clave);
            cmd.Parameters.AddWithValue("@IdRol", usuario.IdRol > 0 ? usuario.IdRol : 3);

            await cn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : -99;
        }

        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
