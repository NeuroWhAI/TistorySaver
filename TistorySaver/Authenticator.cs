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


            var fragment = ParseUriParameters(rawFragment);


            if (fragment.ContainsKey("state") == false
                || fragment["state"] != AuthState)
            {
                throw new Exception("리디렉션 데이터가 올바르지 않습니다.");
            }

            string code;

            if (fragment.ContainsKey("code"))
            {
                code = fragment["code"];
            }
            else
            {
                throw new Exception("인증 토큰을 찾을 수 없습니다.");
            }


            string rawRes;

            using (var client = new WebClient())
            {
                string sec = "neuaf587d1d9fcd1ac41eef0c9b565ff3519b0b6199802edb8e642293ecbf9c3722791b7792ro";

                var url = new StringBuilder("https://www.tistory.com/oauth/access_token");
                url.Append($"?client_id={ClientId}");
                url.Append($"&client_secret={new string(sec.Skip(3).Reverse().Skip(2).ToArray())}");
                url.Append($"&redirect_uri={CallBack}");
                url.Append($"&code={code}");
                url.Append("&grant_type=authorization_code");

                rawRes = client.DownloadString(url.ToString());
            }

            var res = ParseUriParameters(rawRes);

            if (res.ContainsKey("access_token"))
            {
                return res["access_token"];
            }

            if (res.ContainsKey("error_description"))
            {
                throw new Exception(res["error_description"]);
            }

            if (res.ContainsKey("error"))
            {
                throw new Exception(res["error"]);
            }

            throw new Exception("접근 토큰을 찾을 수 없습니다.");
        }

        private string Login()
        {
            var reqAuthUrl = new StringBuilder("https://www.tistory.com/oauth/authorize");
            reqAuthUrl.Append($"?client_id={ClientId}");
            reqAuthUrl.Append($"&redirect_uri={CallBack}");
            reqAuthUrl.Append("&response_type=code");
            reqAuthUrl.Append($"&state={WebUtility.UrlEncode(AuthState)}");

            return LoginService.ShowLoginDialog(reqAuthUrl.ToString());
        }

        private void LoginService_CheckWhenFragmentReceived(FragmentCheckArgs args)
        {
            if (string.IsNullOrEmpty(args.Fragment) == false)
            {
                args.IsDone = args.Fragment.Contains("code=");
            }
        }

        private Dictionary<string, string> ParseUriParameters(string paramText)
        {
            if (paramText.FirstOrDefault() == '?')
            {
                paramText = paramText.Substring(1);
            }

            var list = paramText.Split('&');
            var dic = new Dictionary<string, string>();

            foreach (string data in list)
            {
                var kv = data.Split('=');

                if (kv.Length != 2)
                {
                    throw new Exception("올바르지 않은 응답입니다.");
                }

                string key = WebUtility.UrlDecode(kv[0]);
                string val = WebUtility.UrlDecode(kv[1]);

                dic[key] = val;
            }

            return dic;
        }
    }
}
