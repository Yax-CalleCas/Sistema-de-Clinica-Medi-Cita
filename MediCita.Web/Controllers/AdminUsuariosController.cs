using MediCita.Web.Entidades;
using MediCita.Web.Servicios.Contrato;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;

namespace MediCita.Web.Controllers
{
    // Restringe el acceso exclusivo a usuarios con el rol de Administrador
    [Authorize(Roles = "Administrador")]
    public class AdminUsuariosController : Controller
    {
        private readonly IAdminUsuariosService _admin;
        private readonly IHorarioService _horario;

        public AdminUsuariosController(IAdminUsuariosService admin, IHorarioService horario)
        {
            _admin = admin;
            _horario = horario;
        }

        // --- HELPERS DE RESPUESTA ---
        // Centraliza las notificaciones de éxito mediante TempData y redirección
        private IActionResult Ok(string msg, string action)
        {
            TempData["Success"] = msg;
            return RedirectToAction(action);
        }

        // Centraliza el manejo de errores retornando a la vista con el modelo o redirigiendo al índice
        private IActionResult Fail(string msg, object? model = null)
        {
            TempData["Error"] = msg;
            return model == null ? RedirectToAction(nameof(Index)) : View(model);
        }

        // Carga la lista de especialidades médicas para controles desplegables (Select)
        private async Task LoadEspecialidades(int? selected = null)
        {
            ViewBag.Especialidades = new SelectList(
                await _admin.ListarEspecialidades(),
                "IdEspecialidad",
                "NombreEspec",
                selected
            );
        }

        // --- GESTIÓN DE USUARIOS/PACIENTES ---
        // Lista todos los usuarios registrados en el sistema
        public async Task<IActionResult> Index()
            => View(await _admin.ListarUsuarios());

        // Retorna el formulario para el registro manual de pacientes por parte del admin
        public IActionResult CrearPaciente() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearPaciente(string nombre, string? dni, string correo, string clave)
        {
            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(clave))
                return Fail("Completa todos los campos obligatorios.");

            // Lógica de negocio: Registra al usuario con rol Paciente y devuelve un ID o código de error
            int result = await _admin.CrearPaciente(nombre.Trim(), dni?.Trim(), correo.Trim(), clave);

            return result > 0
                ? Ok("Paciente creado correctamente.", nameof(Index))
                : Fail(result == -1 ? "El correo ya está registrado." : "Error al crear el paciente.");
        }

        // --- GESTIÓN DE MÉDICOS ---
        // Lista exclusivamente al personal médico con sus datos profesionales
        public async Task<IActionResult> Medicos()
            => View(await _admin.ListarMedicos());

        // Inicializa un nuevo objeto Médico con valores por defecto para el formulario
        public async Task<IActionResult> CrearMedico()
        {
            await LoadEspecialidades();
            return View(new Medico
            {
                PrecioConsulta = 80,
                DuracionMinutos = 40,
                Activo = true
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearMedico(Medico model, string clave)
        {
            if (!ModelState.IsValid)
            {
                await LoadEspecialidades(model.IdEspecialidad);
                return View(model);
            }

            // Registro compuesto: Crea la cuenta de usuario y el perfil médico asociado
            int id = await _admin.CrearMedico(
                model.NombreCompleto.Trim(),
                model.DNI?.Trim(),
                model.Correo.Trim(),
                clave,
                model.IdEspecialidad,
                model.CMP.Trim(),
                model.RNE?.Trim(),
                model.Telefono?.Trim(),
                model.PrecioConsulta,
                model.DuracionMinutos,
                model.TipoServicio?.Trim(),
                model.Activo
            );

            return id > 0
                ? Ok("Médico creado correctamente.", nameof(Medicos))
                : Fail(id == -1 ? "Correo duplicado." : "Error al crear el médico.", model);
        }

        // --- EDICIÓN Y SEGURIDAD ---
        // Recupera datos del paciente asegurando que el rol sea el correcto antes de editar
        public async Task<IActionResult> EditarPaciente(int id)
        {
            var paciente = (await _admin.ListarUsuarios())
                .FirstOrDefault(u => u.IdUsuario == id && u.NombreRol == "Paciente");

            return paciente == null ? NotFound() : View(paciente);
        }
        // --- EDICIÓN DE PACIENTE (POST) ---
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPaciente(int idUsuario, string nombre, string? dni, string correo, bool activo)
        {
            // Llamada al servicio que ya tienes implementado
            bool ok = await _admin.EditarPaciente(idUsuario, nombre.Trim(), dni?.Trim(), correo.Trim(), activo);

            if (ok)
                return Ok("Paciente actualizado correctamente.", nameof(Index));
            else
                return Fail("Error al actualizar el paciente o el correo ya existe.");
        }

        // --- EDICIÓN DE MÉDICO (GET) ---
        public async Task<IActionResult> EditarMedico(int id)
        {
            var medico = await _admin.ObtenerMedicoPorId(id);
            if (medico == null) return NotFound();

            await LoadEspecialidades(medico.IdEspecialidad);
            return View(medico);
        }

        // --- EDICIÓN DE MÉDICO (POST) ---
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarMedico(Medico model)
        {
            // Validamos datos básicos
            if (string.IsNullOrWhiteSpace(model.NombreCompleto) || string.IsNullOrWhiteSpace(model.Correo))
            {
                TempData["Error"] = "Nombre y Correo son obligatorios.";
                await LoadEspecialidades(model.IdEspecialidad);
                return View(model);
            }

            // Llamada al método ActualizarMedico de tu servicio
            int result = await _admin.ActualizarMedico(
                model.IdMedico,
                model.NombreCompleto,
                model.DNI,
                model.Correo,
                model.IdEspecialidad,
                model.CMP,
                model.RNE,
                model.Telefono,
                model.PrecioConsulta,
                model.DuracionMinutos,
                model.TipoServicio,
                model.Activo
            );

            if (result > 0)
                return Ok("Médico actualizado correctamente.", nameof(Medicos));

            // Manejo de errores específicos del procedimiento almacenado
            string errorMsg = result == -1 ? "El correo ya está registrado por otro usuario." : "Error al actualizar el médico.";
            TempData["Error"] = errorMsg;
            await LoadEspecialidades(model.IdEspecialidad);
            return View(model);
        }
        // Permite al administrador resetear contraseñas de usuarios en caso de olvido
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarClave(int idUsuario, string nuevaClave)
        {
            if (string.IsNullOrWhiteSpace(nuevaClave))
                return Fail("La contraseña no puede estar vacía.");

            bool ok = await _admin.CambiarClave(idUsuario, nuevaClave);

            return ok
                ? Ok("Contraseña actualizada correctamente.", nameof(Index))
                : Fail("No se pudo cambiar la contraseña.");
        }
    }
}