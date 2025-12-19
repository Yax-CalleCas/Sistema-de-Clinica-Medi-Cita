using Microsoft.AspNetCore.Mvc;

using MediCita.Web.Entidades;
using System.Threading.Tasks;

namespace MediCita.Web.Controllers
{
    public class PacienteController : Controller
    {
        // Página principal del Paciente
        public IActionResult Index()
        {
            return View();
        }

    }
}
