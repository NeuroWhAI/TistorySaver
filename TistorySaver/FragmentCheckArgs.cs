using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TistorySaver
{
    public class FragmentCheckArgs
    {
        public FragmentCheckArgs(string fragment)
        {
            Fragment = fragment;
        }

        public string Fragment { get; set; }
        public bool IsDone { get; set; } = false;
    }
}
