using MediCita.Web.Entidades;
using System.Threading.Tasks;

namespace MediCita.Web.Servicios
{
    public interface IAdminEstadisticas
    {
        Task<PanelEstadisticas> ObtenerEstadisticas();
    }
}
