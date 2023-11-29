using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;

namespace Heaven
{
    public partial class heaven : Window
    {

        private string xmlFile = "Assets/version.xml";
        private string _stateLocateVersionXML;
        private string _stateServerVersionXML;
        private int? idProcessApp = null;
        private bool appsStarting = false;
        private DispatcherTimer dispatcherTimer;
        



        public heaven()
        {
            ServerXMLDownload();
            ServerVersionXML();
            InitializeComponent();
            LocateVersionXML();
            TextBlock();
            CheckAppsLaunchTimer();
        }

        private void LocateVersionXML()
        {
            XmlReader reader = XmlReader.Create(xmlFile);
            while (reader.Read())
            {
                if(reader.NodeType == XmlNodeType.Element)
                {
                    if(reader.Name == "Main")
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
            if(client == null || !client.IsBusy)
            {
                client = new WebClient();
                client.DownloadFileCompleted += CompleteDownloadVersionXMLServer;
                client.DownloadFileAsync(new Uri("https://raw.githubusercontent.com/VsevolodYatsuk/Launcher/main/versionServer.xml"), "Assets/versionServer.xml");
            }
            if(client != null)
            {
                MessageBox.Show("Debug");
            }
        }

        private void CompleteDownloadVersionXMLServer(object sender, AsyncCompletedEventArgs e)
        {
            if(e.Error !=null || e.Cancelled)
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
            if(_stateLocateVersionXML == _stateServerVersionXML)
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


            ProcessStartInfo procInfoLauchGame = new ProcessStartInfo();
            procInfoLauchGame.FileName = @"Game\Battle in the dungeon.exe";
            Process processApp = new Process();
            processApp.StartInfo = procInfoLauchGame;
            processApp.Start();
            idProcessApp = processApp.Id;



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


    } 

}
