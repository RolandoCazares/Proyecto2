namespace proyecto.Models.FPY.Db
{
    public class ReportFPYDBbyProcess
    {
        public int ID { get; set; }

        public string FromDate { get; set; }

        public string ToDate { get; set; }

        public string Product { get; set; }

        public string Process { get; set; }

        public int TotalProduced { get; set; }

        public int TotalFailures { get; set; }

        public int Total { get; set; }

        public double FPY { get; set; }

        public double FPYRolado { get; set; }

        public DateTime Actualizado { get; set; }

        public double Goal { get; set; }

        public string Status { get; set; }

        public double Diferencia { get; set; }
    }
}
