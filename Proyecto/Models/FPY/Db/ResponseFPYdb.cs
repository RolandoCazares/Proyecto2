namespace proyecto.Models.FPY.Db
{
    public class ResponseFPYdb
    {
        public string PeriodoConsultado { get; set; }

        public List<ReportFPYDB> ListaFPYbyProduct { get; set; }

        public List<ReportFPYDBbyProcess> ListaFPYbyProductandProcess { get; set; }
    }
}
