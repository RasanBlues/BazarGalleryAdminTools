using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using BazarGalleryAdminTools.Pages;
using System.Collections;

namespace BazarGalleryAdminTools.ViewModels
{
    public class BaseMasterUserEditorPageViewModel : BaseViewModel
    {
        private bool _IsBusy;

        public BaseMasterUserEditorPage Page { get; private set; }

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
        public object PickerSelectedItem
        {
            get => Page.Picker.SelectedItem;
        }
        public int PickerSelectedIndex
        {
            get => Page.Picker.SelectedIndex;
            set => Page.Picker.SelectedIndex = value;
        }
        public IList PickerOptions
        {
            get => Page.Picker.ItemsSource;
            set
            {
                Page.Picker.ItemsSource = value;
                PickerSelectedIndex = 0;
            }
        }
        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        public BaseMasterUserEditorPageViewModel(BaseMasterUserEditorPage page, Action save, bool defaultValidations = true)
        {
            Page = page;
            if (defaultValidations)
                SaveCommand = new Command(() =>
               {
                   SaveDefaultValidationAsync(save);
               });
            else
                SaveCommand = new Command(save);

            CancelCommand = new Command(async () =>
            {
                var cancel = string.IsNullOrWhiteSpace(Page.Username + Page.PhoneNumber + Page.Password + Page.ConfirmPassword);
                if (!cancel)
                    cancel = await Page.DisplayAlert("Unsaved Changes", "Are you sure you want to cancel?", "Yes", "No");

                if (cancel)
                    await Page.Navigation.PopModalAsync();
            });
        }
        public BaseMasterUserEditorPageViewModel(BaseMasterUserEditorPage page, Action save, Action cancel, bool defaultValidations = true)
        {
            Page = page;

            if (defaultValidations)
                SaveCommand = new Command(() =>
                {
                    SaveDefaultValidationAsync(save);
                });
            else
                SaveCommand = new Command(save);

            CancelCommand = new Command(cancel);
        }

        protected async void SaveDefaultValidationAsync(Action save)
        {
            if (IsBusy)
                return;

            if (string.IsNullOrWhiteSpace(Page.Username))
            {
                await Page.DisplayAlert("Error", "Name must be filled", "OK");
                return;
            }
            if (string.IsNullOrEmpty(Page.Password) || string.IsNullOrEmpty(Page.ConfirmPassword))
            {
                await Page.DisplayAlert("Error", "Passwords must be filled", "OK");
                return;
            }
            if (Page.Password != Page.ConfirmPassword)
            {
                await Page.DisplayAlert("Error", "Password's don't match", "OK");
                return;
            }
            if (!BazarGallery.User.IsPhoneNumberValid(Page.PhoneNumber))
            {
                await Page.DisplayAlert("Error", "Phone number must be 11 numbers in (07xx xxx xxxx) format, or 10 number in (053 xxx xxxx)", "OK");
                return;
            }


            save();
        }
    }
}
