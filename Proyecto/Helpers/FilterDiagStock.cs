using NuGet.Packaging;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using OpenQA.Selenium.Internal;
using proyecto.Contracts;
using proyecto.Models.FPY.Historial;
using proyecto.Models.StockDiag;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace proyecto.Helpers
{
    public class FilterDiagStock : IFilterDiagStock
    {

        private readonly IMesRepositoryAguascalientes _mesRepositoryAgs;
        public FilterDiagStock(IMesRepositoryAguascalientes mesRepositoryAgs)
        {
            _mesRepositoryAgs = mesRepositoryAgs;
        }


        public async Task<List<ResponseDiagStockFinal>> FilterByProductAndWorkShift(string Producto, DateTime FromDate, DateTime ToDate, string workShift)
        {
            List<string> arregloIntervalosHoras = GenerateIntervals(FromDate, ToDate);
            arregloIntervalosHoras.RemoveAt(arregloIntervalosHoras.Count - 1);

            #region hola
            var (arregloDeArreglosDeCOMCCELLSoProcesos, Goal, GoalRolado, SerialNumberChange) = ObtenerEstacionesFPY(Producto);
            string FirstProcessName = arregloDeArreglosDeCOMCCELLSoProcesos[0][0];
            string LastProcessName = arregloDeArreglosDeCOMCCELLSoProcesos[arregloDeArreglosDeCOMCCELLSoProcesos.Length - 1][0];
            int FirstProcessCount = 0;
            int LastProcessCount = 0;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(Producto + "- DiagStock - De: " + FromDate + "a: " + ToDate + " Inicio: " + DateTime.Now);
            List<string> arregloDeGoalsPorProcesos = new List<string>();
            List<string> arregloProcesosVsProcesos = new List<string>();
            int PiezasIngresadasEnFirstProcess = 0;
            int PiezasIngresadasEnLastProcess = 0;
            string ProcessA = "";
            string ProcessB = "";
            string ProcessC = "";
            string EstacionesProcesoC = "";
            bool SwitchBE = false;
            int ContadorProceso = 0;
            int ContadorHistorialesDescargados = 0;
            List<HistoryModel> ResultadoA = new List<HistoryModel>();
            List<HistoryModel> ResultadoB = new List<HistoryModel>();
            List<HistoryModel> ResultadoC = new List<HistoryModel>();
            List<HistoryModel> HistorialDeSeriesPCBA = new List<HistoryModel>();
            List<HistoryModel> HistorialDeSeriesBE = new List<HistoryModel>();
            List<HistoryModel> HistorialReprocesos = new List<HistoryModel>();
            List<HistoryModel> HistorialReprocesosEnTurno = new List<HistoryModel>();
            List<HistoryModelDSParametricosString> HistorialPiezasPendientesEnTurno = new List<HistoryModelDSParametricosString>();
            List<HistoryModelDSParametricosString> HistorialPiezasPendientesPorAnalizarEnTurno = new List<HistoryModelDSParametricosString>();
            List<HistoryModelDSParametricosString> HistorialPiezasPendientesPorIngresarALineaEnTurno = new List<HistoryModelDSParametricosString>();

            List<ResponseDiagStock> ResultadoFinal = new List<ResponseDiagStock>();
            List<ResponseDiagStockFinal> ResultadoFinalFinal = new List<ResponseDiagStockFinal>();

            foreach (var RenglonArreglo in arregloDeArreglosDeCOMCCELLSoProcesos)
            {
                ContadorProceso++;
                List<HistoryModel> ResultadoATemp = new List<HistoryModel>();
                List<HistoryModel> ResultadoBTemp = new List<HistoryModel>();
                List<HistoryModel> ResultadoCTemp = new List<HistoryModel>();
                ResponseDiagStock ResutadoComparacion = new ResponseDiagStock();
                List<HistoryModelDSParametricosString> ResultadoLastUbication = new List<HistoryModelDSParametricosString>();
                List<HistoryModelDSParametricosString> ResultadoSCRAPUbication = new List<HistoryModelDSParametricosString>();
                List<TopOffenderStock> ResultadoTopOffenderStock = new List<TopOffenderStock>();
                List<string> ResultadoSerialNumbersDif = new List<string>();
                List<ResponseStockByHour> ResultadoHistorialPorHora = new List<ResponseStockByHour>();
                var (ProcesoAnterior, ProcesoNuevo, ProcessAvsProcessB, Comparar) = CambioDeProceso(ProcessB, RenglonArreglo[0]);
                ProcessA = ProcesoAnterior;
                ProcessB = ProcesoNuevo;
                int countArreglos = arregloDeArreglosDeCOMCCELLSoProcesos.Count();
                if(ContadorProceso < countArreglos)
                {
                    ProcessC = arregloDeArreglosDeCOMCCELLSoProcesos[ContadorProceso][0];
                    EstacionesProcesoC = arregloDeArreglosDeCOMCCELLSoProcesos[ContadorProceso][3];
                }else
                {
                    ProcessC = "";
                    EstacionesProcesoC = "";
                }
                
                int PorProcesar = 0;
                int PorAnalizar = 0;
                bool ProcesoEsLabel = false;
                bool TerminoLabel = false;
                int DiferenciaResultado = 0;
                if (Comparar)
                {
                    arregloDeGoalsPorProcesos.Add(ProcessAvsProcessB);
                    ResultadoA.Clear();
                    ResultadoA.AddRange(ResultadoB);
                    ResultadoB.Clear();
                }


                string[] arregloComcells = SplitEstaciones(RenglonArreglo[3]);

                #endregion

                List<HistoryModel> resultadoTemp = new List<HistoryModel>();
                if (RenglonArreglo[0] == "LABEL")
                {
                    ProcesoEsLabel = true;
                    List<string> cambioSerialNumber = new List<string>(ResultadoA.Select(h => h.Serial_Number));
                    resultadoTemp = await _mesRepositoryAgs.GetUnitChange(cambioSerialNumber);
                    ResultadoB.AddRange(resultadoTemp);
                }
                else
                {
                    if (ContadorProceso == 1)
                    {
                        foreach (string comccell in arregloComcells)
                        {
                            resultadoTemp = await _mesRepositoryAgs.GetHistoryDiagStock(RenglonArreglo[0], comccell, FromDate, ToDate);
                            ResultadoB.AddRange(resultadoTemp);
                        }
                        List<string> NumerosSerieResultadoTemp = ResultadoB.Select(w => w.Serial_Number).ToList();

                        HistorialDeSeriesPCBA = await _mesRepositoryAgs.GetHistoryBySerialID(NumerosSerieResultadoTemp);
                        List<HistoryModel> ResultadoScrapTemp = HistorialDeSeriesPCBA.Where(h => h.COMMCELL.ToUpper().Contains("REP")).ToList();

                        HistorialReprocesos.AddRange(ResultadoScrapTemp);
                        ResultadoB.Clear();
                        foreach (string Comcell in arregloComcells)
                        {
                            resultadoTemp = HistorialDeSeriesPCBA.Where(w => w.COMMCELL == Comcell).ToList();
                            foreach (var item in resultadoTemp)
                            {
                                item.Proceso = RenglonArreglo[0];
                            }
                            ResultadoB.AddRange(resultadoTemp);
                        }
                    }
                    else
                    {
                        List<string> NumerosSerieProcesadosA = new List<string>();
                        if (ProcessA == "LABEL")
                        {
                            SwitchBE = true;
                            NumerosSerieProcesadosA = ResultadoA.Select(w => w.result).ToList();
                        }
                        else
                        {
                            NumerosSerieProcesadosA = ResultadoA.Select(w => w.Serial_Number).ToList();
                        }

                        if (!SwitchBE)
                        {
                            foreach (string Comcell in arregloComcells)
                            {
                                resultadoTemp = HistorialDeSeriesPCBA.Where(w => w.COMMCELL == Comcell).ToList();
                                foreach (var item in resultadoTemp)
                                {
                                    item.Proceso = RenglonArreglo[0];
                                }

                                ResultadoB.AddRange(resultadoTemp);
                            }
                        }
                        else
                        {
                            if (ContadorHistorialesDescargados == 0 && ProcessA == "LABEL")
                            {
                                ContadorHistorialesDescargados++;
                                HistorialDeSeriesBE = await _mesRepositoryAgs.GetHistoryBySerialID(NumerosSerieProcesadosA);
                                List<HistoryModel> ResultadoScrapTempBE = HistorialDeSeriesBE.Where(h => h.COMMCELL.ToUpper().Contains("REP")).ToList();
                                HistorialReprocesos.AddRange(ResultadoScrapTempBE);
                                ResultadoB.Clear();
                                foreach (string Comcell in arregloComcells)
                                {
                                    resultadoTemp = HistorialDeSeriesBE.Where(w => w.COMMCELL == Comcell).ToList();
                                    foreach (var item in resultadoTemp)
                                    {
                                        item.Proceso = RenglonArreglo[0];
                                    }
                                    ResultadoB.AddRange(resultadoTemp);
                                }
                            }
                            else if (ContadorHistorialesDescargados == 1 && ProcessA != "LABEL")
                            {
                                ResultadoB.Clear();
                                foreach (string Comcell in arregloComcells)
                                {
                                    resultadoTemp = HistorialDeSeriesBE.Where(w => w.COMMCELL == Comcell).ToList();
                                    foreach (var item in resultadoTemp)
                                    {
                                        item.Proceso = RenglonArreglo[0];
                                    }
                                    ResultadoB.AddRange(resultadoTemp);
                                }
                            }
                        }





                    }

                    List<HistoryModel> ResultadoBtemp = FiltradoTotal(ResultadoB, Producto, RenglonArreglo[2], EstacionesProcesoC, HistorialDeSeriesPCBA, HistorialDeSeriesBE, SwitchBE);

                    if (RenglonArreglo[0] == FirstProcessName)
                    {
                        FirstProcessCount = ResultadoBtemp.Count();
                    }
                    if (RenglonArreglo[0] == LastProcessName)
                    {
                        LastProcessCount = ResultadoBtemp.Count();
                    }
                    ResultadoB.Clear();
                    ResultadoB.AddRange(ResultadoBtemp);
                    if (ContadorProceso == 1)
                    {
                        PiezasIngresadasEnFirstProcess = ResultadoB.Count;
                    } 
                    
                }




                if (Comparar)
                {
                    if (ProcessA != "LABEL" && ProcessB == "LABEL")
                    {
                        TerminoLabel = false;
                        ProcesoEsLabel = true;
                    }
                    else if (ProcessA == "LABEL" && ProcessB != "LABEL")
                    {
                        TerminoLabel = true;
                        ProcesoEsLabel = false;
                    }
                    else if (ProcessA != "LABEL" && ProcessB != "LABEL")
                    {
                        TerminoLabel = false;
                        ProcesoEsLabel = false;
                    }
                    var (Diferencia, SerialNumbersDif, SNwithDataDif, SNwithDataDifFail, resultadoComparadoConFallaSerialNumber, resultadoBDepurado) = ComparacionDatos(ResultadoA, ResultadoB, TerminoLabel, ProcesoEsLabel);
                    ResultadoB.Clear();
                    ResultadoB.AddRange(resultadoBDepurado);
                    if (Diferencia > 0)
                    {

                        ResultadoSerialNumbersDif.AddRange(SerialNumbersDif);
                        DiferenciaResultado = Diferencia;
                        List<string> resultadoComparadoConFallaSerialNumberFinal = new List<string>();
                        try
                        {
                            int contador = 0;
                            //Revisar numeros de serie ultimo pase o falla
                            foreach (string itemSerieSospechosa in resultadoComparadoConFallaSerialNumber)
                            {

                                List<HistoryModel> HistorialSerieSospechosa = new List<HistoryModel>();
                                HistorialSerieSospechosa = HistorialDeSeriesPCBA.Where(w => w.Serial_Number == itemSerieSospechosa && !w.COMMCELL.ToUpper().Contains("REP") && !w.COMMCELL.ToUpper().Contains("SCRAP")).ToList();
                                if (HistorialSerieSospechosa.Count == 0)
                                {
                                    HistorialSerieSospechosa = HistorialDeSeriesBE.Where(w => w.Serial_Number == itemSerieSospechosa && !w.COMMCELL.ToUpper().Contains("REP") && !w.COMMCELL.ToUpper().Contains("SCRAP")).ToList();
                                }
                                if (HistorialSerieSospechosa.Count != 0)
                                {
                                    contador++;
                                }

                                List<HistoryModelDSstring> HistoryModelDSconDatetime = new List<HistoryModelDSstring>();

                                #region ConversionDateTime
                                //Convertir a datetime para filtrar
                                foreach (var item in HistorialSerieSospechosa)
                                {
                                    var newItem = new HistoryModelDSstring();
                                    newItem.Serial_Number = item.Serial_Number;
                                    newItem.Modelo = item.Modelo;
                                    newItem.Proceso = item.Proceso;
                                    newItem.COMMCELL = item.COMMCELL;
                                    newItem.EVENT_HOUR = item.EVENT_HOUR;
                                    newItem.ID_TYPE = item.ID_TYPE;
                                    newItem.result = item.result;
                                    newItem.ORDEN = item.ORDEN;


                                    // Convertir EVENT_DATE de string a DateTime
                                    if (DateTime.TryParseExact(item.EVENT_DATE, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime))
                                    {
                                        // Asignar el valor convertido a la propiedad datetime de tipo DateTime
                                        newItem.EVENT_DATE = dateTime;
                                    }
                                    else
                                    {
                                        // Manejar el caso en el que la conversión no sea exitosa
                                        // Aquí puedes asignar un valor por defecto, lanzar una excepción, etc.
                                        throw new Exception($"No se pudo convertir EVENT_DATE: {item.EVENT_DATE}");
                                    }

                                    // Agregar newItem a ResultadoLastUbication
                                    HistoryModelDSconDatetime.Add(newItem);
                                }
                                #endregion

                                HistoryModelDSstring renglonDesicion = HistoryModelDSconDatetime.OrderByDescending(w => w.EVENT_DATE).FirstOrDefault();

                                if (renglonDesicion.result == "F")
                                {
                                    resultadoComparadoConFallaSerialNumberFinal.Add(renglonDesicion.Serial_Number);
                                }

                            }



                            string stringSerialNumbers = string.Join("','", resultadoComparadoConFallaSerialNumber);



                            List<HistoryModelDSParametricosString> ResultadoLastDateTemp = new List<HistoryModelDSParametricosString>();
                            List<HistoryModelDSParametricosString> ResultadoLastUbicationTemp = new List<HistoryModelDSParametricosString>();
                            List<HistoryModelDSParametricosString> ResultadoSCRAPUbicationTemp = new List<HistoryModelDSParametricosString>();


                            ResultadoLastUbicationTemp = await _mesRepositoryAgs.GetLastFailBySerialID(resultadoComparadoConFallaSerialNumberFinal);
                            ResultadoSCRAPUbicationTemp = await _mesRepositoryAgs.GetSCRAPFailBySerialID(stringSerialNumbers);
                            List<string> seriesScrap = ResultadoSCRAPUbicationTemp.Select(w => w.Serial_Number).ToList();

                            ResultadoLastUbicationTemp.RemoveAll(item => seriesScrap.Contains(item.Serial_Number));

                            foreach (var item in ResultadoLastUbicationTemp)
                            {

                                int CountProbadasEnProceso;
                                int CountReprocesadas;
                                int CountScrap;
                                bool ParaLinea = false;
                                List<HistoryModelDSParametricosString> ResultadoLastUbicationFinal = new List<HistoryModelDSParametricosString>();
                                List<HistoryModel> AllInformacionNumerosSerieSospechosos = new List<HistoryModel>();
                                AllInformacionNumerosSerieSospechosos = HistorialDeSeriesPCBA.Where(w => w.Serial_Number == item.Serial_Number).OrderBy(w => w.EVENT_DATE).ToList();
                                if (AllInformacionNumerosSerieSospechosos.Count == 0)
                                {
                                    AllInformacionNumerosSerieSospechosos = HistorialDeSeriesBE.Where(w => w.Serial_Number == item.Serial_Number).OrderBy(w => w.EVENT_DATE).ToList();
                                }
                                var uniqueItems = AllInformacionNumerosSerieSospechosos
                                    .GroupBy(x => new
                                    {
                                        x.Serial_Number,
                                        x.Modelo,
                                        x.Proceso,
                                        x.COMMCELL,
                                        x.EVENT_DATE,
                                        x.EVENT_HOUR,
                                        x.ID_TYPE,
                                        x.result,
                                        x.ORDEN
                                    })
                                    .Select(g => g.First())
                                    .ToList();
                                List<HistoryModel> AllInformacionNumerosSerieSospechososTemp = uniqueItems.Where(w => w.Proceso == ProcessA).Distinct().ToList();
                                // Filtrar elementos únicos por todas las propiedades

                                CountProbadasEnProceso = AllInformacionNumerosSerieSospechososTemp.Count;


                                CountReprocesadas = uniqueItems.Where(h => h.COMMCELL.ToUpper().Contains("REP")).Count();
                                CountScrap = uniqueItems.Where(h => h.COMMCELL.ToUpper().Contains("SCRAP")).Count();
                                HistoryModel StatusUltimoRenglon = uniqueItems.OrderByDescending(w => w.EVENT_DATE).FirstOrDefault();
                                if (StatusUltimoRenglon.COMMCELL.ToUpper().Contains("REP") && CountScrap == 0)
                                {
                                    PorProcesar++;
                                    ParaLinea = true;
                                }
                                else
                                {
                                    PorAnalizar++;
                                }

                                HistoryModelDSParametricosString RenglonFinal = new HistoryModelDSParametricosString
                                {
                                    Serial_Number = item.Serial_Number,
                                    ID_TYPE = item.ID_TYPE,
                                    run_date = item.run_date,
                                    ORDEN = item.ORDEN,
                                    COMMCELL = item.COMMCELL,
                                    Modelo = item.Modelo,
                                    Proceso = item.Proceso,
                                    Version = item.Version,
                                    TestID = item.TestID,
                                    Value = item.Value,
                                    LSL = item.LSL,
                                    USL = item.USL,
                                    Descripcion = item.Descripcion,
                                    Result = item.Result,
                                    datetime = item.datetime,
                                    CountProbadasEnProceso = CountProbadasEnProceso.ToString(),
                                    CountReprocesadas = CountReprocesadas.ToString(),
                                    ParaLinea = ParaLinea,
                                };
                                ResultadoLastUbication.Add(RenglonFinal);

                            }
                            ResultadoSCRAPUbication.AddRange(ResultadoSCRAPUbicationTemp);

                        }
                        catch (Exception ex)
                        {

                        }

                    }
                    ResultadoATemp.AddRange(ResultadoA);
                    ResultadoBTemp.AddRange(ResultadoB);

                    List<HistoryModelDSParametricosString> FalladoFueraTurno = new List<HistoryModelDSParametricosString>();
                    FalladoFueraTurno.AddRange(ResultadoLastUbication);

                    int cantidadDeIntervalos = arregloIntervalosHoras.Count;
                    int contadorIntervalo = 0;
                    foreach (string RenglonPerido in arregloIntervalosHoras)
                    {
                        contadorIntervalo++;
                        List<HistoryModelDSParametricosString> historialDeStockHora = new List<HistoryModelDSParametricosString>();
                        var (FromDateHour, ToDateHour) = ParseInterval(RenglonPerido);


                        try
                        {
                            #region ConvertirADatetime
                            List<HistoryModelDSParametricos> historialDeStockHoraTemp = new List<HistoryModelDSParametricos>();
                            foreach (var item in ResultadoLastUbication)
                            {

                                // Crear una nueva instancia de HistoryModelDS
                                var newItem = new HistoryModelDSParametricos();

                                // Asignar los valores que son iguales en ambos modelos
                                newItem.Serial_Number = item.Serial_Number;
                                newItem.ID_TYPE = item.ID_TYPE;
                                newItem.run_date = item.run_date;
                                newItem.ORDEN = item.ORDEN;
                                newItem.COMMCELL = item.COMMCELL;
                                newItem.Modelo = item.Modelo;
                                newItem.Proceso = item.Proceso;
                                newItem.Version = item.Version;
                                newItem.TestID = item.TestID;
                                newItem.Value = item.Value;
                                newItem.LSL = item.LSL;
                                newItem.USL = item.USL;
                                newItem.Descripcion = item.Descripcion;
                                newItem.Result = item.Result;
                                newItem.CountReprocesadas = item.CountReprocesadas;
                                newItem.CountProbadasEnProceso = item.CountProbadasEnProceso;
                                newItem.ParaLinea = item.ParaLinea;

                                // Convertir EVENT_DATE de string a DateTime
                                if (DateTime.TryParseExact(item.datetime, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime))
                                {
                                    // Asignar el valor convertido a la propiedad datetime de tipo DateTime
                                    newItem.datetime = dateTime;
                                }
                                else
                                {
                                    // Manejar el caso en el que la conversión no sea exitosa
                                    // Aquí puedes asignar un valor por defecto, lanzar una excepción, etc.
                                    throw new Exception($"No se pudo convertir EVENT_DATE: {item.datetime}");
                                }

                                // Agregar newItem a ResultadoLastUbication
                                historialDeStockHoraTemp.Add(newItem);
                            }



                            List<HistoryModelDSParametricos> historialDeStockHoraFinal = historialDeStockHoraTemp
                                .Where(w => w.datetime >= FromDateHour && w.datetime <= ToDateHour)
                                .ToList();

                            foreach (var item in historialDeStockHoraFinal)
                            {


                                // Crear una nueva instancia de HistoryModelDS
                                var newItem = new HistoryModelDSParametricosString();

                                newItem.Serial_Number = item.Serial_Number;
                                newItem.ID_TYPE = item.ID_TYPE;
                                newItem.run_date = item.run_date;
                                newItem.ORDEN = item.ORDEN;
                                newItem.COMMCELL = item.COMMCELL;
                                newItem.Modelo = item.Modelo;
                                newItem.Proceso = item.Proceso;
                                newItem.Version = item.Version;
                                newItem.TestID = item.TestID;
                                newItem.Value = item.Value;
                                newItem.LSL = item.LSL;
                                newItem.USL = item.USL;
                                newItem.Descripcion = item.Descripcion;
                                newItem.Result = item.Result;
                                newItem.datetime = item.datetime.ToString("dd/MM/yyyy HH:mm:ss");
                                newItem.CountProbadasEnProceso = item.CountProbadasEnProceso;
                                newItem.CountReprocesadas = item.CountReprocesadas;
                                newItem.ParaLinea = item.ParaLinea;


                                // Agregar newItem a ResultadoLastUbication
                                historialDeStockHora.Add(newItem);
                                
                            }
                            #endregion

                            // Suponiendo que FalladoFueraTurno es una lista de objetos con una propiedad Serial_Number
                            var serialNumbersToRemove = historialDeStockHora.Select(item => item.Serial_Number).ToHashSet();

                            FalladoFueraTurno.RemoveAll(item => serialNumbersToRemove.Contains(item.Serial_Number));

                        }
                        catch (Exception ex)
                        {
                            // Manejar la excepción, por ejemplo, registrarla o lanzarla nuevamente
                        }

                        // Método para validar y convertir una cadena en formato específico a DateTime






                        ResponseStockByHour ObjetoStockByHour = new ResponseStockByHour();
                        string RangoHora = FormatInterval(RenglonPerido);
                        ObjetoStockByHour = new ResponseStockByHour()
                        {
                            RangoHoras = RangoHora,
                            HistorialDeEsaHora = historialDeStockHora,
                        };

                        ResultadoHistorialPorHora.Add(ObjetoStockByHour);

                        if (cantidadDeIntervalos == contadorIntervalo)
                        {
                            ResponseStockByHour ObjetoStockByHourFueraTurno = new ResponseStockByHour();
                            ObjetoStockByHour = new ResponseStockByHour()
                            {
                                RangoHoras = "Fuera de Turno",
                                HistorialDeEsaHora = FalladoFueraTurno,
                            };

                            ResultadoHistorialPorHora.Add(ObjetoStockByHour);

                            ObjetoStockByHour = new ResponseStockByHour()
                            {
                                RangoHoras = "SCRAP en Turno",
                                HistorialDeEsaHora = ResultadoSCRAPUbication,
                            };

                            ResultadoHistorialPorHora.Add(ObjetoStockByHour);

                        }

                    }
                    List<TopOffenderStock> ResultadoTopOffenderTemp = ResultadoLastUbication
                        .GroupBy(x => x.Descripcion)
                        .Select(g => new TopOffenderStock
                        {
                            Descripcion = g.Key,
                            CountTest = g.Count().ToString()
                        })
                        .ToList();
                    ResultadoTopOffenderStock.AddRange(ResultadoTopOffenderTemp);

                    List<string> NumerosDeSerieSospechosos = new List<string>();
                    NumerosDeSerieSospechosos = ResultadoLastUbication.Select(w => w.Serial_Number).ToList();
                    HistorialPiezasPendientesEnTurno.AddRange(ResultadoLastUbication);
                    HistorialPiezasPendientesPorAnalizarEnTurno.AddRange(ResultadoLastUbication.Where(w => w.ParaLinea == false));
                    HistorialPiezasPendientesPorIngresarALineaEnTurno .AddRange(ResultadoLastUbication.Where(w => w.ParaLinea == true));
                    if (ContadorProceso == countArreglos)
                    {
                        PiezasIngresadasEnLastProcess = ResultadoBTemp.Count;
                    }
                    ResutadoComparacion = new ResponseDiagStock()
                    {
                        ProcessAvsProcessB = ProcessAvsProcessB,
                        Diferencia = DiferenciaResultado,
                        ResultadoProcesoA = ResultadoATemp,
                        ResultadoProcesoB = ResultadoBTemp,
                        SerialNumbersDif = ResultadoSerialNumbersDif,
                        SNwithDataDif = SNwithDataDif,
                        SNwithDataDifFail = ResultadoLastUbication,
                        ResultadoSCRAPUbication = ResultadoSCRAPUbication,
                        ResultadoTopOffenderStock = ResultadoTopOffenderStock,
                        ResponseStockByHour = ResultadoHistorialPorHora,
                        PorAnalizar = PorAnalizar,
                        PorProbarrEnLinea = PorProcesar,
                    };
                    ResultadoFinal.Add(ResutadoComparacion);
                }
            }



            #region ConvertirADatetime
            List<HistoryModelDSstring> historialReprocesoTemp = new List<HistoryModelDSstring>();
            foreach (var item in HistorialReprocesos)
            {
                // Crear una nueva instancia de HistoryModelDS
                var newItem = new HistoryModelDSstring();

                // Asignar los valores que son iguales en ambos modelos
                newItem.Serial_Number = item.Serial_Number;
                newItem.Modelo = item.Modelo;
                newItem.Proceso = item.Proceso;
                newItem.COMMCELL = item.COMMCELL;
                newItem.EVENT_HOUR = item.EVENT_HOUR;
                newItem.ID_TYPE = item.ID_TYPE;
                newItem.result = item.result;
                newItem.ORDEN = item.ORDEN;


                // Convertir EVENT_DATE de string a DateTime
                if (DateTime.TryParseExact(item.EVENT_DATE, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime))
                {
                    // Asignar el valor convertido a la propiedad datetime de tipo DateTime
                    newItem.EVENT_DATE = dateTime;
                }
                else
                {
                    // Manejar el caso en el que la conversión no sea exitosa
                    // Aquí puedes asignar un valor por defecto, lanzar una excepción, etc.
                    throw new Exception($"No se pudo convertir EVENT_DATE: {item.EVENT_DATE}");
                }

                // Agregar newItem a ResultadoLastUbication
                historialReprocesoTemp.Add(newItem);
            }



            List<HistoryModelDSstring> historialReprocesoCasiFinal = historialReprocesoTemp
                .Where(w => w.EVENT_DATE >= FromDate && w.EVENT_DATE <= ToDate)
                .ToList();

            foreach (var item in historialReprocesoCasiFinal)
            {
                // Crear una nueva instancia de HistoryModelDS
                var newItem = new HistoryModel();

                newItem.Serial_Number = item.Serial_Number;
                newItem.Modelo = item.Modelo;
                newItem.Proceso = item.Proceso;
                newItem.COMMCELL = item.COMMCELL;
                newItem.EVENT_HOUR = item.EVENT_HOUR;
                newItem.ID_TYPE = item.ID_TYPE;
                newItem.result = item.result;
                newItem.ORDEN = item.ORDEN;


                // Agregar newItem a ResultadoLastUbication
                HistorialReprocesosEnTurno.Add(newItem);
            }
            #endregion



            ResponseDiagStockFinal ObjetoResultadoFinalFinal = new ResponseDiagStockFinal()
            {
                Producto = Producto,
                DateToDate = FromDate + " to " + ToDate,
                Turno = workShift,
                NameFirstProcessLastProcess = FirstProcessName + " to " + LastProcessName,
                DifFirstProcessLastProcess = FirstProcessCount - LastProcessCount,
                DiagStockDataRaw = ResultadoFinal,
                HistorialReprocesosEnTurno = HistorialReprocesosEnTurno,
                HistorialPiezasPendientesEnTurno = HistorialPiezasPendientesEnTurno,
                HistorialPiezasPendientesPorAnalizarEnTurno = HistorialPiezasPendientesPorAnalizarEnTurno,
                HistorialPiezasPendientesPorIngresarALineaEnTurno = HistorialPiezasPendientesPorIngresarALineaEnTurno,
                PiezasIngresadasEnFirstProcess = PiezasIngresadasEnFirstProcess,
                PiezasIngresadasEnLastProcess = PiezasIngresadasEnLastProcess,
                DiferenciaPiezasIngresadasEnFirstProcess = (PiezasIngresadasEnFirstProcess - PiezasIngresadasEnLastProcess).ToString(),
            };
            ResultadoFinalFinal.Add(ObjetoResultadoFinalFinal);


            return ResultadoFinalFinal;
        }




        #region Filtros
        private (List<HistoryModel> historialFiltrado, List<HistoryModel> historialRegistrosGolden) FiltrarPorOrden(List<HistoryModel> historial, string Product, string IDTYPE)
        {
            List<HistoryModel> resultado = new List<HistoryModel>();

            if (Product.Contains("FGEN"))
            {
                List<HistoryModel> HistorialFiltradoPorModelo = FiltrarPorModelo(historial, Product);
                List<HistoryModel> HistorialFiltradoPorIDTYPE = FiltrarPorIDTYPE(HistorialFiltradoPorModelo, IDTYPE);
                resultado.AddRange(HistorialFiltradoPorIDTYPE);
            }
            else
            {
                resultado.AddRange(historial);
            }

            // Filtrar la lista por el valor "GOLDEN" en la columna ORDEN
            var historialFiltrado = resultado.Where(h => !h.ORDEN.ToUpper().Contains("GOLDEN")).ToList();
            var historialRegistrosGolden = resultado
                .Where(h => h.ORDEN.ToUpper().Contains("GOLDEN"))
                .GroupBy(h => h.Serial_Number)
                .Select(group => group.First())
                .ToList();

            return (historialFiltrado, historialRegistrosGolden);
        }
        public List<HistoryModel> FiltrarPorModelo(List<HistoryModel> historial, string Producto)
        {
            List<HistoryModel> historialSeriesOtros = new List<HistoryModel>();
            List<HistoryModel> historialFiltrado = new List<HistoryModel>();
            foreach (HistoryModel renglonSerie in historial)
            {
                string serieMayusculas = renglonSerie.Serial_Number.ToUpper();


                if (!serieMayusculas.Contains("K") && Producto.Contains("GEN3"))
                {
                    historialFiltrado.Add(renglonSerie);
                }
                else if (serieMayusculas.Contains("K") && Producto == "FGEN1M")
                {
                    historialFiltrado.Add(renglonSerie);

                }
                else if (Producto == "FGEN1MR" || Producto == "FGEN1MR_Line_BE_4" || Producto == "FGEN1MR_Line_BE_5" || Producto == "FGEN1MR_Line_PCBA_3" || Producto == "FGEN1MR_Line_PCBA_4")
                {
                    bool ContieneNum = ContieneSoloNumeros(serieMayusculas);
                    if (ContieneNum)
                    {
                        historialFiltrado.Add(renglonSerie);
                    }
                    else if (renglonSerie.Proceso == "RELAY TEST")
                    {
                        historialFiltrado.Add(renglonSerie);
                    }
                }
                else
                {
                    historialSeriesOtros.Add(renglonSerie);
                }
            }
            return historialFiltrado;
        }
        private List<HistoryModel> FiltrarPorIDTYPE(List<HistoryModel> historial, string IDTYPE)
        {
            var historialFiltradoSinIDTYPE = historial.Where(h => h.ID_TYPE != IDTYPE).ToList();
            var historialFiltrado = historial.Where(h => h.ID_TYPE == IDTYPE).ToList();

            return historialFiltrado;
        }
        static bool ContieneSoloNumeros(string input)
        {
            // Patrón de expresión regular que coincide con cualquier carácter que no sea un número
            string patron = @"[^0-9]";

            // Si no hay coincidencias con el patrón, el string solo contiene números
            return !Regex.IsMatch(input, patron);
        }
        private List<HistoryModel> OrdenarPorFechaYHoraAscendente(List<HistoryModel> historialFiltrado)
        {
            // Ordenar la lista por EVENT_DATE y EVENT_HOUR de forma ascendente
            var historialOrdenado = historialFiltrado
            .OrderByDescending(h => h.EVENT_DATE)
            .ThenByDescending(h => h.EVENT_HOUR)
            .ToList();
            ;

            return historialOrdenado;
        }
        private List<HistoryModel> FiltrarRegistrosGoldenConOrdenesProductivas(List<HistoryModel> resultadoSinDuplicados, List<HistoryModel> historialRegistrosGolden, string EstacionesProcesoC, List<HistoryModel> HistorialDeSeriesPCBA, List<HistoryModel> HistorialDeSeriesBE, bool SwitchBE)
        {
                

            // Obtener los valores únicos de la columna Serial_Number en historialRegistrosGolden
            HashSet<string> serialNumbersGolden = new HashSet<string>(historialRegistrosGolden.Select(h => h.Serial_Number));

            // Filtrar resultadoSinDuplicados excluyendo los registros con Serial_Number que ya existen en historialRegistrosGolden
            List<HistoryModel> resultadoFiltrado = resultadoSinDuplicados
                .Where(h => !serialNumbersGolden.Contains(h.Serial_Number))
                .ToList();

            if(EstacionesProcesoC != "")
            {
                List<HistoryModel> resultadoFueraDuendes = FueraDuendesQueCreanOrdenesGoldens(historialRegistrosGolden, EstacionesProcesoC, HistorialDeSeriesPCBA, HistorialDeSeriesBE, SwitchBE);
                resultadoFiltrado.AddRange(resultadoFueraDuendes);
            }

            return resultadoFiltrado;
        }
        private List<HistoryModel> FueraDuendesQueCreanOrdenesGoldens(List<HistoryModel> historialRegistrosGolden, string EstacionesProcesoC, List<HistoryModel> HistorialDeSeriesPCBA, List<HistoryModel> HistorialDeSeriesBE, bool SwitchBE)
        {
            string[] arregloComcells = SplitEstaciones(EstacionesProcesoC);
            List<HistoryModel> resultado = new List<HistoryModel>();
            foreach (var item in historialRegistrosGolden)
            {
                if(item.Serial_Number == "006D62426146352")
                {

                }
                List<HistoryModel> HistorialDePiezaQueNoEsGolden = new List<HistoryModel>();
                List<HistoryModel> HistorialDePiezasConOrdenGolden = new List<HistoryModel>();
                if (!SwitchBE)
                {
                    HistorialDePiezasConOrdenGolden = HistorialDeSeriesPCBA.Where(w => w.Serial_Number == item.Serial_Number).ToList();
                }else
                {
                    HistorialDePiezasConOrdenGolden = HistorialDeSeriesBE.Where(w => w.Serial_Number == item.Serial_Number).ToList();
                }
                
                foreach (string Estacion in arregloComcells)
                {
                    List<HistoryModel> HistorialDePiezaQueNoEsGoldenTemp = HistorialDePiezasConOrdenGolden.Where(w => w.COMMCELL == Estacion).ToList();
                    HistorialDePiezaQueNoEsGolden.AddRange(HistorialDePiezaQueNoEsGoldenTemp);
                }
                int cantidadRenglones = HistorialDePiezaQueNoEsGolden.Count;
                if (cantidadRenglones > 0)
                {
                    resultado.Add(item);
                }
            }
            
            

            return resultado;
        }
        private List<HistoryModel> EliminarDuplicadosPorSerial(List<HistoryModel> historialOrdenado)
        {
            // Crear una nueva lista para almacenar elementos únicos por Serial_Number
            List<HistoryModel> historialSinDuplicados = new List<HistoryModel>();
            List<HistoryModel> historialDeDuplicados = new List<HistoryModel>();

            try
            {
                // Iterar sobre la lista ordenada y agregar elementos únicos por Serial_Number
                foreach (var item in historialOrdenado)
                {
                    if (!historialSinDuplicados.Any(h => h.Serial_Number == item.Serial_Number))
                    {
                        historialSinDuplicados.Add(item);
                    }
                    else
                    {
                        historialDeDuplicados.Add(item);
                    }


                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return historialSinDuplicados;
        }
        public List<HistoryModel> FiltradoTotal(List<HistoryModel> ListaParaFiltrar, string Producto, string IdType, string EstacionesProcesoC, List<HistoryModel> HistorialDeSeriesPCBA, List<HistoryModel> HistorialDeSeriesBE, bool SwitchBE)
        {
            List<HistoryModel> resultadoSinFyP = new List<HistoryModel>();
            var (Resultadofiltrado, ResultadoGoldens) = FiltrarPorOrden(ListaParaFiltrar, Producto, IdType);
            List<HistoryModel> resultadoFiltradoGolden = Resultadofiltrado;
            List<HistoryModel> resultadoOrdenado = OrdenarPorFechaYHoraAscendente(resultadoFiltradoGolden);
            List<HistoryModel> resultadoSinDuplicados = EliminarDuplicadosPorSerial(resultadoOrdenado);
            List<HistoryModel> resultadoSinGoldensProductivas = FiltrarRegistrosGoldenConOrdenesProductivas(resultadoSinDuplicados, ResultadoGoldens, EstacionesProcesoC, HistorialDeSeriesPCBA, HistorialDeSeriesBE, SwitchBE);
            #region RemoverFallasYAgregarNecesarias
            List<HistoryModel> resultadoComparadoConFalla = resultadoSinGoldensProductivas
            .Where(w => w.result == "F")
            .ToList();


            



            List<string> resultadoComparadoConFallaSerialNumber = new List<string>(resultadoComparadoConFalla.Select(h => h.Serial_Number));

            
            resultadoSinFyP.AddRange(resultadoSinGoldensProductivas);
            resultadoSinFyP.RemoveAll(item => resultadoComparadoConFallaSerialNumber.Contains(item.Serial_Number));


            foreach (string item in resultadoComparadoConFallaSerialNumber)
            {
                if(item == "006DB2426234201")
                {

                }
                List<HistoryModel> HistorialProcesoA = resultadoSinGoldensProductivas.Where(w => w.Serial_Number == item).ToList();
                if (HistorialProcesoA.Count > 1)
                {
                    List<HistoryModelDSParametricos> HistorialDatetimeTemp = new List<HistoryModelDSParametricos>();
                    foreach (var renglon in HistorialProcesoA)
                    {
                        var newItemConvert = new HistoryModelDSParametricos();

                        newItemConvert.Serial_Number = renglon.Serial_Number;
                        newItemConvert.ID_TYPE = renglon.ID_TYPE;
                        newItemConvert.ORDEN = renglon.ORDEN;
                        newItemConvert.COMMCELL = renglon.COMMCELL;
                        newItemConvert.Modelo = renglon.Modelo;
                        newItemConvert.Proceso = renglon.Proceso;
                        newItemConvert.Result = renglon.result;

                        // Convertir EVENT_DATE de string a DateTime
                        if (DateTime.TryParseExact(renglon.EVENT_DATE, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime))
                        {
                            // Asignar el valor convertido a la propiedad datetime de tipo DateTime
                            newItemConvert.datetime = dateTime;
                        }
                        else
                        {
                            // Manejar el caso en el que la conversión no sea exitosa
                            // Aquí puedes asignar un valor por defecto, lanzar una excepción, etc.
                            throw new Exception($"No se pudo convertir EVENT_DATE: {renglon.EVENT_DATE}");
                        }

                        HistorialDatetimeTemp.Add(newItemConvert);
                    }

                    HistoryModelDSParametricos renglonMasReciente = HistorialDatetimeTemp.OrderByDescending(w => w.datetime).FirstOrDefault();

                    var newItem = new HistoryModel();

                    newItem.Serial_Number = renglonMasReciente.Serial_Number;
                    newItem.ID_TYPE = renglonMasReciente.ID_TYPE;
                    newItem.EVENT_DATE = renglonMasReciente.datetime.ToString("dd/MM/yyyy HH:mm:ss");
                    newItem.ORDEN = renglonMasReciente.ORDEN;
                    newItem.COMMCELL = renglonMasReciente.COMMCELL;
                    newItem.Modelo = renglonMasReciente.Modelo;
                    newItem.Proceso = renglonMasReciente.Proceso;
                    newItem.result = renglonMasReciente.Result;


                    resultadoSinFyP.Add(newItem);
                }else
                {
                    resultadoSinFyP.AddRange(HistorialProcesoA);
                }
            }
            #endregion



            return resultadoSinFyP;
        }
        #endregion

        #region FiltrosMenores
        public List<string> GenerateIntervals(DateTime FromDate, DateTime ToDate)
        {
            List<string> intervals = new List<string>();

            DateTime current = FromDate;

            while (current <= ToDate)
            {
                DateTime intervalStart = current;
                DateTime intervalEnd;

                if (current.Minute < 30)
                {
                    intervalEnd = current.Date.AddHours(current.Hour).AddMinutes(29).AddSeconds(59);
                }
                else
                {
                    intervalEnd = current.Date.AddHours(current.Hour + 1).AddMinutes(29).AddSeconds(59);
                }

                // Adjust end time if it exceeds ToDate
                if (intervalEnd > ToDate)
                {
                    intervalEnd = ToDate;
                }

                string intervalStartString = intervalStart.ToString("dd/MM/yyyy HH:mm:ss"); // Convierte FromDate a cadena

                string intervalEndString = intervalEnd.ToString("dd/MM/yyyy HH:mm:ss"); // Convierte FromDate a cadena

                intervals.Add($"{intervalStartString} to {intervalEndString}");

                current = intervalEnd.AddSeconds(1);
            }

            return intervals;
        }
        public string FormatInterval(string intervalString)
        {
            // Definir el formato esperado del string de intervalo
            string format = "dd/MM/yyyy HH:mm:ss";

            // Obtener las partes del intervalo
            string startString = intervalString.Substring(0, intervalString.IndexOf(" to ")).Trim();
            string endString = intervalString.Substring(intervalString.IndexOf(" to ") + 3).Trim();

            // Variables para almacenar las fechas convertidas
            DateTime start;
            DateTime end;

            // Parsear las fechas al formato DateTime
            bool startParsed = DateTime.TryParseExact(startString, format, null, System.Globalization.DateTimeStyles.None, out start);
            bool endParsed = DateTime.TryParseExact(endString, format, null, System.Globalization.DateTimeStyles.None, out end);

            // Verificar si ambas fechas fueron parseadas correctamente
            if (startParsed && endParsed)
            {
                // Formatear las fechas en el formato deseado
                string formattedInterval = $"{start.ToString("HH:mm").ToLower()} to {end.ToString("HH:mm").ToLower()}";
                return formattedInterval;
            }
            else
            {
                throw new ArgumentException("El formato del intervalo proporcionado no es válido.");
            }
        }
        public static (DateTime FromDate, DateTime ToDate) ParseInterval(string intervalString)
        {
            // Definir el formato esperado del string de intervalo
            string format = "dd/MM/yyyy HH:mm:ss 'to' dd/MM/yyyy HH:mm:ss";

            // Separar el formato en dos partes usando el string " 'to' " como separador
            string[] formats = format.Split(new string[] { " 'to' " }, StringSplitOptions.None);

            if (formats.Length != 2)
            {
                throw new ArgumentException("El formato del intervalo proporcionado no es válido.");
            }

            // Parsear el string intervalString a DateTime para cada parte del intervalo
            DateTime fromDateTime, toDateTime;

            if (DateTime.TryParseExact(intervalString.Split(new string[] { " to " }, StringSplitOptions.None)[0].Trim(), formats[0], null, System.Globalization.DateTimeStyles.None, out fromDateTime) &&
                DateTime.TryParseExact(intervalString.Split(new string[] { " to " }, StringSplitOptions.None)[1].Trim(), formats[1], null, System.Globalization.DateTimeStyles.None, out toDateTime))
            {
                return (fromDateTime, toDateTime);
            }
            else
            {
                throw new ArgumentException("El formato del intervalo proporcionado no es válido.");
            }
        }
        private DateTime EsFechaValida(string fecha)
        {
            DateTime toDate = new DateTime();
            try
            {
                toDate = DateTime.ParseExact(fecha, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);


            }
            catch (Exception Ex)
            {

            }
            return toDate;
        }
        static (string[][], double Goal, double GoalRolado, bool SerialNumberChange) ObtenerEstacionesFPY(string Product)
        {
            double Goal = 0;
            double GoalRolado = 0;
            bool SerialNumberChange = false;
            string[][] arregloDeArreglosDeCOMCCELLSoProcesos = new string[0][];

            if (Product == "FGEN1M")
            {
                Goal = 98.47;
                GoalRolado = 91.12;
                string[] ICTFGEN1M = { "ICT", "98", "CCN_SEMI", "CCN_ICT_PCB0103" };
                string[] FLASHFGEN1M = { "FLASH", "99.3", "CCN_SEMI", "CCN_FLASH_PCB0203" };
                string[] EOLFGEN1M = { "EOL", "97", "CCN_BE2_FIN", "CCN_PRU-FIN1_3140911,CCN_PRU-FIN2_3140911" };
                string[][] ArregloFORDGEN1 = { ICTFGEN1M, FLASHFGEN1M, EOLFGEN1M };
                arregloDeArreglosDeCOMCCELLSoProcesos = arregloDeArreglosDeCOMCCELLSoProcesos.Concat(ArregloFORDGEN1).ToArray();
            }
            else if (Product == "FGEN3")
            {
                Goal = 98.33;
                GoalRolado = 93.45;
                //Datos x arreglo: Proceso, goalProceso, IdType, Estaciones
                string[] ICTFGEN3 = { "ICT", "98", "CCN_SEMI", "CCN_ICT_L3_PCB0103,CCN_ICT_L2_PCB0103,CCN_ICT_PCB0103,AN_ICT_L4_PCB0103" };
                string[] FLASHFGEN3 = { "FLASH", "99.3", "SMD_MOPS", "CCN_FLASH_PCB0203,CCN_FLASH1_FH_L2_PCB0203,AN_FLASH1_FH_L3_PCB0203,AN_FLASH4_FH_L4_PCB0203" };
                string[] LAQUEAR1FGEN3 = { "LAQUEAR1", "99.3", "CCN_SEMI", "CCN_LAQUEAR1_PCB0303,CCN_LAQUEAR1_L2_PCB0303" };
                string[] AOITOPFGEN3 = { "AOITOP", "99.3", "CCN_SEMI", "CCN_AOITOP_PCB0503,CCN_AOITOP_L2_PCB0503,CCN_AOITOP_L2_3063411" };
                string[] LAQUEAR2FGEN3 = { "LAQUEAR2", "99.3", "CCN_SEMI", "CCN_LAQUEAR2_PCB0603,CCN_LAQUEAR2_L2_PCB0603" };
                string[] AOIBOTFGEN3 = { "AOIBOT", "99.3", "CCN_SEMI", "CCN_AOIBOT_PCB0803,CCN_AOIBOT_L2_PCB0803,CCN_AOIBOT_L2_3063421" };
                string[] PININS1FGEN3 = { "PININSER1", "99.3", "CCN_SEMI", "PININS1_PCB0104,PININS1_L3_G3_PCB0903,CCN_PININSERT1_PCB0903" };
                string[] PININS2FGEN3 = { "PININSER2", "99.3", "CCN_SEMI", "PININS2_PCB0204,PININS2_L3_G3_PCB1003,CCN_PININSERT2_PCB1003" };
                string[] DESPANEFGEN3 = { "DESPANE2", "99.3", "CCN_SEMI", "CCN_DESPANE1_G3_3060311,CCN_DESPANE2_G3_3060311" };
                string[] EOLFGEN3 = { "EOL", "97", "CCN_BE1_FIN", "CCN_PRU-FIN1_L2_3140911,CCN_PRU-FIN1_L3_3140911,CCN_PRU-FIN2_L2_3140911,CCN_PRU-FIN2_L3_3140911,CCN_PRU-FIN3_L2_3140911,CCN_PRU-FIN3_L3_3140911,CCN_PRU-FIN4_3140911,CCN_PRU-FIN4_L2_3140911,CCN_PRU-FIN4_L3_3140911,CCN_PRU-FIN5_3140911,CCN_PRU-FIN6_3140911,CCN_PRU-FIN3_3140911" };
                string[] PINCHKGEN3 = { "PINCHK", "99", "CCN_BE1_FIN", "CCN_PINCHK1_L2_3171311,CCN_PINCHK_3171311,CCN_PINCHK1_L3_3171311,AN_PINCHEK_L5_317311" };
                string[][] ArregloFGEN3 = { ICTFGEN3, FLASHFGEN3, LAQUEAR1FGEN3, AOITOPFGEN3, LAQUEAR2FGEN3, AOIBOTFGEN3, PININS1FGEN3, PININS2FGEN3, DESPANEFGEN3, EOLFGEN3, PINCHKGEN3 };
                arregloDeArreglosDeCOMCCELLSoProcesos = arregloDeArreglosDeCOMCCELLSoProcesos.Concat(ArregloFGEN3).ToArray();
            }
            else if (Product == "FGEN1MR")
            {
                Goal = 98.05;
                GoalRolado = 90.6;
                string[] ICTGEN1MR = { "ICT", "98", "CCN_SEMI", "AN_ICT3_PCB0103_MR,AN_ICT4_PCB0103_MR" };
                string[] FLASHGEN1MR = { "FLASH", "99.3", "CCN_SEMI", "AN_FLASH1_PCB0203_MR,AN_FLASH4_PCB0203_MR" };
                string[] LAQUEARGEN1MR = { "LAQUEAR", "99.3", "CCN_SEMI", "CCN_LAQUEAR1_PCB0303" };
                string[] AOITOPGEN1MR = { "AOITOP", "99.3", "CCN_SEMI", "CCN_AOITOP_PCB0503" };
                string[] LAQUEAR2GEN1MR = { "LAQUEAR2", "99.3", "CCN_SEMI", "CCN_LAQUEAR2_PCB0603" };
                string[] AOIBOTGEN1MR = { "AOIBOT", "99.3", "CCN_SEMI", "CCN_AOIBOT_PCB0803" };
                string[] PININSERT1GEN1MR = { "PININSERT", "99.3", "CCN_SEMI", "CCN_PININSERT1_PCB0903" };
                string[] PININSERT2GEN1MR = { "PININSERT", "99.3", "CCN_SEMI", "CCN_PININSERT2_PCB1003" };
                string[] MVTFGEN1M = { "MVT", "99", "CCN_BE2_FIN", "CCN_AUTINSP_L4_3143311,AN_AUTINSP_L5_3143311" };
                string[] EOLFGEN1M = { "EOL", "97", "CCN_BE2_FIN", "CCN_PRU-FIN1_L4_3140911,CCN_PRU-FIN2_L4_3140911,CCN_PRU-FIN3_L4_3140911,AN_PRU-FIN1_L5_3140911,AN_PRU-FIN2_L5_3140911,AN_PRU-FIN3_L5_3140911" };
                string[] PINCHECKFGEN1M = { "PINCHECK", "98", "CCN_BE3_FIN", "AN_PINCHECK_L5_3171311" };

                string[][] ArregloFORDGEN1 = { ICTGEN1MR, FLASHGEN1MR, LAQUEARGEN1MR, AOITOPGEN1MR, LAQUEAR2GEN1MR, AOIBOTGEN1MR, PININSERT1GEN1MR, PININSERT2GEN1MR, MVTFGEN1M, EOLFGEN1M, PINCHECKFGEN1M };
                arregloDeArreglosDeCOMCCELLSoProcesos = arregloDeArreglosDeCOMCCELLSoProcesos.Concat(ArregloFORDGEN1).ToArray();
            }
            else if (Product == "MSM")
            {
                SerialNumberChange = true;
                Goal = 98.45;
                GoalRolado = 93.92;
                string[] ICTMSM = { "ICT", "99", "CCN_SEMI", "CCN_ICT1_3180111,CCN_ICT2_3180111" };
                string[] FLASHMSM = { "FLASH", "99.3", "CCN_FLASH", "CCN_FLASH_3180211" };
                string[] DESPANEMSM = { "DESPANE", "99.3", "CCN_FLASH", "CCN_DESPANE_3180311" };
                string[] LABELMSM = { "LABEL", "99", "CCN_SEMI", "CCN_ENS-AUT1B_3181611" };
                string[] EOLMSM = { "EOL", "96.5", "CCN_SEMI", "CCN_PRU-FIN6_3180911,CCN_PRU-FIN2_3180911,CCN_PRU-FIN3_3180911,CCN_PRU-FIN5_3180911,CCN_PRU-FIN1_3180911,CCN_PRU-FIN4_3180911" };
                string[] PINCHEKMSM = { "PINCHECK", "99", "CCN_SEMI", "CCN_PINCHK_3181311" };
                string[][] ArregloMSM = { ICTMSM, FLASHMSM, DESPANEMSM, LABELMSM, EOLMSM, PINCHEKMSM };
                arregloDeArreglosDeCOMCCELLSoProcesos = arregloDeArreglosDeCOMCCELLSoProcesos.Concat(ArregloMSM).ToArray();
            }
            else if (Product == "HONDA")
            {
                Goal = 98;
                GoalRolado = 97;
                string[] FLASHHONDA = { "FLASH", "98.5", "CCN_FLASH", "CCN_TEST_PCB1002,CCN_TEST-UNIT_PCB1002" };
                string[] DESPANEHONDA = { "DESPANE", "98.5", "CCN_SEMI", "CCN_DESPANE_PCB2002" };
                string[] PRESSFITHONDA = { "PRESSFIT", "98.5", "CCN_BE1_FIN", "CCN_PRESSFIT_3131911" };
                string[] ENSAUTHONDA = { "ENS-AUT", "98.5", "CCN_BE1_FIN", "CCN_ENS-AUT_3131611" };
                string[] LASERHONDA = { "LABEL", "98.5", "CCN_BE1_FIN", "CCN_LASER_3130711" };            
                string[] AUTOINSPHONDA = { "ENS-AUT", "98.5", "CCN_BE1_FIN", "CCN_AUTINSP_3133411" };
                string[] PROBARHONDA = { "LASER", "98.5", "CCN_BE1_FIN", "CCN_PROBAR_3130411" };
                string[] ENSAUTHONDA2 = { "ENS-AUT2", "98.5", "CCN_BE1_FIN", "CCN_ENS-AUT_3131621" };
                string[] AUTINSPHONDA2 = { "ENS-AUT2", "98.5", "CCN_BE1_FIN", "CCN_AUTINSP_3133421" };
                string[] EOLHONDA = { "EOL", "98.5", "CCN_BE3_FIN", "CCN_PRU-FIN_3130911" };
                string[] PINCHEKHONDA = { "PINCHECK", "98.5", "CCN_BE3_FIN", "CCN_CHKPIN_3131311" };
                string[][] ArregloHONDA = { FLASHHONDA, DESPANEHONDA, PRESSFITHONDA, ENSAUTHONDA, LASERHONDA, AUTOINSPHONDA, PROBARHONDA, ENSAUTHONDA2, AUTINSPHONDA2, EOLHONDA, PINCHEKHONDA };
                arregloDeArreglosDeCOMCCELLSoProcesos = arregloDeArreglosDeCOMCCELLSoProcesos.Concat(ArregloHONDA).ToArray();
            }
            else if (Product == "SUBARU")
            {
                SerialNumberChange = true;
                Goal = 98.75;
                GoalRolado = 90.4;
                string[] ICTSUBARU = { "ICT", "98", "CCN_BE2_FIN", "CCN_ICT_3170111,CCN_ICT2_3170111" };
                string[] FLASHSUBARU = { "FLASH", "98", "CCN_BE2_FIN", "CCN_FLASH2_3170211,CCN_FLASH3_3170211" };
                string[] LABELSUBARU = { "LABEL", "98", "CCN_BE2_FIN", "CCN_ENS-MAN_3171111,3171111" };
                string[] SWLSUBARU = { "SWL", "98", "CCN_BE2_FIN", "CCN_SWL_3170811,CCN_SWL2_3170811" };
                string[] EOLSUBARU = { "EOL", "98", "CCN_BE2_FIN", "CCN_PRU-FIN1_3170911,CCN_PRU-FIN2_3170911,CCN_PRU-FIN3_3170911,CCN_PRU-FIN4_3170911,CCN_PRU-FIN5_3170911,CCN_PRU-FIN6_3170911,CCN_PRU-FIN7_3170911,CCN_PRU-FIN8_3170911" };
                string[] BATTESTSUBARU = { "BATTERY TEST", "98", "CCN_BE2_FIN", "CCN_BATTEST_3170411,CCN_BATTEST2_3170411" };
                string[] RFTSUBARU = { "RFT", "98", "CCN_BE2_FIN", "CCN_RFT1_3171011,CCN_RFT2_3171011,CCN_RFT3_3171011,CCN_RFT4_3171011,CCN_RFT5_3171011,CCN_RFT6_3171011,CCN_RFT7_3171011,CCN_RFT8_3171011,CCN_RFT9_3171011,CCN_RFT10_3171011" };
                string[] TLCSUBARU = { "TLC", "98", "CCN_BE2_FIN", "CCN_TLC1_3171211,CCN_TLC2_3171211,CCN_TLC3_3171211,CCN_TLC4_3171211,CCN_TLC5_3171211,CCN_TLC6_3171211,CCN_TLC7_3171211,CCN_TLC8_3171211" };
                string[] PINCHECKSUBARU = { "PINCHECK", "98", "CCN_BE2_FIN", "CCN_CHKPIN_3171301,CCN_CHKPIN2_3171301" };

                string[][] ArregloSUBARU = { ICTSUBARU, FLASHSUBARU, LABELSUBARU, SWLSUBARU, EOLSUBARU, BATTESTSUBARU, RFTSUBARU, TLCSUBARU, PINCHECKSUBARU };
                arregloDeArreglosDeCOMCCELLSoProcesos = arregloDeArreglosDeCOMCCELLSoProcesos.Concat(ArregloSUBARU).ToArray();
            }
            else if (Product == "ONSTAR")
            {
                SerialNumberChange = true;
                Goal = 98;
                GoalRolado = 86.81;
                string[] ICTONSTAR = { "ICT", "98", "CCN_SEMI", "CCN_ICT_3160111,CCN_ICT_3160112" };
                string[] BATTTESTONSTAR = { "BATTTEST", "98", "CCN_SEMI", "CCN_BATTTEST_3160412,CCN_BATTTEST_3160411" };
                string[] LABELONSTAR = { "LABEL", "98", "CCN_BE2_FIN", "CCN_LABEL_3160721,CCN_LABEL_3160722" };
                string[] SWLONSTAR = { "SWL", "98", "CCN_BE2_FIN", "CCN_SWL_3160811,CCN_SWL2_3160811,CCN_SWL_3160812,CCN_SWL2_3160812,CCN_SWL3_3160812" };
                string[] EOLONSTAR = { "EOL", "98", "CCN_BE2_FIN", "CCN_PRUFIN_3160911,CCN_PRUFIN2_3160911,CCN_PRUFIN3_3160911,CCN_PRUFIN_3160912,CCN_PRUFIN2_3160912,CCN_PRUFIN3_3160912" };
                string[] RFTONSTAR = { "RFT", "98", "CCN_BE2_FIN", "CCN_RFT_3161011,CCN_RFT_3161012" };
                string[] TLCONSTAR = { "TLC", "98", "CCN_BE2_FIN", "CCN_TLC_3161211,CCN_TLC2_3161211,CCN_TLC3_3161211,CCN_TLC4_3161211,CCN_TLC5_3161211,CCN_TLC_3161212,CCN_TLC2_3161212,CCN_TLC3_3161212,CCN_TLC4_3161212,CCN_TLC5_3161212,CCN_TLC6_3161212" };
                string[] PINCHECKONSTAR = { "PINCHECK", "98", "CCN_BE2_FIN", "CCN_CHKPIN_LA_3161321,CCN_CHKPIN_LB_3161321" };
                string[][] ArregloONSTAR = { ICTONSTAR, BATTTESTONSTAR, LABELONSTAR, SWLONSTAR, EOLONSTAR, RFTONSTAR, TLCONSTAR, PINCHECKONSTAR };
                arregloDeArreglosDeCOMCCELLSoProcesos = arregloDeArreglosDeCOMCCELLSoProcesos.Concat(ArregloONSTAR).ToArray();
            }
            else if (Product == "LTA")
            {
                Goal = 97.75; //Dato de Oscar // 97 por estacion
                GoalRolado = 91.3;
                string[] ICTLTA = { "ICT", "97", "CCN_SEMI", "AN_ICT_3020112" };
                string[] FLASHLTA = { "FLASH", "98", "SMD_MOPS", "AN_FLASH_3020212" };
                string[] EOLLTA = { "EOL", "98", "CCN_SEMI", "AN_EOL1_3010911,AN_EOL2_3010911" };
                string[] PINCKLTA = { "PINCK", "98", "CCN_SEMI", ",AN_PINCHK_3011311" };

                string[][] ArregloONSTAR = { ICTLTA, FLASHLTA, EOLLTA, PINCKLTA };
                arregloDeArreglosDeCOMCCELLSoProcesos = arregloDeArreglosDeCOMCCELLSoProcesos.Concat(ArregloONSTAR).ToArray();
            }
            else if (Product == "HYUNDAI_DCU")
            {
                Goal = 97;
                GoalRolado = 85;
                string[] ICTHYUNDAI_DCU = { "ICT", "97", "CCN_SEMI", "AN_ICT_3250111" };
                string[] FLASHHYUNDAI_DCU = { "FLASH", "97", "SMD_MOPS", "AN_FLASH_3250211" };
                string[] SWLHYUNDAI_DCU = { "SWL", "97", "CCN_BE1_FIN", "AN_SWL1_3250811,AN_SWL2_3250811" };
                string[] EOLHYUNDAI_DCU = { "EOL", "97", "CCN_BE1_FIN", "AN_RFT1_3250911,AN_RFT2_3250911" };
                string[] PINCKHYUNDAI_DCU = { "TLC", "97", "CCN_BE1_FIN", "AN_TLC1_3251211,AN_TLC2_3251211,AN_TLC3_3251211" };
                string[][] ArregloHYUNDAI_DCU = { ICTHYUNDAI_DCU, FLASHHYUNDAI_DCU, SWLHYUNDAI_DCU, EOLHYUNDAI_DCU, PINCKHYUNDAI_DCU };
                arregloDeArreglosDeCOMCCELLSoProcesos = arregloDeArreglosDeCOMCCELLSoProcesos.Concat(ArregloHYUNDAI_DCU).ToArray();
            }


            return (arregloDeArreglosDeCOMCCELLSoProcesos, Goal, GoalRolado, SerialNumberChange);
        }
        public (string ProcesoAnterior, string ProcesoNuevo, string ProcessAvsProcessB, bool Comparar) CambioDeProceso(string ProcessA, string ProcessB)
        {
            string ProcesoAnterior = "";
            string ProcesoNuevo = "";
            string ProcessAvsProcessB = "";
            bool Comparar = false;
            if (ProcessA == "" && ProcessB != "")
            {
                ProcesoNuevo = ProcessB;
            }
            else if (ProcessA != "" && ProcessB != "")
            {
                Comparar = true;
                ProcesoAnterior = ProcessA;
                ProcesoNuevo = ProcessB;
                ProcessAvsProcessB = ProcesoAnterior + "-" + ProcesoNuevo;
            }
            return (ProcesoAnterior, ProcesoNuevo, ProcessAvsProcessB, Comparar);
        }
        public string[] SplitEstaciones(string Estaciones)
        {
            string[] arregloComcells = new string[0];
            if (Estaciones.Contains(","))
            {

                arregloComcells = Estaciones.Split(",");
            }
            else
            {
                arregloComcells = new string[] { Estaciones };
            }

            return arregloComcells;
        }
        public (int Diferencia, List<string> SerialNumbersDif, List<HistoryModel> SNwithDataDif, List<HistoryModel> resultadoComparadoConFallaFinal, List<string> resultadoComparadoConFallaSerialNumber, List<HistoryModel> ResultadoBDepurado) ComparacionDatos(List<HistoryModel> ResultadoA, List<HistoryModel> ResultadoB, bool TerminoLabel, bool ProcesoEsLabel)
        {
            int Diferencia = 0;
            List<string> SerialNumbersDif = new List<string>();
            List<HistoryModel> SNwithDataDif = new List<HistoryModel>();
            List<HistoryModel> resultadoComparado = new List<HistoryModel>();
            List<HistoryModel> resultadoComparadoCoincidente = new List<HistoryModel>();
            List<HistoryModel> ResultadoBDepurado = new List<HistoryModel>();
            List<HistoryModel> resultadoComparadoConFallaFinal = new List<HistoryModel>();

            List<string> serialNumbersListaPequeña = new List<string>();

            
            if(TerminoLabel)
            {
                serialNumbersListaPequeña = new List<string>(ResultadoB.Select(h => h.Serial_Number));
                resultadoComparado = ResultadoA
                    .Where(h => !serialNumbersListaPequeña.Contains(h.result))
                    .ToList();
                resultadoComparadoCoincidente = ResultadoA
                    .Where(h => serialNumbersListaPequeña.Contains(h.result))
                    .ToList();
            }
            else
            {
                serialNumbersListaPequeña = new List<string>(ResultadoB.Select(h => h.Serial_Number));
                resultadoComparado = ResultadoA
                    .Where(h => !serialNumbersListaPequeña.Contains(h.Serial_Number))
                    .ToList();
                resultadoComparadoCoincidente = ResultadoA
                    .Where(h => serialNumbersListaPequeña.Contains(h.Serial_Number))
                    .ToList();
            }



            Diferencia = resultadoComparado.Count();

            if (!TerminoLabel)
            {
                List<string> SerialNumbersDifString = new List<string>(resultadoComparado.Select(h => h.Serial_Number));
                SerialNumbersDif.AddRange(SerialNumbersDifString);
            }
            else
            {
                List<string> SerialNumbersDifString = new List<string>(resultadoComparado.Select(h => h.result));
                SerialNumbersDif.AddRange(SerialNumbersDifString);
            }

            List<string> resultadoComparadoSerialNumbers = new List<string>();
            

            if (!TerminoLabel)
            {
                resultadoComparadoSerialNumbers = new List<string>(resultadoComparadoCoincidente.Select(h => h.Serial_Number));
                ResultadoBDepurado = ResultadoB.Where(h => resultadoComparadoSerialNumbers.Contains(h.Serial_Number))
                    .ToList();
            }
            else
            {
                resultadoComparadoSerialNumbers = new List<string>(resultadoComparadoCoincidente.Select(h => h.result));
                ResultadoBDepurado = ResultadoB.Where(h => resultadoComparadoSerialNumbers.Contains(h.Serial_Number))
                    .ToList();
            }

            List<HistoryModel> resultadoComparadoConFalla = resultadoComparado
            .Where(w => w.result == "F")
            .ToList();
            List <string> resultadoComparadoConFallaSerialNumber = new List<string>(resultadoComparadoConFalla.Select(h => h.Serial_Number));

            
            return (Diferencia, SerialNumbersDif, resultadoComparado, resultadoComparadoConFalla, resultadoComparadoConFallaSerialNumber, ResultadoBDepurado);
        }
        #endregion
    }
}