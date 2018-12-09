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
        public Authenticator(string clientId, string clientSecret, string callBack)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
            CallBack = callBack;
        }

        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string CallBack { get; set; }

        public string Token { get; set; }

        private string AuthState { get; set; } = "neurowhai";

        public async Task<string> Authorize()
        {
            OpenAuthPage();


            using (var listener = new HttpListener())
            {
                listener.Prefixes.Add(CallBack + "/");

                listener.Start();


                var ctx = await listener.GetContextAsync();

                var req = ctx.Request;
                var res = ctx.Response;


                res.StatusCode = 200;
                res.ContentType = "text/plain; charset=utf-8";
                res.ContentEncoding = Encoding.UTF8;

                using (var sw = res.OutputStream)
                {
                    var bytes = Encoding.UTF8.GetBytes("Tistory Saver를 확인해주세요.\n이 페이지는 닫으셔도 됩니다.");
                    res.ContentLength64 = bytes.LongLength;
                    await sw.WriteAsync(bytes, 0, bytes.Length);
                }


                var query = req.QueryString;

                if (query.AllKeys.Contains("state") == false
                    || query["state"] != AuthState)
                {
                    throw new Exception("리디렉션 데이터가 올바르지 않습니다.");
                }

                if (query.AllKeys.Contains("error="))
                {
                    throw new Exception(string.Format("({0}) {1}",
                        query["error"],
                        query["error_reason"]));
                }

                if (query.AllKeys.Contains("code"))
                {
                    string code = query["code"];


                    var reqTokenUrl = new StringBuilder("https://www.tistory.com/oauth/access_token");
                    reqTokenUrl.Append($"?client_id={ClientId}");
                    reqTokenUrl.Append($"&client_secret={ClientSecret}");
                    reqTokenUrl.Append($"&redirect_uri={CallBack}");
                    reqTokenUrl.Append($"&code={code}");
                    reqTokenUrl.Append("&grant_type=authorization_code");

                    var reqToken = WebRequest.CreateHttp(reqTokenUrl.ToString());
                    reqToken.Method = "GET";
                    reqToken.Timeout = 30 * 1000;

                    using (var resToken = reqToken.GetResponse())
                    {
                        using (var sr = new StreamReader(resToken.GetResponseStream()))
                        {
                            string token = sr.ReadToEnd();

                            string tokenPrefix = "access_token=";

                            if (token.StartsWith(tokenPrefix))
                            {
                                Token = token.Substring(tokenPrefix.Length);

                                return Token;
                            }
                            else
                            {
                                throw new Exception("토큰 발행에 실패하였습니다.");
                            }
                        }
                    }
                }
            }


            return string.Empty;
        }

        private void OpenAuthPage()
        {
            var reqAuthUrl = new StringBuilder("https://www.tistory.com/oauth/authorize");
            reqAuthUrl.Append($"?client_id={ClientId}");
            reqAuthUrl.Append($"&redirect_uri={CallBack}");
            reqAuthUrl.Append("&response_type=code");
            reqAuthUrl.Append($"&state={AuthState}");

            Process.Start(reqAuthUrl.ToString());
        }
    }
}
