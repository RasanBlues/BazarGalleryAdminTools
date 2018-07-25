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
    public class ModeratorDashboardViewModel : BaseViewModel
    {
        private int _SubmittedShopsCount;
        private int _ReviewedShopsCount;
        private int _ApprovedShopsCount;
        private int _DeclinedShopsCount;
        private int _ResearcherCount;

        public ModeratorDashboardPage Page { get; private set; }
        
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
        public int ResearcherCount
        {
            get => _ResearcherCount;
            set
            {
                _ResearcherCount = value;
                OnPropertyChanged();
            }
        }

        public ICommand LogoutCommand { get; set; }

        public ModeratorDashboardViewModel(ModeratorDashboardPage page)
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
            if (Page.IsBusy == true)
                return;
            Page.IsBusy = true;
            try
            {
                var notReviewed = 0;
                var approvedShopsCount = 0;
                var declinedShopsCount = 0;
                var users = await DataManager.Default.UserTable.Where(user => (user.MasterUserID == User.Current.ID) && user.UserType == UserType.Salesman).ToListAsync();
                ResearcherCount = users.Count;
                foreach (User user in users)
                {
                    var query = (await DataManager.Default.ShopTable.Take(0).Where(shop => (shop.SalesmenUserID == user.ID) && (shop.Status == ShopStatus.NotReviewed)).IncludeTotalCount().ToEnumerableAsync());
                    notReviewed += Convert.ToInt32(((IQueryResultEnumerable<Shop>)query).TotalCount);
                    query = (await DataManager.Default.ShopTable.Take(0).Where(shop => (shop.SalesmenUserID == user.ID) && (shop.Status == ShopStatus.Approved)).IncludeTotalCount().ToEnumerableAsync());
                    approvedShopsCount += Convert.ToInt32(((IQueryResultEnumerable<Shop>)query).TotalCount);
                    query = (await DataManager.Default.ShopTable.Take(0).Where(shop => (shop.SalesmenUserID == user.ID) && (shop.Status == ShopStatus.Declined)).IncludeTotalCount().ToEnumerableAsync());
                    declinedShopsCount += Convert.ToInt32(((IQueryResultEnumerable<Shop>)query).TotalCount);
                }
                ApprovedShopsCount = approvedShopsCount;
                DeclinedShopsCount = declinedShopsCount;
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


            Page.IsBusy = false;
        }
    }
}
