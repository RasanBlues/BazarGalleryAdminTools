using System;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using BazarGallery;
using BazarGalleryAdminTools.ViewModels;

namespace BazarGalleryAdminTools.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public Entry PasswordEntry => UIPasswordEntry;

        public LoginPage()
        {
            InitializeComponent();

            BindingContext = new LoginPageViewModel(this);
        }
    }
}