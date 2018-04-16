using System;
using System.IO;
using System.Security.Cryptography;

namespace SharedClasses
{
    public class Crypt
    {
        private static string password = "wpfg^m@js#4!8";
        private static RNGCryptoServiceProvider r = new RNGCryptoServiceProvider();
        public static int SaltLength = 0xFF;

        // returns an array of 255 random bytes.
        public static byte[] GetSalt()
        {
            byte[] Salt = new byte[SaltLength];
            r.GetBytes(Salt);
            return Salt;
        }

        #region Crypt/Decrypt functions

        public static byte[] Encrypt(byte[] input, byte[] Salt, string givenpassword = null)
        {
            PasswordDeriveBytes pdb =
              new PasswordDeriveBytes((givenpassword == null) ? password : givenpassword, // Change this
              Salt); // Change this
            MemoryStream ms = new MemoryStream();
            Aes aes = new AesManaged();
            aes.Key = pdb.GetBytes(aes.KeySize / 8);
            aes.IV = pdb.GetBytes(aes.BlockSize / 8);
            aes.Padding = PaddingMode.Zeros;
            CryptoStream cs = new CryptoStream(ms,
              aes.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(input, 0, input.Length);
            cs.Close();
            pdb.Dispose();
            return ms.ToArray();
        }
        public static byte[] Decrypt(byte[] input, byte[] Salt, string givenpassword = null)
        {
            PasswordDeriveBytes pdb =
              new PasswordDeriveBytes((givenpassword == null) ? password : givenpassword, // Change this
              Salt); // Change this
            MemoryStream ms = new MemoryStream();
            Aes aes = new AesManaged();
            aes.Key = pdb.GetBytes(aes.KeySize / 8);
            aes.IV = pdb.GetBytes(aes.BlockSize / 8);
            aes.Padding = PaddingMode.Zeros;
            CryptoStream cs = new CryptoStream(ms,
              aes.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(input, 0, input.Length);
            cs.Close();
            pdb.Dispose();
            return ms.ToArray();
        }

        #endregion
        #region Encode Text
        public static byte[] Base64Encode(string plainText)
        {
            return System.Text.Encoding.UTF8.GetBytes(plainText);
        }

        public static string Base64Decode(byte[] base64EncodedData)
        {
            return System.Text.Encoding.UTF8.GetString(base64EncodedData);
        }
        #endregion
    }
}
