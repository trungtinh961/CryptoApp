using System;
using System.Text;
using System.Windows;
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
            if (rbtnEncrypt.IsChecked == true && !txtOpenKey.Text.Equals(""))
            {
                if (rbtnAES.IsChecked == true)
                {
                    Encrypt_AES(txtOpenKey.Text,txtNameFile.Text);
                    txtNameFile.Text = "";
                    txtOpenKey.Text = "";
                    return;
                } 
                else if (rbtnDES.IsChecked == true)
                {
                    Encrypt_DES(txtOpenKey.Text, txtNameFile.Text);
                    txtNameFile.Text = "";
                    txtOpenKey.Text = "";
                    return;
                } 
                else if (rbtnSHA256.IsChecked == true)
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
            else if (rbtnDecrypt.IsChecked == true && !txtOpenKey.Text.Equals(""))
            {
                if (rbtnAES.IsChecked == true)
                {
                    Decrypt_AES(txtOpenKey.Text, txtNameFile.Text);
                    txtNameFile.Text = "";
                    txtOpenKey.Text = "";
                    return;
                }
                else if (rbtnDES.IsChecked == true)
                {
                    Decrypt_DES(txtOpenKey.Text, txtNameFile.Text);
                    txtNameFile.Text = "";
                    txtOpenKey.Text = "";
                    return;
                }
                else if (rbtnSHA256.IsChecked == true)
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
                if (rbtnSHA256.IsChecked == true)
                {
                    File.WriteAllText(txtNameFile.Text + ".hash", hashFileSHA256(txtNameFile.Text));
                    txtNameFile.Text = "";
                    txtOpenKey.Text = "";
                    return;
                }
                else
                {
                    MessageBox.Show("Hãy chọn MD5.");
                    return;
                }
            }
        }

        static string hashFileSHA256(string filename)
        {
            using (var hash = SHA256.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hashValue = hash.ComputeHash(stream);
                    return BitConverter.ToString(hashValue).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        private void Encrypt_DES(string key_filename, string in_filename)
        {
            throw new NotImplementedException();
        }

        private void Decrypt_DES(string key_filename, string in_filename)
        {
            throw new NotImplementedException();
        }

        private void Encrypt_AES(string key_filename, string in_filename)
        {
            FileStream fsIn = new FileStream(in_filename, FileMode.Open, FileAccess.Read);
            FileStream fsOut = new FileStream(CreateOutFile(in_filename,".enc"), FileMode.OpenOrCreate, FileAccess.Write);

            //Hash file input with SHA256
            File.WriteAllText(CreateOutFile(in_filename, ".hashenc"), hashFileSHA256(in_filename));

            byte[] keyBytes = Encoding.UTF8.GetBytes(File.ReadAllText(key_filename));

            // Hash the password with SHA256
            keyBytes = SHA256.Create().ComputeHash(keyBytes);

            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (RijndaelManaged AES = new RijndaelManaged())
            {
                AES.KeySize = 256;
                AES.BlockSize = 128;
                var key = new Rfc2898DeriveBytes(keyBytes, saltBytes, 1000);
                AES.Key = key.GetBytes(AES.KeySize / 8);
                AES.IV = key.GetBytes(AES.BlockSize / 8);
                AES.Mode = CipherMode.CBC;
                using (var cs = new CryptoStream(fsOut, AES.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    int bufferLen = 64 * 1024;
                    byte[] buffer = new byte[bufferLen];
                    int bytesRead;
                    do
                    {
                        bytesRead = fsIn.Read(buffer, 0, bufferLen);
                        cs.Write(buffer, 0, bytesRead);
                    } while (bytesRead != 0);
                    cs.Close();
                }
                fsIn.Close();
            }    
        }

        private void Decrypt_AES(string key_filename, string in_filename)
        {
            FileStream fsIn = new FileStream(in_filename, FileMode.Open, FileAccess.Read);
            FileStream fsOut = new FileStream(Path.ChangeExtension(in_filename, null), FileMode.OpenOrCreate, FileAccess.Write);

            byte[] keyBytes = Encoding.UTF8.GetBytes(File.ReadAllText(key_filename));

            // Hash the password with SHA256
            keyBytes = SHA256.Create().ComputeHash(keyBytes);

            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (RijndaelManaged AES = new RijndaelManaged())
            {
                AES.KeySize = 256;
                AES.BlockSize = 128;
                var key = new Rfc2898DeriveBytes(keyBytes, saltBytes, 1000);
                AES.Key = key.GetBytes(AES.KeySize / 8);
                AES.IV = key.GetBytes(AES.BlockSize / 8);
                AES.Mode = CipherMode.CBC;
                using (var cs = new CryptoStream(fsOut, AES.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    int bufferLen = 64 * 1024;
                    byte[] buffer = new byte[bufferLen];
                    int bytesRead;
                    do
                    {
                        bytesRead = fsIn.Read(buffer, 0, bufferLen);
                        cs.Write(buffer, 0, bytesRead);
                    } while (bytesRead != 0);
                    cs.Close();
                }
                fsIn.Close();
            }
        }

        private string CreateOutFile(string in_filename, string extension)
        {
            string path = Path.GetDirectoryName(in_filename);
            string filename = Path.GetFileName(in_filename);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(in_filename);
            if (!Directory.Exists(path + "\\" + fileNameWithoutExtension))
            {
                Directory.CreateDirectory(path + "\\" + fileNameWithoutExtension);
            }
            return path + "\\" + fileNameWithoutExtension + "\\" + filename + extension;
        }
    }
}
