using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace LogAnalyzer
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        static Func<List<string>, List<string>, bool> cbThread;
        DataTable dtContents = new DataTable();
        Thread thread;
        public MainWindow()
        {
            InitializeComponent();
            cbThread = cb_MergeDone;
            dtContents.Columns.Add("No");
            dtContents.Columns.Add("Contents");
            dtContents.Columns.Add("File");

            dg_Contents.ItemsSource = dtContents.DefaultView;
        }

        private void bn_OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog fdlg = new CommonOpenFileDialog();
            fdlg.IsFolderPicker = true;

            string strExtensions = "*.txt";
            if (cbCSV.IsChecked == true)
                strExtensions = "*.csv";
            if(fdlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                listFile.Items.Clear();
                string strFolder = fdlg.FileName;
                tbPath.Text = strFolder;
                string[] files = Directory.GetFiles(strFolder, strExtensions, SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    listFile.Items.Add(file);
                }
                thread = new Thread(new ParameterizedThreadStart(Th_DataMerge));
                thread.Start(files);
            }
        }

        private bool cb_MergeDone(List<string> mergedata, List<string> files)
        {
            bool bRet = false;
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate(){
                dtContents.Rows.Clear();
                for (int i = 0; i < mergedata.Count; i++)
                {
                    dtContents.Rows.Add(i, mergedata[i], files[i]);
                }
            }));
            return bRet;
        }

        static private void Th_DataMerge(object obj)
        {
            string[] list = obj as string[];
            List<string> data = new List<string>();
            List<string> file = new List<string>();
            for (int i = 0; i < list.Length; i++)
            {
                var Reader = File.OpenText(list[i]);
                while (!Reader.EndOfStream)
                {
                    data.Add(Reader.ReadLine());
                    file.Add(list[i]);
                }
            }

            cbThread?.Invoke(data, file);
        }

        private void TbSearchString_TextChanged(object sender, TextChangedEventArgs e)
        {
            string strSearchFilter = tbSearchString.Text;

            try
            {
                if (strSearchFilter == "")
                    dtContents.DefaultView.RowFilter = string.Empty;
                else
                    dtContents.DefaultView.RowFilter = $"Contents like '%{strSearchFilter}%'";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void ListFile_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBox lb = sender as ListBox;
            if(lb != null)
            {
                string strFile = lb.SelectedItem as string;
                try
                {
                    Process.Start(strFile);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                
            }
        }
    }
}
