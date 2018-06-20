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
            _RMCrypto = new RijndaelManaged();
        }

        public string Path { get => _path; set => _path = value; }
        public CancellationTokenSource TokenSource { get => _tokenSource; set => _tokenSource = value; }

        public void StartEncrypt(String password)
        {
            _tokenSource = new CancellationTokenSource();
            Task.Run(() => Encrypt(password));
        }

        public void StartDecipher(String password)
        {
            _tokenSource = new CancellationTokenSource();
            Task.Run(() => Decipher(password));
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

                cs = new CryptoStream(fsCrypt,
                    _RMCrypto.CreateEncryptor(key, key),
                    CryptoStreamMode.Write);

                fsIn = new FileStream(_path, FileMode.Open);

                long fileLength = fsIn.Length;
                long totalBytes = 0;
                double persentage = 0;                
                int chunkSize;
                int part = 100;
                checked
                {
                    while (true)
                    {
                        if (fsIn.Length % part == 0)
                        {
                            chunkSize = (int)fsIn.Length / part;
                            break;
                        }
                        part++;
                    }
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
            finally
            {
                fsIn?.Close();
                cs?.Close();
                fsCrypt?.Close();
                if (_tokenSource != null && _tokenSource.Token.IsCancellationRequested)
                {
                    DeleteFile(fsCrypt);
                }
            }
        }

        public void Decipher(String password)
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

                cs = new CryptoStream(fsOut,
                    _RMCrypto.CreateDecryptor(key, key),
                    CryptoStreamMode.Write);
                long fileLength = fsCrypt.Length;
                long totalBytes = 0;
                double persentage = 0;


                int chunkSize;
                int part = 100;
                checked
                {
                    while (true)
                    {
                        if (fsCrypt.Length % part == 0)
                        {
                            chunkSize = (int)fsCrypt.Length / part;
                            break;
                        }
                        part++;
                        if (part > 200)
                        {
                            chunkSize = (int)fsCrypt.Length / part;
                            break;
                        }
                    }
                }

                for (long i = 0; i < fsCrypt.Length; i += chunkSize)
                {
                    byte[] chunkData = new byte[chunkSize];
                    int bytesRead = 0;
                    while ((bytesRead = fsCrypt.Read(chunkData, 0, chunkSize)) > 0)
                    {
                        totalBytes += bytesRead;
                        persentage = (double)totalBytes * 100.0 / fileLength;                        
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
            finally
            {
                fsOut?.Close();
                cs?.Clear();
                cs?.Close();
                fsCrypt?.Close();
                if (_tokenSource != null &&_tokenSource.Token.IsCancellationRequested)
                {
                    DeleteFile(fsOut);
                }
            }
        }

        private void DeleteFile(FileStream file)
        {
            File.Delete(file.Name);
        }
    }
}
