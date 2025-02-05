using System.ComponentModel.DataAnnotations.Schema;

namespace proyecto.Models
{
    public class AnalysisDiag
    {
        public string Family { get; set; }
        public string Model { get; set; }
        public string SerialNumber { get; set; }        
        public string Process { get; set; }
        public string StationID { get; set; }
        public string TestID { get; set; }
        public string TestDescription { get; set; }        
        public string Unit { get; set; }
        public string LSL { get; set; }
        public string USL { get; set; }
        public string Value { get; set; }
    }
}
