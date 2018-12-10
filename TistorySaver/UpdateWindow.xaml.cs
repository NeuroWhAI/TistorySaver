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
using System.Windows.Shapes;
using System.Diagnostics;

namespace TistorySaver
{
    /// <summary>
    /// UpdateWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UpdateWindow : Window
    {
        public UpdateWindow()
        {
            InitializeComponent();
        }

        private void Button_Yes_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://neurowhai.tistory.com/297");
            this.Close();
        }

        private void Button_No_Click(object sender, RoutedEventArgs e)
        {
            var win = new AuthWindow();
            Application.Current.MainWindow = win;
            win.Show();
            this.Close();
        }
    }
}
