using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyecto.Data;
using proyecto.Models;
using proyecto.Models.FPYtest;

namespace proyecto.Controllers.ControllersFPYtest
{
    [Route("api/[controller]")]
    [ApiController]
    public class StationsFPYtestsController : ControllerBase
    {
        private readonly AnalysisDbContext _context;

        public StationsFPYtestsController(AnalysisDbContext context)
        {
            _context = context;
        }

        // GET: api/StationsFPYtests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StationsFPYtest>>> GetStationsFPYtest()
        {
            return await _context.StationsFPYtest.ToListAsync();
        }

        // GET: api/StationsFPYtests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<StationsFPYtest>> GetStationsFPYtest(string id)
        {
            var stationsFPYtest = await _context.StationsFPYtest.FindAsync(id);

            if (stationsFPYtest == null)
            {
                return NotFound();
            }

            return stationsFPYtest;
        }

        // GET: api/StationsFPYtestsByProduct/5
        [HttpGet("{Producto}/{Proceso}")]
        public async Task<ActionResult<List<StationsFPYtest>>> GetStationsFPYtestPorProducto(string Producto, string Proceso)
        {
            var stationsFPYtest = await _context.StationsFPYtest.Where(w => w.Product == Producto && w.Process == Proceso).ToListAsync();

            if (stationsFPYtest == null)
            {
                return NotFound();
            }

            return stationsFPYtest;
        }

        // PUT: api/StationsFPYtests/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStationsFPYtest(string id, StationsFPYtest stationsFPYtest)
        {
            if (id != stationsFPYtest.ID)
            {
                return BadRequest();
            }

            _context.Entry(stationsFPYtest).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StationsFPYtestExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/StationsFPYtests
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<StationsFPYtest>> PostStationsFPYtest(StationsFPYtest stationsFPYtest)
        {
            _context.StationsFPYtest.Add(stationsFPYtest);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (StationsFPYtestExists(stationsFPYtest.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetStationsFPYtest", new { id = stationsFPYtest.ID }, stationsFPYtest);
        }

        // DELETE: api/StationsFPYtests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStationsFPYtest(string id)
        {
            var stationsFPYtest = await _context.StationsFPYtest.FindAsync(id);
            if (stationsFPYtest == null)
            {
                return NotFound();
            }

            _context.StationsFPYtest.Remove(stationsFPYtest);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StationsFPYtestExists(string id)
        {
            return _context.StationsFPYtest.Any(e => e.ID == id);
        }
    }
}
