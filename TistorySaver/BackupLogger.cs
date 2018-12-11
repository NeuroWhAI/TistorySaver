using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TistorySaver
{
    public class BackupLogger
    {
        public BackupLogger()
        {

        }

        private readonly object m_syncObj = new object();
        private Dictionary<string, List<string>> m_log = new Dictionary<string, List<string>>();

        public void Add(string postId, string message)
        {
            List<string> que = GetPostMessages(postId);

            lock (m_syncObj)
            {
                que.Add(message);
            }
        }

        public bool IsEmpty => m_log.Count == 0;

        public IEnumerable<string> EnumeratePost()
        {
            return m_log.Keys.AsEnumerable();
        }

        public IEnumerable<string> EnumerateMessages(string postId)
        {
            List<string> que = GetPostMessages(postId);

            return que.AsEnumerable();
        }

        private List<string> GetPostMessages(string postId)
        {
            lock (m_syncObj)
            {
                if (m_log.ContainsKey(postId))
                {
                    return m_log[postId];
                }
                else
                {
                    var que = new List<string>();
                    m_log.Add(postId, que);

                    return que;
                }
            }
        }
    }
}
