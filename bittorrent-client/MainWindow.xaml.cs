﻿using bittorrent_client.BO;
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
using torrent_library;

namespace bittorrent_client
{
    /// <summary>
    /// MainWindow.xaml etkileşim mantığı
    /// </summary>
    public partial class MainWindow : Window
    {
        private SqTorrentContext Context;

        private Window DownloadWindow { get; set; }


        public MainWindow()
        {
            InitializeComponent();

            log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo("log4.config"));
            Loaded += MainWindow_Loaded;
            Context = SqTorrentContext.GetInstance();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new System.Uri("DownloadWindow.xaml",
         UriKind.RelativeOrAbsolute));

        }

        private void ShowAddMagnetPopup(object sender, EventArgs e)
        {
            AddMagnetPopup.IsOpen = true;
        }

        private void BtnAddMangetOK_Click(object sender, RoutedEventArgs e)
        {
            TorrentProcessor processor = new TorrentProcessor();
            processor.StartProcess(TextBoxMagnet.Text);
            Context.AddProcessor(processor);
            AddMagnetPopup.IsOpen = false;
        }

        private void BtnAddMagnetCancel_Click(object sender, RoutedEventArgs e)
        {
            AddMagnetPopup.IsOpen = false;
        }
    }
}
