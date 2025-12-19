using MediCita.Web.Entidades;
using System.Threading.Tasks;

namespace MediCita.Web.Servicios.Contrato
{
    public interface IUsuarioService
    {
        Task<Usuario> ValidarUsuario(string correo, string clave);
        Task<int> RegistrarUsuario(Usuario usuario);

    }
}
