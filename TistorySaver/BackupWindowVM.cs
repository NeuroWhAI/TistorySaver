using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using TistorySaver.TistoryApiResponses;

namespace TistorySaver
{
    public class BackupWindowVM : ViewModelBase
    {
        public BackupWindowVM()
        {
            FindFolderCommand = new RelayCommand(OnFindFolder, false);
            StartBackupCommand = new RelayCommand(OnStartBackup, false);


#if DEBUG
            UserInfo = "neurowhai@neurowhai.test";
            BlogList.AddRange(new[]
            {
                new BlogInfoData { BlogId="1234", Name="neurowhai-1", Title="내 블로그 1" },
                new BlogInfoData { BlogId="1235", Name="neurowhai-2", Title="내 블로그 2" },
                new BlogInfoData { BlogId="1236", Name="neurowhai-3", Title="내 블로그 3" },
            });

            CurrentPage = "[123] 테스트 글 제목입니다. 투 머치 토커 타이틀 얍얍.";
#endif
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
        public string ErrorMessage { get; set; }

        public bool IsIdle { get; set; } = false;
        public string UserInfo { get; set; }
        public List<BlogInfoData> BlogList { get; set; } = new List<BlogInfoData>();
        public int SelectedBlogIndex { get; set; } = 0;
        public string Folder { get; set; }
        public int TotalPageCount { get; set; } = 1;
        public int NumPageCompleted { get; set; } = 0;
        public string CurrentPage { get; set; }

        public RelayCommand FindFolderCommand { get; set; }
        public RelayCommand StartBackupCommand { get; set; }

        public ILogWinService LogService { get; set; } = null;

        private TistoryApi Api { get; set; }

        public async Task Initialize(string apiToken)
        {
            Api = new TistoryApi(apiToken);


            try
            {
                var blogInfo = await Api.GetBlogInfo();

                UserInfo = blogInfo.Id;
                OnPropertyChanged("UserInfo");

                BlogList = blogInfo.Blogs;
                OnPropertyChanged("BlogList");

                FindFolderCommand.IsEnabled = true;
                StartBackupCommand.IsEnabled = true;

                IsIdle = true;
                OnPropertyChanged("IsIdle");
            }
            catch (Exception e)
            {
                ShowError(e.Message);

                CreateDebugLog(e.Message, e.StackTrace);
            }
        }

        private void OnFindFolder()
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Folder = dialog.SelectedPath;
                    OnPropertyChanged("Folder");
                }
            }
        }

        private async void OnStartBackup()
        {
            if (Directory.Exists(Folder) == false)
            {
                ShowError("폴더가 존재하지 않습니다.");

                return;
            }


            string blogName;

            if (SelectedBlogIndex >= 0 && SelectedBlogIndex < BlogList.Count)
            {
                blogName = BlogList[SelectedBlogIndex].Name;
            }
            else
            {
                ShowError("선택된 블로그가 유효하지 않습니다.");

                return;
            }


            FindFolderCommand.IsEnabled = false;
            StartBackupCommand.IsEnabled = false;

            IsIdle = false;
            OnPropertyChanged("IsIdle");

            HideError();


            var logger = new BackupLogger();
            var bakMgr = new BackupManager(Folder, blogName);
            bakMgr.Logger = logger;


            try
            {
                var categories = await Api.ListCategory(blogName);

                if (categories.Categories != null)
                {
                    foreach (var category in categories.Categories)
                    {
                        bakMgr.AddCategory(category.Id, category.Name, category.Parent);
                    }
                }


                int pageNum = 1;
                var postList = await Api.ListPost(blogName, pageNum);

                TotalPageCount = postList.TotalCount;
                OnPropertyChanged("TotalPageCount");

                NumPageCompleted = 0;
                OnPropertyChanged("NumPageCompleted");


                while (NumPageCompleted < TotalPageCount)
                {
                    // 현재 페이지의 글 백업.
                    foreach (var page in postList.Posts)
                    {
                        // 작업 중인 페이지 표시.
                        CurrentPage = string.Format("[{0}] {1}", page.Id, page.Title);
                        OnPropertyChanged("CurrentPage");

                        // 존재하는 백업이 없으면 백업 진행.
                        if (bakMgr.BackupExists(page.CategoryId, page.Id) == false)
                        {
                            string content = string.Empty;

                            int maxRetryCount = 5;
                            for (int retry = 0; retry <= maxRetryCount; ++retry)
                            {
                                try
                                {
                                    var post = await Api.ReadPost(blogName, page.Id);
                                    content = post.Content;

                                    break;
                                }
                                catch (Exception e)
                                {
                                    if (retry < maxRetryCount)
                                    {
                                        ShowError(e.Message + " > 재시도 대기...");
                                        await Task.Delay(3000);
                                        HideError();
                                    }
                                    else
                                    {
                                        ShowError("게시글을 불러올 수 없습니다.");
                                        logger.Add(page.Id, "글의 내용을 받아올 수 없습니다.");

                                        content = string.Empty;
                                    }
                                }
                            }

                            if (string.IsNullOrEmpty(content) == false)
                            {
                                try
                                {
                                    await bakMgr.Backup(page.CategoryId, page.Id,
                                        page.Title, content);
                                }
                                catch
                                {
                                    ShowError("게시글의 백업에 실패하였습니다.");
                                    logger.Add(page.Id, "불완전 백업.");
                                }
                            }
                        }

                        NumPageCompleted += 1;
                        OnPropertyChanged("NumPageCompleted");
                    }

                    // 다음 페이지 불러오기.
                    if (NumPageCompleted < TotalPageCount)
                    {
                        pageNum += 1;

                        int maxRetryCount = 10;
                        for (int retry = 0; retry <= maxRetryCount; ++retry)
                        {
                            try
                            {
                                postList = await Api.ListPost(blogName, pageNum);

                                break;
                            }
                            catch (Exception e)
                            {
                                if (retry < maxRetryCount)
                                {
                                    ShowError(e.Message + " > 재시도 대기...");
                                    await Task.Delay(5000);
                                    HideError();
                                }
                                else
                                {
                                    ShowError("다음 페이지를 불러올 수 없습니다.");
                                    return;
                                }
                            }
                        }
                    }
                }


                HideError();

                CurrentPage = string.Empty;
                OnPropertyChanged("CurrentPage");
            }
            catch (Exception e)
            {
                if (e.InnerException == null)
                {
                    ShowError(e.Message);

                    CreateDebugLog(e.Message, e.StackTrace);
                }
                else
                {
                    var inner = e.InnerException;

                    ShowError(inner.Message);

                    CreateDebugLog(e.Message, e.StackTrace, "---- Inner Exception ----",
                        inner.Message, inner.StackTrace);
                }
            }
            finally
            {
                // Show backup log.
                if (logger.IsEmpty == false)
                {
                    var logVm = new LogWindowVM();
                    logVm.SetLog(logger);

                    LogService.ShowLogDialog(logVm);
                }


                IsIdle = true;
                OnPropertyChanged("IsIdle");

                FindFolderCommand.IsEnabled = true;
                StartBackupCommand.IsEnabled = true;
            }
        }

        private void ShowError(string error)
        {
            ErrorMessage = error;
            OnPropertyChanged("ErrorMessage");
            OnPropertyChanged("HasError");
        }

        private void HideError()
        {
            ErrorMessage = string.Empty;
            OnPropertyChanged("HasError");
            OnPropertyChanged("ErrorMessage");
        }

        private void CreateDebugLog(params string[] lines)
        {
            try
            {
                File.WriteAllLines("log.txt", lines);
            }
            catch
            { }
        }
    }
}
