namespace proyecto.Models.FPY
{
    public class ReportFPYByProcess
    {
        public string Process { get; set; }

        public int TotalProduced { get; set; }

        public int TotalFailures { get; set; }
        
        public int Total { get; set; }

        public double FPY { get; set; }
    }
}
