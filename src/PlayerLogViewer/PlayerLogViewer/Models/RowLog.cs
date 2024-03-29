﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerLogViewer.Models
{
    internal class RowLog
    {
        public RowLog()
        { }
        public RowLog(int id) : this()
        {
            Id = id;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsMarkedRow { get; set; }
        public bool IsError { get; set; }
        public bool IsCriticalError { get; set; }
        public ObservableCollection<RowLowRowsData> Rows { get; set; } = new ObservableCollection<RowLowRowsData>();

        public void Save()
        {
            Name = string.Empty;

            for (int i = 0; i < Rows.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(Rows[i].Header))
                {
                    Rows.RemoveAt(i);
                    i--;
                }
                else
                    break;
            }

            if (Rows.Count > 0)
                SetNewName(Rows[0].Header);
        }

        private void SetNewName(string firstRow)
        {
            if (firstRow.StartsWith("[Game Connection]"))
            {
                if (Contains(firstRow, "Keepalive", "destroyed"))
                    Name = "Клиент закрыт";
                else if (Contains(firstRow, "Main", "destroyed"))
                    Name = "Подключение разорвано";
            }
            else if (firstRow.StartsWith("[Sentry]"))
            {
                if (Contains(firstRow, "Shutdown complete"))
                    Name = "Метаклиент закрыт";
            }
            else if (firstRow.StartsWith("Request "))
            {
                Name = "Запрос к серверу";
            }
            else if (firstRow.StartsWith("[Job "))
            {
                Name = "Выполнение задания";
            }
            else if (firstRow.StartsWith("NullReferenceException:")
                || firstRow.StartsWith("KeyNotFoundException:")
                || firstRow.StartsWith("ArgumentNullException:"))
            {
                Name = "Критическая ошибка";

                IsCriticalError = true;
                IsMarkedRow = true;
            }
            else if (Contains(firstRow, "Failed")
                || Contains(firstRow, "is missing")
                || Contains(firstRow, "Error")
                || Contains(firstRow, "not found")
                || Contains(firstRow, "not be found")
                )
            {
                Name = "Ошибка";

                if (Contains(firstRow, "script", "is missing"))
                    Name += ". Не найден скрипт";

                IsError = true;
                IsMarkedRow = true;
            }
            else if (firstRow.StartsWith("[Vivox Manager]")
                || firstRow.StartsWith("[Vivox Channel]"))
            {
                if (Contains(firstRow, "Destroy complete"))
                    Name = "Vivox закрыт";
                else if (Contains(firstRow, "Disconnecting"))
                    Name = "Vivox разрыв соединения";
                else if (Contains(firstRow, "Destroying"))
                    Name = "Vivox попытка закрытия";
                else if (Contains(firstRow, "Waiting for login"))
                    Name = "Vivox попытка авторизации";
                else if (Contains(firstRow, "Refreshing", "audio", "output"))
                    Name = "Vivox обновление списка устройств вывода";
                else if (Contains(firstRow, "Audio", "input"))
                    Name = "Vivox обновление списка устройств ввода";
                else if (Contains(firstRow, "Refreshing", "complete"))
                    Name = "Vivox обновление завершено";

                else if (Contains(firstRow, "party", "disconnecting"))
                    Name = "Vivox попытка отключения от группового чата";
                else if (Contains(firstRow, "party", "disconnected"))
                    Name = "Vivox групповой чат отключен";
                else if (Contains(firstRow, "party", "connection"))
                    Name = "Vivox коммандный (игровой) чат подключен";

                else if (Contains(firstRow, "team-", "disconnecting"))
                    Name = "Vivox попытка отключения от коммандного (игрового) чата";
                else if (Contains(firstRow, "team-", "disconnected"))
                    Name = "Vivox коммандный (игровой) чат отключен";
                else if (Contains(firstRow, "team-", "connecting"))
                    Name = "Vivox попытка подключения к коммандному (игровому) чату";
                else if (Contains(firstRow, "team-", "connected"))
                    Name = "Vivox коммандный (игровой) чат подключен";

                else if (Contains(firstRow, "Emtpy session", "connection"))
                    Name = "Vivox обнуление сессии";
                else if (Contains(firstRow, "Channel", "finalization"))
                    Name = "Канал связи разорван";
            }
            else if (firstRow.StartsWith("[Vivox]"))
            {
                if (Contains(firstRow, "(ConnectedChannel "))
                    Name = "Vivox. Подключен канал: " + firstRow["[Vivox][log_info] (ConnectedChannel".Length..].Trim(')', '{', '}');
                else if (Contains(firstRow, "(ConnectingChannel"))
                    Name = "Vivox. Попытка подключения к каналу";

                else if (Contains(firstRow, "(DisconnectingAccount )"))
                    Name = "Vivox. Отключение аккаунта";
                else if (Contains(firstRow, "(DisconnectedChannel "))
                    Name = "Vivox. Отключен канал:" + firstRow["[Vivox][log_info] (DisconnectedChannel ".Length..].Trim(')', '{', '}');
                else if (Contains(firstRow, "(DestroyConnector "))
                    Name = "Vivox. Уничтожен коннектор: " + firstRow["[Vivox][log_info] (DestroyConnector ".Length..].Trim(')', '{', '}');
            }
            else if (firstRow.StartsWith("Keepalive request"))
            {
                Name = "Запрос на сохранение к серверу";
            }
            else if (firstRow.StartsWith("Warning"))
            {
                Name = "Предупреждение";
            }
            else if (firstRow.StartsWith("Login:"))
            {
                Name = "Авторизация";
            }
            else if (firstRow.StartsWith("[Game Application]"))
            {
                if (Contains(firstRow, "Setting module LoginModule"))
                    Name = "Инициализация клиента";
            }
            else if (firstRow.StartsWith("[Replay Storage]"))
            {
                if (Contains(firstRow, "Indexing completed"))
                    Name = "Индексирование реплеев";
                else if (Contains(firstRow, "Indexing started"))
                    Name = "Найдено реплеев: " + GetTextBetween(firstRow, "[Replay Storage] Indexing started: ");
            }
            else if (firstRow.StartsWith("Language:"))
            {
                if (Contains(firstRow, "English"))
                    Name = "Смена языка на English";
                else if (Contains(firstRow, "Russian"))
                    Name = "Смена языка на Russian";
                else if (Contains(firstRow, "Unknown"))
                    Name = "Смена языка на Служебный";
            }
            else if (firstRow.StartsWith("[Game Connection]"))
            {
                if (Contains(firstRow, "JWT", "expired", "Refreshing"))
                    Name = "Текстовый чат запрос на обновление устаревшено токена";
            }
            else if (firstRow.StartsWith("[Client Analytics]"))
            {
                Name = "Подключение аналитики";
            }
            else if (firstRow.StartsWith("UnloadTime:"))
            {
                Name = "Выгружены ресурсы: " + firstRow["UnloadTime:".Length..];
            }
            else if (firstRow.StartsWith("Unloading"))
            {
                if (Contains(firstRow, "Serialized files"))
                    Name = "Выгружены файлы: " + GetTextBetween(firstRow, "Unloading ");
                if (Contains(firstRow, "Assets"))
                    Name = "Выгружены ресурсы: " + GetTextBetween(firstRow, "Unloading ");
            }
            else if (firstRow.StartsWith("GC.Collect"))
            {
                Name = "Сборка мусора";
            }
            else if (firstRow.StartsWith("[Bundle Loader]"))
            {
                if (Contains(firstRow, "Bundle", "unloaded"))
                    Name = "Выгрузка текстур";
            }
            else if (firstRow.StartsWith("------------Loading:"))
            {
                Name = "Загрузка карты";
            }
            else if (firstRow.StartsWith("[Game Application]"))
            {
                if (Contains(firstRow, "Setting module"))
                    Name = "Подключен модуль: " + firstRow["[Game Application] Setting module ".Length..];
                else if (Contains(firstRow, "Switching from"))
                    Name = "Переключение с модуля: " + firstRow["[Game Application] Switching from ".Length..];
            }
            else if (firstRow.StartsWith("[Client]"))
            {
                if (Contains(firstRow, "Disconnected"))
                    Name = "Клиент отключен";
                else if (Contains(firstRow, "Connected"))
                    Name = "Клиент подключен";
                else if (Contains(firstRow, "Connecting"))
                    Name = "Подключение клиента к: " + firstRow["[Client] Connecting ".Length..];
            }
            else if (firstRow.StartsWith("Unloading"))
            {
                if (Contains(firstRow, "Assets", "reduce memory usage"))
                    Name = "Выгрузка ресурсов";
            }
            else if (firstRow.StartsWith("Notification PARTYINVITE ignored"))
            {
                if (Contains(firstRow, "Duplicate notification"))
                    Name = "Приглашение в команду. Дубль уведомления";
            }
            else if (Contains(firstRow, "[CDS Manager]"))
            {
                Name = "[CDS Manager]...";
            }
            else if (Contains(firstRow, "state:"))
            {
                bool inBattle = firstRow.Contains("inBattle:False");
                bool isLoggedIn = firstRow.Contains("isLoggerIn:True");

                Name = $"В бою: {inBattle} Залогинен: {isLoggedIn}";
            }
        }

        private string GetTextBetween(string sourse, string start, string end = " ")
        {
            string text = sourse[start.Length..];
            int posSpace = text.IndexOf(end);
            text = text[..posSpace];

            return text;
        }

        private bool Contains(string row, params string[] param)
        {
            bool allContains = true;

            foreach (string item in param)
            {
                if (!row.Contains(item))
                {
                    allContains = false;
                    break;
                }
            }

            return allContains;
        }
    }
}