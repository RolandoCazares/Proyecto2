using proyecto.Models;

namespace proyecto.Contracts
{
    public interface IMesRepository
    {        
        List<TestDiag> GetHistory(string SerialNumber);
    }
}
