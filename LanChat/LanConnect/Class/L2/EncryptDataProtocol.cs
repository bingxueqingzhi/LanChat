using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LanConnect.Class.L1;


//firt 16 bytes is IV
//the rest is encrypted data bytes

namespace LanConnect.Class.L2
{
    public class EncryptDataProtocol
    {
        public bool encrypt = true;
        public byte[] GetData(byte[] data, byte[] pwd)
        {
            if (encrypt)
            {
                Random ra = new Random();
                byte[] IV = new byte[16];
                ra.NextBytes(IV);
                EncryptionStream es = new EncryptionStream();
                //byte[] ndata = es.aes256_en_de(data, pwd, encrypt, IV);//TBD, has problem
                List<byte> edata = new List<byte>(1);
                edata.AddRange(IV);
                edata.AddRange(es.aes256_en_de(data, pwd, encrypt, IV));
                return edata.ToArray();
            }
            else
            {
                byte[] IV = new byte[16];
                Array.Copy(data, IV, 16);
                byte[] edata = new byte[data.Length - 16];
                //List<byte> edata = new List<byte>(1);
                Array.Copy(data,16,edata,0,data.Length - 16);
                EncryptionStream es = new EncryptionStream();
                return es.aes256_en_de(edata.ToArray(), pwd, encrypt, IV);
            }
        }
    }
}
