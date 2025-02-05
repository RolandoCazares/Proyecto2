using proyecto.Contracts;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Globalization;
using System.Diagnostics;
using proyecto.Helpers;
using Microsoft.Extensions.Hosting;

namespace proyecto.Data
{
    /// <summary>
    /// This class allows to query data from Continental MES database.
    /// </summary>
    public class OracleDbContext : IDbContext
    {
        private static readonly Lazy<OracleDbContext> lazy = new Lazy<OracleDbContext>(() => new OracleDbContext());
        public static OracleDbContext Instance { get => lazy.Value; }

        private OracleConnection oracleConnection;
        private OracleDataAdapter oracleDataAdapter;

        //private static string connectionString = $"User Id=mdice_reports;Password=mdice_reports;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=FADB003)(PORT=1521))(CONNECT_DATA=(SID=MESFADWH)));Min Pool Size=0";
        //private static string connectionString ="Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=NGDB021-vip.auto.contiwan.com) (PORT = 1521))(CONNECT_DATA=(SERVICE_NAME=REPORTING.auto.contiwan.com)));User ID = mesread;;Password=Continental2018!!;Min Pool Size=0;";
        //private static string connectionString = $"User Id=wiprep;Password=ySNx6m7SU7HYS23;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=10.247.165.106)(PORT=1521))(CONNECT_DATA=(SID=MESFA1)));Min Pool Size=0";
        private static string connectionString = $"User Id=evaread;Password=evaread;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=10.247.165.106)(PORT=1521))(CONNECT_DATA=(SID=MESFA1)));Min Pool Size=0";


        private OracleDbContext()
        {
            oracleConnection = new OracleConnection(connectionString);
        }

        public DataTable RunQuery(string Query)
        {
            DataTable result = new DataTable();
            try
            {
                oracleDataAdapter = new OracleDataAdapter(Query, oracleConnection);

                oracleConnection.Open();
                Console.Write(".");
                oracleDataAdapter.Fill(result);
                Console.Write(",");
                oracleConnection.Close();

            }
            catch (System.Exception)
            {
                oracleConnection.Close();
                return result;
            }
            return result;
        }
        public (DataTable, int) RunQueryFPY(string Query, string info)
        {
            DataTable result = new DataTable();
            int successFlag = 0;
            int maxRetries = 3;
            int retryCount = 0;

            while (retryCount < maxRetries)
            {
                OracleDataAdapter oracleDataAdapter = null;
                try
                {
                    using (oracleDataAdapter = new OracleDataAdapter(Query, oracleConnection))
                    {
                        oracleConnection.Open();
                        Console.Write(".");
                        oracleDataAdapter.Fill(result);
                        //throw new InvalidOperationException("Simulated exception for testing.");
                        Console.Write(",");
                        successFlag = 1;
                        break;
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine($"Error al ejecutar la consulta (Intento {retryCount + 1}): {ex.Message}");
                    retryCount++;
                    Thread.Sleep(1000); // Espera de 1 segundo antes de reintentar
                }
                finally
                {
                    if (oracleDataAdapter != null)
                    {
                        oracleDataAdapter.Dispose(); // Asegurarse de que el adaptador se libere
                    }
                    if (oracleConnection.State == ConnectionState.Open)
                    {
                        oracleConnection.Close();
                    }
                }
            }

            return (result, successFlag);
        }
    }
}
