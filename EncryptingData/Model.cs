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
        private RijndaelManaged _RMCrypto;

        public Model()
        {
            _tokenSource = new CancellationTokenSource();
            _RMCrypto = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.None };
        }

        public string Path { get => _path; set => _path = value; }
        public CancellationTokenSource TokenSource { get => _tokenSource; set => _tokenSource = value; }

        public void StartEncrypt(String password)
        {
            Task.Run(() => Encrypt(password));
        }

        public void Encrypt(String password)
        {
            FileStream fsCrypt = null;
            CryptoStream cs = null;
            FileStream fsIn = null;
            try
            {
                UnicodeEncoding UE = new UnicodeEncoding();
                byte[] key = UE.GetBytes(password);

                string cryptFile = System.IO.Path.GetDirectoryName(_path) + "\\" + System.IO.Path.GetFileNameWithoutExtension(_path) + " enc" + System.IO.Path.GetExtension(_path);
                fsCrypt = new FileStream(cryptFile, FileMode.Create);

                _RMCrypto.Padding = PaddingMode.ANSIX923;
                cs = new CryptoStream(fsCrypt,
                    _RMCrypto.CreateEncryptor(key, key),
                    CryptoStreamMode.Write);

                fsIn = new FileStream(_path, FileMode.Open);

                long fileLength = fsIn.Length;
                long totalBytes = 0;
                double persentage = 0;                
                int chunkSize;
                checked
                {
                    chunkSize = (int)fsIn.Length / 100;
                }

                for (long i = 0; i < fsIn.Length; i += chunkSize)
                {                   
                    byte[] chunkData = new byte[chunkSize];
                    int bytesRead = 0;
                    while ((bytesRead = fsIn.Read(chunkData, 0, chunkSize)) > 0)
                    {
                        totalBytes += bytesRead;
                        persentage = (double)totalBytes * 100.0 / fileLength;                        
                        if (bytesRead != chunkSize)
                        {
                            for (int x = bytesRead - 1; x < chunkSize; x++)
                            {
                                chunkData[x] = 0;
                            }
                        }
                        cs.Write(chunkData, 0, bytesRead);
                        OnProgressChanged(persentage);
                        _tokenSource.Token.ThrowIfCancellationRequested();
                    }
                }
                cs.FlushFinalBlock();
                OnActionEnded();
            }
            catch (AggregateException ae)
            {
                if (ae.InnerException is OperationCanceledException)
                {
                    MessageBox.Show("Encrypting was canceled!", "+");
                    //DeleteFile(fsCrypt);
                }
                else
                {
                    MessageBox.Show(ae.Message, "Oooops");
                    //DeleteFile(fsCrypt);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Oooops");
                //DeleteFile(fsCrypt);
            }
            finally
            {
                fsIn.Close();
                cs.Close();
                fsCrypt.Close();                
            }
        }

        public void StartDecipher(String password)
        {
            FileStream fsCrypt = null;
            CryptoStream cs = null;
            FileStream fsOut = null;
            try
            {
                UnicodeEncoding UE = new UnicodeEncoding();
                byte[] key = UE.GetBytes(password);
                string cryptFile = _path;
                fsCrypt = new FileStream(cryptFile, FileMode.Open);                

                fsOut = new FileStream(cryptFile.Remove(cryptFile.IndexOf(" enc"), 4), FileMode.Create);

                _RMCrypto.Padding = PaddingMode.None;
                cs = new CryptoStream(fsOut,
                    _RMCrypto.CreateDecryptor(key, key),
                    CryptoStreamMode.Write);
                long fileLength = fsCrypt.Length;
                long totalBytes = 0;
                double persentage = 0;


                int chunkSize;
                checked
                {
                    chunkSize = (int)fsCrypt.Length / 100;
                }

                for (long i = 0; i < fsCrypt.Length; i += chunkSize)
                {
                    byte[] chunkData = new byte[chunkSize];
                    int bytesRead = 0;
                    while ((bytesRead = fsCrypt.Read(chunkData, 0, chunkSize)) > 0)
                    {
                        totalBytes += bytesRead;
                        persentage = (double)totalBytes * 100.0 / fileLength;                        
                        if (bytesRead != chunkSize)
                        {
                            for (int x = bytesRead - 1; x < chunkSize; x++)
                            {
                                chunkData[x] = 0;
                            }
                        }
                        cs.Write(chunkData, 0, bytesRead);
                        OnProgressChanged(persentage);
                        _tokenSource.Token.ThrowIfCancellationRequested();
                    }
                }
                cs.FlushFinalBlock();
                OnActionEnded();
            }
            catch (AggregateException ae)
            {
                if (ae.InnerException is OperationCanceledException)
                {
                    MessageBox.Show("Decrypting was canceled!", "+");
                    //DeleteFile(fsOut);
                }
                else
                {
                    MessageBox.Show(ae.Message, "Oooops");
                    //DeleteFile(fsOut);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Oooops");
                //DeleteFile(fsOut);
            }
            finally
            {
                fsOut.Close();
                cs.Close();
                fsCrypt.Close();                
            }
        }

        private void DeleteFile(FileStream file)
        {
            File.Delete(file.Name);
        }
    }
}
