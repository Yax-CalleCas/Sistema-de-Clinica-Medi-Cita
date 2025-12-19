using MediCita.Web.Entidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediCita.Web.Servicios.Contrato
{
    public interface ICitaService
    {
        // Listar médicos disponibles según especialidad y fecha
        Task<List<Medico>> ListarMedicosDisponibles(int? idEspecialidad = null, DateTime? fecha = null);

        // Listar citas según el paciente o el médico
        Task<List<Cita>> ListarCitas(int? idPaciente, int? idMedico);

        // Crear una nueva cita
        Task<int> CrearCita(int idPaciente, int idMedico, DateTime fecha, TimeSpan horaInicio, TimeSpan horaFin, decimal monto);

        // Atender una cita (marcar como atendida, cancelada, etc.)
        Task<bool> AtenderCita(int idCita, string estado, string? nota = null);

        // Listar horarios disponibles de un médico para una fecha
        Task<List<HorarioMedico>> ListarHorariosDisponibles(int idMedico, DateTime fecha);
    }
}
