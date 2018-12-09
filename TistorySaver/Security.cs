using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace TistorySaver
{
    public static class Security
    {
        public static byte[] EncryptString(string str, out byte[] key)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            key = new byte[20];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(key);
            }

            byte[] encryptedBytes = ProtectedData.Protect(bytes, key,
                DataProtectionScope.CurrentUser);

            return encryptedBytes;
        }

        public static string DecryptString(byte[] encryptedStrBytes, byte[] keyBytes)
        {
            byte[] bytes = ProtectedData.Unprotect(encryptedStrBytes, keyBytes,
                        DataProtectionScope.CurrentUser);

            return Encoding.UTF8.GetString(bytes);
        }
    }
}
