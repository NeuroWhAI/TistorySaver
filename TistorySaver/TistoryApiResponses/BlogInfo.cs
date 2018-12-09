using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TistorySaver.TistoryApiResponses
{
    public class BlogInfo
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public List<BlogInfoData> Blogs { get; set; }
    }
}
