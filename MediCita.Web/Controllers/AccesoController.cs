using MediCita.Web.Entidades;
using MediCita.Web.Servicios.Contrato;
using MediCita.Web.Servicios.Implementacion;
using MediCita.Web.Utilidades;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public class AccesoController : Controller
{
    // Inyección de servicios necesarios para la gestión de usuarios y ventas
    private readonly IUsuarioService _usuarioService;
    private readonly IAdminUsuariosService _adminService;
    private readonly IVentaService _ventaService;

    public AccesoController(IUsuarioService usuarioService, IAdminUsuariosService adminService, IVentaService ventaService)
    {
        _usuarioService = usuarioService;
        _adminService = adminService;
        _ventaService = ventaService;
    }

    // --- VISTA LOGIN ---
    // Muestra la vista de acceso. Si el usuario ya está autenticado, lo redirige a su panel correspondiente.
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirigirSegunRol(User.FindFirst(ClaimTypes.Role)?.Value);
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string correo, string clave)
    {
        // Validación básica de campos vacíos
        if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(clave))
        {
            TempData["Error"] = "Debe ingresar correo y contraseña.";
            return RedirectToAction("Login");
        }

        // Cifrado de la contraseña proporcionada para comparar con la base de datos
        string claveHash = UsuarioService.HashPassword(clave);
        Usuario? usuario = await _usuarioService.ValidarUsuario(correo, claveHash);

        if (usuario == null)
        {
            TempData["Error"] = "Correo o contraseña incorrectos.";
            return RedirectToAction("Index", "Home");
        }

        // --- CARGAR CARRITO PERSISTENTE ---
        // Recupera los productos que el usuario dejó en el carrito en sesiones anteriores desde SQL
        var carritoGuardado = await _ventaService.ObtenerCarritoPersistente(usuario.IdUsuario);
        if (carritoGuardado != null && carritoGuardado.Any())
        {
            // Serializa y guarda la lista de productos en la sesión actual del servidor
            HttpContext.Session.SetObject("CarritoCompra", carritoGuardado);
        }

        // --- GENERAR CLAIMS (IDENTIDAD) ---
        // Información que se incrustará en la cookie de autenticación para uso global en la app
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, usuario.NombreCompleto),
            new Claim(ClaimTypes.Email, usuario.Correo),
            new Claim(ClaimTypes.Role, usuario.NombreRol),
            new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString())
        };

        // Claim específico para médicos, necesario para filtrar citas por su ID de especialista
        if (usuario.NombreRol == "Medico" && usuario.IdMedico > 0)
            claims.Add(new Claim("IdMedico", usuario.IdMedico.ToString()));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var props = new AuthenticationProperties { AllowRefresh = true, IsPersistent = true };

        // Crea la cookie de autenticación en el navegador del cliente
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity), props);

        return RedirigirSegunRol(usuario.NombreRol);
    }

    // --- VISTA REGISTRO ---
    public IActionResult Registro() => View();

    [HttpPost]
    public async Task<IActionResult> Registro(string nombreCompleto, string dni, string correo, string clave)
    {
        // Validación básica
        if (string.IsNullOrWhiteSpace(nombreCompleto) ||
            string.IsNullOrWhiteSpace(correo) ||
            string.IsNullOrWhiteSpace(clave))
        {
            TempData["ErrorRegistro"] = "Todos los campos obligatorios deben completarse.";
            return RedirectToAction("Registro");
        }

        // Validación de formato de correo (backend)
        if (!System.Net.Mail.MailAddress.TryCreate(correo, out _))
        {
            TempData["ErrorRegistro"] = "El correo electrónico no tiene un formato válido.";
            return RedirectToAction("Registro");
        }

        Usuario nuevoPaciente = new()
        {
            NombreCompleto = nombreCompleto.Trim(),
            DNI = string.IsNullOrWhiteSpace(dni) ? null : dni.Trim(),
            Correo = correo.Trim(),
            Clave = UsuarioService.HashPassword(clave),
            IdRol = 3,
            Activo = true
        };

        int idGenerado = await _usuarioService.RegistrarUsuario(nuevoPaciente);

        switch (idGenerado)
        {
            case -1:
                TempData["ErrorRegistro"] = "El correo electrónico ya se encuentra registrado.";
                return RedirectToAction("Registro");

            case -2:
                TempData["ErrorRegistro"] = "El DNI ya se encuentra registrado.";
                return RedirectToAction("Registro");

            case <= 0:
                TempData["ErrorRegistro"] = "No se pudo completar el registro.";
                return RedirectToAction("Registro");

            default:
                TempData["ExitoRegistro"] = "Cuenta creada correctamente. Ahora puedes iniciar sesión.";
                return RedirectToAction("Index", "Home");
        }
    }

    // --- CERRAR SESIÓN ---
    // Elimina la cookie de autenticación y limpia los datos temporales de la sesión (como el carrito)
    public async Task<IActionResult> Salir()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }

    // --- LÓGICA DE ENRUTAMIENTO POR ROL ---
    // Centraliza la redirección del usuario a su respectiva área de trabajo tras el login
    private IActionResult RedirigirSegunRol(string? rol)
    {
        return rol switch
        {
            "Administrador" => RedirectToAction("Dashboard", "Admin"),
            "Medico" => RedirectToAction("MisCitasMedico", "Citas"),
            "Paciente" => RedirectToAction("Catalogo", "Venta"),
            _ => RedirectToAction("Index", "Home"),
        };
    }
}