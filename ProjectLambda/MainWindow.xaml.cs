using MahApps.Metro.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using ProjectLambda.Base;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace ProjectLambda
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            //DataContext = new MainWindowViewModel();
        }
    }

    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            Log = new ObservableCollection<string>();
            ReadExcelfileCommand = new BaseCommand((p) => CanReadExcelfile(), (p) => ReadExcelfile());
            CopyFilesCommand = new BaseCommand((p) => CanCopyFiles(), (p) => CopyFiles());

            CurrentJobNumber = 0;
            PercentageDone = -1;

            CurrentTask = "Welcome";
        }

        private string _CurrentTask;
        public string CurrentTask
        {
            get { return _CurrentTask; }
            set { SetProperty(MethodBase.GetCurrentMethod(), ref _CurrentTask, value); }
        }

        private string _ExcelFilepath;
        public string ExcelFilepath
        {
            get { return _ExcelFilepath; }
            set { SetProperty(MethodBase.GetCurrentMethod(), ref _ExcelFilepath, value); }
        }

        private BaseCommand _ReadExcelfileCommand;
        public BaseCommand ReadExcelfileCommand
        {
            get { return _ReadExcelfileCommand; }
            set { SetProperty(MethodBase.GetCurrentMethod(), ref _ReadExcelfileCommand, value); }
        }

        private ObservableCollection<LambdaFile> _Files;
        public ObservableCollection<LambdaFile> Files
        {
            get { return _Files; }
            set { SetProperty(MethodBase.GetCurrentMethod(), ref _Files, value); }
        }

        public async void ReadExcelfile()
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                // this is a rather ugly hack, should and could be done better
                if (dialog.ShowDialog().ToString() == "Ok")
                {
                    ExcelFilepath = dialog.FileName;
                }
            }

            if (!File.Exists(ExcelFilepath))
            {
                MessageBox.Show("The selected file doesn't exist");
                return;
            }

            CurrentTask = "Reading Excel file";
            Files = await Task.Run(() => VariaHelper.ReadExcelFile(ExcelFilepath, AddLog));
            CopyJobs = new ObservableCollection<LambdaCopyJob>();
            foreach (var file in Files)
            {
                CopyJobs.Add(new LambdaCopyJob(file, CopyJobState.Registered));
            }
            CurrentTask = "Ready";
        }

        public bool CanReadExcelfile()
        {
            return true;
        }


        private ObservableCollection<LambdaCopyJob> _CopyJobs;
        public ObservableCollection<LambdaCopyJob> CopyJobs
        {
            get { return _CopyJobs; }
            set { SetProperty(MethodBase.GetCurrentMethod(), ref _CopyJobs, value); }
        }

        private BaseCommand _CopyFilesCommand;
        public BaseCommand CopyFilesCommand
        {
            get { return _CopyFilesCommand; }
            set { SetProperty(MethodBase.GetCurrentMethod(), ref _CopyFilesCommand, value); }
        }

        private int _CurrentJobNumber;
        public int CurrentJobNumber
        {
            get { return _CurrentJobNumber; }
            set
            {
                SetProperty(MethodBase.GetCurrentMethod(), ref _CurrentJobNumber, value);
                if (CopyJobs != null && CopyJobs.Count > 0)
                {
                    PercentageDone = ((double)CurrentJobNumber / (double)CopyJobs.Count) * 100;
                }
                else
                {
                    PercentageDone = -1;
                }
            }
        }

        private double _PercentageDone;
        public double PercentageDone
        {
            get { return _PercentageDone; }
            set { SetProperty(MethodBase.GetCurrentMethod(), ref _PercentageDone, value); }
        }

        private bool _CopyJobsRunning;
        public bool CopyJobsRunning
        {
            get { return _CopyJobsRunning; }
            set { SetProperty(MethodBase.GetCurrentMethod(), ref _CopyJobsRunning, value); }
        }

        public async void CopyFiles()
        {
            AddLog(LogLevel.INFO, $"FileCopy", $"Starting to copy {CopyJobs.Count} files");
            CurrentJobNumber = 0;
            CurrentTask = "Copying files";
            CopyJobsRunning = true;

            foreach (LambdaCopyJob copyJob in CopyJobs)
            {
                if (!copyJob.CanStart())
                {
                    AddLog(LogLevel.WARNING, "CopyFiles", $"Cannot copy file with sourcepath {copyJob.File.SourcePath} either because it was already copied or either the sourcepath or the targetpath are not valid");
                    continue;
                }
                await Task.Run(() => copyJob.Start(AddLog));
                CurrentJobNumber++;
            }

            AddLog(LogLevel.INFO, $"FileCopy", $"Finished copying files");
            CurrentTask = "Ready";
            CopyJobsRunning = false;
        }

        public bool CanCopyFiles()
        {
            return !CopyJobsRunning && CopyJobs != null && CopyJobs.Count > 0;
        }

        private ObservableCollection<string> _Log;
        public ObservableCollection<string> Log
        {
            get { return _Log; }
            set { SetProperty(MethodBase.GetCurrentMethod(), ref _Log, value); }
        }

        private void AddLog(LogLevel logLevel, string sender, string log)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                string logValue = $"[{DateTime.Now.ToLongTimeString()}][{logLevel}]\t[{sender}]\t{log}";
                Log.Add(logValue);
            });
        }
    }
}
