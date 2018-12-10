using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.IO;

namespace TistorySaver
{
    public class AuthWindowVM : ViewModelBase
    {
        public AuthWindowVM()
        {
            StartAuth = new RelayCommand(OnStartAuth);


            LoadClientData();
        }

        public string AppId { get; set; } = "2977b1972273c9fbce392246e8bde208";
        public string CallBack { get; set; } = "https://neurowhai.tistory.com/297";
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
        public string ErrorMessage { get; set; }

        public RelayCommand StartAuth { get; set; }

        public ILoginWindowService LoginService { get; set; } = null;
        public event Action<string> WhenTokenReceived;

        private readonly string ClientFile = "client.dat";

        private void OnStartAuth()
        {
            StartAuth.IsEnabled = false;

            try
            {
                if (LoginService == null)
                {
                    throw new InvalidOperationException();
                }

                var auth = new Authenticator(LoginService, AppId, CallBack);
                string token = auth.Authorize();

                SaveClientData();

                if (WhenTokenReceived != null)
                {
                    WhenTokenReceived(token);
                }
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
                StartAuth.IsEnabled = true;
            }
        }

        private void ShowError(string error)
        {
            ErrorMessage = error;
            OnPropertyChanged("ErrorMessage");
            OnPropertyChanged("HasError");
        }

        private void SaveClientData()
        {
            using (var bw = new BinaryWriter(new FileStream(ClientFile, FileMode.Create)))
            {
                byte[] key;
                byte[] encryptedId = Security.EncryptString(AppId, out key);

                bw.Write(encryptedId.Length);
                bw.Write(encryptedId);
                bw.Write(key.Length);
                bw.Write(key);

                bw.Write(CallBack);
            }
        }

        private void LoadClientData()
        {
            if (File.Exists(ClientFile))
            {
                try
                {
                    using (var br = new BinaryReader(new FileStream(ClientFile, FileMode.Open)))
                    {
                        int strLen = br.ReadInt32();
                        byte[] strBytes = br.ReadBytes(strLen);

                        int keyLen = br.ReadInt32();
                        byte[] keyBytes = br.ReadBytes(keyLen);

                        AppId = Security.DecryptString(strBytes, keyBytes);

                        CallBack = br.ReadString();
                    }
                }
                catch
                {
                    ShowError("저장된 인증 정보를 불러올 수 없습니다.");
                }
            }
        }
    }
}
