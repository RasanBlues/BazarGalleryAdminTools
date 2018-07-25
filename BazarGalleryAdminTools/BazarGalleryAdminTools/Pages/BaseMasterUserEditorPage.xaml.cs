using BazarGalleryAdminTools.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BazarGalleryAdminTools.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BaseMasterUserEditorPage : ContentPage
    {
        private BaseMasterUserEditorPageViewModel _ViewModel;

        public BaseMasterUserEditorPageViewModel ViewModel
        {
            get => _ViewModel;
            set
            {
                BindingContext =
                    _ViewModel = value;
            }
        }
        public string Username
        {
            get => UIUsernameEntry.Text;
            set => UIUsernameEntry.Text = value;
        }
        public string PhoneNumber
        {
            get => UIPhoneEntry.Text;
            set => UIPhoneEntry.Text = value;
        }
        public string Password
        {
            get => UIPasswordEntry.Text;
            set => UIPasswordEntry.Text = value;
        }
        public string ConfirmPassword
        {
            get => UIConfirmPasswordEntry.Text;
            set => UIConfirmPasswordEntry.Text = value;
        }
        public bool EnglishTranslation
        {
            get => UIEnglishSwitch.IsToggled;
            set => UIEnglishSwitch.IsToggled = value;
        }
        public bool ArabicTranslation
        {
            get => UIArabicSwitch.IsToggled;
            set => UIArabicSwitch.IsToggled = value;
        }
        public Picker Picker
        {
            get => UIPicker;
        }

        public BaseMasterUserEditorPage()
        {
            InitializeComponent();
        }

        public BaseMasterUserEditorPage(bool showPicker, bool showTranslationLayout)
        {
            InitializeComponent();

            UIPicker.IsVisible =
            UIPickerLabel.IsVisible = showPicker;

            UIEnglishTranslationLayout.IsVisible =
            UIArabicTranslationLayout.IsVisible = showTranslationLayout;
        }

        protected override bool OnBackButtonPressed()
        {
            if (ViewModel != null)
                ViewModel.CancelCommand.Execute(null);
            return true;
        }
    }
}