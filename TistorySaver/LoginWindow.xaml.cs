﻿using System;
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
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Reflection;

namespace TistorySaver
{
    /// <summary>
    /// LoginWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoginWindow : Window, ILoginWindow
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        public event Action<FragmentCheckArgs> CheckWhenFragmentReceived;

        private string m_fragment = null;
        public string LoginFragment => m_fragment;

        void ILoginWindow.ShowDialog()
        {
            this.ShowDialog();
        }

        public void NavigatePage(string url)
        {
            this.WebBox.Navigate(url);
        }

        private void WebBox_Navigated(object sender, NavigationEventArgs e)
        {
            // Set WebBrowser to silent mode.
            dynamic activeX = this.WebBox.GetType().InvokeMember("ActiveXInstance",
                BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, this.WebBox, new object[] { });
            activeX.Silent = true;


            // Check fragment.
            if (CheckWhenFragmentReceived != null)
            {
                var args = new FragmentCheckArgs(e.Uri?.Fragment);
                CheckWhenFragmentReceived(args);

                if (args.IsDone)
                {
                    m_fragment = e.Uri?.Fragment;
                    this.Close();
                }
            }
        }
    }
}
