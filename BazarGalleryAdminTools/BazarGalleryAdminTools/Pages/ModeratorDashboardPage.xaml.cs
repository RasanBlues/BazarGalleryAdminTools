using BazarGalleryAdminTools.ViewModels;
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
	public partial class ModeratorDashboardPage : ContentPage
	{
        public ModeratorDashboardViewModel ViewModel { get; private set; }
        public ModeratorDashboardPage (SalesmenListViewModel listViewModel = null)
		{
			InitializeComponent ();
            Title = "داشبۆرد";
            BindingContext = 
            ViewModel = new ModeratorDashboardViewModel(this);
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