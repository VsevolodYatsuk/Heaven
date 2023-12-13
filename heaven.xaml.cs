using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using System.Xml;
using System.IO.Compression;
using System.Windows.Threading;
using Newtonsoft.Json;
using SharpCompress.Archives;
using SharpCompress.Common;
using System.Net;

namespace Heaven
{
    public partial class heaven : Window
    {
        private void AddGameInfo(string name, string gitHubLink)
        {
            GameInfo gameInfo = new GameInfo
            {
                Name = name,
                GitHubLink = gitHubLink
            };

            gamesInfo.Add(gameInfo);
            SaveGamesInfo();
        }

        private string xmlFile = "Assets/version.xml";
        private string _stateLocateVersionXML;
        private string _stateServerVersionXML;
        private int? idProcessApp = null;
        private bool appsStarting = false;
        private DispatcherTimer dispatcherTimer;
        private string gameExePath;
        private string gameFolderPath;
        private string configFilePath = "config.txt";
        private List<GameInfo> gamesInfo = new List<GameInfo>();

        public heaven()
        {
            InitializeComponent();
            InitializeGamesInfo();
            LoadConfig();
            ServerXMLDownload();
            ServerVersionXML();
            LocateVersionXML();
            TextBlock();
            CheckAppsLaunchTimer();
        }

        private void InitializeGamesInfo()
        {
            gamesInfo.Add(new GameInfo { Name = "Kanava", GitHubLink = "https://github.com/VsevolodYatsuk/game1/raw/main/game.rar" });
            gamesInfo.Add(new GameInfo { Name = "Tetris", GitHubLink = "https://github.com/VsevolodYatsuk/game1/raw/main/Tetris.rar" });
            gamesInfo.Add(new GameInfo { Name = "russ_vs_lizards", GitHubLink = "https://github.com/VsevolodYatsuk/game1/raw/main/russ_vs_lizards.rar" });
            
        }

        private void LoadConfig()
        {
            if (File.Exists(configFilePath))
            {
                gameFolderPath = File.ReadAllText(configFilePath);
            }
        }

        private void SaveConfig()
        {
            File.WriteAllText(configFilePath, gameFolderPath);
        }

