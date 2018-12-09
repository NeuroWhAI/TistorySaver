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

        public string AppId { get; set; }
        public string SecretKey { get; set; }
        public string CallBack { get; set; } = "http://localhost:9842/tistory-auth";
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
        public string ErrorMessage { get; set; }

        public RelayCommand StartAuth { get; set; }

        public event Action<string> WhenTokenReceived;

        private readonly string ClientFile = "client.dat";

        private async void OnStartAuth()
        {
            StartAuth.IsEnabled = false;

            try
            {
                var auth = new Authenticator(AppId, SecretKey, CallBack);
                string token = await auth.Authorize();

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
                byte[] encryptedSecret = Security.EncryptString(SecretKey, out key);

                bw.Write(encryptedSecret.Length);
                bw.Write(encryptedSecret);
                bw.Write(key.Length);
                bw.Write(key);

                bw.Write(AppId);
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

                        SecretKey = Security.DecryptString(strBytes, keyBytes);

                        AppId = br.ReadString();
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
