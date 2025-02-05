using System.Data;

namespace proyecto.Contracts
{
    public interface IDbContextAguascalientes
    {
        DataTable RunQuery(string Query);

        DataTable RunQueryWip(string Query);
    }
}