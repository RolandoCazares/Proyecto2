namespace proyecto.Models.FPY
{
    public class ReportFPYByProcessAndModel
    {
        public string Process { get; set; }

        public string Model { get; set; }

        public int TotalProduced { get; set; }

        public int TotalFailures { get; set; }

        public int Total { get; set; }

        public double FPY { get; set; }
    }
}
