using BazarGallery;
using BazarGallery.Models;
using BazarGalleryAdminTools.Pages;
using Microsoft.AppCenter.Crashes;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BazarGalleryAdminTools.ViewModels
{
    public class SalesmanDashboardViewModel : BaseViewModel
    {
        public SalesmanDashboardPage Page { get; private set; }

        private int _SubmittedShopsCount;
        private int _ReviewedShopsCount;
        private int _ApprovedShopsCount;
        private int _DeclinedShopsCount;
        private bool _IsBusy;

        public User CurrentUser { set; get; }

        public int SubmittedShopsCount
        {
            get => _SubmittedShopsCount;
            set
            {
                _SubmittedShopsCount = value;
                OnPropertyChanged();
            }
        }
        public int ReviewedShopsCount
        {
            get => _ReviewedShopsCount;
            set
            {
                _ReviewedShopsCount = value;
                OnPropertyChanged();
            }
        }
        public int ApprovedShopsCount
        {
            get => _ApprovedShopsCount;
            set
            {
                _ApprovedShopsCount = value;
                OnPropertyChanged();
            }
        }
        public int DeclinedShopsCount
        {
            get => _DeclinedShopsCount;
            set
            {
                _DeclinedShopsCount = value;
                OnPropertyChanged();
            }
        }

        public bool IsBusy
        {
            get => _IsBusy;
            set
            {

                _IsBusy =
                Page.IsBusy = value;

                OnPropertyChanged();
                OnPropertyChanged("CanLogout");
            }
        }

        public bool CanLogout
        {
            get => !IsBusy;
        }

        public ICommand LogoutCommand { get; set; }

        public SalesmanDashboardViewModel(SalesmanDashboardPage page)
        {
            Page = page;
            CurrentUser = User.Current;

            LogoutCommand = new Command(async () =>
            {
                User.Logout();
                await Page.Navigation.PushAsync(new LoginPage());
                Page.Navigation.RemovePage((Page)Page.Parent);
            });
        }

        public async void LoadData()
        {
            IsBusy = true;
            try
            {
                var query = (await DataManager.Default.ShopTable.Take(0).Where(shop => (shop.SalesmenUserID == User.Current.ID) && (shop.Status == ShopStatus.NotReviewed)).IncludeTotalCount().ToEnumerableAsync());
                var notReviewed = Convert.ToInt32(((IQueryResultEnumerable<Shop>)query).TotalCount);
                query = (await DataManager.Default.ShopTable.Take(0).Where(shop => (shop.SalesmenUserID == User.Current.ID) && (shop.Status == ShopStatus.Approved)).IncludeTotalCount().ToEnumerableAsync());
                ApprovedShopsCount = Convert.ToInt32(((IQueryResultEnumerable<Shop>)query).TotalCount);
                query = (await DataManager.Default.ShopTable.Take(0).Where(shop => (shop.SalesmenUserID == User.Current.ID) && (shop.Status == ShopStatus.Declined)).IncludeTotalCount().ToEnumerableAsync());
                DeclinedShopsCount = Convert.ToInt32(((IQueryResultEnumerable<Shop>)query).TotalCount);
                ReviewedShopsCount = ApprovedShopsCount + DeclinedShopsCount;
                SubmittedShopsCount = notReviewed + ApprovedShopsCount + DeclinedShopsCount;
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
