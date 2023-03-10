using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Process process = new Process();
        List<Folder> folders = new List<Folder>();
        List<Button> mybuttons = new List<Button>();
        String TCsFolderPath = @"\\10.249.6.68\Share\Marwan\02.11.2022\IMIB_TCs\IMIB_Installation_Test_Set\";
        String selectedItem;
        bool boolTCStatus_Unknown = false;
        string str_TCStatus = null;
        bool bool_TCStatus_Passed = false;
        String str_TCStatus_in_Label = "unknown";
        String str_TCStatusColor = "Black";
        bool workCompleted= false;
        bool bool_ReportAvailability = false;
        BackgroundWorker worker = new BackgroundWorker();


        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_STYLE = -16;

        private const int WS_MAXIMIZEBOX = 0x10000; //maximize button
        private const int WS_MINIMIZEBOX = 0x20000; //minimize button
        public object ItemCollection { get; private set; }
        [DllImport("user32", CharSet = CharSet.Auto)]
        public static extern int EnableWindow(IntPtr hwnd, bool bEnable);
        public MainWindow()
        {
            InitializeComponent();
            //--------------------Worker----------------
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            
            //------------------------------------------
            this.SourceInitialized += MainWindow_SourceInitialized;
           this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
           
        }

        


        //----------------------Worker------------------------
        private delegate void mydelegate(int i); 
        private void displayi(int i) 
        {
            //lblProcess.Content= "Background Workd: "+i.ToString();
        }
        private void Worker_DoWork(object sender, DoWorkEventArgs e) 
        {
            if (worker.CancellationPending)
            {
                e.Cancel= true;
                return;
            }
            kill_UFT();
            String command = @"UFTBatchRunnerCMD.exe -visible N -source " + '"' + selectedItem + '"';
            StartProcess(command);

        }
        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                UpdateList();
                kill_UFT();
                workCompleted = true;
                btnStop.IsEnabled= false;
                BTN.IsEnabled= true;
                //lblComplete.Content = "Complete Background Work";
            }
            else
            {
                UpdateList();
                kill_UFT();
                workCompleted = true;

                //lblComplete.Content = "Fail";
            }
        }
        //---------------------------------------------------

        private void ViewTestCasesfolders(String TCsFolderPath)
        {
            String TCNameStarter = "[IMIBNext]";
            string currentDirectoryPath = TCsFolderPath;
            if (Directory.Exists(currentDirectoryPath))
            {
                folders.Clear();
                foreach (var d in System.IO.Directory.GetDirectories(TCsFolderPath))
                {
                    if (d.Contains(TCNameStarter))
                    {
                        var dir = new DirectoryInfo(d);
                        var dirName = dir.Name;
                        String XMLTCStatusFilePath = TCsFolderPath + dirName + @"\Report\Report\run_results";
                        checkTCStatus(XMLTCStatusFilePath, dirName);
                        folders.Add(new Folder() { FolderName = dirName, FolderPath = TCsFolderPath + dirName, TCStatus = str_TCStatus_in_Label , TCStatusColor= str_TCStatusColor , Folder_bool_ReportAvailability = bool_ReportAvailability});

                    }
                    else
                    {
                        //MessageBox.Show("No TCs Starting with [IMIBNext] are Found!", "TCs Folder", MessageBoxButton.OK, MessageBoxImage.Information);

                    }
                }
            }
            else
            {
                MessageBox.Show("No TCs are Found!", "TCs Folder", MessageBoxButton.OK, MessageBoxImage.Information);  
            }
        }
        private void ViewValorantFolder(String ValorantFolderPath)
        {
            string currentDirectoryPath = ValorantFolderPath;
            if (Directory.Exists(currentDirectoryPath))
            {
                folders.Clear();
                foreach (var d in System.IO.Directory.GetDirectories(ValorantFolderPath))
                {
                    var dir = new DirectoryInfo(d);
                    var dirName = dir.Name;
                    String XMLTCStatusFilePath = ValorantFolderPath + dirName + @"\run_results.xml";
                    checkTCStatus(XMLTCStatusFilePath, dirName);
                    folders.Add(new Folder() { FolderName = dirName, FolderPath = TCsFolderPath + dirName, TCStatus = str_TCStatus_in_Label, TCStatusColor = str_TCStatusColor });

                }
            }
            else
            {
                MessageBox.Show("No TCs are Found!", "TCs Folder", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public class Folder
        {
            public string FolderName { get; set; } = string.Empty;
            public string FolderPath { get; set; } = string.Empty;
            public string TCStatus { get; set; } = string.Empty;
            public string TCStatusColor { get; set; } = string.Empty;
            public bool Folder_bool_ReportAvailability { get; set; }
        }


        private void btnViewList_Click(object sender, RoutedEventArgs e)
        {
            
            //ViewValorantFolder(@"C:\Riot Games\");
            if (listviewTCs.ItemsSource == null)
            {
                //ViewValorantFolder(@"C:\Riot Games\");
                ViewTestCasesfolders(TCsFolderPath);
                listviewTCs.ItemsSource = folders;
                
            }
            else
            {
                listviewTCs.ItemsSource = null;
                //ViewValorantFolder(@"C:\Riot Games\");
                ViewTestCasesfolders(TCsFolderPath);
                listviewTCs.ItemsSource = folders;
            }
            
        }
       /* private void btnFrontalTC_Click(object sender, RoutedEventArgs e)
        { 
            String command = @"UFTBatchRunnerCMD.exe -visible N -source " + '"' + selectedItem + '"';
            StartProcess(command);
        }*/
       
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listviewTCs.ItemsSource != null)
            {
                Folder file = (Folder)listviewTCs.SelectedItems[0];
                selectedItem = file.FolderPath;
                btnReportViewer.IsEnabled= file.Folder_bool_ReportAvailability;
            }                
            
            //MessageBox.Show(file.FolderPath);
        }

        private void BTN_Click(object sender, RoutedEventArgs e)
        {
            BTN.IsEnabled = false;
            btnStop.IsEnabled= true;
            worker.RunWorkerAsync();

            for (int i = 0; i < 100; i++)
            {
                if (workCompleted)
                {
                    for (int j = i; j<100; j++)
                    {
                        System.Threading.Thread.Sleep(50);
                        pb1.Dispatcher.Invoke(() => pb1.Value = 100, DispatcherPriority.Background); Thread.Sleep(50);
                    }
                    break;
                }
                System.Threading.Thread.Sleep(800);
                mydelegate deli = new mydelegate(displayi);
                pb1.Dispatcher.Invoke(() => pb1.Value = i, DispatcherPriority.Background); Thread.Sleep(50);
                //pb1.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, deli, i);
            }

        }
            private static void StartProcess(String command)
        {

            Process process= new Process();
            process.StartInfo.FileName= "cmd.exe";
            process.StartInfo.CreateNoWindow= true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            //process.StartInfo.UseShellExecute= false;
            process.Start();
            process.StandardInput.WriteLine(@"cd C:\Windows\system32");
            process.StandardInput.WriteLine(command);
            process.StandardInput.Flush();
            process.StandardInput.Close();
            process.WaitForExit();
            //MessageBox.Show(process.StandardOutput.ReadToEnd());
            

        }
        private IntPtr _windowHandle;
        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            _windowHandle = new WindowInteropHelper(this).Handle;

            //disable maximize button
            DisableMaximizeButton();
        }

        protected void DisableMaximizeButton()
        {
            if (_windowHandle == IntPtr.Zero)
                throw new InvalidOperationException("The window has not yet been completely initialized");

            SetWindowLong(_windowHandle, GWL_STYLE, GetWindowLong(_windowHandle, GWL_STYLE) & ~WS_MAXIMIZEBOX);
        }

        private void btnReportViewer_Click(object sender, RoutedEventArgs e)
        {
            String reportFileName = selectedItem + @"\Report\Report\run_results.html";
            pb1.Dispatcher.Invoke(() => pb1.Value = 0, DispatcherPriority.Background);
            if (File.Exists(reportFileName))
            {
                Process.Start(new ProcessStartInfo { FileName = reportFileName, UseShellExecute = true });
            }
            else
            {
                MessageBox.Show("Report is NOT found!", "TC Report", MessageBoxButton.OK, MessageBoxImage.Information);
            }

        }
        public void checkTCStatus(String XMLTCStatusFilePath, String TCName)
        {
            String XmlFilePath = XMLTCStatusFilePath+@".xml";
            String HTMLFilePath = XMLTCStatusFilePath+@".html";
            if (File.Exists(XmlFilePath) && File.Exists(HTMLFilePath))
            {
                bool_ReportAvailability = true;
                XmlDataDocument xmldoc = new XmlDataDocument();
                XmlNodeList xmlnodelist;
                FileStream fs = new FileStream(XmlFilePath, FileMode.Open, FileAccess.Read);
                xmldoc.Load(fs);
                xmlnodelist = xmldoc.GetElementsByTagName("Data");
                for (int i = 0; i <= xmlnodelist.Count - 1; i++)
                {
                    if (xmlnodelist[i].ChildNodes[0].InnerText == TCName)
                    {
                        str_TCStatus = xmlnodelist[i].ChildNodes.Item(7).InnerText.Trim();
                        //MessageBox.Show(str_TCStatus);
                        if (str_TCStatus == "Passed")
                        {
                            bool_TCStatus_Passed = true;
                            str_TCStatus_in_Label = "Passed";
                            str_TCStatusColor = "Green";
                    

                        }
                        else if (str_TCStatus == "Failed")
                        {
                            bool_TCStatus_Passed = false;
                            str_TCStatus_in_Label = "Failed";
                            str_TCStatusColor = "DarkRed";
                        }
                        break;
                    }
                    else
                    {
                        str_TCStatus_in_Label = "NotFound";
                    }
                }
            }
            else
            {
                boolTCStatus_Unknown = true;
                str_TCStatus_in_Label = "No Run";
                str_TCStatusColor = "Black";
                bool_ReportAvailability = false;
                // TC status is unknown
            }

        }
        private void UpdateList()
        {
            //ViewValorantFolder(@"C:\Riot Games\");
            if (listviewTCs.ItemsSource == null)
            {
                //ViewValorantFolder(@"C:\Riot Games\");
                ViewTestCasesfolders(TCsFolderPath);
                listviewTCs.ItemsSource = folders;

            }
            else
            {
                listviewTCs.ItemsSource = null;
                //ViewValorantFolder(@"C:\Riot Games\");
                ViewTestCasesfolders(TCsFolderPath);
                listviewTCs.ItemsSource = folders;
            }
        }
       

        private void kill_UFT()
        {
            foreach (var process in Process.GetProcessesByName("UFT"))
            {
                process.Kill();
            }
           //Process.GetProcessesByName("UFT.exe").kill
        }

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            worker.WorkerSupportsCancellation = true;
            worker.CancelAsync();
        }
    }
}

