using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using BazarGalleryAdminTools.Pages;
using Xamarin.Forms;
using System.Linq;
using BazarGallery;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.AppCenter.Crashes;

namespace BazarGalleryAdminTools.ViewModels
{
    public class TranslatorsListViewModel : BaseListViewModel
    {
        public override ICommand OnRefresh { get; set; }
        public override ICommand OnSearch { get; set; }
        public override ICommand OnItemSelected { get; set; }
        public override ICommand OnItemAdded { get; set; }

        public TranslatorsListViewModel(ListViewPage page)
        {
            OnRefresh = new Command(Refresh);
            OnSearch = new Command(OnSeachTextChanged);
            OnItemSelected = new Command(OnItemSelectedAsync);
            OnItemAdded = new Command(AddItemAsync);

            Page = page;

            Page.OnSeachTextChanged += OnSearchRequested;
            Page.OnItemSelected += OnSelectionRequest;
            Page.OnItemDeleted += OnItemDeletedAsync;
            Page.OnItemEdited += OnItemEditedAsync;
        }

        private void OnSeachTextChanged(object obj)
        {
            string searchText = obj as string;
            Items = (BaseItems as List<User>).Where(u => u.Username.ToLower().Contains(searchText.ToLower()) || u.PhoneNumber.Contains(searchText)).ToList();
            OnPropertyChanged(nameof(Items));
        }

        protected override void OnSearchRequested(string searchText)
        {
            OnSearch.Execute(searchText);
        }
        protected override void OnSelectionRequest(object item)
        {
            OnItemSelected.Execute(item);
        }
        protected async void OnItemSelectedAsync(object obj)
        {
            if (IsBusy)
                return;

            User user = obj as User;
            if (user != null)
            {
                IsBusy = true;
                await Page.DisplayAlert("NotImplemented", "NotImplemented", "OK");
                IsBusy = false;
                Page.ListView.SelectedItem = null;
            }
        }
        protected override async void OnItemDeletedAsync(object obj)
        {
            User user = obj as User;
            bool shouldDelete = await Page.DisplayAlert($"Delete {user.Username}", $"Are you sure you want to delete {user}?", "Yes", "No");
            if (shouldDelete)
            {
                await DataManager.Default.UserTable.DeleteAsync(user);
                Refresh();
            }
        }
        protected override async void OnItemEditedAsync(object obj)
        {
            User user = obj as User;
            var editorPage = new BaseMasterUserEditorPage(false, true);

            editorPage.Username = user.Username;
            editorPage.PhoneNumber = user.PhoneNumber;
            editorPage.Password =
            editorPage.ConfirmPassword = Constants.LocalPassword;
            editorPage.EnglishTranslation = user.Data.Contains(Constants.EnglishTranslationID);
            editorPage.ArabicTranslation = user.Data.Contains(Constants.ArabicTranslationID);

            BaseMasterUserEditorPageViewModel viewModel = null;
            viewModel = new BaseMasterUserEditorPageViewModel(page: editorPage,
            save: async () =>
            {
                if (!(editorPage.EnglishTranslation || editorPage.ArabicTranslation))
                {
                    await Page.DisplayAlert("Error", "At least one translation language must be selected", "OK");
                    return;
                }

                viewModel.IsBusy = true;

                var phoneOwner = (await DataManager.Default.UserTable.Where(u => u.PhoneNumber == editorPage.PhoneNumber).ToListAsync()).FirstOrDefault();
                if (phoneOwner != null && phoneOwner.ID != user.ID)
                {
                    await Page.DisplayAlert("Error", "Phone number is associated with other account", "OK");
                    viewModel.IsBusy = false;
                    return;
                }

                string translationData = String.Empty;

                if (editorPage.EnglishTranslation)
                    translationData += Constants.EnglishTranslationID;
                if (editorPage.ArabicTranslation)
                    translationData += Constants.ArabicTranslationID;


                user.Username = editorPage.Username;
                user.PhoneNumber = editorPage.PhoneNumber;
                if (user.Password != Constants.LocalPassword)
                    user.Password = editorPage.Password;//User.EncryptPassword(editorPage.Password);

                user.Data = translationData;



                await RegisterAPI.UpdateAsync(user);
                viewModel.IsBusy = false;
                //Refresh();
                await Page.Navigation.PopModalAsync();

            });

            editorPage.ViewModel = viewModel;


            await Page.Navigation.PushModalAsync(editorPage);
        }

        private async void AddItemAsync()
        {
            var editorPage = new BaseMasterUserEditorPage(false, true);

            BaseMasterUserEditorPageViewModel viewModel = null;
            viewModel = new BaseMasterUserEditorPageViewModel(page: editorPage,
            save: async () =>
            {
                if (!(editorPage.EnglishTranslation || editorPage.ArabicTranslation))
                {
                    await Page.DisplayAlert("Error", "At least one translation language must be selected", "OK");
                    return;
                }

                viewModel.IsBusy = true;

                if ((await DataManager.Default.UserTable.Where(u => u.PhoneNumber == editorPage.PhoneNumber).ToListAsync()).FirstOrDefault() != null)
                {
                    await Page.DisplayAlert("Error", "Phone number is associated with other account", "OK");
                    viewModel.IsBusy = false;
                    return;
                }

                string translationData = String.Empty;

                if (editorPage.EnglishTranslation)
                    translationData += Constants.EnglishTranslationID;
                if (editorPage.ArabicTranslation)
                    translationData += Constants.ArabicTranslationID;

                User user = new User()
                {
                    Username = editorPage.Username,
                    PhoneNumber = editorPage.PhoneNumber,
                    Password = editorPage.Password,
                    MasterUserID = User.Current.ID,
                    UserType = UserType.Translator,
                    Data = translationData
                };

                await RegisterAPI.InsertAsync(user);

                viewModel.IsBusy = false;
                Refresh();
                await Page.Navigation.PopModalAsync();

            });

            editorPage.ViewModel = viewModel;


            await Page.Navigation.PushModalAsync(editorPage);
        }

        private async void Refresh()
        {
            IsBusy = true;
            try
            {
                BaseItems = await DataManager.Default.UserTable.Where(u => u.UserType == UserType.Translator).Take(ItemsPerPage).IncludeTotalCount().ToListAsync();
                TotalItemsCount = ((IQueryResultEnumerable<User>)BaseItems).TotalCount;
            }
            catch (System.Net.Http.HttpRequestException)
            {
                await Page.DisplayAlert("Error", $"Couldn't resolve host.{Environment.NewLine}Please check your internet connection and try again.", "OK");
            }
            catch (Exception e)
            {
                await Page.DisplayAlert("Error", e.ToString(), "OK");
                Crashes.TrackError(e);
            }
            IsBusy =
            Page.IsRefreshing = false;
        }
    }
}
