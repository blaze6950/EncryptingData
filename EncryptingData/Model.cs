using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        public void StartEncrypt()
        {

        }

        public void StartDecipher()
        {

        }
    }
}
