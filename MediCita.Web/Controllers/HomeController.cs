using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization; // Librería para proteger la vista
using Microsoft.AspNetCore.Mvc;

namespace MediCita.Web.Controllers
{
   // [Authorize] // <--- ¡Esto protege todo el controlador!
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Denegado(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }


        // CERRAR SESIÓN (AccesoController.cs)
        public async Task<IActionResult> Salir()
        {
            // 1. Borra la cookie de autenticación
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // 2. Limpia todas las variables de sesión (como el carrito)
            HttpContext.Session.Clear();

            // 3. Redirige al Login
            return RedirectToAction("Index", "Home");
        }
    }
}
