using BazarGallery.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BazarGalleryAdminTools.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ShopValidatorPage : TabbedPage
    {
        public CustomMap Map => UIMapView;
        public StackLayout AlbumsStack => UIAlbumsStack;
        public Grid TimeGrid => UITimeGrid;
        public StackLayout ServicesStack => UIServicesStack;
        public Label SubcategoriesLabel => UISubcategoriesLabel;

        public ShopValidatorPage()
        {
            InitializeComponent();

            BindingContext = new ViewModels.ShopValidatorViewModel(this, new BazarGallery.Models.Shop());
        }
        public ShopValidatorPage(BazarGallery.Models.Shop shop, bool canValidate=false)
        {
            InitializeComponent();
            BindingContext = new ViewModels.ShopValidatorViewModel(this, shop,canValidate);
        }
    }
}