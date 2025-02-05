using proyecto.Contracts;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace proyecto.Data
{
    public class MesDbContextAguascalientes : IDbContextAguascalientes
    {
        private static readonly Lazy<MesDbContextAguascalientes> lazy = new Lazy<MesDbContextAguascalientes>(() => new MesDbContextAguascalientes());
        public static MesDbContextAguascalientes Instance { get => lazy.Value; }

        private OracleConnection oracleConnection;
        private OracleDataAdapter oracleDataAdapter;

        private static string connectionStringWip = $"User Id=wiprep;Password=ySNx6m7SU7HYS23;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=10.247.165.106)(PORT=1521))(CONNECT_DATA=(SID=MESFA1)));Min Pool Size=0";
        private static string connectionString = $"User Id=evaread;Password=evaread;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=10.247.165.106)(PORT=1521))(CONNECT_DATA=(SID=MESFA1)));Min Pool Size=0";


        public DataTable RunQuery(string Query)
        {
            // Definir una nueva conexión específica para este método
            using (var oracleConnection = new OracleConnection(connectionString))
            {
                DataTable result = new DataTable();
                try
                {
                    using (var oracleDataAdapter = new OracleDataAdapter(Query, oracleConnection))
                    {
                        oracleConnection.Open();
                        Console.Write(".");
                        oracleDataAdapter.Fill(result);
                        Console.Write(",");
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine($"Error al ejecutar RunQuery: {ex.Message}");
                    // O considera lanzar una excepción si es necesario
                }
                // La conexión se cierra automáticamente cuando sale del bloque using
                return result;
            }
        }

        public DataTable RunQueryWip(string Query)
        {
            // Definir una nueva conexión específica para este método
            using (var oracleConnection = new OracleConnection(connectionStringWip))
            {
                DataTable result = new DataTable();
                try
                {
                    using (var oracleDataAdapter = new OracleDataAdapter(Query, oracleConnection))
                    {
                        oracleConnection.Open();
                        Console.Write(".");
                        oracleDataAdapter.Fill(result);
                        Console.Write(",");
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine($"Error al ejecutar RunQueryWip: {ex.Message}");
                    // O considera lanzar una excepción si es necesario
                }
                // La conexión se cierra automáticamente cuando sale del bloque using
                return result;
            }
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
                    using (var oracleConnection = new OracleConnection(connectionString))
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
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine($"Error al ejecutar la consulta (Intento {retryCount + 1}): {ex.Message}");
                    retryCount++;
                    Thread.Sleep(3000);
                }
                finally
                {
                    if (oracleDataAdapter != null)
                    {
                        oracleDataAdapter.Dispose();
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