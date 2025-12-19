using Microsoft.AspNetCore.Mvc;
using MediCita.Web.Entidades;
using MediCita.Web.Servicios.Contrato;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MediCita.Web.Controllers
{
    public class CitasController : Controller
    {
        private readonly ICitaService _citaService;

        public CitasController(ICitaService citaService)
        {
            _citaService = citaService;
        }

        // --- FLUJO DE RESERVA ---

        // Lista médicos usando la entidad consolidada 'Medico'
        public async Task<IActionResult> Crear(int? idEspecialidad, DateTime? fecha)
        {
            fecha ??= DateTime.Today;
            var medicos = await _citaService.ListarMedicosDisponibles(idEspecialidad, fecha);

            // Agrupación para evitar duplicados en la lista de selección de profesionales
            var listaUnica = medicos.GroupBy(m => m.IdMedico)
                                    .Select(g => g.First())
                                    .ToList();
            return View(listaUnica);
        }

        // Usa 'HorarioMedico' para mostrar los bloques de tiempo
      

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearCita(int idPaciente, int idMedico, DateTime fecha, string horaInicioStr, string horaFinStr, decimal monto)
        {
            TimeSpan.TryParse(horaInicioStr, out TimeSpan horaInicio);
            TimeSpan.TryParse(horaFinStr, out TimeSpan horaFin);

            // El servicio registra la cita usando los parámetros atómicos
            await _citaService.CrearCita(idPaciente, idMedico, fecha.Date, horaInicio, horaFin, monto);

            TempData["Success"] = "Cita creada correctamente";
            return RedirectToAction(nameof(MisCitasPaciente));
        }

        // --- SEGUIMIENTO (PACIENTE Y MÉDICO) ---

        public async Task<IActionResult> MisCitasPaciente()
        {
            int idPaciente = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var citas = await _citaService.ListarCitas(idPaciente, null);
            return View(citas ?? new List<Cita>());
        }

        public async Task<IActionResult> MisCitasMedico()
        {
            int idMedico = Convert.ToInt32(User.FindFirstValue("IdMedico") ?? "0");
            var citas = await _citaService.ListarCitas(null, idMedico);
            return View(citas ?? new List<Cita>());
        }

        // --- GESTIÓN MÉDICA (ATENCIÓN) ---

        public IActionResult Atender(int idCita)
        {
            // Pasamos un objeto Cita vacío o con el ID para vincularlo a la vista
            return View(new Cita { IdCita = idCita });
        }

        //mostrar hoarios 
        public async Task<IActionResult> Horarios(int idMedico, DateTime fecha)
        {
            if (fecha < new DateTime(2019, 1, 1))
                fecha = DateTime.Today;

            var horarios = await _citaService.ListarHorariosDisponibles(idMedico, fecha);

            ViewBag.IdPaciente = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            ViewBag.IdMedico = idMedico;
            ViewBag.Fecha = fecha;

            var medico = (await _citaService.ListarMedicosDisponibles(null, fecha))
                            .FirstOrDefault(m => m.IdMedico == idMedico);

            ViewBag.Precio = medico?.PrecioConsulta ?? 0;

            return View(horarios);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        // Ahora recibimos el objeto 'Cita' consolidado que ya tiene Estado y Nota
        public async Task<IActionResult> Atender(Cita modelo)
        {
            if (modelo.IdCita <= 0) return BadRequest();

            // Actualizamos la cita en la DB usando los campos consolidados
            await _citaService.AtenderCita(modelo.IdCita, modelo.Estado ?? "A", modelo.Nota);

            TempData["Success"] = "Consulta finalizada con éxito.";
            return RedirectToAction(nameof(MisCitasMedico));
        }
    }
}