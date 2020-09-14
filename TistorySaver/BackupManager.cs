﻿using System;
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
            Root = Path.Combine(folder, PathUtil.SafePath(blogName));

            Categories.Add("-2", "ts_공지");
        }

        public BackupLogger Logger { get; set; } = null;

        public string Root { get; set; }

        public Dictionary<string, string> Categories { get; private set; } = new Dictionary<string, string>();
        public Dictionary<string, string> ParentMap { get; private set; } = new Dictionary<string, string>();

        public string SignatureFileName { get; set; } = "ts_bak.txt";

        public void AddCategory(string id, string name, string parentId)
        {
            Categories.Add(id, PathUtil.SafePath(name));

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

        public async Task Backup(string categoryId, string pageId, string title, string content)
        {
            Exception latestError = null;


            string path = GetBackupPath(categoryId, pageId);


            // 폴더 비우기.
            try
            {
                foreach (string file in Directory.EnumerateFiles(path))
                {
                    File.Delete(file);
                }
            }
            catch
            {
                Logger?.Add(pageId, "백업 폴더를 비울 수 없습니다.");
                // 폴더 정리 실패는 백업 실패로 간주하지 않음.
            }


            // 제목 백업
            try
            {
                using (var sw = File.CreateText(Path.Combine(path, "title.txt")))
                {
                    await sw.WriteAsync(title);
                }
            }
            catch
            {
                Logger?.Add(pageId, "제목 백업 파일을 만들 수 없습니다.");
            }


            // 리소스 다운로드 및 경로 수정.
            try
            {
                content = ReplaceImageKage(content);

                var doc = new HtmlDocument();
                doc.LoadHtml(content);

                using (var client = new WebClient())
                {
                    await BackupResources(client, path, pageId, "img", doc,
                        "//img", "src", "filename");
                    await BackupResources(client, path, pageId, "attachment", doc,
                        "//a[contains(@href,\'tistory.com/attachment/\')]", "href");
                }

                content = doc.DocumentNode.OuterHtml;
            }
            catch (Exception e)
            {
                latestError = e;
            }


            try
            {
                using (var sw = File.CreateText(Path.Combine(path, "content.html")))
                {
                    await sw.WriteAsync(content);
                }
            }
            catch
            {
                Logger?.Add(pageId, "글 백업 파일을 만들 수 없습니다.");
                throw; // 백업 실패.
            }


            if (latestError != null)
            {
                throw new AggregateException(latestError);
            }


            // 완료 표시 파일 생성.
            try
            {
                path = Path.Combine(path, SignatureFileName);
                File.Create(path).Close();
            }
            catch
            {
                Logger?.Add(pageId, "백업을 완료할 수 없습니다.");
            }
        }

        private string ReplaceImageKage(string html)
        {
            var buffer = new StringBuilder();

            string prefix = "[##_Image|kage@";
            string suffix = "_##]";

            int offset = 0;

            while (offset < html.Length)
            {
                int begin = html.IndexOf(prefix, offset);

                if (begin < 0)
                {
                    buffer.Append(html.Substring(offset, html.Length - offset));
                    break;
                }

                if (begin > offset)
                {
                    buffer.Append(html.Substring(offset, begin - offset));
                }

                int end = html.IndexOf(suffix, begin);

                if (end < 0)
                {
                    break;
                }

                string kage = html.Substring(begin, end - begin + suffix.Length);
                string[] data = kage.Split('|');

                if (data.Length >= 5)
                {
                    string src = data[1].Substring(data[1].IndexOf('@') + 1);

                    buffer.Append($"<img src=\"https://k.kakaocdn.net/dn/{src}\"");
                    buffer.Append($" {data[3]}");
                    buffer.Append(">");
                }

                offset = end + suffix.Length;
            }


            return buffer.ToString();
        }

        private async Task<string> MakeResourceName(string path, string rcType, string src, string hint)
        {
            string filename = hint;

            if (string.IsNullOrEmpty(filename))
            {
                try
                {
                    var uri = new Uri(src);
                    filename = Path.GetFileName(uri.LocalPath);
                }
                catch
                { }
            }

            string extension = string.Empty;

            if (filename.Contains('.') == false)
            {
                string mime = await GetContentTypeFromUri(src);
                extension = MimeToExtension(mime);

                filename += extension;
            }

            filename = PathUtil.SafePath(filename);

            int srcNum = 1;

            while (File.Exists(Path.Combine(path, filename)))
            {
                filename = rcType + srcNum + extension;
                srcNum += 1;
            }

            return filename;
        }

        private string MimeToExtension(string mime)
        {
            mime = mime.ToLower();

            switch (mime)
            {
                case "image/x-jg": return ".art";
                case "image/bmp": return ".bmp";
                case "image/x-cmx": return ".cmx";
                case "image/cis-cod": return ".cod";
                case "image/gif": return ".gif";
                case "image/x-icon": return ".ico";
                case "image/ief": return ".ief";
                case "image/pjpeg": return ".jfif";
                case "image/jpeg": return ".jpg";
                case "image/jpg": return ".jpg";
                case "image/x-macpaint": return ".mac";
                case "image/x-portable-bitmap": return ".pbm";
                case "image/pict": return ".pct";
                case "image/x-portable-graymap": return ".pgm";
                case "image/png": return ".png";
                case "image/x-portable-anymap": return ".pcm";
                case "image/x-portable-pixmap": return ".ppm";
                case "image/x-quicktime": return ".qti";
                case "image/x-cmu-raster": return ".ras";
                case "image/x-vnd.rn-realflash": return ".rf";
                case "image/x-rgb": return ".rgb";
                case "image/tiff": return ".tif";
                case "image/vnd.wap.wbmp": return ".wbmp";
                case "image/vnd.ms-photo": return ".wdp";
                case "image/x-xbitmap": return ".xbm";
                case "image/x-xpixmap": return ".xpm";
                case "image/x-xwindowdump": return ".xwd";
                case "image/webp": return ".webp";
                case "image/svg+xml": return ".svg";
            }

            return string.Empty;
        }

        private async Task<string> GetContentTypeFromUri(string uri)
        {
            try
            {
                var req = WebRequest.CreateHttp(uri);
                req.Method = "HEAD";

                using (var res = await req.GetResponseAsync())
                {
                    return res.ContentType;
                }
            }
            catch
            { }

            return string.Empty;
        }

        /// <summary>
        /// 페이지 내의 리소스들을 다운로드 합니다.
        /// </summary>
        /// <param name="path">저장할 폴더 경로입니다.</param>
        /// <param name="pageId">게시글 아이디입니다.</param>
        /// <param name="rcType">리소스 종류로서 파일 이름에 접두사로 붙을 수 있습니다.</param>
        /// <param name="doc">검색할 HTML 문서 객체입니다.</param>
        /// <param name="nodePath">XPath에 해당하는 노드를 검색합니다.</param>
        /// <param name="srcAttribute">리소스 경로가 담긴 속성입니다.</param>
        /// <param name="filenameAttribute">리소스 이름이 담긴 속성입니다.</param>
        /// <returns></returns>
        private async Task BackupResources(WebClient client, string path, string pageId, string rcType,
            HtmlDocument doc, string nodePath, string srcAttribute,
            string filenameAttribute = "")
        {
            var nodes = doc.DocumentNode.SelectNodes(nodePath);

            if (nodes == null)
            {
                return;
            }

            foreach (var node in nodes)
            {
                string src = node.GetAttributeValue(srcAttribute, string.Empty);

                if (string.IsNullOrWhiteSpace(src) == false
                    && src.Last() != '/')
                {
                    string filename = string.Empty;

                    if (string.IsNullOrEmpty(filenameAttribute) == false)
                    {
                        filename = WebUtility.HtmlDecode(node.GetAttributeValue(filenameAttribute, string.Empty));
                    }

                    try
                    {
                        filename = await MakeResourceName(path, rcType, src, filename);
                    }
                    catch
                    {
                        Logger?.Add(pageId, string.Format("리소스 이름 부여 실패.\n src: {0}", src));
                        continue;
                    }

                    int kageIndex = src.IndexOf("/kage@");
                    if (kageIndex >= 0)
                    {
                        string imgPath = src.Substring(kageIndex + 6);
                        if (!string.IsNullOrEmpty(imgPath))
                        {
                            src = "https://blog.kakaocdn.net/dn/" + imgPath;

                            int slashIdx = imgPath.IndexOf('/');
                            if (slashIdx > 0)
                            {
                                filename = imgPath.Substring(0, slashIdx) + filename;
                            }
                        }
                    }

                    string dest = Path.Combine(path, filename);

                    int maxRetryCount = 5;
                    for (int retry = 0; retry <= maxRetryCount; ++retry)
                    {
                        try
                        {
                            await client.DownloadFileTaskAsync(src, dest);

                            break;
                        }
                        catch (WebException e)
                        {
                            if (retry < maxRetryCount)
                            {
                                if (e.Response is HttpWebResponse res
                                    && (res.StatusCode == HttpStatusCode.Forbidden
                                    || res.StatusCode == HttpStatusCode.NotFound))
                                {
                                    await Task.Delay(200);
                                }
                                else
                                {
                                    await Task.Delay(5000);
                                }
                            }
                            else
                            {
                                File.Delete(dest);

                                Logger?.Add(pageId, string.Format("리소스 다운로드 실패.\n src: {0}", src));
                            }
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
            pageId = PathUtil.SafePath(pageId);

            string path;

            if (Categories.ContainsKey(categoryId))
            {
                if (ParentMap.ContainsKey(categoryId))
                {
                    string parentId = ParentMap[categoryId];

                    if (Categories.ContainsKey(parentId))
                    {
                        path = Path.Combine(Root,
                            Categories[parentId], Categories[categoryId],
                            pageId);
                    }
                    else
                    {
                        path = Path.Combine(Root,
                            "ts_고아", Categories[categoryId],
                            pageId);
                    }
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
    }
}
