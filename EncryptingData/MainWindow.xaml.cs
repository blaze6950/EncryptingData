using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EncryptingData
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Model _model;
        public MainWindow()
        {
            InitializeComponent();
            _model = new Model();
        }

        private void ButtonFile_Click(object sender, RoutedEventArgs e)
        {
            var fileD = new OpenFileDialog();
            fileD.Multiselect = false;
            var res = fileD.ShowDialog();
            if (res != null && res == true)
            {
                _model.Path = fileD.FileName;
                TextBoxPathFile.Text = _model.Path;
            }
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)RadioButtonEncrypt.IsChecked)
            {
                _model.StartEncrypt();
            }
            else
            {
                _model.StartDecipher();
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            _model.TokenSource.Cancel();
        }        
    }
}
