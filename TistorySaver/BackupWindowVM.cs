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
            }
        }

        private void OnFindFolder()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Folder = dialog.SelectedPath;
                OnPropertyChanged("Folder");
            }
        }

        private async void OnStartBackup()
        {
            if (Directory.Exists(Folder) == false)
            {
                ShowError("폴더가 존재하지 않습니다.");

                return;
            }


            FindFolderCommand.IsEnabled = false;
            StartBackupCommand.IsEnabled = false;

            IsIdle = false;
            OnPropertyChanged("IsIdle");

            HideError();

            try
            {
                string blogName;

                if (SelectedBlogIndex >= 0 && SelectedBlogIndex < BlogList.Count)
                {
                    blogName = BlogList[SelectedBlogIndex].Name;
                }
                else
                {
                    throw new IndexOutOfRangeException("선택된 블로그가 유효하지 않습니다.");
                }


                var bakMgr = new BackupManager(Folder, blogName);


                var categories = await Api.ListCategory(blogName);

                foreach (var category in categories.Categories)
                {
                    bakMgr.AddCategory(category.Id, category.Name, category.Parent);
                }


                int currentPage = 1;
                var postList = await Api.ListPost(blogName, currentPage);

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
                                catch (WebException e)
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
                                        content = string.Empty;
                                    }
                                }
                            }

                            if (string.IsNullOrEmpty(content) == false)
                            {
                                try
                                {
                                    await bakMgr.Backup(page.CategoryId, page.Id, content);
                                }
                                catch
                                {
                                    // 불완전 백업.
                                    ShowError("게시글의 백업에 실패하였습니다.");
                                }
                            }
                        }

                        NumPageCompleted += 1;
                        OnPropertyChanged("NumPageCompleted");
                    }

                    // 다음 페이지 불러오기.
                    if (NumPageCompleted < TotalPageCount)
                    {
                        currentPage += 1;

                        int maxRetryCount = 10;
                        for (int retry = 0; retry <= maxRetryCount; ++retry)
                        {
                            try
                            {
                                postList = await Api.ListPost(blogName, currentPage);

                                break;
                            }
                            catch (WebException e)
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
            }
            catch (Exception e)
            {
                if (e.InnerException == null)
                {
                    ShowError(e.Message);
                }
                else
                {
                    ShowError(e.InnerException.Message);
                }
            }
            finally
            {
                IsIdle = true;
                OnPropertyChanged("IsIdle");

                FindFolderCommand.IsEnabled = true;
                StartBackupCommand.IsEnabled = true;

                CurrentPage = string.Empty;
                OnPropertyChanged("CurrentPage");
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
    }
}
