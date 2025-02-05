namespace proyecto.Models.FPY
{
    public class ReportFPYByProcessAndStationDayHour
    {
        public string Process { get; set; }

        public string COMMCELL { get; set; }

        public string Day { get; set; }

        public string Hour { get; set; }

        public int TotalProduced { get; set; }

        public int TotalFailures { get; set; }

        public int Total { get; set; }

        public double FPY { get; set; }

    }
}
