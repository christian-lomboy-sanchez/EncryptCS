using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;

class Program
{
    static void Main()
    {
        string url = "http://192.168.3.93:9090/blob.bin";

        string desktop = Environment.GetFolderPath(
            Environment.SpecialFolder.DesktopDirectory);

        string rawFile = Path.Combine(desktop, "downloaded.bin");
        string encryptedFile = Path.Combine(desktop, "downloaded.bin.enc");

        string keyString = "0123456789ABCDEFGHIJKLMNOPQRSTUV";
        string ivString  = "0123456789ABCDEF";

        try
        {
            // Download raw binary data.
            byte[] rawData;

            using (HttpClient client = new HttpClient())
            {
                rawData = client.GetByteArrayAsync(url)
                                .GetAwaiter()
                                .GetResult();
            }

            // Save original binary data.
            File.WriteAllBytes(rawFile, rawData);

            // Convert the supplied ASCII key/IV to bytes.
            byte[] key = System.Text.Encoding.ASCII.GetBytes(keyString);
            byte[] iv  = System.Text.Encoding.ASCII.GetBytes(ivString);

            byte[] encryptedData;

            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.BlockSize = 128;

                aes.Key = key;
                aes.IV = iv;

                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    encryptedData = encryptor.TransformFinalBlock(
                        rawData,
                        0,
                        rawData.Length
                    );
                }
            }

            // Save encrypted binary data.
            File.WriteAllBytes(encryptedFile, encryptedData);

            Console.WriteLine("Original:");
            Console.WriteLine(rawFile);

            Console.WriteLine();
            Console.WriteLine("AES-256-CBC encrypted:");
            Console.WriteLine(encryptedFile);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}