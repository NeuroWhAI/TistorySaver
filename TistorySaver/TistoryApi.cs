using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using TistorySaver.TistoryApiResponses;

namespace TistorySaver
{
    using KeyValueMap = Dictionary<string, string>;

    public class TistoryApi
    {

        public TistoryApi(string token)
        {
            Token = token;
        }

        public string Token { get; set; }

        private async Task<string> ReceiveString(string resource, KeyValueMap parameters = null)
        {
            var urlBuilder = new StringBuilder($"https://www.tistory.com/apis/{resource}?");
            urlBuilder.Append($"access_token={Token}");
            urlBuilder.Append("&output=json");

            if (parameters != null)
            {
                foreach (var kv in parameters)
                {
                    urlBuilder.Append($"&{kv.Key}={kv.Value}");
                }
            }


            var req = WebRequest.CreateHttp(urlBuilder.ToString());
            req.Method = "GET";
            req.Timeout = 30 * 1000;

            try
            {
                using (var res = await req.GetResponseAsync())
                {
                    using (var sr = new StreamReader(res.GetResponseStream()))
                    {
                        return await sr.ReadToEndAsync();
                    }
                }
            }
            catch (WebException e)
            {
                using (var res = e.Response)
                {
                    using (var sr = new StreamReader(res.GetResponseStream()))
                    {
                        return await sr.ReadToEndAsync();
                    }
                }
            }
        }

        private async Task<JObject> ReceiveJson(string resource, KeyValueMap parameters = null)
        {
            return JObject.Parse(await ReceiveString(resource, parameters));
        }

        private async Task<JToken> ReceiveJToken(string resource, KeyValueMap parameters = null)
        {
            var json = await ReceiveJson(resource, parameters);

            var tistory = json["tistory"];
            var status = tistory.Value<string>("status");

            if (status != "200")
            {
                var error = tistory.Value<string>("error_message");
                throw new Exception(string.Format("({0}) {1}", status, error));
            }

            var item = tistory["item"];

            return item;
        }

        private async Task<T> ReceiveObject<T>(string resource, KeyValueMap parameters = null)
        {
            return (await ReceiveJToken(resource, parameters)).ToObject<T>();
        }

        public async Task<BlogInfo> GetBlogInfo()
        {
            return await ReceiveObject<BlogInfo>("blog/info");
        }

        public async Task<PostList> ListPost(string blogName, int page)
        {
            return await ReceiveObject<PostList>("post/list",
                new KeyValueMap
                {
                    { "blogName", blogName },
                    { "page", page.ToString() },
                });
        }

        public async Task<CategoryList> ListCategory(string blogName)
        {
            return await ReceiveObject<CategoryList>("category/list",
                new KeyValueMap
                {
                    { "blogName", blogName },
                });
        }

        public async Task<PostRead> ReadPost(string blogName, string postId)
        {
            return await ReceiveObject<PostRead>("post/read",
                new KeyValueMap
                {
                    { "blogName", blogName },
                    { "postId", postId },
                });
        }
    }
}
