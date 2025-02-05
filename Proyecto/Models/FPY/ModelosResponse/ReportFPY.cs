namespace proyecto.Models.FPY
{
    public class ReportFPY
    {
        public int TotalProduced { get; set; }
        public int TotalProducedRunAndRate { get; set; }

        public int TotalFailures { get; set; }

        public int TotalFailuresRunAndRate { get; set; }

        public int Total { get; set; }

        public int TotalRunAndRate { get; set; }

        public double FPY { get; set; }

        public double FPYRunAndRate { get; set; }

        public double FPYRolado { get; set; }


        public double FPYRolandoRunAndRate { get; set; }
    }
}
