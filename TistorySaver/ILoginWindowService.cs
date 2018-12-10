using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TistorySaver
{
    public interface ILoginWindowService
    {
        event Action<FragmentCheckArgs> CheckWhenFragmentReceived;

        string ShowLoginDialog(string url);
    }
}
