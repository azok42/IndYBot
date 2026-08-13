using System.Security.Cryptography;

public class SecurityHelper
{
   private static byte[]? MasterKey;

   public static void Init(string masterKey)
   {
      MasterKey = Convert.FromBase64String(masterKey);

      if (MasterKey.Length != 32)
         throw new ArgumentException("Master key must be exactly 32 bytes!");
   }

   public static string Encrypt(string plain)
   {
      if (string.IsNullOrEmpty(plain))
         return plain;

      if (MasterKey == null)
         throw new InvalidOperationException("SecurityHelper not initialized!");

      using Aes aes = Aes.Create();
      aes.Key = MasterKey;
      aes.GenerateIV();

      using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
      using var memoryStream = new MemoryStream();

      memoryStream.Write(aes.IV, 0, aes.IV.Length);

      using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
      using (var streamWriter = new StreamWriter(cryptoStream))
      {
         streamWriter.Write(plain);
      }

      return Convert.ToBase64String(memoryStream.ToArray());
   }

   public static string Decrypt(string cipher)
   {
      if (string.IsNullOrEmpty(cipher))
         return cipher;

      if (MasterKey == null)
         throw new InvalidOperationException("SecurityHelper not initialized!");

      var fullCipher = Convert.FromBase64String(cipher);

      using Aes aes = Aes.Create();

      var iv = new byte[16];
      Array.Copy(fullCipher, 0, iv, 0, iv.Length);

      aes.Key = MasterKey;
      aes.IV = iv;

      using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

      using var memoryStream = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length);
      using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
      using var streamReader = new StreamReader(cryptoStream);

      return streamReader.ReadToEnd();
   }
}
