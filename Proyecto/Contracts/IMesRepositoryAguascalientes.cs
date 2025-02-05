using proyecto.Models;
using proyecto.Models.FPY.Historial;
using proyecto.Models.StockDiag;

namespace proyecto.Contracts
{
    public interface IMesRepositoryAguascalientes
    {        
        List<Test> GetHistory(string SerialNumber);

        Task<List<HistoryModel>> GetHistoryDiagStock(string Proceso, string Estacion, DateTime fromDate, DateTime toDate);

        Task<List<HistoryModel>> GetLastUbicationBySerialID(string stringSerialNumbers);

        Task<List<HistoryModel>> GetUnitChange(List<string> stringSerialNumbers);

        Task<List<HistoryModelDSParametricosString>> GetLastFailBySerialID(List <string> stringSerialNumbers);


        Task<List<HistoryModelDSParametricosString>> GetSCRAPFailBySerialID(string stringSerialNumbers);

        Task<List<HistoryModel>> GetHistoryBySerialID(List<string> stringSerialNumbers);

    }
}
