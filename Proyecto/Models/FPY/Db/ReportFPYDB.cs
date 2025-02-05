namespace proyecto.Models.FPY.Db
{
    public class ReportFPYDB
    {
        public int ID { get; set; }

        public string FromDate { get; set; }

        public string ToDate { get; set; }

        public string Product { get; set; }

        public int TotalProduced { get; set; }

        public int TotalFailures { get; set; }


        public int Total { get; set; }


        public double FPY { get; set; }


        public double FPYRolado { get; set; }

        public DateTime Actualizado { get; set; }

        public double Goal { get; set; }

        public double GoalRolado { get; set; }

    }
}
