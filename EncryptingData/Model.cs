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

    public delegate void ProgressChanged(double progress);
    public delegate void ActionEnded();

    class Model
    {
        private string _path;
        private CancellationTokenSource _tokenSource;
        public event ProgressChanged OnProgressChanged;
        public event ActionEnded OnActionEnded;

        public Model()
        {
            _tokenSource = new CancellationTokenSource();
        }

        public string Path { get => _path; set => _path = value; }
        public CancellationTokenSource TokenSource { get => _tokenSource; set => _tokenSource = value; }

        public void StartEncrypt(String password)
        {
            Task.Run(() => Encrypt(password));
        }

        public void Encrypt(String password)
        {
            try
            {
                UnicodeEncoding UE = new UnicodeEncoding();
                byte[] key = UE.GetBytes(password);

                string cryptFile = System.IO.Path.GetDirectoryName(_path) + "\\" + System.IO.Path.GetFileNameWithoutExtension(_path) + " enc" + System.IO.Path.GetExtension(_path);
                FileStream fsCrypt = new FileStream(cryptFile, FileMode.Create);

                RijndaelManaged RMCrypto = new RijndaelManaged();

                CryptoStream cs = new CryptoStream(fsCrypt,
                    RMCrypto.CreateEncryptor(key, key),
                    CryptoStreamMode.Write);

                FileStream fsIn = new FileStream(_path, FileMode.Open);

                long fileLength = fsIn.Length;
                long totalBytes = 0;
                int data = 0;
                while ((data = fsIn.ReadByte()) != -1)
                {
                    totalBytes += data;
                    double persentage = (double)totalBytes * 100.0 / fileLength;
                    cs.WriteByte((byte)data);
                    _tokenSource.Token.ThrowIfCancellationRequested();
                    OnProgressChanged(persentage);
                }


                fsIn.Close();
                cs.Close();
                fsCrypt.Close();
                OnActionEnded();
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

        public void StartDecipher(String password)
        {
            try
            {
                UnicodeEncoding UE = new UnicodeEncoding();
                byte[] key = UE.GetBytes(password);
                string cryptFile = _path;
                FileStream fsCrypt = new FileStream(cryptFile, FileMode.Open);

                RijndaelManaged RMCrypto = new RijndaelManaged();

                CryptoStream cs = new CryptoStream(fsCrypt,
                    RMCrypto.CreateDecryptor(key, key),
                    CryptoStreamMode.Read);

                FileStream fsOut = new FileStream(cryptFile.Remove(cryptFile.IndexOf(" enc"), 4), FileMode.Create);
                long fileLength = fsCrypt.Length;
                long totalBytes = 0;
                int data = 0;
                while ((data = cs.ReadByte()) != -1)
                {
                    fsOut.WriteByte((byte)data);
                    double persentage = (double)totalBytes * 100.0 / fileLength;
                    OnProgressChanged(persentage);
                }
                    

                fsOut.Close();
                cs.Close();
                fsCrypt.Close();
                OnActionEnded();
            }
            catch (AggregateException ae)
            {
                if (ae.InnerException is OperationCanceledException)
                {
                    MessageBox.Show("Decrypting was canceled!", "+");
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
    }
}
