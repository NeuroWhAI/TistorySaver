using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TistorySaver.TistoryApiResponses
{
    public class CategoryListData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Parent { get; set; }
        public string Label { get; set; }
        public int Entries { get; set; }
    }
}
