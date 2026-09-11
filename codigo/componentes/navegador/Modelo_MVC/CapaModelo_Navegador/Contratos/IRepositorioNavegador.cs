using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;

namespace CapaModelo_Navegador.Contratos
{
    public interface IRepositorioNavegador
    {
        OdbcDataAdapter LlenarTabla(string tabla);
        List<string> ObtenerTablas();
        DataTable ConsultarTodo(string tabla);
        List<string> ObtenerColumnas(string tabla);

        bool ExisteLlavePrimaria(
            string tabla,
            string[] campos,
            string[] valores);

        bool ExisteValorCampo(
            string tabla,
            string campo,
            string valor);

        bool InsertarRegistro(
            string tabla,
            Dictionary<string, string> datos);

        bool ActualizarRegistro(
            string tabla,
            Dictionary<string, string> valores,
            Dictionary<string, string> claves);

        bool EliminarRegistro(
            string tabla,
            Dictionary<string, string> claves);

        DataTable ObtenerEsquemaTabla(string tabla);

        object ObtenerSiguienteValorLlave(
            string tabla,
            string columna);

        void EjecutarSql(string sql);

        void GuardarDatos(string sql);

        OdbcDataAdapter FiltrarTabla(
            string tabla,
            string columna,
            string valor);
    }
}