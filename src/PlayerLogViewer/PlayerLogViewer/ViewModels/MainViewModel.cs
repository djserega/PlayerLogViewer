using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace PlayerLogViewer.ViewModels
{
    internal class MainViewModel : BindableBase, ISingleton
    {
        public bool OnlyIsError { get; set; }
        public ICommand OnlyIsErrorChangeCommand { get => new DelegateCommand(() => SetFilterListLogView()); }
        public Models.RowLog SelectedListLog { get; set; }
        public ICollectionView ListLogView { get; set; }
        public ObservableCollection<Models.RowLog> ListLog { get; } = new ObservableCollection<Models.RowLog>();
        public ICommand ReadPlayerLogCommand { get => new AsyncCommand(async () => await LoadPlayerLog()); }
        public ICommand OpenLogFileCommand { get => new DelegateCommand(() => OpenLogFile()); }

        private async Task LoadPlayerLog()
        {
            Logger.Inf("[Command] LoadPlayerLog");

            ListLog.Clear();

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
                    int i = 0;
                    do
                    {
                        line = await reader.ReadLineAsync();
                        i++;

                        if (line != null)
                            readedData.Insert(0, line);
                    }
                    while (line != null);

                    Logger.Inf("Load lines {lines}", readedData.Count);
                }
            }

            Logger.Inf("Processed data");

            Models.RowLog? currentLog = default;
            foreach (string row in readedData)
            {
                if (row.StartsWith("(Filename:"))
                {
                    if (currentLog != null)
                        currentLog.Save();

                    currentLog = new Models.RowLog(ListLog.Count);
                    ListLog.Add(currentLog);
                }
                else if (currentLog != null)
                {
                    currentLog.Rows.Insert(0, row);
                }
            }

            Logger.Inf("Create row {RowLog}", ListLog.Count);

            if (currentLog != null)
                currentLog.Save();

            ListLogView = CollectionViewSource.GetDefaultView(ListLog);

            SetFilterListLogView();
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

            ListLogView.Filter = el => !OnlyIsError || (((Models.RowLog)el).IsError && OnlyIsError);
        }
    }
}
