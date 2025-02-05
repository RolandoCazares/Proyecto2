using proyecto.Models.FPY.Historial;

namespace proyecto.Models.StockDiag
{
    public class ResponseDiagStock
    {
        public string ProcessAvsProcessB { get; set; }

        public int Diferencia { get; set; }
            
        public List<string> SerialNumbersDif { get; set; } 
        
        public List<HistoryModelDSParametricosString> SNwithDataDifFail { get; set; }
        public List<HistoryModelDSParametricosString> ResultadoSCRAPUbication { get; set; }

        public List<HistoryModel> SNwithDataDif { get; set; }

        public List<HistoryModel> ResultadoProcesoA { get; set; }

        public List<HistoryModel> ResultadoProcesoB { get; set; }

        public List<ResponseStockByHour> ResponseStockByHour { get; set; }

        public List<TopOffenderStock> ResultadoTopOffenderStock { get; set; }


        public int PorAnalizar { get; set; }

        public int PorProbarrEnLinea { get; set; }
    }
}
