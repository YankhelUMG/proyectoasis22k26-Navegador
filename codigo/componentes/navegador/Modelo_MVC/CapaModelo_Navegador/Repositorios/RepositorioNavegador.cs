using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CapaModelo_Navegador.Contratos;

namespace CapaModelo_Navegador.Repositorios
{
    public class RepositorioNavegador : IRepositorioNavegador
    {
        private conexionBD conn = new conexionBD();

        // =====================================================
        // CONSULTAS
        // =====================================================

        public OdbcDataAdapter LlenarTabla(string tabla)
        {
            Validar(tabla);

            OdbcConnection conexion = conn.conexion();

            return new OdbcDataAdapter(
                "SELECT * FROM " + tabla,
                conexion);
        }

        public List<string> ObtenerTablas()
        {
            List<string> tablas = new List<string>();

            using (OdbcConnection conexion = conn.conexion())
            {
                DataTable datos = conexion.GetSchema("Tables");

                foreach (DataRow fila in datos.Rows)
                {
                    string nombre = Valor(fila, "TABLE_NAME");
                    string tipo = Valor(fila, "TABLE_TYPE");

                    if (!string.IsNullOrEmpty(nombre) &&
                        (string.IsNullOrEmpty(tipo) ||
                         tipo.IndexOf("TABLE",
                         StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        if (!tablas.Contains(
                            nombre,
                            StringComparer.OrdinalIgnoreCase))
                        {
                            tablas.Add(nombre);
                        }
                    }
                }
            }

            tablas.Sort();
            return tablas;
        }

        public DataTable ConsultarTodo(string tabla)
        {
            Validar(tabla);

            return EjecutarConsulta(
                "SELECT * FROM " + tabla);
        }

        public List<string> ObtenerColumnas(string tabla)
        {
            List<string> columnas = new List<string>();

            using (OdbcConnection conexion = conn.conexion())
            {
                DataTable datos = conexion.GetSchema(
                    "Columns",
                    new string[] { null, null, tabla, null });

                foreach (DataRow fila in datos.Rows)
                {
                    string columna = Valor(
                        fila, "COLUMN_NAME");

                    if (!string.IsNullOrEmpty(columna))
                        columnas.Add(columna);
                }
            }

            return columnas;
        }

        public OdbcDataAdapter FiltrarTabla(
            string tabla,
            string columna,
            string valor)
        {
            Validar(tabla);
            Validar(columna);

            string sql =
                "SELECT * FROM " + tabla +
                " WHERE " + columna + " LIKE ?";

            OdbcConnection conexion = conn.conexion();

            OdbcCommand comando =
                new OdbcCommand(sql, conexion);

            comando.Parameters.AddWithValue(
                "@valor", "%" + valor + "%");

            return new OdbcDataAdapter(comando);
        }

        // =====================================================
        // VALIDACIONES
        // =====================================================

        public bool ExisteLlavePrimaria(
            string tabla,
            string[] campos,
            string[] valores)
        {
            if (campos == null || valores == null ||
                campos.Length == 0 ||
                campos.Length != valores.Length)
                return false;

            Validar(tabla);

            string where = CrearWhere(campos);

            string sql =
                "SELECT COUNT(*) FROM " +
                tabla + " WHERE " + where;

            return EjecutarScalar(sql, valores) > 0;
        }

        public bool ExisteValorCampo(
            string tabla,
            string campo,
            string valor)
        {
            Validar(tabla);
            Validar(campo);

            string sql =
                "SELECT COUNT(*) FROM " +
                tabla +
                " WHERE " +
                campo +
                " = ?";

            return EjecutarScalar(
                sql,
                new string[] { valor }) > 0;
        }

        // =====================================================
        // INSERTAR
        // =====================================================

        public bool InsertarRegistro(
            string tabla,
            Dictionary<string, string> datos)
        {
            if (datos == null || datos.Count == 0)
                return false;

            Validar(tabla);

            string columnas = string.Join(
                ", ",
                datos.Keys.Select(x =>
                {
                    Validar(x);
                    return x;
                }));

            string parametros = string.Join(
                ", ",
                datos.Keys.Select(x => "?"));

            string sql =
                "INSERT INTO " + tabla +
                " (" + columnas + ") VALUES (" +
                parametros + ")";

            return Ejecutar(sql, datos.Values.ToArray());
        }

        // =====================================================
        // ACTUALIZAR
        // =====================================================

        public bool ActualizarRegistro(
            string tabla,
            Dictionary<string, string> valores,
            Dictionary<string, string> claves)
        {
            if (valores == null || valores.Count == 0 ||
                claves == null || claves.Count == 0)
                return false;

            Validar(tabla);

            var datos = valores
                .Where(x => !claves.ContainsKey(x.Key))
                .ToList();

            if (datos.Count == 0)
                return false;

            string campos = string.Join(
                ", ",
                datos.Select(x =>
                {
                    Validar(x.Key);
                    return x.Key + " = ?";
                }));

            string where = CrearWhere(
                claves.Keys.ToArray());

            string sql =
                "UPDATE " + tabla +
                " SET " + campos +
                " WHERE " + where;

            List<string> parametros =
                datos.Select(x => x.Value).ToList();

            parametros.AddRange(claves.Values);

            return Ejecutar(
                sql,
                parametros.ToArray());
        }

        // =====================================================
        // ELIMINAR
        // =====================================================

        public bool EliminarRegistro(
            string tabla,
            Dictionary<string, string> claves)
        {
            if (claves == null || claves.Count == 0)
                return false;

            Validar(tabla);

            string sql =
                "DELETE FROM " +
                tabla +
                " WHERE " +
                CrearWhere(claves.Keys.ToArray());

            return Ejecutar(
                sql,
                claves.Values.ToArray());
        }

        // =====================================================
        // ESQUEMA
        // =====================================================

        public DataTable ObtenerEsquemaTabla(
            string tabla)
        {
            Validar(tabla);

            DataTable esquema = new DataTable();

            esquema.Columns.Add("COLUMN_NAME");
            esquema.Columns.Add("DATA_TYPE");
            esquema.Columns.Add("IS_NULLABLE");
            esquema.Columns.Add("IS_PRIMARY_KEY",
                typeof(bool));
            esquema.Columns.Add("IS_AUTOINCREMENT",
                typeof(bool));

            using (OdbcConnection conexion = conn.conexion())
            {
                DataTable columnas = conexion.GetSchema(
                    "Columns",
                    new string[] { null, null, tabla, null });

                foreach (DataRow fila in columnas.Rows)
                {
                    DataRow nueva = esquema.NewRow();

                    nueva["COLUMN_NAME"] =
                        Valor(fila, "COLUMN_NAME");

                    nueva["DATA_TYPE"] =
                        Valor(fila, "DATA_TYPE");

                    nueva["IS_NULLABLE"] =
                        Valor(fila, "IS_NULLABLE");

                    string auto =
                        Valor(fila, "IS_AUTOINCREMENT");

                    nueva["IS_AUTOINCREMENT"] =
                        auto.Equals("YES",
                        StringComparison.OrdinalIgnoreCase) ||
                        auto.Equals("TRUE",
                        StringComparison.OrdinalIgnoreCase) ||
                        auto == "1";

                    nueva["IS_PRIMARY_KEY"] = false;

                    esquema.Rows.Add(nueva);
                }

                // Detecta las llaves primarias del driver ODBC.
                try
                {
                    DataTable pk = conexion.GetSchema(
                        "Primary_Keys",
                        new string[] { null, null, tabla });

                    foreach (DataRow fila in pk.Rows)
                    {
                        string columna =
                            Valor(fila, "COLUMN_NAME");

                        DataRow[] filas =
                            esquema.Select(
                                "COLUMN_NAME = '" +
                                columna.Replace("'", "''") +
                                "'");

                        foreach (DataRow dato in filas)
                            dato["IS_PRIMARY_KEY"] = true;
                    }
                }
                catch
                {
                    // Algunos drivers ODBC no exponen Primary_Keys.
                }
            }

            return esquema;
        }

        // =====================================================
        // SIGUIENTE ID
        // =====================================================

        public object ObtenerSiguienteValorLlave(
            string tabla,
            string columna)
        {
            Validar(tabla);
            Validar(columna);

            object resultado = EjecutarScalarObjeto(
                "SELECT MAX(" +
                columna +
                ") FROM " +
                tabla);

            if (resultado == null ||
                resultado == DBNull.Value)
                return 1L;

            long numero;

            if (long.TryParse(
                resultado.ToString(),
                out numero))
                return numero + 1;

            return null;
        }

        // =====================================================
        // SQL
        // =====================================================

        public void EjecutarSql(string sql)
        {
            Ejecutar(sql, new string[0]);
        }

        public void GuardarDatos(string sql)
        {
            EjecutarSql(sql);
        }

        // =====================================================
        // MÉTODOS AUXILIARES
        // =====================================================

        private DataTable EjecutarConsulta(string sql)
        {
            DataTable datos = new DataTable();

            using (OdbcConnection conexion = conn.conexion())
            using (OdbcDataAdapter adapter =
                new OdbcDataAdapter(sql, conexion))
            {
                adapter.Fill(datos);
            }

            return datos;
        }

        private bool Ejecutar(
            string sql,
            string[] parametros)
        {
            using (OdbcConnection conexion = conn.conexion())
            using (OdbcCommand comando =
                new OdbcCommand(sql, conexion))
            {
                foreach (string parametro in parametros)
                    comando.Parameters.AddWithValue(
                        "@p", parametro);

                return comando.ExecuteNonQuery() > 0;
            }
        }

        private int EjecutarScalar(
            string sql,
            string[] parametros)
        {
            return Convert.ToInt32(
                EjecutarScalarObjeto(
                    sql, parametros));
        }

        private object EjecutarScalarObjeto(
            string sql,
            params string[] parametros)
        {
            using (OdbcConnection conexion = conn.conexion())
            using (OdbcCommand comando =
                new OdbcCommand(sql, conexion))
            {
                foreach (string parametro in parametros)
                    comando.Parameters.AddWithValue(
                        "@p", parametro);

                return comando.ExecuteScalar();
            }
        }

        private string CrearWhere(
            string[] campos)
        {
            foreach (string campo in campos)
                Validar(campo);

            return string.Join(
                " AND ",
                campos.Select(x => x + " = ?"));
        }

        private string Valor(
            DataRow fila,
            string columna)
        {
            if (!fila.Table.Columns.Contains(columna) ||
                fila[columna] == DBNull.Value)
                return "";

            return fila[columna].ToString();
        }

        private void Validar(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor) ||
                !Regex.IsMatch(
                    valor,
                    @"^[A-Za-z0-9_$.]+$"))
            {
                throw new ArgumentException(
                    "Nombre no válido.");
            }
        }
    }
}