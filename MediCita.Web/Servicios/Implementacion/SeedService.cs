using MediCita.Web.Servicios.Contrato;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace MediCita.Web.Servicios.Implementacion
{
    public class SeedService : ISeedService
    {
        private readonly string _cadena;

        public SeedService(IConfiguration configuration)
        {
            _cadena = configuration.GetConnectionString("CadenaSQL")
                ?? throw new InvalidOperationException("Cadena de conexión no encontrada.");
        }

        public async Task CrearUsuariosInicialesAsync()
        {
            await using var cn = new SqlConnection(_cadena);
            await cn.OpenAsync();

            // Solo crear el administrador
            var admin = new
            {
                Correo = "admin@medicita.com",
                Nombre = "Jack Cibertec",
                Clave = "admin123",
                Rol = 1
            };

            // 1️ Comprobar si el admin ya existe
            await using var cmdCheck = new SqlCommand("SELECT IdUsuario FROM tb_Usuarios WHERE Correo = @Correo", cn);
            cmdCheck.Parameters.AddWithValue("@Correo", admin.Correo);
            var existe = await cmdCheck.ExecuteScalarAsync();

            if (existe != null)
            {
                Console.WriteLine($"Ya existe: {admin.Correo}");
            }
            else
            {
                // 2️ Crear administrador
                await using var cmdUser = new SqlCommand("usp_CrearUsuarioCompleto", cn)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };

                cmdUser.Parameters.AddWithValue("@NombreCompleto", admin.Nombre);
                cmdUser.Parameters.AddWithValue("@DNI", "00000000");
                cmdUser.Parameters.AddWithValue("@Correo", admin.Correo);
                cmdUser.Parameters.AddWithValue("@Clave", UsuarioService.HashPassword(admin.Clave));
                cmdUser.Parameters.AddWithValue("@IdRol", admin.Rol);

                int idUsuario = Convert.ToInt32(await cmdUser.ExecuteScalarAsync());
                Console.WriteLine($"Administrador creado: {admin.Correo} → IdUsuario {idUsuario}");
            }

            Console.WriteLine("Usuario administrador verificado/creado correctamente.");
        }
    }
}
