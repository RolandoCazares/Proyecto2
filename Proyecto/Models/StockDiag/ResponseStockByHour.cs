using proyecto.Models.FPY.Historial;

namespace proyecto.Models.StockDiag
{
    public class ResponseStockByHour
    {
        public string RangoHoras { get; set; }

        public List<HistoryModelDSParametricosString> HistorialDeEsaHora { get; set; }
    }
}
