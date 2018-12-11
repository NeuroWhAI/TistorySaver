using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TistorySaver
{
    public class LogWinService : ILogWinService
    {
        public void ShowLogDialog(LogWindowVM vm)
        {
            var win = new LogWindow()
            {
                DataContext = vm
            };

            win.ShowDialog();
        }
    }
}
