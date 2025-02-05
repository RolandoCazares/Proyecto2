
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using proyecto.Contracts;
using proyecto.Models.ExampleModels;
using proyecto.Data;
using System.Security.Policy;
using System.Xml.Linq;
using System;
using NuGet.Configuration;
using proyecto.Models.Top10;
using proyecto.Models.FPY.TopOffender;
using Microsoft.CodeAnalysis.Host;
using proyecto.Models.FPY.Historial;
using proyecto.Models.StockDiag;
using System.Diagnostics;
using proyecto.Models;
using static System.Collections.Specialized.BitVector32;

namespace proyecto.Helpers
{
    /// <summary>
    /// Data table helper allows to convert retrieved data from databases to actual object to use them trhough the application.
    /// </summary>
    public static class DataTableHelper
    {
        private static readonly IDbContext _context = OracleDbContext.Instance;

        public static List<Top10aGranel> DataTableToDataGranelTop10(DataTable data)
        {
            List<Top10aGranel> resultado = new List<Top10aGranel>();

            foreach (DataRow row in data.Rows)
            {
                try
                {
                    string Serial_Number = row["Serial_Number"].ToString();
                    string Modelo = row["Modelo"].ToString();
                    string Proceso = row["Proceso"].ToString();
                    string COMMCELL = row["COMMCELL"].ToString();
                    string FECHA = row["FECHA"].ToString();
                    string EVENT_DATE = row["EVENT_DATE"].ToString();
                    string EVENT_HOUR = row["EVENT_HOUR"].ToString();
                    string ID_TYPE = row["ID_TYPE"].ToString();
                    string ORDEN = row["ORDEN"].ToString();
                    string TEST_NAME = row["TEST_NAME"].ToString();
                    string PASS_FAIL = row["PASS_FAIL"].ToString();
                    string RESULT_OF_TEST = row["RESULT_OF_TEST"].ToString();
                    string LSL = row["LSL"].ToString();
                    string USL = row["USL"].ToString();
                    string UNIDAD_MEDIDA = row["UNIDAD_MEDIDA"].ToString();
     

                    resultado.Add(new Top10aGranel()
                    {
                        Serial_Number = Serial_Number,
                        Modelo=Modelo,
                        Proceso=Proceso,
                        COMMCELL= COMMCELL,
                        FECHA= FECHA,
                        EVENT_DATE = EVENT_DATE,
                        EVENT_HOUR= EVENT_HOUR,
                        ID_TYPE= ID_TYPE,
                        ORDEN= ORDEN,
                        TEST_NAME= TEST_NAME,
                        PASS_FAIL= PASS_FAIL,
                        RESULT_OF_TEST= RESULT_OF_TEST,
                        UNIDAD_MEDIDA= UNIDAD_MEDIDA

                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return resultado;
        }

        public static List<Top10Data> DataTableToDataTop10byGroup(List<Top10aGranel> Data)
        {

            List<Top10Data> DataListTabla = Data
            .GroupBy(item => new { item.Proceso, item.COMMCELL, item.TEST_NAME })
            .Select(group => new Top10Data
            {
                Proceso = group.Key.Proceso,
                COMMCELL = group.Key.COMMCELL,
                TEST_NAME = group.Key.TEST_NAME,
                TEST_COUNT = group.Count()
            })
            .ToList();

            return DataListTabla;
        }

        public static List<Top10Objeto> DataTableToDataTop10(List<Top10Data> DataListTabla)
        {
            List<Top10Objeto> resultadoGrafica = new List<Top10Objeto>();

            List<string> Procesos = DataListTabla.Select(item => item.Proceso).Distinct().ToList();
            foreach (string Proceso in Procesos)
            {
                List<Top10ByChart> DataParaBarras = DataListTabla
                .Where(s => s.Proceso == Proceso)
                .Select(item => new Top10ByChart
                {
                    TEST_NAME = item.TEST_NAME,
                    TEST_COUNT = item.TEST_COUNT
                }).OrderByDescending(s => s.TEST_COUNT)
                .Take(10)
                .ToList();

                Top10Objeto top10Objeto = new Top10Objeto()
                {
                    Proceso = Proceso,
                    dataOfProcess = DataParaBarras,

                };

                resultadoGrafica.Add(top10Objeto);
            }

            


            return resultadoGrafica;
        }

        public static List<ExampleModel> DataTableToDataExample(DataTable data)
        {
            List<ExampleModel> result = new List<ExampleModel>();
            foreach (DataRow row in data.Rows)
            {
                try
                {
                    //var cultureInfo = new CultureInfo("de-DE");
                    string ORDER_ID = row["ORDER_ID"].ToString();
                    string UNIT_ID_TYPE = row["UNIT_ID_TYPE"].ToString();
                    string MATERIAL_ID = row["MATERIAL_ID"].ToString();
                    //DateTime date = DateTime.Parse(dateAsString, cultureInfo); 
                    //DateTime date = DateTime.ParseExact(dateAsString, "dd-MM-yyyy hh:mm:ss", null);
                    //DateTime date = DateTime.ParseExact(dateAsString, "M/d/yyyy h:mm:ss tt", null);
                    string UNIT_ID = row["UNIT_ID"].ToString();//jjkjk
                    string STATION_ID = row["STATION_ID"].ToString();
                    int TEST_RUN = int.Parse(row["TEST_RUN"].ToString());
                    
                    string EVENT_DATE = row["EVENT_DATE"].ToString();
                    string EVENT_HOUR = row["EVENT_HOUR"].ToString();
                    string TESTRUN_RESULT = row["TESTRUN_RESULT"].ToString();
                    string TESTSTEP_RESULT = row["TESTSTEP_RESULT"].ToString();
                    string TESTSTEP_DESC = row["TESTSTEP_DESC"].ToString();
                    string LSL = row["LSL"].ToString();
                    string USL = row["USL"].ToString();
                    string MEASUREMENT_VALUE = row["MEASUREMENT_VALUE"].ToString();
                    string MEASUREMENT_UNIT = row["MEASUREMENT_UNIT"].ToString();



                    result.Add(new ExampleModel()
                    {
                        ORDER_ID = ORDER_ID,
                        UNIT_ID_TYPE = UNIT_ID_TYPE,
                        MATERIAL_ID = MATERIAL_ID,
                        UNIT_ID = UNIT_ID,
                        STATION_ID = STATION_ID,
                        TEST_RUN = TEST_RUN,
                        EVENT_DATE = EVENT_DATE,
                        EVENT_HOUR= EVENT_HOUR,
                        TESTRUN_RESULT = TESTRUN_RESULT,
                        TESTSTEP_RESULT = TESTSTEP_RESULT,
                        TESTSTEP_DESC = TESTSTEP_DESC,
                        LSL = LSL,
                        USL = USL,
                        MEASUREMENT_VALUE = MEASUREMENT_VALUE,
                        MEASUREMENT_UNIT = MEASUREMENT_UNIT,
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return result;
        }

        

        static string ObtenerDiaJuliano(DateTime fecha)
        {
            // Calcular el día juliano
            int diaJuliano = fecha.DayOfYear;

            // Formatear a un número de tres dígitos, agregando ceros iniciales si es necesario
            return diaJuliano.ToString("D3");
        }





        static int ObtenerNumeroJulianoDesdeSerial(string serial)
        {
            // Supongamos que el número juliano está en las posiciones 5 y 6 del serial.
            // Puedes ajustar esto según el formato real de tu serial404K62400758152.
            string numeroJulianoStr = serial.Substring(7, 3);
            return int.Parse(numeroJulianoStr);
        }

        static bool VerificarRango(int numeroJuliano, double valorJulianoInicial, double valorJulianoFinal)
        {
            valorJulianoInicial = valorJulianoInicial -30;
            return numeroJuliano >= valorJulianoInicial && numeroJuliano <= valorJulianoFinal;
        }

        

            public static List<Test> DataTableToFailBitdata(DataTable data)
        {
            List<Test> result = new List<Test>();
            foreach (DataRow row in data.Rows)
            {
                try
                {
                    //var cultureInfo = new CultureInfo("de-DE");
                    string Serial_Number = row["SerialNumber"].ToString();
                    string State = row["State"].ToString();
                    //string RunDate_string = row["RunDate"].ToString();
                    //DateTime parsedDate = DateTime.ParseExact(RunDate_string, "dd/MM/yyyy hh:mm:ss tt", null);
                    string StationId = row["StationId"].ToString();//jjkjk
                    string Model = row["Model"].ToString();
                    string TestNumber = row["TestNumber"].ToString();//jjkjk
                    string Description = row["Description"].ToString();
                    string Value = row["Value"].ToString();
                    string LSL = row["LSL"].ToString();
                    string USL = row["USL"].ToString();


                    result.Add(new Test()
                    {
                        SerialNumber = Serial_Number,
                        State = State,
                        StationId = StationId,
                        Model = Model,
                        TestNumber = TestNumber,
                        Description = Description,
                        Value = Value,
                        LSL = LSL,
                        USL = USL,
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return result;
        }


        public static List<TestDiag> DataTableToFailDiagStock(DataTable data)
        {
            List<TestDiag> result = new List<TestDiag>();
            foreach (DataRow row in data.Rows)
            {
                try
                {
                    //var cultureInfo = new CultureInfo("de-DE");
                    string Serial_Number = row["SerialNumber"].ToString();
                    string State = row["State"].ToString();
                    string Process = row["testplan"].ToString();
                    //DateTime parsedDate = DateTime.ParseExact(RunDate_string, "dd/MM/yyyy hh:mm:ss tt", null);
                    string StationId = row["StationId"].ToString();//jjkjk
                    string Model = row["Model"].ToString();
                    string TestNumber = row["TestNumber"].ToString();//jjkjk
                    string Description = row["Description"].ToString();
                    string Value = row["Value"].ToString();
                    string LSL = row["LSL"].ToString();
                    string USL = row["USL"].ToString();


                    result.Add(new TestDiag()
                    {
                        SerialNumber = Serial_Number,
                        State = State,
                        Process = Process,
                        StationId = StationId,
                        Model = Model,
                        TestNumber = TestNumber,
                        Description = Description,
                        Value = Value,
                        LSL = LSL,
                        USL = USL,
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return result;
        }



        public static List<HistoryModel> DataTableToFPYdata(DataTable data)
        {
            List<HistoryModel> result = new List<HistoryModel>();
            foreach (DataRow row in data.Rows)
            {
                try
                {
                    //var cultureInfo = new CultureInfo("de-DE");
                    string Serial_Number = row["ID_UNIT"].ToString();
                    string Modelo = row["MATERIAL"].ToString();
                    string COMMCELL = row["STATION"].ToString();//jjkjk
                    string Proceso = row["PROCESO"].ToString();//jjkjk
                    
                    string event_date_string = row["DATETIME"].ToString();

                    DateTime parsedDate = DateTime.ParseExact(event_date_string, "dd/MM/yyyy hh:mm:ss tt", null);
                    string fecha = parsedDate.ToString("dd/MM/yyyy");
                    string hora24 = parsedDate.ToString("HH:mm:ss");

                    // Convertir a DateOnly
                    DateOnly EVENT_DATE = new DateOnly(parsedDate.Year, parsedDate.Month, parsedDate.Day);
                    string event_hour_string = row["DATETIME"].ToString();

                    string ID_TYPE = row["ID_TYPE"].ToString();
                    string resultado = row["RESULT"].ToString();
                    string orden = row["JOB"].ToString();


                    result.Add(new HistoryModel()
                    {
                        Serial_Number = Serial_Number,
                        Modelo = Modelo,
                        Proceso = Proceso,
                        COMMCELL = COMMCELL,
                        EVENT_DATE = fecha,
                        EVENT_HOUR = hora24,
                        ID_TYPE = ID_TYPE,
                        result = resultado,
                        ORDEN = orden
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }
            
            return result;
        }


        public static List<HistoryModel> DataTableStockdata(DataTable data)
        {
            List<HistoryModel> result = new List<HistoryModel>();
            foreach (DataRow row in data.Rows)
            {
                try
                {
                    //var cultureInfo = new CultureInfo("de-DE");
                    string Serial_Number = row["ID_UNIT"].ToString();
                    string Modelo = row["MATERIAL"].ToString();
                    string COMMCELL = row["STATION"].ToString();//jjkjk
                    string Proceso = row["PROCESO"].ToString();//jjkjk

                    string event_date_string = row["DATETIME"].ToString();

                    DateTime parsedDate = DateTime.ParseExact(event_date_string, "dd/MM/yyyy hh:mm:ss tt", null);
                    string toDateAsString = parsedDate.ToString("dd/MM/yyyy HH:mm:ss");
                    string fecha = parsedDate.ToString("dd/MM/yyyy");
                    string hora24 = parsedDate.ToString("HH:mm:ss tt");

                    // Convertir a DateOnly
                    DateOnly EVENT_DATE = new DateOnly(parsedDate.Year, parsedDate.Month, parsedDate.Day);
                    string event_hour_string = row["DATETIME"].ToString();

                    string ID_TYPE = row["ID_TYPE"].ToString();
                    string resultado = row["RESULT"].ToString();
                    string orden = row["JOB"].ToString();


                    result.Add(new HistoryModel()
                    {
                        Serial_Number = Serial_Number,
                        Modelo = Modelo,
                        Proceso = Proceso,
                        COMMCELL = COMMCELL,
                        EVENT_DATE = toDateAsString,
                        EVENT_HOUR = hora24,
                        ID_TYPE = ID_TYPE,
                        result = resultado,
                        ORDEN = orden
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return result;
        }

        public static List<HistoryModelDSParametricosString> DataTableStockdataLastFail(DataTable data)
        {
            List<HistoryModelDSParametricosString> result = new List<HistoryModelDSParametricosString>();
            foreach (DataRow row in data.Rows)
            {
                try
                {
                    //var cultureInfo = new CultureInfo("de-DE");
                    string Serial_Number = row["id_unit"].ToString();
                    string IdType = row["id_type"].ToString();
                    string event_date_string = row["datetime"].ToString();
                    string datetime_string = row["datetime"].ToString();
                    string orden = row["job"].ToString();
                    string COMMCELL = row["station"].ToString();//jjkjk
                    string Modelo = row["material"].ToString();
                    string Proceso = row["testplan"].ToString();//jjkjk
                    string Version = row["version"].ToString();//jjkjk
                    string TESTID = row["testid"].ToString();//jjkjk
                    string VALUE = row["value"].ToString();//jjkjk
                    string LSL = row["LSL"].ToString();
                    string USL = row["USL"].ToString();
                    string resultado = row["result"].ToString();
                    string Description = row["description"].ToString();


                    result.Add(new HistoryModelDSParametricosString()
                    {
                        Serial_Number = Serial_Number,
                        ID_TYPE = IdType,
                        run_date = event_date_string,
                        datetime = datetime_string,
                        ORDEN = orden,
                        COMMCELL = COMMCELL,
                        Modelo = Modelo,
                        Proceso = Proceso,
                        Version = Version,
                        TestID = TESTID,
                        Value = VALUE,
                        LSL = LSL,
                        USL = USL,
                        Result = resultado,
                        Descripcion = Description,

                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return result;
        }


        public static List<SpecifictTestModel> DataTableSpecifictdata(DataTable data)
        {
            List<SpecifictTestModel> result = new List<SpecifictTestModel>();
            foreach (DataRow row in data.Rows)
            {
                try
                {
                    //var cultureInfo = new CultureInfo("de-DE");
                    string Id_unit = row["id_unit"].ToString();
                    string Id_type = row["id_type"].ToString();
                    string Datetime = row["datetime"].ToString();//jjkjk
                    string Material = row["material"].ToString();//jjkjk

                    string Testplan = row["testplan"].ToString();


                    string Version = row["version"].ToString();

                    string Station = row["station"].ToString();
                    string Job = row["job"].ToString();
                    string Testid = row["testid"].ToString();

                    string Description = row["description"].ToString();

                    string Value = row["value"].ToString();
                    string Resultado = row["result"].ToString();
                    string usl = row["USL"].ToString();

                    string lsl = row["LSL"].ToString();
                    string Textinfo = row["textinfo"].ToString();


                    result.Add(new SpecifictTestModel()
                    {
                        id_unit = Id_unit,
                        id_type = Id_type,
                        datetime = Datetime,
                        material = Material,
                        testplan = Testplan,
                        version = Version,
                        station = Station,
                        job = Job,
                        testid = Testid,
                        description = Description,

                        value = Value,
                        result = Resultado,
                        USL = usl,
                        LSL = lsl,
                        textinfo = Textinfo,

                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return result;
        }

        public static List<SpecificProcess> DataTableSpecificProcessdata(DataTable data)
        {
            List<SpecificProcess> result = new List<SpecificProcess>();
            foreach (DataRow row in data.Rows)
            {
                try
                {
                    //var cultureInfo = new CultureInfo("de-DE");
                    string Id_unit = row["id_unit"].ToString();
                    string Id_type = row["id_type"].ToString();
                    string Datetime = row["datetime"].ToString();//jjkjk
                    string Material = row["material"].ToString();//jjkjk

                    string Testplan = row["testplan"].ToString();


                    string Station = row["station"].ToString();
                    string Job = row["job"].ToString();

                    string Resultado = row["result"].ToString();

                    string Textinfo = row["textinfo"].ToString();


                    result.Add(new SpecificProcess()
                    {
                        id_unit = Id_unit,
                        id_type = Id_type,
                        datetime = Datetime,
                        material = Material,
                        testplan = Testplan,
                        station = Station,
                        job = Job,
                        textinfo = Textinfo,
                        result = Resultado,

                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return result;
        }


        public static List<HistoryRow> DataTableToFPYdataTopOffender(DataTable data)
        {
            List<HistoryRow> result = new List<HistoryRow>();
            foreach (DataRow row in data.Rows)
            {
                try
                {
                    //var cultureInfo = new CultureInfo("de-DE");
                    string Id_unit = row["id_unit"].ToString();
                    string Id_type = row["id_type"].ToString();
                    string Datetime = row["datetime"].ToString();//jjkjk
                    string Material = row["material"].ToString();//jjkjk

                    string Testplan = row["Proceso"].ToString();


                    string Version = row["version"].ToString();

                    string Station = row["station"].ToString();
                    string Job = row["job"].ToString();
                    string Testid = row["testid"].ToString();

                    string Description = row["description"].ToString();

                    string Value = row["value"].ToString();
                    string Resultado = row["result"].ToString();
                    string usl = row["USL"].ToString();

                    string lsl = row["LSL"].ToString();


                    result.Add(new HistoryRow()
                    {
                        id_unit = Id_unit,
                        id_type = Id_type,
                        datetime = Datetime,
                        material = Material,
                        testplan = Testplan,
                        version = Version,
                        station = Station,
                        job = Job,
                        testid = Testid,
                        description = Description,

                        value = Value,
                        result = Resultado,
                        USL = usl,
                        LSL = lsl
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return result;
        }


    }
}



