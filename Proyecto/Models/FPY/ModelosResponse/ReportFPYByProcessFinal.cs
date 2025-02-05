namespace proyecto.Models.FPY
{
    public class ReportFPYByProcessFinal
    {
        public string Process { get; set; }

        public int TotalProduced { get; set; }

        public int TotalProducedRunAndRate { get; set; }

        public int TotalFailures { get; set; }

        public int TotalFailuresRunAndRate { get; set; }

        public int Total { get; set; }

        public int TotalRunAndRate { get; set; }

        public double FPY { get; set; }

        public double FPYRoladoProceso { get; set; }

        public double FPYRunAndRate { get; set; }
    }
}
