using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Text;
using BazarGallery.Models;
using BazarGallery;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;

namespace BazarGalleryAdminTools
{
    public class DataManager
    {
        public static DataManager Default
        {
            get
            {
                if (_Default == null)
                {
                    _Default = new DataManager();
                }
                return _Default;
            }
        }
        private static DataManager _Default;

        public static readonly HttpClient HttpClient = new HttpClient();

        public MobileServiceClient Client { get; private set; }

        public CloudStorageAccount CloudStorageAccount { get; private set; }
        public CloudBlobContainer AppContentContainer { get; private set; }
        public CloudBlobContainer UserContentContainer { get; private set; }
        public CloudBlobContainer ShopContentContainer { get; private set; }
        public CloudBlobContainer AlbumContentContainer { get; private set; }


        public IMobileServiceTable<User> UserTable { get; private set; }
        public IMobileServiceTable<Service> ServiceTable { get; private set; }
        public IMobileServiceTable<ServiceDescription> ServiceDescriptionTable { get; private set; }

        public IMobileServiceTable<Category> CategoryTable { get; private set; }
        public IMobileServiceTable<Subcategory> SubcategoryTable { get; private set; }
        public IMobileServiceTable<ShopSubcategory> ShopSubcategoryTable { get; private set; }


        public IMobileServiceTable<Shop> ShopTable { get; private set; }
        public IMobileServiceTable<ShopWorkingHours> ShopWorkingHoursTable { get; private set; }
        public IMobileServiceTable<Album> AlbumTable { get; private set; }
        public IMobileServiceTable<AlbumImage> AlbumImageTable { get; private set; }

        public DataManager()
        {
            Client = new MobileServiceClient(Constants.ApplicationURL);
            CloudStorageAccount = CloudStorageAccount.Parse(Constants.StorageConnectionString);

            AppContentContainer = CloudStorageAccount.CreateCloudBlobClient().GetContainerReference("appcontent");
            UserContentContainer = CloudStorageAccount.CreateCloudBlobClient().GetContainerReference("usercontent");
            ShopContentContainer = CloudStorageAccount.CreateCloudBlobClient().GetContainerReference("shopcontent");
            AlbumContentContainer = CloudStorageAccount.CreateCloudBlobClient().GetContainerReference("albumcontent");

            UserTable = Client.GetTable<User>();
            ServiceTable = Client.GetTable<Service>();
            ServiceDescriptionTable = Client.GetTable<ServiceDescription>();
            CategoryTable = Client.GetTable<Category>();
            SubcategoryTable = Client.GetTable<Subcategory>();
            ShopSubcategoryTable = Client.GetTable<ShopSubcategory>();


            ShopTable = Client.GetTable<Shop>();
            ShopWorkingHoursTable = Client.GetTable<ShopWorkingHours>();
            AlbumTable = Client.GetTable<Album>();
            AlbumImageTable = Client.GetTable<AlbumImage>();
        }

        public static IMobileServiceTableQuery<User> GetModeratorSalesmen(User masterUser){
            return Default.UserTable.Where(user => user.UserType == UserType.Salesman && user.MasterUserID == masterUser.ID);
        }
        public static IMobileServiceTableQuery<Shop> GetSalesmanShops(User user)
        {
            return Default.ShopTable.Where(masterShop => masterShop.SalesmenUserID == user.ID);
        }
        public static async Task<ShopWorkingHours> GetShopWorkingHoursAsync(Shop shop)
        {
            return (await Default.ShopWorkingHoursTable.Where(h => h.ShopID == shop.ID).Take(1).ToListAsync()).FirstOrDefault();
        }
    }
}
