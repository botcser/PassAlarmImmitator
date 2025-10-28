using System.Security.Cryptography;
using System.Text;

namespace Casualbunker.Server.Common
{
    /// <summary>
    /// Md5 helper.
    /// </summary>
    public static class Md5
    {
        /// <summary>
        /// Compute Md5-hash from string.
        /// </summary>
        public static string ComputeHash(string input)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);

            return ComputeHash(inputBytes);
        }

        public static string ComputeHash(byte[] inputBytes)
        {
            var hash = MD5.Create().ComputeHash(inputBytes);
            var stringBuilder = new StringBuilder();

            for (var i = 0; i < hash.Length; i++)
            {
                stringBuilder.Append(hash[i].ToString("X2"));
            }

            return stringBuilder.ToString().ToLower();
        }
    }
}