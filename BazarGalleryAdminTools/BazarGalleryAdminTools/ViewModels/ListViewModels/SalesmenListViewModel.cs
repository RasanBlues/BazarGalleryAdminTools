using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using BazarGalleryAdminTools.Pages;
using BazarGallery.Models;
using Xamarin.Forms;
using System.Linq;
using BazarGallery;
using Microsoft.AppCenter.Crashes;
using Microsoft.WindowsAzure.MobileServices;

namespace BazarGalleryAdminTools.ViewModels
{
    public class SalesmenListViewModel : BaseListViewModel
    {
        public override ICommand OnRefresh { get; set; }
        public override ICommand OnSearch { get; set; }
        public override ICommand OnItemSelected { get; set; }
        public override ICommand OnItemAdded { get; set; }


        public SalesmenListViewModel(ListViewPage page)
        {
            Page = page;

            OnRefresh = new Command(Refresh);
            OnItemSelected = new Command(OnItemSelectedAsync);
            OnItemAdded = new Command(AddItemAsync);
            OnSearch = new Command(OnSeachTextChanged);

            Page.OnSeachTextChanged += OnSearchRequested;
            Page.OnItemSelected += OnSelectionRequest;
            Page.OnItemDeleted += OnItemDeletedAsync;
            Page.OnItemEdited += OnItemEditedAsync;
        }

        private void OnSeachTextChanged(object obj)
        {
            string searchText = obj as string;
            if (!string.IsNullOrEmpty(searchText))
                Items = (BaseItems as List<User>).Where(u => u.Username.ToLower().Contains(searchText.ToLower()) || u.PhoneNumber.Contains(searchText)).ToList();
            else Items = BaseItems;
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
                await Page.Navigation.PushAsync(new SalesmanTabbedPage(false,user,false));
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
            var editorPage = new BaseMasterUserEditorPage(true, false);

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
                user.MasterUserID = (editorPage.Picker.SelectedItem as User).ID;


                await RegisterAPI.UpdateAsync(user);
                viewModel.IsBusy = false;
                Refresh();
                await Page.Navigation.PopModalAsync();

            })
            {
                PickerOptions = await DataManager.Default.UserTable.Where(u => u.UserType == UserType.Moderator).ToListAsync()
            };

            var moderator = ((List<User>)viewModel.PickerOptions).Where(u => u.ID == user.MasterUserID).Take(1).ToList().FirstOrDefault();
            if (moderator != null)
            {
                viewModel.PickerSelectedIndex = viewModel.PickerOptions.IndexOf(moderator);
            }

            editorPage.ViewModel = viewModel;
            await Page.Navigation.PushModalAsync(editorPage);
        }

        protected async void AddItemAsync()
        {
            var editorPage = new BaseMasterUserEditorPage(true, false);
            BaseMasterUserEditorPageViewModel viewModel = null;
            viewModel = new BaseMasterUserEditorPageViewModel(page: editorPage,
            save: async () =>
            {
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
                    MasterUserID = (editorPage.Picker.SelectedItem as User).ID,
                    UserType = UserType.Salesman,
                    Data = String.Empty
                };

                await RegisterAPI.InsertAsync(user);

                viewModel.IsBusy = false;
                Refresh();
                await Page.Navigation.PopModalAsync();

            })
            {
                PickerOptions = await DataManager.Default.UserTable.Where(u => u.UserType == UserType.Moderator).ToListAsync()
            };
            editorPage.ViewModel = viewModel;


            await Page.Navigation.PushModalAsync(editorPage);
        }

        private async void Refresh()
        {
            IsBusy = true;
            try
            {
                BaseItems = await DataManager.Default.UserTable.Where(u => u.UserType == UserType.Salesman).IncludeTotalCount().Take(ItemsPerPage).ToListAsync();
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
