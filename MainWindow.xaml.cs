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
using System.Security.Cryptography;
using Microsoft.Win32;
using System.IO;

namespace CryptoApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            txtNameFile.Text = HashMD5("C:\\Users\\Trung Tinh\\Desktop\\key.txt");
        }

        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All Files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == true)
            {
                txtNameFile.Text = openFileDialog.FileName;
            }
        }

        private void btnOpenKey_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt";
            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == true)
            {
                txtOpenKey.Text = openFileDialog.FileName;
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (txtNameFile.Text.Equals(""))
            {
                MessageBox.Show("Hãy chọn tệp tin cần xử lí và tệp chứa khóa.");
                return;
            }
            if (rbtnEncrypt.IsChecked == true && txtOpenKey.Text.Equals(""))
            {
                if (rbtnAES.IsChecked == true)
                {
                    Encrypt_AES(txtOpenKey.Text,txtNameFile.Text);
                    return;
                } 
                else if (rbtnDES.IsChecked == true)
                {
                    Encrypt_DES(txtOpenKey.Text, txtNameFile.Text);
                    return;
                } 
                else if (rbtnMD5.IsChecked == true)
                {
                    MessageBox.Show("Hãy chọn DES hoặc AES.");
                    return;
                }
                else
                {
                    MessageBox.Show("Bạn chưa chọn giải thuật!");
                    return;
                }
            }
            else if (rbtnDecrypt.IsChecked == true && txtOpenKey.Text.Equals(""))
            {
                if (rbtnAES.IsChecked == true)
                {
                    Decrypt_AES(txtOpenKey.Text, txtNameFile.Text);
                    return;
                }
                else if (rbtnDES.IsChecked == true)
                {
                    Decrypt_DES(txtOpenKey.Text, txtNameFile.Text);
                    return;
                }
                else if (rbtnMD5.IsChecked == true)
                {
                    MessageBox.Show("Hãy chọn DES hoặc AES.");
                    return;
                }
                else
                {
                    MessageBox.Show("Bạn chưa chọn giải thuật!");
                    return;
                }
            }
            else if (rbtnHash.IsChecked == true)
            {
                if (rbtnMD5.IsChecked == true)
                {
                    HashMD5(txtOpenKey.Text);
                    return;
                }
                else
                {
                    MessageBox.Show("Hãy chọn MD5.");
                    return;
                }
            }    

        }

        private string HashMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        private void Encrypt_DES(string key_filename, string in_filename, string out_filename = "")
        {
            throw new NotImplementedException();
        }

        private void Decrypt_DES(string key_filename, string in_filename, string out_filename = "")
        {
            throw new NotImplementedException();
        }

        private void Encrypt_AES(string key_filename, string in_filename, string out_filename = "")
        {
            throw new NotImplementedException();
        }

        private void Decrypt_AES(string key_filename, string in_filename, string out_filename = "")
        {
            throw new NotImplementedException();
        }



    }
}
