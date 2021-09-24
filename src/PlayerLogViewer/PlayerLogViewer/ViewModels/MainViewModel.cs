using DevExpress.Mvvm;
using GalaSoft.MvvmLight.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace PlayerLogViewer.ViewModels
{
    internal class MainViewModel : BindableBase, ISingleton
    {
        private int _countSecondTimer;
        private Timer _timerAutoUpdate;

        public MainViewModel()
        {
            _timerAutoUpdate = new Timer();
            _timerAutoUpdate.Elapsed += TimerAutoUpdate_Elapsed;

            SetTimerInterval();

            LogfilePath = GetFileLogPath();
        }

        private void TimerAutoUpdate_Elapsed(object sender, ElapsedEventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(
                  async () =>
                  {
                      await LoadPlayerLog();
                  });
        }

        public bool OnlyIsError { get; set; }
        public ICommand OnlyIsErrorChangeCommand { get => new DelegateCommand(() => SetFilterListLogView()); }
        public bool OnlyIsCriticalError { get; set; }
        public Models.RowLog SelectedListLog { get; set; }
        public ICollectionView ListLogView { get; set; }
        public ObservableCollection<Models.RowLog> ListLog { get; } = new ObservableCollection<Models.RowLog>();
        public ICommand ReadPlayerLogCommand { get => new AsyncCommand(async () => await LoadPlayerLog()); }
        public ICommand OpenLogFileCommand { get => new DelegateCommand(() => OpenLogFile()); }
        public int CountSecondTimer
        {
            get => _countSecondTimer;
            set
            {
                if (value >= 0)
                {
                    _countSecondTimer = value;
                    SetTimerInterval();
                }
            }
        }
        public bool TimerIsActive { get; set; }
        public Visibility VisibilityProgressLoading { get; set; } = Visibility.Collapsed;
        public string LogfilePath { get; set; }

        private void SetTimerInterval()
        {
            TimerIsActive = false;
            _timerAutoUpdate.Stop();

            if (CountSecondTimer > 0)
            {
                _timerAutoUpdate.Interval = CountSecondTimer * 1000;
                _timerAutoUpdate.Start();
                TimerIsActive = true;
            }
        }

        private async Task LoadPlayerLog()
        {
            if (TimerIsActive)
                _timerAutoUpdate.Stop();

            Logger.Inf("[Command] LoadPlayerLog");

            VisibilityProgressLoading = Visibility.Visible;

            string? path = GetFileLogPath();

            if (string.IsNullOrEmpty(path))
                return;

            Logger.Inf("Reading file");

            List<string> readedData = new();

            using (FileStream fs = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader reader = new(fs, Encoding.Default))
                {
                    long maxByte = 10000000;

                    long readingByte;
                    if (reader.BaseStream.Length > maxByte)
                        readingByte = maxByte;
                    else
                        readingByte = reader.BaseStream.Length;

                    Logger.Inf("Reading byte {bytes}", readingByte);

                    _ = reader.BaseStream.Seek(-readingByte, SeekOrigin.End);

                    string? line;
                    do
                    {
                        line = await reader.ReadLineAsync();

                        if (!string.IsNullOrWhiteSpace(line))
                            readedData.Insert(0, line);
                    }
                    while (line != null);

                    Logger.Inf("Load lines {lines}", readedData.Count);
                }
            }

            Logger.Inf("Processed data");

            ListLog.Clear();

            Models.RowLog? currentLog = default;
            foreach (string textRow in readedData)
            {
                if (textRow.StartsWith("(Filename:"))
                {
                    if (currentLog != null)
                        currentLog.Save();

                    currentLog = new Models.RowLog(ListLog.Count);
                    ListLog.Add(currentLog);
                }
                else if (currentLog != null)
                {
                    currentLog.Rows.Insert(0, new Models.RowLowRowsData(textRow));
                }
            }

            Logger.Inf("Create row {RowLog}", ListLog.Count);

            if (currentLog != null)
                currentLog.Save();

            ListLogView = CollectionViewSource.GetDefaultView(ListLog);

            SetFilterListLogView();

            VisibilityProgressLoading = Visibility.Collapsed;

            if (TimerIsActive)
                _timerAutoUpdate.Start();
        }

        private void OpenLogFile()
        {
            Logger.Inf("[Command] OpenLogFile");

            string? path = GetFileLogPath();

            if (string.IsNullOrEmpty(path))
                return;

            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }

        private static string? GetFileLogPath()
        {
            string locallowPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low";

            string path = Path.Combine(
                locallowPath,
                "1CGS",
                "Caliber_s1",
                "Player.log");

            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.Exists)
            {
                Logger.Inf("Finded file {path}", path);
            }
            else
            {
                Logger.Inf("File is not exists {path}", path);
                MessageBox.Show("Файл логов не обнаружен");
                return null;
            }

            return path;
        }

        private void SetFilterListLogView()
        {
            Logger.Inf("Set filter: OnlyIsError - {Error}", OnlyIsError);

            ListLogView.Filter = el => 
                (!OnlyIsError && !OnlyIsCriticalError) 
                || (((Models.RowLog)el).IsError && OnlyIsError)
                || (((Models.RowLog)el).IsCriticalError && OnlyIsCriticalError);
        }
    }
}
