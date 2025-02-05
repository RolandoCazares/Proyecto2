using System.Data;

namespace proyecto.Contracts
{
    public interface IDbContext
    {

        DataTable RunQuery(string Query);

        //DataTable RunQueryWip(string Query);

        (DataTable, int) RunQueryFPY(string Query, string info);

        
    }
}
