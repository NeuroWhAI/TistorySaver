using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TistorySaver.TistoryApiResponses
{
    public class PostListData
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string PostUrl { get; set; }
        public string Visibility { get; set; }
        public string CategoryId { get; set; }
        public string Date { get; set; }
    }
}
