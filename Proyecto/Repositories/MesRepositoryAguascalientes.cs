
using proyecto.Contracts;
using proyecto.Data;
using proyecto.Helpers;
using proyecto.Models;
using proyecto.Models.FPY.Historial;
using proyecto.Models.StockDiag;
using System;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

namespace proyecto.Repositories
{
    public class MesRepositoryAguacalientes : IMesRepositoryAguascalientes
    {
        private static IDbContextAguascalientes dbContext = MesDbContextAguascalientes.Instance;
        private static Regex serialNumberPattern = new Regex("^[A-Z0-9_.-]*$");

        public List<Test> GetHistory(string SerialNumber)
        {
            List<Test> testList = new List<Test>();

            if (serialNumberPattern.Match(SerialNumber).Success)
            {
                string query = MesQueryFabric.QueryForFailsHistoryBitacora(SerialNumber);
                DataTable queryResult = dbContext.RunQuery(query);
                testList = DataTableHelper.DataTableToFailBitdata(queryResult);
            }

            return testList;
        }

        public async Task<List<HistoryModel>> GetHistoryDiagStock(string Proceso, string Estacion, DateTime fromDate, DateTime toDate)
        {
            List<HistoryModel> testList = new List<HistoryModel>();
            string query = MesQueryFabric.QueryForPassAndFailsFPY(Proceso, Estacion, fromDate, toDate);
            DataTable queryResult = dbContext.RunQuery(query);
            testList = DataTableHelper.DataTableStockdata(queryResult);

            return testList;
        }

        public async Task<List<HistoryModel>> GetLastUbicationBySerialID(string stringSerialNumbers)
        {
            List<HistoryModel> testList = new List<HistoryModel>();
            string query = MesQueryFabric.QueryForLastUbicationBySerialID(stringSerialNumbers);
            DataTable queryResult = dbContext.RunQuery(query);
            testList = DataTableHelper.DataTableStockdata(queryResult);

            return testList;
        }

        public async Task<List<HistoryModel>> GetHistoryBySerialID(List<string> listaArregloSeriales)
        {

            int blockSize = 300;
            List<HistoryModel> testList = new List<HistoryModel>();
            for (int i = 0; i < listaArregloSeriales.Count; i += blockSize)
            {
                int currentBlockSize = Math.Min(blockSize, listaArregloSeriales.Count - i);

                List<string> currentBlock = listaArregloSeriales.GetRange(i, currentBlockSize);
                string stringSerialNumbers = String.Join("','", currentBlock);
                string query = MesQueryFabric.QueryForHistoryBySerialID(stringSerialNumbers);
                DataTable queryResult = dbContext.RunQuery(query);
                List<HistoryModel> testListTemp = DataTableHelper.DataTableStockdata(queryResult);
                testList.AddRange(testListTemp);
            }

            return testList;
        }

