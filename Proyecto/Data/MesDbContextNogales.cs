using proyecto.Contracts;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace proyecto.Data
{
    public class MesDbContextNogales : IDbContextNogales
    {
        private static readonly Lazy<MesDbContextNogales> lazy = new Lazy<MesDbContextNogales>(() => new MesDbContextNogales());
        public static MesDbContextNogales Instance { get => lazy.Value; }

        private OracleConnection oracleConnection;
        private OracleDataAdapter oracleDataAdapter;

        private static string connectionString = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=NGDB021-vip.auto.contiwan.com)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=REPORTING.auto.contiwan.com)));User ID=ANALYSIS_IC;Password=BigDataAIC123;Min Pool Size = 0";

        private MesDbContextNogales()
        {
            oracleConnection = new OracleConnection(connectionString);
        }

        public DataTable RunQuery(string Query)
        {
            DataTable result = new DataTable();
            oracleDataAdapter = new OracleDataAdapter(Query, oracleConnection);

            try
            {
                oracleConnection.Open();

                oracleDataAdapter.Fill(result);

                oracleConnection.Close();
            }
            catch (Exception) { }

            return result;
        }
    }
}

