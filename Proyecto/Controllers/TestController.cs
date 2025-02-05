using proyecto.Contracts;
using proyecto.Helpers;
using proyecto.Models.ExampleModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Globalization;
using proyecto.Models.Top10;
using proyecto.Models.FPY;
using static System.Collections.Specialized.BitVector32;
using System.Diagnostics;
using proyecto.Models;

using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using proyecto.Data;
using System.Reflection;
using proyecto.Models.Diagnostico;
using NuGet.Packaging;
using proyecto.Models.FPY.TopOffender;

namespace analysistools.api.Controllers
{
    [Route("api")]
    [ApiController]
    public class FPYtestController : ControllerBase
    {
        private readonly IMesRepositoryFPY _mesRepository;
        private readonly IFilterFPY _filterFPY;

        private readonly AnalysisDbContext _context;

        private static IDbContext dbContext = OracleDbContext.Instance;

        public FPYtestController(IMesRepositoryFPY mesRepository, AnalysisDbContext context, IFilterFPY filterFPY)
        {
            _context = context;
            _mesRepository = mesRepository;
            _filterFPY = filterFPY;
        }

        

        
        [HttpGet("MES/SpecifictTest/{fromDateStr}/{toDateStr}")]
        public ActionResult GetDataFromMESSpecificTest(string fromDateStr, string toDateStr)
        {
            DateTime fromDate = DateTime.ParseExact(fromDateStr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime toDate = DateTime.ParseExact(toDateStr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            Top10Final failures = _mesRepository.GetDataSpecifict("GEN3", fromDate, toDate);


            return Ok(failures);
        }

        [HttpGet("MES/SpecifictTestBySerial/{SerialStrings}")]
        public ActionResult GetDataFromMESSpecificTestBySerials(string SerialStrings)
        {
            
            Top10Final failures = _mesRepository.GetDataSpecifict(SerialStrings, DateTime.Now, DateTime.Now);

            return Ok(failures);
        }


        //[HttpGet("getDataAndSave")]
        //public async Task<ActionResult<List<TodoDiag>>> GetDataAndSave()
        //{
        //    try
        //    {
        //        string filePath = @"C:\Users\uif91762.CW01\source\repos\Book1.csv";

        //        // Verificar si el archivo existe
        //        if (!System.IO.File.Exists(filePath))
        //        {
        //            return NotFound("El archivo no existe.");
        //        }

        //        var todoDiagList = new List<TodoDiag>();

        //        // Leer todas las líneas del archivo CSV
        //        var lines = await System.IO.File.ReadAllLinesAsync(filePath);

        //        // Iterar sobre cada línea (excluyendo la primera si contiene encabezados)
        //        for (int i = 1; i < lines.Length; i++)
        //        {
        //            var columns = lines[i].Split(','); // Separar los valores de la línea por comas

        //            // Verificar si la línea tiene suficientes columnas
        //            if (columns.Length >= 6)
        //            {
        //                // Crear un objeto TodoDiag y agregarlo a la lista


        //                todoDiagList.Add(new TodoDiag
        //                {
        //                    Analista = columns[0],
        //                    SerialNumber = columns[1],
        //                    Product = columns[2].ToUpper(),
        //                    Process = columns[3],
        //                    Componente = columns[4],
        //                    Comentario = columns[5]
        //                });
        //            }
        //            else
        //            {
        //                // La línea no tiene suficientes columnas, puedes manejar esto de acuerdo a tus necesidades
        //                // Por ejemplo, puedes ignorar esta línea o registrar un error
        //                Console.WriteLine($"La línea {i} no tiene suficientes columnas.");
        //            }

        //        }


        //        int Contador = todoDiagList.Count;
        //        foreach (var renglonAnalisis in todoDiagList)
        //        {
        //            Contador--;

        //            if(Contador == 46080)
        //            {
        //                Console.WriteLine(renglonAnalisis.SerialNumber.ToString());
        //            }
        //            Console.WriteLine(Contador);
        //            List<Test> testList = new List<Test>();
        //            List<Test> testListFiltrado = new List<Test>();
        //            string NumeroDeSerie = renglonAnalisis.SerialNumber.ToString();
        //            string Producto = renglonAnalisis.Product.ToString();
        //            string serialNumberCorto = null;
        //            if (NumeroDeSerie.Contains("´=:") )
        //            {
        //                if(NumeroDeSerie.Contains("G"))
        //                {
        //                    string recorte = NumeroDeSerie.Remove(0, 59);
        //                    string recorteFinal = recorte.Remove(13);

        //                    if (recorteFinal != null)
        //                    {
        //                        serialNumberCorto = recorteFinal;
        //                    }
        //                }

        //            }
        //            else
        //            {
        //                serialNumberCorto = NumeroDeSerie;
        //            }
        //            if (serialNumberCorto != null)
        //            {
        //                string query = MesQueryFabric.QueryForFailsHistoryBitacora(NumeroDeSerie);
        //                DataTable queryResult = dbContext.RunQuery(query);
        //                testList = DataTableHelper.DataTableToFailBitdata(queryResult);
        //                testListFiltrado = testList.Where(w => !w.StationId.Contains("SMT") && !w.StationId.Contains("AOI") && !w.StationId.Contains("REP") && !w.StationId.Contains("ANALYSIS") && !w.Model.ToUpper().Contains("GOLDEN")).ToList();
        //                foreach (var renglonHistorial in testListFiltrado)
        //                {
        //                    AnalysisAgs newAnalysis = new AnalysisAgs()
        //                    {
        //                        Family = renglonAnalisis.Product,
        //                        Model = renglonHistorial.Model,
        //                        SerialNumber = renglonHistorial.SerialNumber,
        //                        Process = renglonAnalisis.Process,
        //                        StationID = renglonHistorial.StationId,
        //                        TestID = renglonHistorial.TestNumber,
        //                        TestDescription = renglonHistorial.Description,
        //                        LSL = renglonHistorial.LSL,
        //                        USL = renglonHistorial.USL,
        //                        Value = renglonHistorial.Value,
        //                        Component = renglonAnalisis.Componente,
        //                        Comment = renglonAnalisis.Comentario,
        //                        Personel = renglonAnalisis.Analista
        //                    };

        //                    _context.AnalysisAgs.Add(newAnalysis);
        //                }
        //                await _context.SaveChangesAsync();
        //            }
        //            else
        //            {
        //                Console.WriteLine(renglonAnalisis.SerialNumber);
        //            }

        //        }

        //        return Ok(todoDiagList);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error: {ex.Message}");  
        //        return StatusCode(500, $"Error al leer el archivo: {ex.Message}");
        //    }
        //}


        [HttpGet("getDataAndSave")]
        public async Task<ActionResult<List<TodoDiag>>> GetDataAndSave()
        {

            List<AnalysisDiag> resultado = new List<AnalysisDiag>();
            try
            {
                string filePath = @"C:\Users\uif91762.CW01\source\repos\Book12.csv";

                // Verificar si el archivo existe
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("El archivo no existe.");
                }

                var todoDiagList = new List<SerialProduct>();

                // Leer todas las líneas del archivo CSV
                var lines = await System.IO.File.ReadAllLinesAsync(filePath);

                // Iterar sobre cada línea (excluyendo la primera si contiene encabezados)
                for (int i = 1; i < lines.Length; i++)
                {
                    var columns = lines[i].Split(','); // Separar los valores de la línea por comas

                    // Verificar si la línea tiene suficientes columnas
                    if (columns.Length >= 2)
                    {
                        // Crear un objeto TodoDiag y agregarlo a la lista


                        todoDiagList.Add(new SerialProduct
                        {
                            SerialNumber = columns[0],
                            Product = columns[1].ToUpper(),
                        });
                    }
                    else
                    {
                        // La línea no tiene suficientes columnas, puedes manejar esto de acuerdo a tus necesidades
                        // Por ejemplo, puedes ignorar esta línea o registrar un error
                        Console.WriteLine($"La línea {i} no tiene suficientes columnas.");
                    }

                }


                int Contador = todoDiagList.Count;
                foreach (var renglonAnalisis in todoDiagList)
                {
                    Contador--;

                    Console.WriteLine(Contador);
                    List<TestDiag> testList = new List<TestDiag>();
                    List<TestDiag> testListFiltrado = new List<TestDiag>();
                    string NumeroDeSerie = renglonAnalisis.SerialNumber.ToString();
                    string Producto = renglonAnalisis.Product.ToString();
                    string serialNumberCorto = null;
                    if (NumeroDeSerie.Contains("´=:"))
                    {
                        if (NumeroDeSerie.Contains("G"))
                        {
                            string recorte = NumeroDeSerie.Remove(0, 59);
                            string recorteFinal = recorte.Remove(13);

                            if (recorteFinal != null)
                            {
                                serialNumberCorto = recorteFinal;
                            }
                        }

                    }
                    else
                    {
                        serialNumberCorto = NumeroDeSerie;
                    }
                    if (serialNumberCorto != null)
                    {
                        string query = MesQueryFabric.QueryForLastFailHistory(NumeroDeSerie);
                        DataTable queryResult = dbContext.RunQuery(query);
                        testList = DataTableHelper.DataTableToFailDiagStock(queryResult);
                        testListFiltrado = testList.Where(w => !w.StationId.Contains("SMT") && !w.StationId.Contains("AOI") && !w.StationId.Contains("REP") && !w.StationId.Contains("ANALYSIS")).ToList();
                        foreach (var renglonHistorial in testListFiltrado)
                        {
                            AnalysisDiag newAnalysis = new AnalysisDiag()
                            {
                                Family = renglonAnalisis.Product,
                                Model = renglonHistorial.Model,
                                Process = renglonHistorial.Process,
                                SerialNumber = renglonHistorial.SerialNumber,
                                StationID = renglonHistorial.StationId,
                                TestID = renglonHistorial.TestNumber,
                                TestDescription = renglonHistorial.Description,
                                LSL = renglonHistorial.LSL,
                                USL = renglonHistorial.USL,
                                Value = renglonHistorial.Value,
                            };
                            resultado.Add(newAnalysis);

                        }
                    }
                    else
                    {
                        Console.WriteLine(renglonAnalisis.SerialNumber);
                    }
                }

                var analysisDiagCounts = resultado
               .GroupBy(ad => new { ad.Family, ad.TestDescription })
               .Select(g => new AnalysisDiagCount
               {
                   Family = g.Key.Family,
                   TestDescription = g.Key.TestDescription,
                   Count = g.Count()
               })
                .OrderBy(ad => ad.Family)
                .ThenBy(ad => ad.Count)
               .ToList();

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, $"Error al leer el archivo: {ex.Message}");
            }
        }
    }
}
