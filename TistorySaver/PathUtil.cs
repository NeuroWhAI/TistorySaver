using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TistorySaver
{
    public static class PathUtil
    {
        private static char[] InvalidChars = Path.GetInvalidPathChars()
            .Concat(Path.GetInvalidFileNameChars())
            .ToArray();

        public static string SafePath(string path, string replace = "_")
        {
            return string.Join(replace, path.Split(InvalidChars));
        }
    }
}
