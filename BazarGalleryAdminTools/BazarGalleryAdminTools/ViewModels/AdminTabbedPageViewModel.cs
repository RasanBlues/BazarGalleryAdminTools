using BazarGallery;
using BazarGallery.Models;
using BazarGalleryAdminTools.Pages;
using Microsoft.AppCenter.Crashes;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace BazarGalleryAdminTools.ViewModels
{
    public class AdminTabbedPageViewModel : BaseViewModel
    {
        private bool _IsBusy;
        private int _SalesmenCount;
        private int _AdminsCount;
        private int _ModeratorsCount;
        private int _TranslatorsCount;
        private int _NormalUsersCount;
        private int _TotalUsersCount;
        private int _NotReviewedCount;
        private int _TotalShopsCount;
        private int _ApprovedCount;
        private int _DeclinedCount;
        private int _AlbumsCount;
        private int _ImagesCount;
        private int _ServiceDescriptionsCount;
        private int _CategoriesCount;
        private int _SubcategoriesCount;
        private int _KeywordsCount;
        private int _ServicesCount;

        public ICommand LogoutCommand { get; set; }
        public User CurrentUser { set; get; }
        public AdminTabbedPaged Page { get; private set; }

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
        public int SalesmenCount
        {
            get => _SalesmenCount;
            set
            {
                _SalesmenCount = value;
                OnPropertyChanged();
            }
        }
        public int AdminsCount
        {
            get => _AdminsCount;
            set
            {
                _AdminsCount = value;
                OnPropertyChanged();
            }
        }
        public int ModeratorsCount
        {
            get => _ModeratorsCount;
            set
            {
                _ModeratorsCount = value;
                OnPropertyChanged();
            }
        }
        public int TranslatorsCount
        {
            get => _TranslatorsCount;
            set
            {
                _TranslatorsCount = value;
                OnPropertyChanged();
            }
        }
        public int NormalUsersCount
        {
            get => _NormalUsersCount;
            set
            {
                _NormalUsersCount = value;
                OnPropertyChanged();
            }
        }
        public int TotalUsersCount
        {
            get => _TotalUsersCount;
            set
            {
                _TotalUsersCount = value;
                Page.UserCountSpanText = string.Format(" ({0} Total)", _TotalUsersCount);
            }
        }

        public int NotReviewedCount
        {
            get => _NotReviewedCount;
            set
            {
                _NotReviewedCount = value;
                OnPropertyChanged();
            }
        }
        public int TotalShopsCount
        {
            get => _TotalShopsCount;
            set
            {
                _TotalShopsCount = value;
                Page.ShopCountSpanText = string.Format(" ({0} Total)", _TotalShopsCount);
            }
        }
        public int ApprovedCount
        {
            get => _ApprovedCount;
            set
            {
                _ApprovedCount = value;
                OnPropertyChanged();
            }
        }
        public int DeclinedCount
        {
            get => _DeclinedCount;
            set
            {
                _DeclinedCount = value;
                OnPropertyChanged();
            }
        }

        public int AlbumsCount
        {
            get => _AlbumsCount;
            set
            {
                _AlbumsCount = value;
                OnPropertyChanged();
            }
        }
        public int ImagesCount
        {
            get => _ImagesCount;
            set
            {
                _ImagesCount = value;
                OnPropertyChanged();
            }
        }
        public int ServiceDescriptionsCount
        {
            get => _ServiceDescriptionsCount;
            set
            {
                _ServiceDescriptionsCount = value;
                OnPropertyChanged();
            }
        }
        public int CategoriesCount
        {
            get => _CategoriesCount;
            set
            {
                _CategoriesCount = value;
                OnPropertyChanged();
            }
        }
        public int SubcategoriesCount
        {
            get => _SubcategoriesCount;
            set
            {
                _SubcategoriesCount = value;
                OnPropertyChanged();
            }
        }
        public int KeywordsCount
        {
            get => _KeywordsCount;
            set
            {
                _KeywordsCount = value;
                OnPropertyChanged();
            }
        }
        public int ServicesCount
        {
            get => _ServicesCount;
            set
            {
                _ServicesCount = value;
                OnPropertyChanged();
            }
        }

        public AdminTabbedPageViewModel(AdminTabbedPaged page)
        {
            Page = page;
            CurrentUser = User.Current;

            LogoutCommand = new Command(async () =>
            {
                User.Logout();
                await Page.Navigation.PushAsync(new LoginPage());
                Page.Navigation.RemovePage(Page);
            });
        }
        public async void LoadData()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                var query = await DataManager.Default.UserTable.Take(0).Where(user => (user.UserType == UserType.Admin)).IncludeTotalCount().ToEnumerableAsync();
                AdminsCount = Convert.ToInt32(((IQueryResultEnumerable<User>)query).TotalCount);
                query = await DataManager.Default.UserTable.Take(0).Where(user => (user.UserType == UserType.Moderator)).IncludeTotalCount().ToEnumerableAsync();
                ModeratorsCount = Convert.ToInt32(((IQueryResultEnumerable<User>)query).TotalCount);
                query = await DataManager.Default.UserTable.Take(0).Where(user => (user.UserType == UserType.Salesman)).IncludeTotalCount().ToEnumerableAsync();
                SalesmenCount = Convert.ToInt32(((IQueryResultEnumerable<User>)query).TotalCount);
                query = await DataManager.Default.UserTable.Take(0).Where(user => (user.UserType == UserType.Translator)).IncludeTotalCount().ToEnumerableAsync();
                TranslatorsCount = Convert.ToInt32(((IQueryResultEnumerable<User>)query).TotalCount);
                query = await DataManager.Default.UserTable.Take(0).Where(user => (user.UserType == UserType.User)).IncludeTotalCount().ToEnumerableAsync();
                NormalUsersCount = Convert.ToInt32(((IQueryResultEnumerable<User>)query).TotalCount);
                TotalUsersCount = AdminsCount + ModeratorsCount + SalesmenCount + TranslatorsCount + NormalUsersCount;

                var shopQuery = await DataManager.Default.ShopTable.Take(0).Where(shop => (shop.Status == ShopStatus.NotReviewed)).IncludeTotalCount().ToEnumerableAsync();
                NotReviewedCount = Convert.ToInt32(((IQueryResultEnumerable<Shop>)shopQuery).TotalCount);
                shopQuery = await DataManager.Default.ShopTable.Take(0).Where(shop => (shop.Status == ShopStatus.Approved)).IncludeTotalCount().ToEnumerableAsync();
                ApprovedCount = Convert.ToInt32(((IQueryResultEnumerable<Shop>)shopQuery).TotalCount);
                shopQuery = await DataManager.Default.ShopTable.Take(0).Where(shop => (shop.Status == ShopStatus.Declined)).IncludeTotalCount().ToEnumerableAsync();
                DeclinedCount = Convert.ToInt32(((IQueryResultEnumerable<Shop>)shopQuery).TotalCount);

                TotalShopsCount = NotReviewedCount + ApprovedCount + DeclinedCount;

                var albumQuery = await DataManager.Default.AlbumTable.Take(0).IncludeTotalCount().ToEnumerableAsync();
                AlbumsCount = Convert.ToInt32(((IQueryResultEnumerable<Album>)albumQuery).TotalCount);

                var imageQuery = await DataManager.Default.AlbumImageTable.Take(0).IncludeTotalCount().ToEnumerableAsync();
                ImagesCount = Convert.ToInt32(((IQueryResultEnumerable<Album>)albumQuery).TotalCount);

                var serviceDescriptionsQuery = await DataManager.Default.ServiceDescriptionTable.Take(0).IncludeTotalCount().ToEnumerableAsync();
                ServiceDescriptionsCount = Convert.ToInt32(((IQueryResultEnumerable<ServiceDescription>)serviceDescriptionsQuery).TotalCount);

                var servicesQuery = await DataManager.Default.ServiceTable.Take(0).IncludeTotalCount().ToEnumerableAsync();
                ServicesCount = Convert.ToInt32(((IQueryResultEnumerable<Service>)servicesQuery).TotalCount);

                if (Categories_API.Categories.Count == 0)
                    await Categories_API.LoadCategories();
                
                CategoriesCount = Categories_API.Categories.Count;
                KeywordsCount = Keywords_API.Keywords.Count;
                SubcategoriesCount = Subcategories_API.Subcategories.Count;
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

            IsBusy = false;
        }
    }
}
