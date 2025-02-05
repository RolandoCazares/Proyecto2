
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;
using NuGet.Packaging;
using System.Linq;
using System.Collections.Generic;
using proyecto.Contracts;
using proyecto.Models.FPY;
using proyecto.Models.FPY.Db;
using proyecto.Models.FPY.Historial;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using proyecto.Data;
using proyecto.Models;

namespace proyecto.Helpers
{
    public class FiltersFPY : IFilterFPY 
    {

        private readonly IMesRepositoryFPY _mesRepositoryFPY;
        private readonly AnalysisDbContext _context;

        public FiltersFPY(IMesRepositoryFPY mesRepositoryFPY, AnalysisDbContext context)
        {
            _mesRepositoryFPY = mesRepositoryFPY;
            _context = context;
        }

        static (DateTime FromDate, DateTime ToDate) GetWeekDates(string weekString)
        {
            // Parse la cadena de la semana en un objeto CultureInfo
            CultureInfo culture = CultureInfo.InvariantCulture;
            Calendar calendar = culture.Calendar;

            // Obtenga el año y la semana del formato "yyyy-Www"
            int year = int.Parse(weekString.Substring(0, 4));
            int week = int.Parse(weekString.Substring(6));

            // Obtenga la fecha del primer día de la semana
            DateTime jan1 = new DateTime(year, 1, 1);
            DateTime firstDayOfWeek = jan1.AddDays((week - 1) * 7 - (int)jan1.DayOfWeek + (int)DayOfWeek.Monday);

            // Calcule la fecha del último día de la semana (domingo)
            DateTime lastDayOfWeek = firstDayOfWeek.AddDays(6);

            // Ajuste las horas, minutos y segundos
            DateTime fromDateTime = new DateTime(firstDayOfWeek.Year, firstDayOfWeek.Month, firstDayOfWeek.Day, 0, 0, 0);
            DateTime toDateTime = new DateTime(lastDayOfWeek.Year, lastDayOfWeek.Month, lastDayOfWeek.Day, 23, 59, 59);

            return (fromDateTime, toDateTime);
        }

