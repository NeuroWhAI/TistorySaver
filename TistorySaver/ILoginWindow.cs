using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TistorySaver
{
    public interface ILoginWindow
    {
        event Action<FragmentCheckArgs> CheckWhenFragmentReceived;

        string LoginFragment { get; }

        void ShowDialog();
        void NavigatePage(string url);
    }
}
