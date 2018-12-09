using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using HtmlAgilityPack;

namespace TistorySaver
{
    public class BackupManager
    {
        public BackupManager(string folder, string blogName)
        {
            Root = Path.Combine(folder, SafePath(blogName));
        }

        public string Root { get; set; }

        public Dictionary<string, string> Categories { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> ParentMap { get; set; } = new Dictionary<string, string>();

        public string SignatureFileName { get; set; } = "ts_bak.txt";

        public void AddCategory(string id, string name, string parentId)
        {
            Categories.Add(id, SafePath(name));

            if (string.IsNullOrEmpty(parentId) == false)
            {
                ParentMap[id] = parentId;
            }
        }

        public bool BackupExists(string categoryId, string pageId)
        {
            // 완료 표시 파일이 있으면 백업이 된 것으로 간주.
            string path = GetBackupPath(categoryId, pageId);
            path = Path.Combine(path, SignatureFileName);

            return File.Exists(path);
        }

        public async Task Backup(string categoryId, string pageId, string content)
        {
            Exception latestError = null;


            string path = GetBackupPath(categoryId, pageId);


            // 리소스 다운로드 및 경로 수정.
            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(content);

                await BackupResources(path, "img", doc,
                    "//img", "src", "filename");
                await BackupResources(path, "attachment", doc,
                    "//a[contains(@href,\'tistory.com/attachment/\')]", "href");

                content = doc.DocumentNode.OuterHtml;
            }
            catch (Exception e)
            {
                latestError = e;
            }


            using (var sw = File.CreateText(Path.Combine(path, "content.html")))
            {
                await sw.WriteAsync(content);
            }


            if (latestError != null)
            {
                throw new AggregateException(latestError);
            }


            // 완료 표시 파일 생성.
            path = Path.Combine(path, SignatureFileName);
            File.Create(path).Close();
        }

        /// <summary>
        /// 페이지 내의 리소스들을 다운로드 합니다.
        /// </summary>
        /// <param name="path">저장할 폴더 경로입니다.</param>
        /// <param name="rcType">리소스 종류로서 파일 이름에 접두사로 붙을 수 있습니다.</param>
        /// <param name="doc">검색할 HTML 문서 객체입니다.</param>
        /// <param name="nodePath">XPath에 해당하는 노드를 검색합니다.</param>
        /// <param name="srcAttribute">리소스 경로가 담긴 속성입니다.</param>
        /// <param name="filenameAttribute">리소스 이름이 담긴 속성입니다.</param>
        /// <returns></returns>
        private async Task BackupResources(string path, string rcType,
            HtmlDocument doc, string nodePath, string srcAttribute, string filenameAttribute = "")
        {
            var nodes = doc.DocumentNode.SelectNodes(nodePath);

            if (nodes == null)
            {
                return;
            }

            int srcNum = 1;

            foreach (var node in nodes)
            {
                string src = node.GetAttributeValue(srcAttribute, string.Empty);

                if (string.IsNullOrWhiteSpace(src) == false)
                {
                    string filename = string.Empty;

                    if (string.IsNullOrEmpty(filenameAttribute) == false)
                    {
                        filename = WebUtility.HtmlDecode(node.GetAttributeValue(filenameAttribute, string.Empty));
                    }

                    if (string.IsNullOrEmpty(filename))
                    {
                        var uri = new Uri(src);
                        filename = Path.GetFileName(uri.LocalPath);
                    }

                    while (File.Exists(Path.Combine(path, filename)))
                    {
                        filename = rcType + srcNum;
                        srcNum += 1;
                    }

                    string dest = Path.Combine(path, filename);

                    int maxRetryCount = 5;
                    for (int retry = 0; retry <= maxRetryCount; ++retry)
                    {
                        try
                        {
                            using (var client = new WebClient())
                            {
                                client.DownloadFile(src, dest);
                            }

                            break;
                        }
                        catch (WebException)
                        {
                            if (retry < maxRetryCount)
                            {
                                await Task.Delay(5000);
                            }
                        }
                        catch
                        {
                            break;
                        }
                    }

                    if (File.Exists(dest))
                    {
                        src = string.Format("{0}", filename);

                        node.SetAttributeValue(srcAttribute, src);
                    }
                }
            }
        }

        private string GetBackupPath(string categoryId, string pageId)
        {
            pageId = SafePath(pageId);

            string path;

            if (Categories.ContainsKey(categoryId))
            {
                if (ParentMap.ContainsKey(categoryId))
                {
                    string parentId = ParentMap[categoryId];

                    path = Path.Combine(Root,
                        Path.Combine(Categories[parentId], Categories[categoryId]),
                        pageId);
                }
                else
                {
                    path = Path.Combine(Root, Categories[categoryId], pageId);
                }
            }
            else
            {
                path = Path.Combine(Root, "ts_미분류", pageId);
            }

            if(Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        private string SafePath(string path)
        {
            foreach (char ch in Path.GetInvalidPathChars())
            {
                path = path.Replace(ch, '_');
            }

            return path;
        }
    }
}