        public async Task<List<HistoryModelDSParametricosString>> GetLastFailBySerialID(List<string> ListstringSerialNumbers)
        {
            List<HistoryModelDSParametricosString> testList = new List<HistoryModelDSParametricosString>();
            List<HistoryModelDSParametricosString> testListFinal = new List<HistoryModelDSParametricosString>();

            string Columna1 = "run.run_state";
            string Columna2 = "med.MRK_EIN_GUT";
            List<string> NumerosDeSerieUnicosEncontrados = new List<string>();
            foreach (var item in ListstringSerialNumbers)
            {
                string query = MesQueryFabric.QueryForAllFailsBySerialID2(item, Columna2);
                DataTable queryResult = dbContext.RunQuery(query);
                List<HistoryModelDSParametricosString> resuladoTemp = DataTableHelper.DataTableStockdataLastFail(queryResult);
                if(resuladoTemp.Count > 0)
                {
                    string NumeroSerieEncontrado = resuladoTemp.Select(w => w.Serial_Number).FirstOrDefault();
                    NumerosDeSerieUnicosEncontrados.Add(NumeroSerieEncontrado);
                }
                testList.AddRange(resuladoTemp);
            }

            foreach(var NumeroSerie in NumerosDeSerieUnicosEncontrados)
            {
                List<HistoryModelDSParametricosString> testListTemp = testList.Where(w => w.Serial_Number == NumeroSerie).ToList();
                List<HistoryModelDSParametricos> testListTempDatetime = new List<HistoryModelDSParametricos>();

                foreach (var item in testListTemp)
                {
                    var newItemConvert = new HistoryModelDSParametricos();

                    newItemConvert.Serial_Number = item.Serial_Number;
                    newItemConvert.ID_TYPE = item.ID_TYPE;
                    newItemConvert.run_date = item.run_date;
                    newItemConvert.ORDEN = item.ORDEN;
                    newItemConvert.COMMCELL = item.COMMCELL;
                    newItemConvert.Modelo = item.Modelo;
                    newItemConvert.Proceso = item.Proceso;
                    newItemConvert.Version = item.Version;
                    newItemConvert.TestID = item.TestID;
                    newItemConvert.Value = item.Value;
                    newItemConvert.LSL = item.LSL;
                    newItemConvert.USL = item.USL;
                    newItemConvert.Descripcion = item.Descripcion;
                    newItemConvert.Result = item.Result;
                    newItemConvert.CountReprocesadas = item.CountReprocesadas;
                    newItemConvert.CountProbadasEnProceso = item.CountProbadasEnProceso;
                    newItemConvert.ParaLinea = item.ParaLinea;

                    // Convertir EVENT_DATE de string a DateTime
                    if (DateTime.TryParseExact(item.datetime, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime))
                    {
                        // Asignar el valor convertido a la propiedad datetime de tipo DateTime
                        newItemConvert.datetime = dateTime;
                    }
                    else
                    {
                        // Manejar el caso en el que la conversión no sea exitosa
                        // Aquí puedes asignar un valor por defecto, lanzar una excepción, etc.
                        throw new Exception($"No se pudo convertir EVENT_DATE: {item.datetime}");
                    }

                    testListTempDatetime.Add(newItemConvert);
                }

                // Suponiendo que testListTempDatetime es una lista de HistoryModelDSParametricos
                var fechaMasReciente = testListTempDatetime.Max(w => w.datetime);

                // Filtra los elementos que tienen la fecha más reciente
                List<HistoryModelDSParametricos> ListaFiltradaPorFecha = testListTempDatetime
                    .Where(w => w.datetime.Date == fechaMasReciente.Date)
                    .ToList();
                int Contador = ListaFiltradaPorFecha.Count;

                HistoryModelDSParametricos testListTempDatetimeRenglon = ListaFiltradaPorFecha.OrderByDescending(w => w.TestID).FirstOrDefault();

                var newItem = new HistoryModelDSParametricosString();

                newItem.Serial_Number = testListTempDatetimeRenglon.Serial_Number;
                newItem.ID_TYPE = testListTempDatetimeRenglon.ID_TYPE;
                newItem.run_date = testListTempDatetimeRenglon.run_date;
                newItem.ORDEN = testListTempDatetimeRenglon.ORDEN;
                newItem.COMMCELL = testListTempDatetimeRenglon.COMMCELL;
                newItem.Modelo = testListTempDatetimeRenglon.Modelo;
                newItem.Proceso = testListTempDatetimeRenglon.Proceso;
                newItem.Version = testListTempDatetimeRenglon.Version;
                newItem.TestID = testListTempDatetimeRenglon.TestID;
                newItem.Value = testListTempDatetimeRenglon.Value;
                newItem.LSL = testListTempDatetimeRenglon.LSL;
                newItem.USL = testListTempDatetimeRenglon.USL;
                newItem.Descripcion = testListTempDatetimeRenglon.Descripcion;
                newItem.Result = testListTempDatetimeRenglon.Result;
                newItem.datetime = testListTempDatetimeRenglon.datetime.ToString("dd/MM/yyyy HH:mm:ss");
                newItem.CountProbadasEnProceso = testListTempDatetimeRenglon.CountProbadasEnProceso;
                newItem.CountReprocesadas = testListTempDatetimeRenglon.CountReprocesadas;
                newItem.ParaLinea = testListTempDatetimeRenglon.ParaLinea;


                testListFinal.Add(newItem);




            }




            return testListFinal;
        }

        public async Task<List<HistoryModelDSParametricosString>> GetSCRAPFailBySerialID(string stringSerialNumbers)
        {
            List<HistoryModelDSParametricosString> testList = new List<HistoryModelDSParametricosString>();
            string query = MesQueryFabric.QueryForAllFailsBySerialID(stringSerialNumbers);
            DataTable queryResult = dbContext.RunQuery(query);
            testList = DataTableHelper.DataTableStockdataLastFail(queryResult);
            return testList;
        }

        public async Task<List<HistoryModel>> GetUnitChange(List<string> listaArregloSeriales)
        {
            int blockSize = 300;
            List<HistoryModel> testList = new List<HistoryModel>();
            for (int i = 0; i < listaArregloSeriales.Count; i += blockSize)
            {
                int currentBlockSize = Math.Min(blockSize, listaArregloSeriales.Count - i);

                List<string> currentBlock = listaArregloSeriales.GetRange(i, currentBlockSize);
                string stringSerialNumbers = String.Join("','", currentBlock);
                string query = MesQueryFabric.QueryForUnitChengeSerialNumber(stringSerialNumbers);
                DataTable queryResult = dbContext.RunQueryWip(query);
                List<HistoryModel> testListTemp = DataTableHelper.DataTableStockdata(queryResult);
                testList.AddRange(testListTemp);
            }
            return testList;
        }

    }
}
