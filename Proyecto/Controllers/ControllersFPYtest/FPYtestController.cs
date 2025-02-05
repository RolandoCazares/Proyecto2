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
using proyecto.Models.FPY.TopOffender;
using System.Threading;
using proyecto.Data;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using proyecto.Models.FPY.Db;

namespace proyecto.api.Controllers
{
    [Route("api")]
    [ApiController]
    public class FPYtestController : ControllerBase
    {
        private readonly IMesRepositoryFPY _mesRepository;
        private readonly IFilterFPY _filterFPY;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1); // Permite 1 solicitud a la vez
        private readonly AnalysisDbContext _context;

        public FPYtestController(IMesRepositoryFPY mesRepository, IFilterFPY filterFPY, AnalysisDbContext context)
        {
            _mesRepository = mesRepository;
            _filterFPY = filterFPY;
            _context = context;
        }

        [HttpGet("MES/FPY/{Product}/{TypeSearch}/{Week}/{startDate}/{endDate}")]
        public async Task<ActionResult<Response>> GetFPY(string Product, int TypeSearch, string Week, string startDate, string endDate)
        {
            await _semaphore.WaitAsync();
            try
            {
                List<Response> ProducedAndFilter = await _filterFPY.FilterFPYProducto(Product, TypeSearch, Week, startDate, endDate);
                return Ok(ProducedAndFilter);
            }
            finally
            {
                _semaphore.Release();
            }
        }



        [HttpGet("FPYtestController/{Product}/{Process}/{Estacion}/{IdType}/{fromDateStr}/{toDateStr}")]
        public async Task<ActionResult<Response>> GetFPYstationDataFromMES(string Product, string Process, string Estacion, string IdType, string fromDateStr, string toDateStr)
        {
            await _semaphore.WaitAsync();
            try
            {
                DateTime fromDate = DateTime.ParseExact(fromDateStr, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                DateTime toDate = DateTime.ParseExact(toDateStr, "dd-MM-yyyy", CultureInfo.InvariantCulture);

                List<Response> ProducedAndFilter = await _filterFPY.FilterFPYProductoByStation(Product, Process, Estacion, IdType, fromDate, toDate);
                return Ok(ProducedAndFilter);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        [HttpGet("MES/TopOffenders/{Product}/{Fecha}/{Process}/{Estacion}/{Day}/{TypeSearch}/{fromDateStr}/{toDateStr}")]
        public async Task<ActionResult> GetDataFromMESTopOffenders(string Product, string Fecha, string Process, string Estacion, string Day, int TypeSearch, string fromDateStr, string toDateStr)
        {
            await _semaphore.WaitAsync();
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Top" + " - " + Product + " - " + " Inicio: " + DateTime.Now);
                List<TopOffenderFinal> ListaTop = new List<TopOffenderFinal>();
                TopOffenderFinal failures = _mesRepository.GetDataTopOffenders(Product, Fecha, Process, Estacion, Day, TypeSearch, fromDateStr, toDateStr);
                ListaTop.Add(failures);
                Console.WriteLine("");
                Console.WriteLine("Top" + " - " + Product + " Termino: " + DateTime.Now);
                Console.ResetColor();

                return Ok(ListaTop);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        [HttpGet("FPYespectacularController")]
        public async Task<ActionResult<Response>> GetFPYespectacularDataFromMES()
        {
            await _semaphore.WaitAsync();
            try
            {
                DateTime now = DateTime.Now;
                DateTime ahora = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0); // Hora actual, minutos y segundos en 0
                string ahoraString = ahora.ToString("yyyy-MM-dd HH:mm:ss"); // Formato deseado
                string FirstRenglonDBFPYlast24hours = _context.MinuteroFPYs
                .OrderBy(r => r.ID).Select(r => r.Datetime) // Cambia 'Fecha' por la propiedad que determines como criterio de orden
                .LastOrDefault();
                DateTime Last24hours = ahora.AddDays(-1);
                if (FirstRenglonDBFPYlast24hours != ahoraString)
                {
                    string Product = "Todos"; // Ajusta según sea necesario
                    int TypeSearch = 3; // Ajusta según sea necesario
                    string Week = "null"; // Ajusta según sea necesario

                    

                    string Last24hoursString = Last24hours.ToString("yyyy-MM-dd HH:mm:ss"); // Formato deseado
                    List<Response> ProducedAndFilter = await _filterFPY.FilterFPYProducto(Product, TypeSearch, Week, Last24hoursString, ahoraString);

                }
                List< ReportFPYDB> FPYresultProducts = _context.ReporteFPY.Where(w => w.ToDate == ahoraString).ToList();
                List<ReportFPYDBbyProcess> FPYresultProductsbyProcess = _context.ReporteFPYbyProcess.Where(w => w.ToDate == ahoraString).ToList();
                ResponseFPYdb objetoRespuesta = new ResponseFPYdb
                {
                    PeriodoConsultado = $"{Last24hours.ToString("dd/MM/yyyy hh:mm:ss")} al {ahora.ToString("dd/MM/yyyy hh:mm:ss")}",
                    ListaFPYbyProduct = FPYresultProducts,
                    ListaFPYbyProductandProcess = FPYresultProductsbyProcess,
                };
                List<ResponseFPYdb> listaRespuesta = new List<ResponseFPYdb>();
                listaRespuesta.Add(objetoRespuesta);
                return Ok(listaRespuesta);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}


