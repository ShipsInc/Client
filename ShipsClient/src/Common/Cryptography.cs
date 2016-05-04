using System.IO;
using System.Security.Cryptography;

namespace ShipsClient.Common
{
    public class Cryptography
    {
        public static byte[] Encrypt(byte[] data)
        {
            byte[] encryptBytes;
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(Constants.CRYPTOGRAPHY_PASSWORD, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.Close();
                    }
                    encryptBytes = ms.ToArray();
                }
            }
            return encryptBytes;
        }
        public static byte[] Decrypt(byte[] data)
        {
            byte[] decryptBytes;
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(Constants.CRYPTOGRAPHY_PASSWORD, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.Close();
                    }
                    decryptBytes = ms.ToArray();
                }
            }
            return decryptBytes;
        }
    }
}
