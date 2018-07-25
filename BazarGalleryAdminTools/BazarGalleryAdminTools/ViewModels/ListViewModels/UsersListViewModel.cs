using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using BazarGalleryAdminTools.Pages;
using Xamarin.Forms;
using System.Linq;
using BazarGallery;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.AppCenter.Crashes;

namespace BazarGalleryAdminTools.ViewModels
{
    class UsersListViewModel : BaseListViewModel
    {
        public override ICommand OnRefresh { get; set; }
        public override ICommand OnSearch { get; set; }
        public override ICommand OnItemSelected { get; set; }
        public override ICommand OnItemAdded { get; set; }

        public UsersListViewModel(ListViewPage page)
        {
            ItemsPerPage = 25;
            OnRefresh = new Command(Refresh);
            OnSearch = new Command(OnSeachTextChangedAsync);
            OnItemSelected = new Command(OnItemSelectedSync);
            OnItemAdded = new Command(AddItemAsync);

            Page = page;
            Page.OnSeachTextChanged += OnSearchRequested;
            Page.OnItemSelected += OnSelectionRequest;
            Page.OnItemDeleted += OnItemDeletedAsync;
            Page.OnItemEdited += OnItemEditedAsync;

            Page.ListView.ItemAppearing += async (sender, e) =>
            {

                if (e.Item == BaseItems[Items.Count - 1])
                {
                    if (BaseItems.Count < TotalItemsCount)
                        await LoadItems();
                }
            };
        }

        private async void OnSeachTextChangedAsync(object obj)
        {
            string searchText = (obj as string).ToLower();

            Items = (BaseItems as List<User>).Where(u => u.Username.ToLower().Contains(searchText) || u.PhoneNumber.Contains(searchText)).ToList();
            if (Items.Count == 0)
            {
                List<User> newList = null;
                try
                {
                    newList = await DataManager.Default.UserTable.Where(u => u.Username.ToLower().Contains(searchText) || u.PhoneNumber.Contains(searchText)).ToListAsync();
                }
                catch { }
                if (newList != null && newList.Count > 0)
                {
                    (BaseItems as List<User>).AddRange(newList);
                    Items = (BaseItems as List<User>).Where(u => u.Username.ToLower().Contains(searchText) || u.PhoneNumber.Contains(searchText)).ToList();
                }
            }
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
        protected void OnItemSelectedSync(object obj)
        {
            if (IsBusy)
                return;

            OnItemEditedAsync(obj);

            //User user = obj as User;
            //if (user != null)
            //{
            //    IsBusy = true;
            //    await Page.DisplayAlert("NotImplemented", "NotImplemented", "OK");
            //    IsBusy = false;
            Page.ListView.SelectedItem = null;
            //}
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
            if (IsBusy)
                return;
            IsBusy = true;
            User user = obj as User;
            var editorPage = new BaseMasterUserEditorPage(false, false);

            editorPage.Username = user.Username;
            editorPage.PhoneNumber = user.PhoneNumber;
            editorPage.Password =
            editorPage.ConfirmPassword = Constants.LocalPassword;

            BaseMasterUserEditorPageViewModel viewModel = null;
            viewModel = new BaseMasterUserEditorPageViewModel(page: editorPage,
            save: async () =>
            {

                viewModel.IsBusy = true;

                var phoneOwner = (await DataManager.Default.UserTable.Where(u => u.PhoneNumber == editorPage.PhoneNumber).ToListAsync()).FirstOrDefault();
                if (phoneOwner != null && phoneOwner.ID != user.ID)
                {
                    await Page.DisplayAlert("Error", "Phone number is associated with other account", "OK");
                    viewModel.IsBusy = false;
                    return;
                }



                user.Username = editorPage.Username;
                user.PhoneNumber = editorPage.PhoneNumber;
                if (user.Password != Constants.LocalPassword)
                    user.Password = editorPage.Password;//User.EncryptPassword(editorPage.Password);


                await RegisterAPI.UpdateAsync(user);
                viewModel.IsBusy = false;
                //Refresh();
                await Page.Navigation.PopModalAsync();

            },
            cancel: async () =>
            {
                var cancel = !((editorPage.Username != user.Username) || (editorPage.PhoneNumber != user.PhoneNumber) || (editorPage.Password != editorPage.ConfirmPassword) || (editorPage.Password != Constants.LocalPassword));
                if (!cancel)
                    cancel = await editorPage.DisplayAlert(Constants.KurdishUnsavedChanges, Constants.KurdishCancelationNotice, Constants.KurdishYes, Constants.KurdishNo);

                if (cancel)
                {
                    await Page.Navigation.PopModalAsync();
                    IsBusy = false;
                }
            }
            );

            editorPage.ViewModel = viewModel;
            await Page.Navigation.PushModalAsync(editorPage);
        }

        private async void AddItemAsync()
        {
            var editorPage = new BaseMasterUserEditorPage(false, false);

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

                User user = new User()
                {
                    Username = editorPage.Username,
                    PhoneNumber = editorPage.PhoneNumber,
                    Password = editorPage.Password,
                    MasterUserID = User.Current.ID,
                    UserType = UserType.Translator
                };

                await RegisterAPI.InsertAsync(user);

                viewModel.IsBusy = false;
                Refresh();
                await Page.Navigation.PopModalAsync();

            });

            editorPage.ViewModel = viewModel;


            await Page.Navigation.PushModalAsync(editorPage);
        }

        private async Task LoadItems()
        {
            IsBusy = true;
            CurrentIndex += ItemsPerPage;
            List<User> nextUsers = await DataManager.Default.UserTable.Where(u => u.UserType == UserType.User).Skip(CurrentIndex).Take(ItemsPerPage).ToListAsync();
            nextUsers.InsertRange(0, (BaseItems as List<User>).Where(u => !nextUsers.Contains(u)));
            BaseItems = nextUsers;
            IsBusy = false;
        }

        private async void Refresh()
        {
            IsBusy = true;
            try
            {
                BaseItems = await DataManager.Default.UserTable.Where(u => u.UserType == UserType.User).IncludeTotalCount().Take(ItemsPerPage).ToListAsync();
                TotalItemsCount = Convert.ToInt32(((IQueryResultEnumerable<User>)BaseItems).TotalCount);
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
