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

namespace TistorySaver
{
    /// <summary>
    /// BackupWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BackupWindow : Window
    {
        public BackupWindow(string apiToken)
        {
            InitializeComponent();

            Token = apiToken;
        }

        private string Token { get; set; }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as BackupWindowVM;
            await vm.Initialize(Token);

            this.Activate();
        }
    }
}
