using System.Security.Cryptography;
using System.Text;

namespace proyecto.Services
{
    public class EncryptionService
    {
        public static string GetSHA256(string str)
        {
            SHA256 sha256 = SHA256.Create();
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] stream = null;
            StringBuilder stringBuilder = new StringBuilder();
            stream = sha256.ComputeHash(encoding.GetBytes(str));
            for (int i = 0; i < stream.Length; i++) stringBuilder.AppendFormat("{0:x2}", stream[i]);
            return stringBuilder.ToString();
        }
    }
}
