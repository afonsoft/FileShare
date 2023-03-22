using System.Security.Cryptography;
using System.Text;

namespace FileShare.Utilities
{
    public static class EncryptorHelpers
    {
        public static string MD5Hash(string text)
        {
            using (var md5 = MD5.Create())
            {
                var result = md5.ComputeHash(Encoding.ASCII.GetBytes(text));
                return Encoding.ASCII.GetString(result).Trim().ToUpper();
            }
        }
    }
}