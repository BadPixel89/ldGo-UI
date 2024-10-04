using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace ldGoUI
{
    /// <summary>
    /// Interaction logic for HelpPage.xaml
    /// </summary>
    public partial class HelpPage : Window
    {
        public HelpPage()
        {
            InitializeComponent();
        }

        private void btn_npcap_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://npcap.com/#download");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void btn_donate_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://ko-fi.com/dktools");
        }
    }
}
