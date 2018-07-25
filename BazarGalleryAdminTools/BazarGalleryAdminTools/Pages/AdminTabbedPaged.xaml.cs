using BazarGallery;
using BazarGalleryAdminTools.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BazarGalleryAdminTools.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AdminTabbedPaged : TabbedPage
    {
        public AdminTabbedPageViewModel ViewModel { get; private set; }
        public string UserCountSpanText
        {
            get => UIUserCountSpan.Text;
            set => UIUserCountSpan.Text = value;
        }
        public string ShopCountSpanText
        {
            get => UIShopCountSpan.Text;
            set => UIShopCountSpan.Text = value;
        }
        public AdminTabbedPaged()
        {
            InitializeComponent();
            BindingContext = ViewModel = new ViewModels.AdminTabbedPageViewModel(this);
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (!IsBusy)
                ViewModel.LoadData();
        }
        public static AdminTabbedPaged GetNewAdminPage()
        {
            var adminPage = new AdminTabbedPaged();

            var moderatorsPage = new ListViewPage() { Title = "چاودێر" };
            var salesmenPage = new ListViewPage() { Title = "ڕیسێرچەر" };
            var translatorsPage = new ListViewPage() { Title = "وەرگێڕ" };
            var usersPage = new ListViewPage() { Title = "بەکارهێنەر" };

            var moderatorsViewModel = new ModeratorsListViewModel(moderatorsPage);
            var salesmenViewModel = new SalesmenListViewModel(salesmenPage);
            var translatorsViewModel = new TranslatorsListViewModel(translatorsPage);
            var usersViewModel = new UsersListViewModel(usersPage);

            moderatorsViewModel.OnRefresh.Execute(null);
            salesmenViewModel.OnRefresh.Execute(null);
            translatorsViewModel.OnRefresh.Execute(null);
            usersViewModel.OnRefresh.Execute(null);

            moderatorsPage.BindingContext = moderatorsViewModel;
            salesmenPage.BindingContext = salesmenViewModel;
            translatorsPage.BindingContext = translatorsViewModel;
            usersPage.BindingContext = usersViewModel;

            adminPage.Children.Add(moderatorsPage);
            adminPage.Children.Add(salesmenPage);
            adminPage.Children.Add(translatorsPage);
            adminPage.Children.Add(usersPage);

            return adminPage;
        }

    }
}