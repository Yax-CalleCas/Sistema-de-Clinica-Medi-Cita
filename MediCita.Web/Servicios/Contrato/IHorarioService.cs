using System;
using System.Threading.Tasks;

namespace MediCita.Web.Servicios.Contrato
{
    public interface IHorarioService
    {
     
        Task GenerarHorarioMedico(
            int idMedico,
            TimeSpan horarioInicio,
            TimeSpan horarioFin,
            int duracionMinutos,
            TimeSpan? almuerzoInicio = null,
            TimeSpan? almuerzoFin = null);
    }
}
