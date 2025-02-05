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
using proyecto.Models.StockDiag;
using static System.Collections.Specialized.BitVector32;
using System.Diagnostics;
using proyecto.Models.FPY.TopOffender;

namespace proyecto.Controllers.ControllersDiagStock
{
    [Route("api")]
    [ApiController]
    public class DiagStockController : ControllerBase
    {
        private readonly IFilterDiagStock _filterDiagStock;

        public DiagStockController(IFilterDiagStock filterDiagStock)
        {
            _filterDiagStock = filterDiagStock;
        }

        [HttpGet("MES/DiagStock/{Product}/{startDate}/{endDate}/{workShift}")]
        public async Task<ActionResult<ResponseDiagStock>> GetDiagStock(string Product, string startDate, string endDate, string workShift)
        {
            DateTime fromDate = DateTime.ParseExact(startDate, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime toDate = DateTime.ParseExact(endDate, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);

            List<ResponseDiagStockFinal> ResponseFilter = await _filterDiagStock.FilterByProductAndWorkShift(Product, fromDate, toDate, workShift);

            return Ok(ResponseFilter);
        }

    }
}
