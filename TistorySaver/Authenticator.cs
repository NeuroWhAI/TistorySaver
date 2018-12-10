using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Diagnostics;

namespace TistorySaver
{
    public class Authenticator
    {
        public Authenticator(ILoginWindowService loginService, string clientId, string callBack)
        {
            LoginService = loginService;
            ClientId = clientId;
            CallBack = callBack;

            LoginService.CheckWhenFragmentReceived += LoginService_CheckWhenFragmentReceived;
        }

        public ILoginWindowService LoginService { get; private set; }
        public string ClientId { get; set; }
        public string CallBack { get; set; }

        public string Token { get; set; }

        private string AuthState { get; set; } = "neurowhai";

        public string Authorize()
        {
            string rawFragment = Login();

            if (string.IsNullOrEmpty(rawFragment))
            {
                throw new Exception("올바르지 않은 응답입니다.");
            }


            var fragmentList = rawFragment.Substring(1).Split('&');
            var fragment = new Dictionary<string, string>();

            foreach (string data in fragmentList)
            {
                var kv = data.Split('=');

                if (kv.Length != 2)
                {
                    throw new Exception("올바르지 않은 응답입니다.");
                }

                string key = WebUtility.UrlDecode(kv[0]);
                string val = WebUtility.UrlDecode(kv[1]);

                fragment[key] = val;
            }


            if (fragment.ContainsKey("state") == false
                || fragment["state"] != AuthState)
            {
                throw new Exception("리디렉션 데이터가 올바르지 않습니다.");
            }

            if (fragment.ContainsKey("access_token"))
            {
                return fragment["access_token"];
            }


            throw new Exception("토큰을 찾을 수 없습니다.");
        }

        private string Login()
        {
            var reqAuthUrl = new StringBuilder("https://www.tistory.com/oauth/authorize");
            reqAuthUrl.Append($"?client_id={ClientId}");
            reqAuthUrl.Append($"&redirect_uri={CallBack}");
            reqAuthUrl.Append("&response_type=token");
            reqAuthUrl.Append($"&state={WebUtility.UrlEncode(AuthState)}");

            return LoginService.ShowLoginDialog(reqAuthUrl.ToString());
        }

        private void LoginService_CheckWhenFragmentReceived(FragmentCheckArgs args)
        {
            if (string.IsNullOrEmpty(args.Fragment) == false)
            {
                args.IsDone = args.Fragment.Contains("access_token=");
            }
        }
    }
}
