namespace proyecto.Models.FPY
{
    public class ReportFPYByProcessAndStationAndDay
    {
        public string Process { get; set; }

        public string COMMCELL { get; set; }

        public string Day { get; set; }

        public int TotalProduced { get; set; }

        public int TotalFailures { get; set; }

        public int Total { get; set; }

        public double FPY { get; set; }

    }
}
