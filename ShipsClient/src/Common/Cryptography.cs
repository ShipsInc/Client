using System;
using System.IO;
using System.Security.Cryptography;

namespace ShipsClient.Common
{
    public class Cryptography
    {
        public static byte[] Encrypt(byte[] input)
        {
            var ms = new MemoryStream();
            var pdb = new PasswordDeriveBytes(Constants.CRYPTOGRAPHY_PASSWORD, Constants.CRYPTOGRAPHY_BYTES);
            var aes = new AesManaged();
            aes.Key = pdb.GetBytes(aes.KeySize / 8);
            aes.IV = pdb.GetBytes(aes.BlockSize / 8);
            var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(input, 0, input.Length);
            try
            {
                cs.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(e.Message);
            }
            return ms.ToArray();
        }

        public static byte[] Decrypt(byte[] input)
        {
            var ms = new MemoryStream();
            var pdb = new PasswordDeriveBytes(Constants.CRYPTOGRAPHY_PASSWORD, Constants.CRYPTOGRAPHY_BYTES);
            Aes aes = new AesManaged();
            aes.Key = pdb.GetBytes(aes.KeySize / 8);
            aes.IV = pdb.GetBytes(aes.BlockSize / 8);
            var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(input, 0, input.Length);
            try
            {
                cs.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(e.Message);
            }
            return ms.ToArray();
        }
    }
}
