using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TistorySaver
{
    public class LogWindowVM : ViewModelBase
    {
        public LogWindowVM()
        {
#if DEBUG
            LogList.Add(new LogItem { Id = "123", Log = "테스트 로그입니다." });
            LogList.Add(new LogItem { Id = "126", Log = "테스트 로그입니다.\n로그 테스트 데이터.\nTEST!" });
            LogList.Add(new LogItem { Id = "142", Log = "테스트 로그입니다." });
#endif
        }

        public class LogItem
        {
            public string Id { get; set; }
            public string Log { get; set; }
        }

        public List<LogItem> LogList { get; set; } = new List<LogItem>();

        public void AddLog(string id, string log)
        {
            LogList.Add(new LogItem { Id = id, Log = log });
            OnPropertyChanged("LogList");
        }
    }
}
