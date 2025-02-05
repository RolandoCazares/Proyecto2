using proyecto.Models.FPY.Historial;

namespace proyecto.Models.StockDiag
{
    public class ResponseDiagStockFinal
    {
        public string Producto { get; set; }

        public string Turno { get; set; }

        public string DateToDate { get; set; }

        public string NameFirstProcessLastProcess { get; set; }

        public int DifFirstProcessLastProcess { get; set; }

        public List<ResponseDiagStock> DiagStockDataRaw { get; set; }

        public List<HistoryModel> HistorialReprocesosEnTurno { get; set; }

        public List <HistoryModelDSParametricosString> HistorialPiezasPendientesEnTurno { get; set; }

        public List<HistoryModelDSParametricosString> HistorialPiezasPendientesPorAnalizarEnTurno { get; set; }

        public List<HistoryModelDSParametricosString> HistorialPiezasPendientesPorIngresarALineaEnTurno { get; set; }

        public int PiezasIngresadasEnLastProcess { get; set; }

        public int PiezasIngresadasEnFirstProcess { get; set; }

        public string DiferenciaPiezasIngresadasEnFirstProcess { get; set; }
    }
}
