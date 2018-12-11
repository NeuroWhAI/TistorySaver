using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TistorySaver
{
    public class LoginService : ILoginWindowService
    {
        public event Action<FragmentCheckArgs> CheckWhenFragmentReceived;

        public string ShowLoginDialog(string url)
        {
            var win = new LoginWindow();
            win.CheckWhenFragmentReceived += args =>
            {
                if (CheckWhenFragmentReceived != null)
                {
                    CheckWhenFragmentReceived(args);
                }
            };

            win.NavigatePage(url);

            win.ShowDialog();

            return win.LoginFragment;
        }
    }
}
