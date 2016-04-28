using System.IO;
using System.Security.Cryptography;

namespace ShipsClient.Common
{
    public static class Cryptography
    {
        public static byte[] Encrypt(byte[] input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                PasswordDeriveBytes pdb = new PasswordDeriveBytes(Constants.CRYPTOGRAPHY_PASSWORD,
                    Constants.CRYPTOGRAPHY_BYTES);
                Aes aes = new AesManaged();
                aes.Key = pdb.GetBytes(aes.KeySize / 8);
                aes.IV = pdb.GetBytes(aes.BlockSize / 8);
                CryptoStream cs = new CryptoStream(ms,
                    aes.CreateEncryptor(), CryptoStreamMode.Write);
                cs.Write(input, 0, input.Length);
                try
                {
                    cs.Close();
                }
                catch (CryptographicException) { }
                return ms.ToArray();
            }
        }

        public static byte[] Decrypt(byte[] input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                PasswordDeriveBytes pdb = new PasswordDeriveBytes(Constants.CRYPTOGRAPHY_PASSWORD,
                    Constants.CRYPTOGRAPHY_BYTES);
                Aes aes = new AesManaged();
                aes.Key = pdb.GetBytes(aes.KeySize / 8);
                aes.IV = pdb.GetBytes(aes.BlockSize / 8);
                CryptoStream cs = new CryptoStream(ms,
                    aes.CreateDecryptor(), CryptoStreamMode.Write);
                cs.Write(input, 0, input.Length);
                try
                {
                    cs.Close();
                }
                catch (CryptographicException) { }
                return ms.ToArray();
            }
        }
    }
}
