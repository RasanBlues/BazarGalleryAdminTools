using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using BazarGalleryAdminTools.Pages;
using System.Threading.Tasks;
using BazarGallery;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Distribute;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace BazarGalleryAdminTools
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            //var task = Task.Run(async () =>
            //{

            //    if (Current.Properties.ContainsKey(Constants.OAuthProperty))
            //    {
            //        string localOAuth = (string)Current.Properties[Constants.OAuthProperty];
            //        var userTableTask = await DataManager.Default.UserTable.Where(u => (u.OAuth == localOAuth)).ToListAsync();
            //        //userTableTask.RunSynchronously();
            //        //if (userTableTask.Wait(10000))
            //        {
            //            var result = userTableTask;//userTableTask.Result;
            //            if (result.Count > 0)
            //            {
            //                User.Login(result[0]);
            //            }
            //        }
            //    }
            //});

            //task.Wait(3000);
            //if (User.IsLoggedIn)
            //{
            //    var user = User.Current;
            //    if (user.UserType == UserType.Admin)
            //        MainPage = new NavigationPage(GetNewAdminPage());
            //    else if (user.UserType == UserType.Salesman)
            //        MainPage = GetNewNavigationPage(new SalesmenTabbedPage1());

            //}
            //else
            MainPage = GetNewNavigationPage(new LoginPage());
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            AppCenter.Start("android=797c27fa-d4ee-4903-8c23-c71a01549051;uwp=2dfe2387-4a63-4127-89bb-b7179936f45c", typeof(Analytics), typeof(Crashes), typeof(Distribute));
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

        
        public static NavigationPage GetNewNavigationPage(Page root)
        {
            return new NavigationPage(root) { FlowDirection = FlowDirection.RightToLeft };
        }
    }
}