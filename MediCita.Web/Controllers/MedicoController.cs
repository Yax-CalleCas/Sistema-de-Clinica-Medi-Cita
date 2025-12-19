using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;

namespace MediCita.Web.Controllers
{
    public class MedicoController : Controller
    {
        // Página principal del Médico
        public IActionResult Index()
        {
            return View();
        }


    }
}
