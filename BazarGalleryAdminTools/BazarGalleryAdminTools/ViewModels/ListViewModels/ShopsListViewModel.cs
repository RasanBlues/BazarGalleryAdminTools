using System;
using System.Collections.Generic;
using System.Windows.Input;
using BazarGalleryAdminTools.Pages;
using BazarGallery.Models;
using Xamarin.Forms;
using System.Linq;
using BazarGallery;
using BazarGalleryAdminTools.Controls;
using System.Globalization;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.AppCenter.Crashes;
using System.Threading.Tasks;

namespace BazarGalleryAdminTools.ViewModels
{
    public class ShopsListViewModel : BaseListViewModel
    {
        public User User { get; set; }
        public bool IsArchiveData { get; set; }

        public override ICommand OnRefresh { get; set; }
        public override ICommand OnSearch { get; set; }
        public override ICommand OnItemSelected { get; set; }
        public override ICommand OnItemAdded { get; set; }

        public ShopsListViewModel(ListViewPage page, User user, bool isArchiveData, bool defaultItemTemplate)
        {
            User = user;
            Page = page;
            IsArchiveData = isArchiveData;

            if (defaultItemTemplate)
                Page.ListView.ItemTemplate = new ShopDataTemplateSelector(Page.Edit_Clicked, Page.Delete_Clicked);

            OnSearch = new Command(OnSeachTextChanged);
            OnRefresh = new Command(Refresh);
            OnItemSelected = new Command(OnItemSelectedAsync);
            OnItemAdded = new Command(AddItemAsync);

            Page.OnSeachTextChanged += OnSearchRequested;
            Page.OnItemSelected += OnSelectionRequest;
            Page.OnItemDeleted += OnItemDeletedAsync;
            Page.OnItemEdited += OnItemEditedAsync;

            if (IsArchiveData)
                Page.ListView.ItemAppearing += async (sender, e) =>
                {

                    if (e.Item == BaseItems[Items.Count - 1])
                    {
                        if (BaseItems.Count < TotalItemsCount)
                            await LoadArchiveItems();
                    }
                };
        }

        private async void OnSeachTextChanged(object obj)
        {
            string searchText = obj as string;
            try
            {
                Items = (BaseItems as List<Shop>).Where(shop => shop.KurdishName.ToLower().Contains(searchText.ToLower()) || shop.CreatedAt.Date.ToLongDateString().ToLower().Contains(searchText) || shop.KurdishStatus.Contains(searchText)).ToList();
            }
            catch (Exception e)
            {
                await Page.DisplayAlert("Error", e.ToString(), "OK");
            }
        }
        private bool IsDate(string searchText)
        {
            if (searchText.Length >= 4)
            {
                foreach (string m in DateTimeFormatInfo.CurrentInfo.MonthNames)
                {
                    if (m.Contains(searchText))
                        return true;
                }
                foreach (string d in Constants.Days)
                {
                    if (d.Contains(searchText))
                        return true;
                }
            }
            return false;
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

            Shop shop = obj as Shop;
            if (shop != null)
            {
                IsBusy = true;
                await Page.Navigation.PushAsync(new ShopValidatorPage(shop));
                IsBusy = false;
                Page.ListView.SelectedItem = null;
            }
        }
        protected override async void OnItemDeletedAsync(object obj)
        {
            Shop shop = obj as Shop;
            if (shop.Status == ShopStatus.Approved)
            {
                await Page.DisplayAlert($"کێشەیەک ڕوویدا", $"ناتوانیت ئەم تۆمارە بسڕیتەوە لەبەرئەوەی پەسەندکراوە.", "باشە");
                return;
            }
            bool shouldDelete = await Page.DisplayAlert($"Delete {shop.KurdishName}", $"Are you sure you want to delete {shop.KurdishName}?", "Yes", "No");
            if (shouldDelete)
            {
                await DataManager.Default.ShopTable.DeleteAsync(shop);
                foreach (var album in await DataManager.Default.AlbumTable.Where(a => a.ShopID == shop.ID).ToEnumerableAsync())
                {
                    foreach (var image in await DataManager.Default.AlbumImageTable.Where(a => a.AlbumID == album.ID).ToEnumerableAsync())
                    {
                        await DataManager.Default.AlbumImageTable.DeleteAsync(image);
                    }

                    await DataManager.Default.AlbumTable.DeleteAsync(album);
                }
                foreach (var subCat in await DataManager.Default.ShopSubcategoryTable.Where(s => s.ShopID == shop.ID).ToEnumerableAsync())
                {
                    await DataManager.Default.ShopSubcategoryTable.DeleteAsync(subCat);
                }
                OnRefresh.Execute(null);
            }
        }
        protected override async void OnItemEditedAsync(object obj)
        {
            Shop shop = obj as Shop;
            //if (shop.Status == ShopStatus.Approved)
            //{
            //    await Page.DisplayAlert($"کێشەیەک ڕوویدا", $"ناتوانیت ئەم تۆمارە دەستکاری بکەیت لەبەرئەوەی پەسەندکراوە.", "باشە");
            //    return;
            //}
            await Page.Navigation.PushModalAsync(new ShopEditorPage(shop, true, Refresh));
        }
        protected async void AddItemAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            var newShopPage = new ShopEditorPage(Refresh);
            await Page.Navigation.PushModalAsync(newShopPage);
            IsBusy = false;
        }

        private async Task LoadArchiveItems()
        {
            IsBusy = true;
            CurrentIndex += ItemsPerPage;
            List<Shop> nextShops = await DataManager.GetSalesmanShops(User.Current).Where(shop => shop.Status != ShopStatus.NotReviewed).Skip(CurrentIndex).Take(ItemsPerPage).ToListAsync();
            nextShops.InsertRange(0, (BaseItems as List<Shop>).Where(shop => !nextShops.Contains(shop)));
            BaseItems = nextShops;
            IsBusy = false;
        }
        private async void Refresh()
        {
            IsBusy = true;

            try
            {
                if (User.UserType == UserType.Salesman)
                {
                    if (IsArchiveData)
                    {
                        BaseItems = await DataManager.GetSalesmanShops(User).Where(shop => shop.Status != ShopStatus.NotReviewed).IncludeTotalCount().Take(ItemsPerPage).ToListAsync();
                        TotalItemsCount = ((IQueryResultEnumerable<Shop>)BaseItems).TotalCount;
                    }
                    else
                    {
                        BaseItems = (await DataManager.GetSalesmanShops(User).Where(shop => ((shop.Status == ShopStatus.NotReviewed) || (shop.CreatedAt.Day == DateTime.Now.Day && shop.CreatedAt.Month == DateTime.Now.Month && shop.CreatedAt.Year == DateTime.Now.Year))).IncludeTotalCount().Take(ItemsPerPage).ToListAsync());
                        TotalItemsCount = ((IQueryResultEnumerable<Shop>)BaseItems).TotalCount;
                    }
                }
                else
                    throw new NotImplementedException("User must be of type Salesman");
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
