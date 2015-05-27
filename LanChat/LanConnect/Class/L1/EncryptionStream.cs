using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Security;
using System.IO;

namespace LanConnect.Class.L1
{
    class EncryptionStream
    {

        byte[] IV = new byte[16];


        public byte[] aes256_en_de(byte[] data, byte[] pwd, bool encrypt, byte[] IV)
        {
            Aes aes = Aes.Create();
            aes.Mode = CipherMode.CFB;
            aes.KeySize = 256;
            aes.Key = pwd;
            if (encrypt)
            {
                MemoryStream ms = new MemoryStream();
                ICryptoTransform aesor = aes.CreateEncryptor(pwd, IV);
                CryptoStream cs = new CryptoStream(ms, aesor, CryptoStreamMode.Write);
                cs.Write(data, 0, data.Length);
                cs.Close();
                ms.Close();
                data = ms.ToArray();
                cs.Dispose();//TBD
                ms.Dispose();//
                return data;
            }
            else
            {
                Int64 i;
                MemoryStream ms = new MemoryStream(data);
                ICryptoTransform aesor = aes.CreateDecryptor(pwd, IV);
                CryptoStream cs = new CryptoStream(ms, aesor, CryptoStreamMode.Read);
                byte[] x = new byte[65535];
                try
                {
                    i = cs.Read(x, 0, x.Length);
                    byte[] dr = new byte[i];
                    Array.Copy(x, dr, i);
                    cs.Dispose();//TBD
                    ms.Dispose();//
                    return dr;
                }
                catch (CryptographicException er)
                {
                    return Encoding.UTF32.GetBytes(er.Message);
                }
            }
        }
        /*
        public byte[] rsa_en_de(byte[] data, byte[] pwd, bool encrypt)
        {
            RSA rsa = RSA.Create();
            rsa.KeySize = 2048;
            RSAParameters key = rsa.ExportParameters(true);
            rsa.ImportParameters(key);
            //////////////////////
            return rsa.EncryptValue(data);
        }

        public int Cnd_en_de(byte[] data, byte[] pwd, bool encrypt)//==> byte[]
        {

            return 0;
        }
        */
    }
}
