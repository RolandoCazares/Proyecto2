using System.Data;

namespace proyecto.Contracts
{
    public interface IDbContextWip
    {
        Task<DataTable> RunQueryWip(string Process, string Station, DateTime FromDate, DateTime ToDate);
    }

}
