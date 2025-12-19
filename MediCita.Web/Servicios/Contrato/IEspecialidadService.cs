using MediCita.Web.Entidades;

namespace MediCita.Web.Servicios.Contrato
{
    public interface IEspecialidadService
    {
        Task<List<Especialidad>> Listar();
    }
}