using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using proyecto.Models.StockDiag;

namespace proyecto.Contracts
{
    public interface IFilterDiagStock
    {

        Task<List<ResponseDiagStockFinal>> FilterByProductAndWorkShift(string Product, DateTime fromDate, DateTime toDate, string workShift);
    }
}
