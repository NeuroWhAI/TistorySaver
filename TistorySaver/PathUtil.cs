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
        public static string SafePath(string path, char replace = '_')
        {
            foreach (char ch in Path.GetInvalidPathChars())
            {
                path = path.Replace(ch, replace);
            }

            return path;
        }

        public static string SafeName(string filename, char replace = '_')
        {
            foreach (char ch in Path.GetInvalidFileNameChars())
            {
                filename = filename.Replace(ch, replace);
            }

            return filename;
        }
    }
}
