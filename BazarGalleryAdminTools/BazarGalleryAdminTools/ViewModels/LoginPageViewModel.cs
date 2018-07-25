using BazarGallery;
using BazarGalleryAdminTools.Pages;
using System;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.WindowsAzure.MobileServices;

namespace BazarGalleryAdminTools.ViewModels
{
    public class LoginPageViewModel : BaseViewModel
    {
        private bool _IsBusy;

        public LoginPage Page { get; private set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public ICommand LoginCommand { get; private set; }
        public ICommand FocusPasswordEntryCommand { get; private set; }

        public bool IsBusy
        {
            get => _IsBusy;
            set
            {
                _IsBusy =
                Page.IsBusy = value;

                OnPropertyChanged();
            }
        }

        public LoginPageViewModel(LoginPage page)
        {
            Page = page;

            LoginCommand = new Command(LoginAsync);
            FocusPasswordEntryCommand = new Command(() =>
            {
                if (PhoneNumber.Length >= 10 && User.IsPhoneNumberValid(PhoneNumber))
                {
                    Page.PasswordEntry.Focus();
                }
            });


            string phone = Preferences.Get(Constants.OAuthPhoneNumber, String.Empty);
            string authID = Preferences.Get(Constants.OAuthProperty, String.Empty);
            if (!string.IsNullOrEmpty(phone) && !string.IsNullOrEmpty(authID))
            {
                PhoneNumber = phone;
                LoginAuth(authID);
            }
        }

        private async void LoginAsync()
        {
            var user = await TryLoginAsync(null);
            if (user != null)
                ToUserPage(user);
        }
        private async void LoginAuth(string authID){
            var user = await TryLoginAsync(authID);
            if (user != null)
                ToUserPage(user);
        }
        private async Task<User> TryLoginAsync(string authID)
        {
            int timeout = 10000;
            var task = LoginAsync(authID);
            if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
            {
                return task.Result;
            }
            else
            {
                IsBusy = false;
                await Page.DisplayAlert("Login Failed", $"Internet connection is required.{Environment.NewLine}Please connect to the internet and try again.", "OK");
                return null;
            }
        }
        private async Task<User> LoginAsync(string authID)
        {
            if (IsBusy)
                return null;
            
            if (string.IsNullOrEmpty(authID))
                if (string.IsNullOrEmpty(PhoneNumber) || (string.IsNullOrEmpty(Password)))
                {
                    await Page.DisplayAlert("Login Failed", "Please enter both Phone and Password", "OK");
                    return null;
                }

            if (!User.IsPhoneNumberValid(PhoneNumber, out string fixedPhoneNumber))
            {
                await Page.DisplayAlert("Login Failed", "Please enter a correct phone number", "OK");
                return null;
            }

            User user = null;
            IsBusy = true;

            try
            {
                user = (await DataManager.Default.UserTable.Where(u => (u.PhoneNumber == fixedPhoneNumber)).ToListAsync()).FirstOrDefault();
            }
            catch (System.Net.Http.HttpRequestException)
            {
                await Page.DisplayAlert("Login Failed", $"Internet connection is required.{Environment.NewLine}Please connect to the internet and try again.", "OK");
                IsBusy = false;
                return null;
            }
            catch (Exception e)
            {
                await Page.DisplayAlert("Login Failed", e.ToString(), "OK");
                IsBusy = false;
                return null;
            }

            if (user != null && ((!string.IsNullOrEmpty(authID) && user.AuthID == authID) || (user.Password == Password)))
            {
                //await RegisterAPI.UpdateAsync(user);
                await User.Login(user);
            }
            else
            {
                user = null;
                await Page.DisplayAlert("Login Failed", "Phone or Password is incorrect", "OK");
            }

            IsBusy = false;

            return user;
        }
        
        private async void ToUserPage(User user)
        {
            switch (user.UserType)
            {
                case UserType.Admin:
                    await Page.Navigation.PushAsync(AdminTabbedPaged.GetNewAdminPage());
                    break;
                case UserType.Moderator:
                    await Page.Navigation.PushAsync(new ModeratorTabbedPage());
                    break;
                case UserType.Salesman:

                    await Page.Navigation.PushAsync(new SalesmanTabbedPage());
                    break;
            }

            Page.Navigation.RemovePage(Page);
        }
    }
}
