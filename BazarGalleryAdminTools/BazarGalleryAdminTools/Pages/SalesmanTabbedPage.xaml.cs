using BazarGallery;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BazarGalleryAdminTools.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SalesmanTabbedPage : TabbedPage
    {
        public ListViewPage TodayPage { get; private set; }
        public ListViewPage ArchivePage { get; private set; }

        public SalesmanTabbedPage(bool includeDashboard = true, User user = null, bool hasAddButton = true)
        {
            InitializeComponent();
            TodayPage = new ListViewPage(hideFooter: !hasAddButton) { Title = "داتای ئەمڕۆ" };
            ArchivePage = new ListViewPage(hideFooter: !hasAddButton) { Title = "ئارشیف" };


            var todayPageViewModel = new ViewModels.ShopsListViewModel(TodayPage, user != null ? user : User.Current, false, true);
            var archivePageViewModel = new ViewModels.ShopsListViewModel(ArchivePage, user != null ? user : User.Current, true, true);


            TodayPage.BindingContext = todayPageViewModel;
            ArchivePage.BindingContext = archivePageViewModel;

            todayPageViewModel.OnRefresh.Execute(null);
            archivePageViewModel.OnRefresh.Execute(null);


            Children.Add(TodayPage);
            Children.Add(ArchivePage);
            if (includeDashboard)
            {
                var dashboardPage = new SalesmanDashboardPage() { Title = "داشبۆرد" };
                Children.Add(dashboardPage);
            }
        }
    }
}