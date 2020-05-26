using System;
using System.Text;
using System.Windows;
using System.Security.Cryptography;
using Microsoft.Win32;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;

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

        private int chunksize = 1024 * 1024;

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

        private void rbtnHash_Checked(object sender, RoutedEventArgs e)
        {
            rbtnSHA256.IsChecked = true;
        }

        private void rbtnSHA256_Checked(object sender, RoutedEventArgs e)
        {
            rbtnHash.IsChecked = true;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (txtNameFile.Text.Equals(""))
            {
                MessageBox.Show("Hãy chọn tệp tin cần xử lí.");
                return;
            }
            if (rbtnHash.IsChecked == true)
            {
                if (rbtnSHA256.IsChecked == true)
                {
                    hashProgreess(txtNameFile.Text, ".hash");                    
                    //txtNameFile.Text = "";
                    //txtOpenKey.Text = "";
                    //MessageBox.Show("Băm thành công!");
                    //return;
                }
                else
                {
                    MessageBox.Show("Hãy chọn SHA256.");
                    return;
                }
            }            
            else if (rbtnEncrypt.IsChecked == true)
            {
                if (txtOpenKey.Text.Equals(""))
                {
                    MessageBox.Show("Hãy chọn tệp tin chứa khóa.");
                    return;
                }

                if (rbtnAES.IsChecked == true)
                {
                    prgBar.Value = 0;
                    BackgroundWorker worker = new BackgroundWorker();
                    string[] filename = { txtOpenKey.Text, txtNameFile.Text };
                    worker.WorkerReportsProgress = true;
                    worker.DoWork += Encrypt_AES;
                    worker.ProgressChanged += worker_ProgressChanged;
                    worker.RunWorkerCompleted += worker_RunWorkerCompleted;
                    worker.RunWorkerAsync(filename);                    
                } 
                else if (rbtnDES.IsChecked == true)
                {
                    prgBar.Value = 0;
                    BackgroundWorker worker = new BackgroundWorker();
                    string[] filename = { txtOpenKey.Text, txtNameFile.Text };
                    worker.WorkerReportsProgress = true;
                    worker.DoWork += Encrypt_DES;
                    worker.ProgressChanged += worker_ProgressChanged;
                    worker.RunWorkerCompleted += worker_RunWorkerCompleted;                    
                    worker.RunWorkerAsync(filename);                    
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
            else if (rbtnDecrypt.IsChecked == true)
            {
                if (txtOpenKey.Text.Equals(""))
                {                    
                    MessageBox.Show("Hãy chọn tệp tin chứa khóa.");
                    return;
                }
                if (rbtnAES.IsChecked == true)
                {
                    prgBar.Value = 0;
                    BackgroundWorker worker = new BackgroundWorker();
                    string[] filename = { txtOpenKey.Text, txtNameFile.Text };
                    worker.WorkerReportsProgress = true;
                    worker.DoWork += Decrypt_AES;
                    worker.ProgressChanged += worker_ProgressChanged;
                    worker.RunWorkerCompleted += worker_RunWorkerCompleted;
                    worker.RunWorkerAsync(filename);
                }
                else if (rbtnDES.IsChecked == true)
                {
                    prgBar.Value = 0;
                    BackgroundWorker worker = new BackgroundWorker();
                    string[] filename = { txtOpenKey.Text, txtNameFile.Text };
                    worker.WorkerReportsProgress = true;
                    worker.DoWork += Decrypt_DES;
                    worker.ProgressChanged += worker_ProgressChanged;
                    worker.RunWorkerCompleted += worker_RunWorkerCompleted;
                    worker.RunWorkerAsync(filename);
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
        }

        private void Encrypt_DES(object sender, DoWorkEventArgs e)
        {
            string[] filename = (string[])e.Argument;
            string in_filename = filename[1];
            string key_filename = filename[0];

            FileStream fsIn = new FileStream(in_filename, FileMode.Open, FileAccess.Read);
            FileStream fsOut = new FileStream(CreateOutFile(in_filename, ".des"), FileMode.OpenOrCreate, FileAccess.Write);

            double progress = 0;
            double filesize = fsIn.Length;

            //Hash file input with SHA256            
            new Thread(new ThreadStart(() => hashProgreess(in_filename, ".hashenc"))).Start();
            

            byte[] keyBytes = Encoding.UTF8.GetBytes(File.ReadAllText(key_filename));

            // Hash the password with SHA256
            keyBytes = SHA256.Create().ComputeHash(keyBytes);

            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            PasswordDeriveBytes pdb = new PasswordDeriveBytes(keyBytes, saltBytes);
            DES alg = DES.Create();
            alg.Key = pdb.GetBytes(8);
            alg.IV = pdb.GetBytes(8);
            try
            {
                CryptoStream cs = new CryptoStream(fsOut, alg.CreateEncryptor(), CryptoStreamMode.Write);
                int bufferLen = chunksize;
                byte[] buffer = new byte[bufferLen];
                int bytesRead;
                do
                {
                    bytesRead = fsIn.Read(buffer, 0, bufferLen);
                    cs.Write(buffer, 0, bytesRead);
                    progress += chunksize;
                    (sender as BackgroundWorker).ReportProgress((int)(progress / filesize * 100));
                } while (bytesRead != 0);
                cs.Close();
                fsIn.Close();
                e.Result = "Mã hóa DES thành công!";
            }
            catch (CryptographicException ex)
            {
                Console.WriteLine("Sorry, error occurred. " + ex);
                e.Result = "Đã có lỗi xảy ra khi mã hóa!";
            }
        }
        
        private void Decrypt_DES(object sender, DoWorkEventArgs e)
        {
            string[] filename = (string[])e.Argument;
            string in_filename = filename[1];
            string key_filename = filename[0];

            FileStream fsIn = new FileStream(in_filename, FileMode.Open, FileAccess.Read);
            FileStream fsOut = new FileStream(Path.ChangeExtension(in_filename, null), FileMode.OpenOrCreate, FileAccess.Write);

            double progress = 0;
            double filesize = fsIn.Length;

            byte[] keyBytes = Encoding.UTF8.GetBytes(File.ReadAllText(key_filename));

            // Hash the password with SHA256
            keyBytes = SHA256.Create().ComputeHash(keyBytes);

            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            PasswordDeriveBytes pdb = new PasswordDeriveBytes(keyBytes, saltBytes);
            DES alg = DES.Create();
            alg.Key = pdb.GetBytes(8);
            alg.IV = pdb.GetBytes(8);
            try
            {
                CryptoStream cs = new CryptoStream(fsOut, alg.CreateDecryptor(), CryptoStreamMode.Write);
                int bufferLen = chunksize;
                byte[] buffer = new byte[bufferLen];
                int bytesRead;
                do
                {
                    bytesRead = fsIn.Read(buffer, 0, bufferLen);
                    cs.Write(buffer, 0, bytesRead);
                    progress += chunksize;
                    (sender as BackgroundWorker).ReportProgress((int)(progress / filesize * 100));
                } while (bytesRead != 0);
                cs.Close();
                fsIn.Close();
                e.Result = "Giải mã DES thành công!";
            }                
            catch (CryptographicException ex)
            {
                Console.WriteLine("Sorry, error occurred. " + ex);
                e.Result = "Đã có lỗi xảy ra khi giải mã!";
            }
        }

        private void Encrypt_AES(object sender, DoWorkEventArgs e)
        {
            string[] filename = (string[])e.Argument;
            string in_filename = filename[1];
            string key_filename = filename[0];

            FileStream fsIn = new FileStream(in_filename, FileMode.Open, FileAccess.Read);
            FileStream fsOut = new FileStream(CreateOutFile(in_filename,".aes"), FileMode.OpenOrCreate, FileAccess.Write);

            double progress = 0;
            double filesize = fsIn.Length;

            //Hash file input with SHA256
            new Thread(new ThreadStart(() => hashProgreess(in_filename, ".hashenc"))).Start();
            
            byte[] keyBytes = Encoding.UTF8.GetBytes(File.ReadAllText(key_filename));

            // Hash the password with SHA256
            //keyBytes = SHA256.Create().ComputeHash(keyBytes);

            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            PasswordDeriveBytes pdb = new PasswordDeriveBytes(keyBytes, saltBytes);
            Aes alg = Aes.Create();
            alg.Key = pdb.GetBytes(256/8);
            alg.IV = pdb.GetBytes(128/8);
            try 
            { 
                CryptoStream cs = new CryptoStream(fsOut, alg.CreateEncryptor(), CryptoStreamMode.Write);
                int bufferLen = chunksize;
                byte[] buffer = new byte[bufferLen];
                int bytesRead;
                do
                {
                    bytesRead = fsIn.Read(buffer, 0, bufferLen);
                    cs.Write(buffer, 0, bytesRead);
                    progress += chunksize;
                    (sender as BackgroundWorker).ReportProgress((int)(progress / filesize * 100));
                } while (bytesRead != 0);
                cs.Close();
                fsIn.Close();
                e.Result = "Mã hóa AES thành công!";
            }
            catch (CryptographicException ex)
            {
                Console.WriteLine("Sorry, error occurred. " + ex);
                e.Result = "Đã có lỗi xảy ra khi mã hóa!";
            }
}

        private void Decrypt_AES(object sender, DoWorkEventArgs e)
        {
            string[] filename = (string[])e.Argument;
            string in_filename = filename[1];
            string key_filename = filename[0];

            FileStream fsIn = new FileStream(in_filename, FileMode.Open, FileAccess.Read);
            FileStream fsOut = new FileStream(Path.ChangeExtension(in_filename, null), FileMode.OpenOrCreate, FileAccess.Write);

            double progress = 0;
            double filesize = fsIn.Length;

            byte[] keyBytes = Encoding.UTF8.GetBytes(File.ReadAllText(key_filename));

            // Hash the password with SHA256
            //keyBytes = SHA256.Create().ComputeHash(keyBytes);

            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            PasswordDeriveBytes pdb = new PasswordDeriveBytes(keyBytes, saltBytes);
            Aes alg = Aes.Create();
            alg.Key = pdb.GetBytes(256 / 8);
            alg.IV = pdb.GetBytes(128 / 8);
            try
            {
                CryptoStream cs = new CryptoStream(fsOut, alg.CreateDecryptor(), CryptoStreamMode.Write);
                int bufferLen = chunksize;
                byte[] buffer = new byte[bufferLen];
                int bytesRead;
                do
                {
                    bytesRead = fsIn.Read(buffer, 0, bufferLen);
                    cs.Write(buffer, 0, bytesRead);
                    progress += chunksize;
                    (sender as BackgroundWorker).ReportProgress((int)(progress / filesize * 100));
                } while (bytesRead != 0);
                cs.Close();
                fsIn.Close();
                e.Result = "Giải mã AES thành công!";
            }
            catch(CryptographicException ex)
            {
                Console.WriteLine("Sorry, error occurred. " + ex);
                e.Result = "Đã có lỗi xảy ra khi giải mã!";
            }
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            prgBar.Value = e.ProgressPercentage;
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            txtNameFile.Text = "";
            txtOpenKey.Text = "";
            MessageBox.Show((string)e.Result);
        }

        public string CreateOutFile(string in_filename, string extension)
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

        public void hashProgreess(string filename, string extension)
        {
            BackgroundWorker worker = new BackgroundWorker();
            string[] name = { filename, extension };
            worker.WorkerReportsProgress = true;
            worker.DoWork += hashFileSHA256;
            worker.ProgressChanged += worker_ProgressChanged_hash;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted_hash;
            worker.RunWorkerAsync(name);
        }

        public void hashFileSHA256(object sender, DoWorkEventArgs e)
        {
            string[] name = (string[])e.Argument;
            string filename = name[0];
            string extension = name[1];
            (sender as BackgroundWorker).ReportProgress(100);
            string result;
            using (var stream = new BufferedStream(File.OpenRead(filename), 10000))
            {
                SHA256Managed sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(stream);
                result = BitConverter.ToString(checksum).Replace("-", String.Empty).ToLower();  
            }
            if (extension.Equals(".hashenc"))
                File.WriteAllText(CreateOutFile(filename, extension), result);
            else
                File.WriteAllText(filename + extension, result);
        }

        void worker_ProgressChanged_hash(object sender, ProgressChangedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                prgBar_hash.IsIndeterminate = true;
            });
            
        }

        void worker_RunWorkerCompleted_hash(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                prgBar_hash.IsIndeterminate = false;
                MessageBox.Show("Băm thành công!");
            });
            
        }
    }
}
