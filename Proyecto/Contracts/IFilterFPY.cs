
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using proyecto.Models.FPY;

namespace proyecto.Contracts
{
    public interface IFilterFPY
    {
        Task<List<Response>> FilterFPYProducto(string Product, int TypeSearch, string Week, string startDate, string endDate);
        Task<List<Response>> FilterFPYProductoByStation(string Familia, string Proceso, string Estacion, string IdType, DateTime FromDate, DateTime ToDate);
    }
}
