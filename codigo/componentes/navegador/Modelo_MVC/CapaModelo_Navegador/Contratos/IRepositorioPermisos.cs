using System.Data;

namespace CapaModelo_Navegador.Contratos
{
    public interface IRepositorioPermisos
    {
        bool ExisteAplicacion(int idAplicacion);

        bool ExisteModulo(int idModulo);

        bool GuardarUsuarioPermisoBD(
            int idUsuario,
            int idAplicacion,
            int idModulo,
            int idPermiso);

        DataTable ValidarUsuario(
            string usuario,
            string clave);
    }
}