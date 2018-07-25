using Xamarin.Forms;

namespace BazarGallery
{
    public static partial class Constants
    {

        public const string ApplicationURL = "https://bazargallery.azurewebsites.net";
        public const string ApplicationScheme = "com.jobstationco.bazargallery";
        public const string StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=bazargallerydiag261;AccountKey=Yr3yJ4U8IVSCh1qzFkhNmP2CRbYEHaOrmBESJCDutv1PFfMaUovbSTC8flOMUcwu3NA9UHSainVd8c7cY4SQEA==;EndpointSuffix=core.windows.net";
        public const string LocalPassword = "@!_PASS_!@";
        public const string LocalImageLink = "@!_IMAGE_!@";
        public const string EmptyImagePlaceholder = "empty_image.png";
        public const string DownloadingPlaceholderImage = "downloading_placeholder.png";

        public const string CategoryTableQueryID = "AllCategories";
        public const string SubcategoryTableQueryID = "AllSubcategories";
        public const string ShopTableQueryID = "AllShops";
        public const string FavouritesTableQueryID = "Favourites";

        public const string OAuthProperty = "UserOAuth";
        public const string OAuthPhoneNumber = "UserOAuthPhoneNumber";
        public const string OAuthPassword = "UserOAuthPass";

        public const string FacebookURL = "https://www.fb.com/{0}";
        public const string InstagramURL = "https://www.instagram.com/{0}";
        public const string TwitterURL = "https://www.twitter.com/{0}";
        public const string YouTubeURL = "https://www.youtube.com/user/{0}";
        public const string SnapchatURL = "https://www.snapchat.com/add/{0}";

        /// <summary>
        /// Format in which the phone numbers must be filled
        /// </summary>
        public const string PhoneNumberFormat = "07xx xxx xxxx";

        /// <summary>
        /// the top offset on the iPhone X
        /// </summary>
        public const double iPhoneXTopOffset = 32;

        /// <summary>
        /// the top offset on iPhone devices, for iPhone X use <see cref="iPhoneXTopOffset"/>
        /// </summary>
        public const double iPhoneTopOffset = 20;
    }
}
