using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BazarGalleryAdminTools.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ModeratorTabbedPage : TabbedPage
    {
        public ModeratorTabbedPage()
        {
            InitializeComponent();

            var moderatorPage = new ModeratorPage() { Title = "ڕیسێرچەرەکان" };

            Children.Add(moderatorPage);
            Children.Add(new ModeratorDashboardPage(moderatorPage.ViewModel));
        }
    }
}