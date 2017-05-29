using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace NoPassAssignment.Controllers
{
    public partial class AccountController
    {
        public string GenerateKey(int size)
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[size];
            rng.GetBytes(buff);
            return Convert.ToBase64String(buff);
        }

        private static Tuple<byte[], byte[]> CreateKeyAndIv()
        {
            using (var rijndael = Rijndael.Create())
            {
                rijndael.GenerateKey();
                rijndael.GenerateIV();
                return new Tuple<byte[], byte[]>(rijndael.Key, rijndael.IV);
            }
        }

        static byte[] EncryptDataUsginAes256(string plainText, byte[] Key, byte[] IV)
        {
            byte[] encrypted;
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;
                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            return encrypted;
        }

        /* Encrypt AES 256 KEY
         Input: string: key, IV
         */
        public byte[] EncryptAESKeyAndDecryptUsingRSA(Tuple<byte[], byte[]> keyAndIv, byte[] text,
            Tuple<byte[], byte[]> concatenatedKeyAndIv)
        {
            var csp = new RSACryptoServiceProvider(2048);
            var privKey = csp.ExportParameters(true);
            var pubKey = csp.ExportParameters(false);

            string pubKeyString;
            {
                var sw = new StringWriter();
                var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                xs.Serialize(sw, pubKey);
                pubKeyString = sw.ToString();
            }

            {
                var sr = new StringReader(pubKeyString);
                var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                pubKey = (RSAParameters) xs.Deserialize(sr);
            }

            csp = new RSACryptoServiceProvider();
            csp.ImportParameters(pubKey);


            var ms = new MemoryStream(
                new byte[concatenatedKeyAndIv.Item1.Length + concatenatedKeyAndIv.Item2.Length + 1], 0,
                concatenatedKeyAndIv.Item1.Length + concatenatedKeyAndIv.Item2.Length + 1, true, true);
            ms.Write(concatenatedKeyAndIv.Item1, 0, concatenatedKeyAndIv.Item1.Length);
            var comma = Encoding.ASCII.GetBytes(",");
            ms.Write(comma, 0, 1);
            ms.Write(concatenatedKeyAndIv.Item2, 0, concatenatedKeyAndIv.Item2.Length);
            byte[] concatenatedKeyAndIvBytes = ms.GetBuffer();
            var concatenatedKeyAndIvEncrypted = csp.Encrypt(concatenatedKeyAndIvBytes, false);


            //DECRYPT - as I already know the private key
            //we want to decrypt, therefore we need a csp and load our private key
            csp = new RSACryptoServiceProvider();
            csp.ImportParameters(privKey);

            var decryptedKeyAndIv = csp.Decrypt(concatenatedKeyAndIvEncrypted, false);
            return decryptedKeyAndIv;
        }

        public string DecryptUsingAes(byte[] plainText, byte[] givenKey, byte[] givenIv)
        {
            string result = null;
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = givenKey;
                aesAlg.IV = givenIv;

                aesAlg.Mode = CipherMode.CBC;
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (var msDecrypt = new MemoryStream(plainText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            result = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return result;
        }
    }
}