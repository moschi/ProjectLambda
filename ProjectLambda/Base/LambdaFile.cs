using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static ProjectLambda.Base.LambdaFile;

namespace ProjectLambda.Base
{
    public enum LogLevel
    {
        DEBUG,
        INFO,
        WARNING,
        ERROR,
        EXCEPTION,
        CRITICAL
    }

    public class LambdaFile : ViewModelBase
    {
        public delegate void AddLogEntryDelegate(LogLevel logLevel, string sender, string log);

        public LambdaFile(string sourcePath, string targetPath)
            :this(sourcePath, targetPath, VariaHelper.GetFileSize(sourcePath))
        { }

        public LambdaFile(string sourcePath, string targetPath, double size)
        {
            SourcePath = sourcePath;
            TargetPath = targetPath;
            Size = size;
        }

        private string _SourcePath;
        public string SourcePath
        {
            get { return _SourcePath; }
            set { SetProperty(MethodBase.GetCurrentMethod(), ref _SourcePath, value); }
        }

        private string _TargetPath;
        public string TargetPath
        {
            get { return _TargetPath; }
            set { SetProperty(MethodBase.GetCurrentMethod(), ref _TargetPath, value); }
        }

        private double _Size;
        public double Size
        {
            get { return _Size; }
            set { SetProperty(MethodBase.GetCurrentMethod(), ref _Size, value); }
        }

        public bool IsReadyForCopy()
        {
            return !(TargetPath.IsEmpty() && SourcePath.IsEmpty());
        }
    }

    public enum CopyJobState
    {
        Registered,
        InProgress,
        Failed,
        Finished
    }

    public class LambdaCopyJob : ViewModelBase
    {
        public LambdaCopyJob(LambdaFile file, CopyJobState state)
        {
            File = file;
            State = state;
        }

        private LambdaFile _File;
        public LambdaFile File
        {
            get { return _File; }
            set { SetProperty(MethodBase.GetCurrentMethod(), ref _File, value); }
        }

        private CopyJobState _State;
        public CopyJobState State
        {
            get { return _State; }
            set { SetProperty(MethodBase.GetCurrentMethod(), ref _State, value); }
        }

        private string _UiMessage;
        public string UiMessage
        {
            get { return _UiMessage; }
            set { SetProperty(MethodBase.GetCurrentMethod(), ref _UiMessage, value); }
        }

        private BaseCommand _StartCommand;
        public BaseCommand StartCommand
        {
            get { return _StartCommand; }
            set { SetProperty(MethodBase.GetCurrentMethod(), ref _StartCommand, value); }
        }

        public bool CanStart()
        {
            return State == CopyJobState.Registered && File.IsReadyForCopy();
        }

        public void Start(AddLogEntryDelegate addLogEntryDelegate)
        {
            State = CopyJobState.InProgress;
            try
            {
                addLogEntryDelegate(LogLevel.INFO, "CopyJob", $"Starting to copy file from {File.SourcePath} to {File.TargetPath}");
                if(!System.IO.File.Exists(File.SourcePath))
                {
                    addLogEntryDelegate(LogLevel.ERROR, $"File with sourcepath {File.SourcePath}", "File not found!");
                    UiMessage = "Skipped";
                    State = CopyJobState.Failed;
                    return;
                }
                if (System.IO.File.Exists(File.TargetPath))
                {
                    addLogEntryDelegate(LogLevel.WARNING, $"File with sourcepath {File.SourcePath}", $"Target path {File.TargetPath} already exists!");
                    UiMessage = "Skipped";
                    State = CopyJobState.Finished;
                    return;
                }

                if(!Directory.Exists(Path.GetDirectoryName(File.TargetPath)))
                {
                    addLogEntryDelegate(LogLevel.INFO, $"File with sourcepath {File.SourcePath}", $"Target directory {Path.GetDirectoryName(File.TargetPath)} did not exist and was created.");
                    Directory.CreateDirectory(Path.GetDirectoryName(File.TargetPath));
                }

                
                System.IO.File.Copy(File.SourcePath, File.TargetPath);

                State = CopyJobState.Finished;
                addLogEntryDelegate(LogLevel.INFO, "CopyJob", $"Finished copying file from {File.SourcePath} to {File.TargetPath}");
            }
            catch(Exception ex)
            {
                addLogEntryDelegate(LogLevel.EXCEPTION, $"File with sourcepath {File.SourcePath}", ex.Message);
                addLogEntryDelegate(LogLevel.WARNING, $"File with sourcepath {File.SourcePath}", "File was not copied!");
                State = CopyJobState.Failed;
            }
        }
    }
}
