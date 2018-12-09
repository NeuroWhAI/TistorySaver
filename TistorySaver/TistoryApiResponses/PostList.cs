using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TistorySaver.TistoryApiResponses
{
    public class PostList
    {
        public string Url { get; set; }
        public int Page { get; set; }
        public int Count { get; set; }
        public int TotalCount { get; set; }
        public List<PostListData> Posts { get; set; }
    }
}
