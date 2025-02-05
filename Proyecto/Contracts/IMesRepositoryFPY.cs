using proyecto.Models.ExampleModels;
using proyecto.Models.FPY;
using proyecto.Models.FPY.TopOffender;
using proyecto.Models.FPY.Historial;
using proyecto.Models.Top10;
using System.Collections.Generic;
namespace proyecto.Contracts
{
    public interface IMesRepositoryFPY
    {
        Task<(Response, bool)> GetFPYData(string Product, DateTime FromDate, DateTime ToDate, string Week, int TypeSearch);

        Task<(Response, bool)> GetFPYDatabyDay(string Product, DateTime FromDate, DateTime ToDate, int TypeSearch);
        
        Task<(Response, bool)> GetDataByStation(string Product, string Process, string Station, string IdType, DateTime FromDate, DateTime ToDate);

        Top10Final GetDataSpecifict(string Product, DateTime FromDate, DateTime ToDate);

        TopOffenderFinal GetDataTopOffenders(string Product, string Fecha, string Process, string Estacion, string Day, int TypeSearch, string fromDateStr, string toDateStr);
    }
}