        public async Task<List<Response>> FilterFPYProducto(string Product, int TypeSearch, string Week, string startDate, string endDate)
        {
            List<Response> results = new List<Response>();
            Response result = new Response();
            bool operationSuccessful;
            if (TypeSearch == 1)
            {
                var Fechas = GetWeekDates(Week.ToUpper());
                DateTime FromDate = Fechas.FromDate;
                DateTime ToDate = Fechas.ToDate;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Product + "- byWeek - " + Week + " Inicio: " + DateTime.Now);
                (result, operationSuccessful) = await _mesRepositoryFPY.GetFPYData(Product, FromDate, ToDate, Week, TypeSearch);
                if (operationSuccessful)
                {

                    results.Add(result);
                    Console.WriteLine("");
                    Console.WriteLine(Product + "- byWeek - " + Week + " Termino Exitosamente: " + DateTime.Now);
                    Console.ResetColor();

                }
                else
                {
                    Console.WriteLine("");
                    Console.WriteLine(Product + "- byWeek - " + Week + " Termino con un error: " + DateTime.Now);
                    Console.ResetColor();
                }
            }
            else if (TypeSearch == 2)
            {
                DateTime fromDate = DateTime.ParseExact(startDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                DateTime toDate = DateTime.ParseExact(endDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);


                Console.ForegroundColor = ConsoleColor.Yellow;

                Console.WriteLine(Product + "- byDay - FromDate:" + startDate + "-ToDate: " + endDate + " Inicio: " + DateTime.Now);
                (result, operationSuccessful) = await _mesRepositoryFPY.GetFPYDatabyDay(Product, fromDate, toDate, TypeSearch);
                if (operationSuccessful)
                {
                    Console.WriteLine("");
                    Console.WriteLine(Product + "- byDay - FromDate:" + startDate + "-ToDate: " + endDate + " Termino Exitosamente: " + DateTime.Now);
                    Console.ResetColor();
                    results.Add(result);
                }
                else
                {
                    Console.WriteLine("");
                    Console.WriteLine(Product + "- byDay - FromDate:" + startDate + "-ToDate: " + endDate + " Termino con un error: " + DateTime.Now);
                    Console.ResetColor();
                }
            } 
            else if (TypeSearch == 3)
            {
                DateTime fromDate = DateTime.ParseExact(startDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                DateTime toDate = DateTime.ParseExact(endDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                string[] StringArregloProductos = { "FGEN1M", "FGEN3", "FGEN1MR", "MSM", "HONDA", "SUBARU", "DCM_LTA", "HYUNDAI_DCU", "ONSTAR" };
                string[] StringArregloLineas = { "FGEN3_PCBA", "FGEN3_BE", "FGEN3_PINCHK", "FGEN3_L1", "FGEN3_L2", "FGEN3_L3", "FGEN3_L4", "FGEN1MR_Line_PCBA_3", "FGEN1MR_Line_PCBA_4", "FGEN1MR_Line_BE_4", "FGEN1MR_Line_BE_5", "SUBARU_L1", "SUBARU_L2", "ONSTAR_L1", "ONSTAR_L2" };

                string FirstRenglonDBFPYlast24hours = _context.MinuteroFPYs
                .OrderBy(r => r.ID).Select(r => r.Datetime) // Cambia 'Fecha' por la propiedad que determines como criterio de orden
                .LastOrDefault();

                if(FirstRenglonDBFPYlast24hours != endDate)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("//-------------------FPY espectacular------------------------//");
                    List<ReportFPYDB> ListaObjetosFPYproducto = new List<ReportFPYDB>();
                    List<ReportFPYDBbyProcess> ListaObjetosFPYproductobyProcess = new List<ReportFPYDBbyProcess>();
                    foreach (string productoDeArreglo in StringArregloProductos)
                    {
                        Console.WriteLine(productoDeArreglo + "- byDay - FromDate:" + startDate + "-ToDate: " + endDate + " Inicio: " + DateTime.Now);
                        (result, operationSuccessful) = await _mesRepositoryFPY.GetFPYDatabyDay(productoDeArreglo, fromDate, toDate, TypeSearch);
                        if (operationSuccessful)
                        {
                            Console.WriteLine("");
                            Console.WriteLine(productoDeArreglo + "- byDay - FromDate:" + startDate + "-ToDate: " + endDate + " Termino Exitosamente: " + DateTime.Now);
                            ReportFPYDB ObjetoReporteFPY = new ReportFPYDB
                            {
                                Product = result.Product,
                                FromDate = startDate,
                                ToDate = endDate,
                                FPY = result.ReportFPY.FPY,
                                FPYRolado = result.ReportFPY.FPYRolado,
                                Total = result.ReportFPY.Total,
                                TotalProduced = result.ReportFPY.TotalProduced,
                                TotalFailures = result.ReportFPY.TotalFailures,
                                Actualizado = DateTime.Now,
                                Goal = result.Goal,
                                GoalRolado = result.GoalRolado,
                            };
                            ListaObjetosFPYproducto.Add(ObjetoReporteFPY);
                            
                            foreach(var renglonProceso in result.ReportFPYByProcess)
                            {
                                string goalString = "";
                                foreach (string Renglon in result.ArregloDeGoalsPorProceso)
                                {
                                    if (Renglon.Contains(renglonProceso.Process))
                                    {
                                        goalString = Renglon;
                                    }
                                }

                                double goalValue = 0;

                                if (!string.IsNullOrEmpty(goalString))
                                {
                                    string[] parts = goalString.Split(':');
                                    if (parts.Length > 1)
                                    {
                                        string valuePart = parts[1].Trim().Replace("%", "");
                                        if (double.TryParse(valuePart, out goalValue))
                                        {
                                            // goalValue ahora contiene el valor numérico
                                        }
                                        else
                                        {
                                            // Manejo del error si la conversión falla
                                            Console.WriteLine("Error al convertir el valor a double.");
                                        }
                                    }
                                }


                                ReportFPYDBbyProcess ObjetoReporteFPYbyProcess = new ReportFPYDBbyProcess
                                {
                                    Product = result.Product,
                                    Process = renglonProceso.Process,
                                    FromDate = startDate,
                                    ToDate = endDate,
                                    FPY = renglonProceso.FPY,
                                    FPYRolado = renglonProceso.FPYRoladoProceso,
                                    Total = renglonProceso.Total,
                                    TotalProduced = renglonProceso.TotalProduced,
                                    TotalFailures = renglonProceso.TotalFailures,
                                    Actualizado = DateTime.Now,
                                    Goal = goalValue,
                                    Status = renglonProceso.FPY >= goalValue ? "OK" : "NOK" ,
                                    Diferencia = renglonProceso.FPY - goalValue,
                                };
                                ListaObjetosFPYproductobyProcess.Add(ObjetoReporteFPYbyProcess);

                            }
                        }
                        else
                        {
                            Console.WriteLine("");
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(productoDeArreglo + "- byDay - FromDate:" + startDate + "-ToDate: " + endDate + " Termino con un error: " + DateTime.Now);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                        }
                    }

                    _context.ReporteFPYbyProcess.RemoveRange(_context.ReporteFPYbyProcess);
                    await _context.SaveChangesAsync();
                    _context.ReporteFPY.RemoveRange(_context.ReporteFPY);
                    await _context.SaveChangesAsync();

                    _context.ReporteFPY.AddRange(ListaObjetosFPYproducto);
                    await _context.SaveChangesAsync();
                    var listaFiltrada = ListaObjetosFPYproductobyProcess
                    .Where(w => w.Total != 0)
                    .ToList();

                    // Ordenar la lista filtrada por Status
                    List<ReportFPYDBbyProcess> ListaFPYordenada = listaFiltrada.OrderBy(w => w.Diferencia).ToList();
                    _context.ReporteFPYbyProcess.AddRange(ListaFPYordenada);
                    await _context.SaveChangesAsync();


                    Console.WriteLine("//-------------------FPY espectacular------------------------//");
                    Console.ResetColor();
                    MinuteroFPY objetoConfirmacion = new MinuteroFPY
                    {
                        Datetime = endDate,
                        Actualizado = DateTime.Now,
                    };
                    _context.MinuteroFPYs.Add(objetoConfirmacion);
                    await _context.SaveChangesAsync();
                }


            }

            

            return results;
        }

        public async Task<List<Response>> FilterFPYProductoByStation(string Familia, string Proceso, string Estacion, string IdType, DateTime FromDate, DateTime ToDate )
        {
            List<Response> results = new List<Response>();
            Response result = new Response();

            string fromDateAsString = FromDate.ToString("dd-MM-yyyy"); // Convierte FromDate a cadena
            string toDateAsString = ToDate.ToString("dd-MM-yyyy"); // Convierte FromDate a cadena

            

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("FPYbyStation" + " - " + Familia + " - " + fromDateAsString + " - " + toDateAsString + " - " + " Inicio: " + DateTime.Now);
            (Response response, bool operationSuccessful) = await _mesRepositoryFPY.GetDataByStation(Familia, Proceso, Estacion, IdType, FromDate, ToDate);

            if (operationSuccessful)
            {
                results.Add(response);
                Console.WriteLine("");
                Console.WriteLine("FPYbyStation" + " - " + Familia + " - " + fromDateAsString + " - " + toDateAsString + " - " + " Termino Exitosamente: "+ DateTime.Now);
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("");
                Console.WriteLine("FPYbyStation" + " - " + Familia + " - " + fromDateAsString + " - " + toDateAsString + " - " + " Termino con un error: " + DateTime.Now);
                Console.ResetColor();
            }

            return results;
        }

    }
}