        private void LocateVersionXML()
        {
            XmlReader reader = XmlReader.Create(xmlFile);
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "Main")
                    {
                        reader.ReadToFollowing("version");
                        _stateLocateVersionXML = reader.ReadElementContentAsString();
                    }
                }
            }
        }

        private static WebClient client;

        public void ServerXMLDownload()
        {
            if (client == null || !client.IsBusy)
            {
                client = new WebClient();
                client.DownloadFileCompleted += CompleteDownloadVersionXMLServer;
                client.DownloadFileAsync(new Uri("https://raw.githubusercontent.com/VsevolodYatsuk/Launcher/main/versionServer.xml"), "Assets/versionServer.xml");
            }
            if (client != null)
            {
                MessageBox.Show("Debug");
            }
        }

        private void CompleteDownloadVersionXMLServer(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null || e.Cancelled)
            {
                MessageBox.Show("Ошибка скачивании" + e.Error);
            }
            else
            {
                MessageBox.Show("Успешное скачивание");
            }
        }

        public void ServerVersionXML()
        {
            XmlTextReader reader = new XmlTextReader("Assets/versionServer.xml");
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "Main")
                    {
                        reader.ReadToFollowing("version");
                        _stateServerVersionXML = reader.ReadElementContentAsString();
                    }
                }

            }
        }

        private void TextBlock()
        {
            _textCurrentVersion.Text += _stateLocateVersionXML;
            _textServerVersion.Text += _stateServerVersionXML;
        }

        private void ButtionUpdateDialogWindow(object sender, RoutedEventArgs e)
        {
            if (_stateLocateVersionXML == _stateServerVersionXML)
            {
                MessageBox.Show("У вас актуальная версия");
            }
            else
            {
                MessageBox.Show("Новая версия!" + _stateServerVersionXML);
            }

        }

        string str;
        private void ButtonLaunchGame(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(gameExePath) && File.Exists(gameExePath))
            {
                ProcessStartInfo procInfoLauchGame = new ProcessStartInfo();
                procInfoLauchGame.FileName = gameExePath;
                Process processApp = new Process();
                processApp.StartInfo = procInfoLauchGame;
                processApp.Start();
                idProcessApp = processApp.Id;
            }
            else
            {
                MessageBox.Show("Игра не найдена. Сначала скачайте и распакуйте игру.");
            }
        }

        private void CheckAppsLaunchTimer()
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(SingleAppTimerCheckMethod);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 2);
            dispatcherTimer.Start();
        }

        public void SingleAppTimerCheckMethod(object sender, EventArgs ea)
        {
            Process[] processedUsers = Process.GetProcesses();

            foreach (Process allprocessed in processedUsers)
            {
                if (allprocessed.Id == idProcessApp)
                {
                    appsStarting = true;
                    break;
                }
                else if (allprocessed.Id != idProcessApp) appsStarting = false;
            }

            if (appsStarting == false)
            {
                AppState.Text = "Открыть игру";
            }
            else
            {
                AppState.Text = "Игра запущена";
            }
        }

        private async void DowloadGame_Click(object sender, RoutedEventArgs e)
        {
            if (ListBoxGames.SelectedItem == null)
            {
                MessageBox.Show("Выберите игру для загрузки.");
                return;
            }

            string selectedGame = ((ListBoxItem)ListBoxGames.SelectedItem).Name;

            // Проверяем, есть ли информация об игре в базе
            GameInfo selectedGameInfo = gamesInfo.FirstOrDefault(info => info.Name == selectedGame);

            if (selectedGameInfo == null)
            {
                MessageBox.Show($"Информация об игре '{selectedGame}' не найдена.");
                return;
            }

            string zipFilePath = Path.Combine(gameFolderPath, $"{selectedGame}.rar");
            string destinationFolder = gameFolderPath;

            // Проверяем, существует ли уже папка с игрой
            if (Directory.Exists(destinationFolder))
            {
                MessageBox.Show("Игра уже скачана на вашем ПК");
                return;
            }

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Используем async/await для асинхронной загрузки файла
                    byte[] fileData = await client.GetByteArrayAsync(selectedGameInfo.GitHubLink);
                    File.WriteAllBytes(zipFilePath, fileData);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки игры: {ex.Message}");
                    return;
                }
            }

            UnzipFile(zipFilePath, destinationFolder);

            // Получаем путь к исполняемому файлу
            string[] exeFiles = Directory.GetFiles(destinationFolder, "*.exe");
            if (exeFiles.Length > 0)
            {
                gameExePath = exeFiles[0];
            }
            else
            {
                MessageBox.Show("Не найдено исполняемых файлов (*.exe) в указанной папке.");
                return;
            }

            SaveConfig(); // Сохраняем путь к папке игры

            // Добавляем информацию об игре в файл
            AddGameInfo(selectedGame, selectedGameInfo.GitHubLink);

            MessageBox.Show("Download, extraction, and cleanup completed!");
        }

        private void UnzipFile(string zipFilePath, string destinationPath)
        {
            try
            {
                using (var archive = ArchiveFactory.Open(zipFilePath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (!entry.IsDirectory)
                        {
                            string fileName = Path.GetFileName(entry.Key);
                            string destinationFile = Path.Combine(destinationPath, fileName);

                            // Проверяем, что директория для файла создана
                            Directory.CreateDirectory(Path.GetDirectoryName(destinationFile));

                            entry.WriteToDirectory(destinationPath, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error extracting ZIP file: {ex.Message}");
            }
        }

        private void DeleteGame_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(gameFolderPath) && Directory.Exists(gameFolderPath))
            {
                try
                {
                    // Удаляем всю папку с игрой
                    Directory.Delete(gameFolderPath, true);
                    gameFolderPath = null; // Обнуляем путь к папке игры после удаления
                    SaveConfig(); // Обновляем файл конфигурации
                    MessageBox.Show("Игра успешно удалена");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting the game: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Игра не найдена. Сначала скачайте и распакуйте игру.");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ListBoxGames.Visibility == Visibility.Collapsed)
            {
                ListBoxGames.Visibility = Visibility.Visible;
                ToggleListButton.Content = "Скрыть список";
                HeaderText.Text = "Открыт список:";
            }
            else
            {
                ListBoxGames.Visibility = Visibility.Collapsed;
                ToggleListButton.Content = "Отобразить список";
                HeaderText.Text = "";
            }
        }

        private void ListBoxGames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBoxGames.SelectedItem != null)
            {
                string selectedGame = ((ListBoxItem)ListBoxGames.SelectedItem).Name;
                _Game.Text = "Игра: " + selectedGame;

                AppState.Visibility = Visibility.Visible;

                DowloadGame.Visibility = Visibility.Visible;
                DeleteGame.Visibility = Visibility.Visible;
                LaunchGame.Visibility = Visibility.Visible;
            }
        }

        private void SaveGamesInfo()
        {
            string jsonContent = JsonConvert.SerializeObject(gamesInfo, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText("gamesInfo.json", jsonContent);
        }
    }
}
