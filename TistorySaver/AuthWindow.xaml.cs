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

namespace TistorySaver
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AuthWindow : Window
    {
        public AuthWindow()
        {
            InitializeComponent();


            var vm = this.DataContext as AuthWindowVM;
            vm.WhenTokenReceived += (token) => this.Dispatcher.Invoke(() => Vm_WhenTokenReceived(token));
        }

        private void Vm_WhenTokenReceived(string token)
        {
            var bakWin = new BackupWindow(token);
            bakWin.Show();

            Application.Current.MainWindow = bakWin;

            this.Close();
        }
    }
}
