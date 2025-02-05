using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyecto.Data;
using proyecto.Models;

namespace proyecto.Controllers.ControllersBitacora
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProcessMsController : ControllerBase
    {
        private readonly AnalysisDbContext _context;

        public ProcessMsController(AnalysisDbContext context)
        {
            _context = context;
        }

        // GET: api/ProcessMs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProcessM>>> GetProcess()
        {
            return await _context.Process.ToListAsync();
        }

        // GET: api/ProcessMs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProcessM>> GetProcessM(string id)
        {
            var processM = await _context.Process.FindAsync(id);

            if (processM == null)
            {
                return NotFound();
            }

            return processM;
        }

        [HttpGet("GetProcessByProduct/{Product}")]
        public async Task<ActionResult<List<ProcessM>>> GetProcessByProduct(string Product)
        {
            var processM = await _context.Process.Where(w => w.Product == Product).ToListAsync();

            if (processM == null)
            {
                return NotFound();
            }

            return processM;
        }

        // PUT: api/ProcessMs/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProcessM(string id, ProcessM processM)
        {
            if (id != processM.ID)
            {
                return BadRequest();
            }

            _context.Entry(processM).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProcessMExists(id))
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

        // POST: api/ProcessMs
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ProcessM>> PostProcessM(ProcessM processM)
        {
            _context.Process.Add(processM);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ProcessMExists(processM.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetProcessM", new { id = processM.ID }, processM);
        }

        // DELETE: api/ProcessMs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProcessM(string id)
        {
            var processM = await _context.Process.FindAsync(id);
            if (processM == null)
            {
                return NotFound();
            }

            _context.Process.Remove(processM);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProcessMExists(string id)
        {
            return _context.Process.Any(e => e.ID == id);
        }
    }
}
