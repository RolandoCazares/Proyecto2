using System.Data;

namespace proyecto.Contracts
{
    public interface IDbContextNogales
    {
        DataTable RunQuery(string Query);
    }
}