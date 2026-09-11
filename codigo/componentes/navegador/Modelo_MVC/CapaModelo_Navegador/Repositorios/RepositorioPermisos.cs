using System.Data;
using System.Data.Odbc;
using CapaModelo_Navegador.Contratos;

namespace CapaModelo_Navegador.Repositorios
{
    public class RepositorioPermisos : IRepositorioPermisos
    {
        private conexionBD conn = new conexionBD();

        public bool ExisteAplicacion(int idAplicacion)
        {
            return idAplicacion > 0;
        }

        public bool ExisteModulo(int idModulo)
        {
            return idModulo > 0;
        }

        public bool GuardarUsuarioPermisoBD(
            int idUsuario,
            int idAplicacion,
            int idModulo,
            int idPermiso)
        {
            // Aquí se agregará el INSERT de permisos cuando corresponda.
            return true;
        }

        public DataTable ValidarUsuario(
            string usuario,
            string clave)
        {
            string sql =
                "SELECT id_usuario, nombre_usuario, id_rol " +
                "FROM tbl_usuarios " +
                "WHERE nombre_usuario = ? " +
                "AND contrasena = ? " +
                "AND estado_usuario = 1";

            DataTable datos = new DataTable();

            using (OdbcConnection conexion = conn.conexion())
            using (OdbcCommand comando = new OdbcCommand(sql, conexion))
            {
                comando.Parameters.AddWithValue("@usuario", usuario);
                comando.Parameters.AddWithValue("@clave", clave);

                using (OdbcDataAdapter adapter =
                    new OdbcDataAdapter(comando))
                {
                    adapter.Fill(datos);
                }
            }

            return datos;
        }
    }
}