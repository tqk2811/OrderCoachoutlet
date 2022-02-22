using OrderCoachoutlet.Helpers;
using OrderCoachoutlet.Queues;
using OrderCoachoutlet.UI.ViewModels;
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
using TqkLibrary.Queues.TaskQueues;
using OrderCoachoutlet.DataClass;
using System.IO;
using Microsoft.Win32;
using System.Threading;

namespace OrderCoachoutlet.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly MainWVM mainWVM;
        readonly TaskQueue<WorkQueue> WorkQueues = new TaskQueue<WorkQueue>();
        public MainWindow()
        {
            ThreadPool.SetMaxThreads(200, 200);
            InitializeComponent();
            this.mainWVM = this.DataContext as MainWVM;
            WorkQueues.OnRunComplete += WorkQueues_OnRunComplete;
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
#if DEBUG
            //Directory.Delete(Singleton.ProfileDir, true);
            //Directory.CreateDirectory(Singleton.ProfileDir);
            //WorkQueues.Add(new WorkQueue(mainWVM.DataManaged, (log) => mainWVM.Logs.Add(log)));
            //WorkQueues.MaxRun = 1;//start run 1 thread
#endif
        }

        private void WorkQueues_OnRunComplete(bool isRequeue)
        {
            mainWVM.IsRunning = false;
        }




        private void btn_loadProxy_Click(object sender, RoutedEventArgs e)
        {
            string file = GetFile();
            if (!string.IsNullOrWhiteSpace(file))
                mainWVM.DataManaged.LoadProxy(file);
        }

        private void btn_loadName_Click(object sender, RoutedEventArgs e)
        {
            string file = GetFile();
            if (!string.IsNullOrWhiteSpace(file))
                mainWVM.DataManaged.LoadName(file);
        }

        private void btn_loadAddress_Click(object sender, RoutedEventArgs e)
        {
            string file = GetFile();
            if (!string.IsNullOrWhiteSpace(file))
                mainWVM.DataManaged.LoadAddress(file);
        }

        private void btn_loadCard_Click(object sender, RoutedEventArgs e)
        {
            string file = GetFile();
            if (!string.IsNullOrWhiteSpace(file))
                mainWVM.DataManaged.LoadCard(file);
        }
        private void btn_loadProduct_Click(object sender, RoutedEventArgs e)
        {
            string file = GetFile();
            if (!string.IsNullOrWhiteSpace(file))
                mainWVM.DataManaged.LoadProduct(file);
        }
        string GetFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
            openFileDialog.Filter = "txt|*.txt|all file|*.*";
            if (openFileDialog.ShowDialog() == true) return openFileDialog.FileName;
            else return null;
        }

        private void btn_start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(!mainWVM.DataManaged.IsAllowRunning)
                {
                    MessageBox.Show("Chưa đủ dữ kiện chạy", "Lỗi");
                    return;
                }

                WorkQueues.ShutDown();
                for (int i = 0; i < mainWVM.ThreadCount; i++)
                {
                    WorkQueues.Add(new WorkQueue(mainWVM.DataManaged, (l) => mainWVM.Logs.Add(l)));
                }
                WorkQueues.MaxRun = mainWVM.ThreadCount;
                mainWVM.IsRunning = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().FullName);
            }
        }

        private void btn_stop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WorkQueues.ShutDown();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().FullName);
            }
        }


    }
}
