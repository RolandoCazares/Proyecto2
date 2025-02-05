

using proyecto.Models.FPY.Historial;

namespace proyecto.Models.FPY
{
    public class Response
    {
        public ReportFPY ReportFPY { get; set; }

        public List<ReportFPYByProcessFinal> ReportFPYByProcess { get; set; }

        public List<ReportFPYByProcessAndStationFinal> ReportFPYByProcessAndStation { get; set; }

        public List<ReportFPYByProcessAndModelFinal> ReportFPYByProcessAndModel { get; set; }

        public List<HistoryModel> Data {  get; set; }

        public List<HistoryModel> DataRunAndRate { get; set; }

        public double Goal {  get; set; }

        public double GoalRolado { get; set; }

        public List<string> ArregloDeGoalsPorProceso { get; set; }

        public string timeElapsed { get; set; }

        public string PeriodoBuscado { get; set; }

        public string Product { get; set; }

        public string Week { get; set; }

        public List<ReportFPYByProcessAndDayFinal> ReportFPYBYProcessAndDay { get; set; }

        public List<ReportFPYByProcessAndStationAndDayFinal> ReportFPYBYProcessAndStationsAndDay { get; set; }

        public List<ReportFPYByProcessAndStationDayHourFinal> ReportFPYBYProcessAndStationsAndDayHour { get; set; }

        public List<ReportFPYByProcessAndDayHourFinal> ReportFPYBYProcessAndDayAndHour { get; set; }

        public int TypeSearch { get; set; }

        public string FromDate { get; set; }

        public string ToDate { get; set; }

    }
}
