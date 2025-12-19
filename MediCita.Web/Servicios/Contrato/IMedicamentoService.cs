using MediCita.Web.Entidades;

namespace MediCita.Web.Servicios.Contrato
{
    public interface IMedicamentoService
    {
        Task<List<Medicamento>> Listar();
        Task<Medicamento> Obtener(int id);
        Task<bool> Guardar(Medicamento modelo);
        Task<bool> Editar(Medicamento modelo);
        Task<bool> Eliminar(int id);
    }
}