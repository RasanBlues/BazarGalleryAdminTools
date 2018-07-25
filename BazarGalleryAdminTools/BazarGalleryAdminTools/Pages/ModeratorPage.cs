using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;
using BazarGalleryAdminTools.ViewModels;
using BazarGallery;
using Microsoft.WindowsAzure.MobileServices;
using BazarGallery.Models;
using Microsoft.AppCenter.Crashes;

namespace BazarGalleryAdminTools.Pages
{
    public class ModeratorPage : ListViewPage
    {
        public SalesmenListViewModel ViewModel { get; private set; }
        public ModeratorPage()
        {
            ViewModel = new SalesmenListViewModel(this);

            ListView.Footer = null;
            ListView.ItemTemplate = new DataTemplate(() =>
            {
                var textCell = new TextCell();

                textCell.SetBinding(TextCell.TextProperty, "Username");
                textCell.SetBinding(TextCell.DetailProperty, "ShopCount", BindingMode.Default, null, "{0} Shops");

                return textCell;
            });

            ViewModel.OnRefresh = new Command(async () =>
            {
                List<User> list = null;
                try
                {
                    var query = DataManager.Default.UserTable.Where(u => (u.MasterUserID == User.Current.ID)).IncludeTotalCount();
                    list = await query.ToListAsync();
                    ViewModel.TotalItemsCount = ((IQueryResultEnumerable<User>)list).TotalCount;
                }
                catch (System.Net.Http.HttpRequestException)
                {
                    await DisplayAlert("Error", $"Couldn't resolve host.{Environment.NewLine}Please check your internet connection and try again.", "OK");
                }
                catch (Exception e)
                {
                    await DisplayAlert("Error", e.ToString(), "OK");
                    Crashes.TrackError(e);
                }

                if (list != null)
                {
                    List<UserItem> items = new List<UserItem>();

                    foreach (var user in list)
                        items.Add(new UserItem(user, this));

                    ViewModel.BaseItems = items;
                }
                IsRefreshing = false;
            });
            ViewModel.OnSearch = new Command((object obj) =>
            {
                string searchText = obj as string;
                if (!string.IsNullOrEmpty(searchText))
                    ViewModel.Items = (ViewModel.BaseItems as List<UserItem>).Where(u => u.Username.ToLower().Contains(searchText.ToLower()) || u.PhoneNumber.Contains(searchText)).ToList();
                else ViewModel.Items = ViewModel.BaseItems;
            });
            ViewModel.OnItemSelected = new Command(async (object userItemObject) =>
            {
                if (ViewModel.IsBusy)
                    return;
                UserItem userItem = userItemObject as UserItem;
                if (userItem == null)
                    return;

                User user = userItem.User;

                ViewModel.IsBusy = true;

                var todayShopsPage = new ListViewPage(hideFooter: true) { Title = user.Username };
                var todayShopsViewModel = new ShopsListViewModel(todayShopsPage, user, false, true);

                todayShopsViewModel.OnItemSelected = new Command(async (object shopObject) =>
                {
                    if (todayShopsViewModel.IsBusy)
                        return;

                    Shop shop = shopObject as Shop;
                    if (shop != null)
                    {
                        IsBusy = true;
                        await Navigation.PushAsync(new ShopValidatorPage(shop, true));
                        IsBusy = false;
                        todayShopsPage.ListView.SelectedItem = null;
                    }
                });

                todayShopsPage.BindingContext = todayShopsViewModel;

                await Navigation.PushAsync(todayShopsPage);
                todayShopsViewModel.OnRefresh.Execute(null);

                ViewModel.IsBusy = false;
                ListView.SelectedItem = null;

            });

            ViewModel.OnRefresh.Execute(null);

            BindingContext = ViewModel;
        }

        public class UserItem : BaseViewModel
        {
            private int _ShopCount;

            public string Username => User.Username;
            public string PhoneNumber => User.PhoneNumber;

            public User User { get; set; }
            public Page Page { get; private set; }
            public int ShopCount
            {
                get => _ShopCount;
                private set
                {
                    _ShopCount = value;
                    OnPropertyChanged();
                }
            }

            public UserItem(User user, Page page)
            {
                User = user;
                Page = page;
                Refresh();
            }
            public async void Refresh()
            {
                try
                {
                    var query = (await DataManager.Default.ShopTable.Take(0).Where(shop => (shop.SalesmenUserID == User.ID) && (shop.Status == ShopStatus.NotReviewed)).IncludeTotalCount().ToEnumerableAsync());
                    ShopCount = Convert.ToInt32(((IQueryResultEnumerable<Shop>)query).TotalCount);
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
            }
        }
    }
}