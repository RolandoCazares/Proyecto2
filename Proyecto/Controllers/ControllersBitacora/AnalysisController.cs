#nullable disable
using proyecto.Contracts;
using proyecto.Data;
using proyecto.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyecto.Models.Bitacora;

namespace proyecto.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalysisController : ControllerBase
    {
        private readonly AnalysisDbContext _context;

        private readonly IMesRepositoryAguascalientes _mesRepositoryAgs;

        public AnalysisController(AnalysisDbContext context, IMesRepositoryAguascalientes mesRepository)
        {
            _context = context;
            _mesRepositoryAgs = mesRepository;
        }

        // GET: api/Analysis/Family/TestNumber
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("{Family}/{TestNumber}")]
        public async Task<ActionResult<List<Analysis>>> GetAnalysisByTestDescription(string Family, string TestNumber)
        {
            TestNumber = TestNumber.ToUpper();
            Family = Family.ToUpper();  

            var analysis = await _context.Analysis.Where(analysis => 
                analysis.TestID.ToUpper().Contains(TestNumber) 
                && analysis.Family.Equals(Family)).ToListAsync();

            if (analysis == null)
            {
                return NotFound();
            }

            return analysis;
        }


        [HttpGet("Ags/{TestDescription}")]
        public async Task<ActionResult<List<AnalysisAgs>>> GetAnalysisByTestNumberAgs(string TestDescription)
        {
            TestDescription = TestDescription.ToUpper();            

            var analysis = await _context.AnalysisAgs.Where(analysis => analysis.TestDescription.ToUpper().Contains(TestDescription)).ToListAsync();

            if (analysis == null)
            {
                return NotFound();
            }

            return analysis;
        }

        [HttpGet("Nog/{TestDescription}")]
        public async Task<ActionResult<List<Analysis>>> GetAnalysisByTestNumberNog(string TestDescription)
        {
            TestDescription = TestDescription.ToUpper();

            var analysis = await _context.Analysis.Where(analysis => analysis.TestDescription.ToUpper().Contains(TestDescription)).ToListAsync();

            if (analysis == null)
            {
                return NotFound();
            }

            return analysis;
        }

        // POST: api/Analysis        
        [HttpPost]
        public async Task<ActionResult<PostAnalysis>> PostAnalysis(PostAnalysis historiAndAnalisis)
        {
            List<Test> history = historiAndAnalisis.history;
            CreateAnalysisDTO analysis = historiAndAnalisis.analysis;
            try 
            {
                foreach (Test test in history)
                {
                    AnalysisAgs newAnalysis = new AnalysisAgs()
                    {
                        Family = analysis.Family,
                        Model = test.Model,
                        SerialNumber = test.SerialNumber,
                        Process = analysis.Process,
                        StationID = test.StationId,
                        TestID = test.TestNumber,
                        TestDescription = test.Description,
                        LSL = test.LSL,
                        USL = test.USL,
                        Value = test.Value,
                        Component = analysis.Component,
                        Comment = analysis.Comment
                    };
                    _context.AnalysisAgs.Add(newAnalysis);
                }
                await _context.SaveChangesAsync();

            }catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }
            return Ok(analysis);
        }

        // POST: api/Analysis        
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("List")]
        public async Task<ActionResult<Analysis>> PostManyAnalysis(List<CreateAnalysisDTO> analysisList)
        {
            foreach (var analysis in analysisList)
            {
                List<Test> history = _mesRepositoryAgs.GetHistory(analysis.SerialNumber);
                foreach (Test test in history)
                {
                    Analysis newAnalysis = new Analysis()
                    {
                        Family = analysis.Family,
                        Model = test.Model,
                        SerialNumber = test.SerialNumber,
                        Process = analysis.Process,
                        StationID = test.StationId,
                        TestID = test.TestNumber,
                        TestDescription = test.Description,
                        LSL = test.LSL,
                        USL = test.USL,
                        Value = test.Value,
                        Component = analysis.Component,
                        Comment = analysis.Comment
                    };
                    _context.Analysis.Add(newAnalysis);
                }
                await _context.SaveChangesAsync();
            }            

            return CreatedAtAction("GetAnalysis", analysisList);
        }

        // DELETE: api/Analysis/5
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnalysis(int id)
        {
            var analysis = await _context.Analysis.FindAsync(id);
            if (analysis == null)
            {
                return NotFound();
            }

            _context.Analysis.Remove(analysis);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
