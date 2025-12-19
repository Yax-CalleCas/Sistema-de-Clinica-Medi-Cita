using Microsoft.AspNetCore.Mvc;
using MediCita.Web.Entidades;
using MediCita.Web.Servicios.Contrato;

namespace MediCita.Web.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class EspecialidadesController : ControllerBase
    {
        private readonly IEspecialidadService _servicio;

        public EspecialidadesController(IEspecialidadService servicio)
        {
            _servicio = servicio;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // Esto retorna los datos en formato JSON automáticamente
            var lista = await _servicio.Listar();
            return Ok(lista);
        }
    }
}