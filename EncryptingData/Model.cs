using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace EncryptingData
{
    class Model
    {
        private string _path;
        private CancellationTokenSource _tokenSource;

        public Model()
        {
            _tokenSource = new CancellationTokenSource();
        }

        public string Path { get => _path; set => _path = value; }
        public CancellationTokenSource TokenSource { get => _tokenSource; set => _tokenSource = value; }

        public void StartEncrypt(String password)
        {
            try
            {
                UnicodeEncoding UE = new UnicodeEncoding();
                byte[] key = UE.GetBytes(password);

                string cryptFile = System.IO.Path.GetDirectoryName(_path) + "\\" + System.IO.Path.GetFileNameWithoutExtension(_path) + " enc" + System.IO.Path.GetExtension(_path);
                FileStream fsCrypt = new FileStream(cryptFile, FileMode.Create);

                RijndaelManaged RMCrypto = new RijndaelManaged();

                CryptoStream cs = new CryptoStream(fsCrypt,
                    RMCrypto.CreateEncryptor(/*key, key*/),
                    CryptoStreamMode.Write);

                FileStream fsIn = new FileStream(_path, FileMode.Open);

                int data;
                while ((data = fsIn.ReadByte()) != -1)
                    cs.WriteByte((byte) data);


                fsIn.Close();
                cs.Close();
                fsCrypt.Close();
            }
            catch (AggregateException ae)
            {
                if (ae.InnerException is OperationCanceledException)
                {
                    MessageBox.Show("Encrypting was canceled!", "+");
                }
                else
                {
                    MessageBox.Show(ae.Message, "Oooops");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Oooops");
            }
        }

        public void StartDecipher()
        {

        }
    }
}
