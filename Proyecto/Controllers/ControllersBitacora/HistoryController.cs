using proyecto.Contracts;
using proyecto.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace proyecto.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistoryController : ControllerBase
    {
        private readonly IMesRepositoryAguascalientes _mesRepositoryAgs;
        public HistoryController(IMesRepositoryAguascalientes mesRepository)
        {
            _mesRepositoryAgs = mesRepository;
        }
        [HttpGet("{SerialNumber}")]
        public ActionResult GetProduct(string SerialNumber)
        {
            List<Test> history = _mesRepositoryAgs.GetHistory(SerialNumber);
            return Ok(JsonSerializer.Serialize(history));
        }
    }
}
