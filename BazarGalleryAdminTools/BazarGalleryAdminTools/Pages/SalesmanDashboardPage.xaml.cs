using BazarGallery;
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
    public partial class SalesmanDashboardPage : ContentPage
    {
        public ViewModels.SalesmanDashboardViewModel ViewModel { get; private set; }
        public SalesmanDashboardPage()
        {
            InitializeComponent();

            BindingContext =
            ViewModel = new ViewModels.SalesmanDashboardViewModel(this);
            ViewModel.LoadData();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (!IsBusy)
                ViewModel.LoadData();
        }
    }
}