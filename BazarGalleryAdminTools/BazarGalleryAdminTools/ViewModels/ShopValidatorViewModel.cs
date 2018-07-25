using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using BazarGallery.Models;
using BazarGalleryAdminTools.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using System.Linq;
using BazarGalleryAdminTools.Controls;
using BazarGallery;
using Microsoft.AppCenter.Crashes;
using System.Threading.Tasks;

namespace BazarGalleryAdminTools.ViewModels
{
    public partial class ShopValidatorViewModel : BaseViewModel
    {
        private bool _IsBusy;
        private bool _CanValidate;
        private string _UserDetails;

        public ShopValidatorPage Page { get; private set; }
        public Shop Shop { get; private set; }
        public List<Album> Albums { get; private set; }
        public List<string> TimeRanges => Constants.TimeRanges;

        public ICommand ApproveCommand { get; set; }
        public ICommand RejectCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand ShowImageCommand { get; private set; }
        public ICommand ShopCoverImageTappedCommand { get; private set; }
        public ICommand ShopImageTappedCommand { get; private set; }
        public bool IsBusy
        {
            get => _IsBusy;
            set
            {
                _IsBusy = value;
                OnPropertyChanged();
            }
        }
        public bool CanValidate
        {
            get => _CanValidate;
            set
            {
                _CanValidate = value;
                OnPropertyChanged();
            }
        }
        public string UserDetails
        {
            get => _UserDetails;
            set
            {
                _UserDetails = value;
                OnPropertyChanged();
            }
        }


        public ShopValidatorViewModel(ShopValidatorPage page, Shop shop, bool canValidate = false)
        {
            IsBusy = true;
            Page = page;
            Shop = shop;
            CanValidate = canValidate;

            ApproveCommand = new Command(ApproveAsync);
            RejectCommand = new Command(RejectAsync);
            CancelCommand = new Command(CancelAsync);
            ShowImageCommand = new Command(ShowImageAsync);
            ShopCoverImageTappedCommand = new Command(ShowShopCoverAsync);
            ShopImageTappedCommand = new Command(ShowShopImageAsync);


            Init();

            if (!string.IsNullOrEmpty(Shop.ID))
            {
                Pin _Pin = new Pin
                {
                    Type = PinType.Place,
                    Label = Shop.Name,
                    Position = new Position(Shop.Latitude, Shop.Longitude)
                };

                if (string.IsNullOrEmpty(_Pin.Label))
                    _Pin.Label = "SHOP.NAME";

                Page.Map.Pins.Add(_Pin);
                Page.Map.MoveToRegion(MapSpan.FromCenterAndRadius(_Pin.Position, new Distance(200)));
            }
        }
        private async void Init()
        {
            if (await UpdateUserDetails())
                try
                {
                    UpdateCategoryDetails();
                    WorkingHours = (await DataManager.Default.ShopWorkingHoursTable.Where(h => h.ShopID == Shop.ID).Take(1).ToListAsync()).FirstOrDefault();

                    Albums = await DataManager.Default.AlbumTable.Where(album => album.ShopID == Shop.ID).ToListAsync();
                    foreach (var album in Albums)
                    {
                        album.Images = await DataManager.Default.AlbumImageTable.Where(i => i.AlbumID == album.ID).ToListAsync();
                        Page.AlbumsStack.Children.Add(AlbumLayout.GetAlbumLayout(album, 80, false, null, null, ShowImageCommand));
                    }

                    var serviceDescriptionList = await DataManager.Default.ServiceDescriptionTable.Where(sd => sd.ShopID == Shop.ID).ToListAsync();
                    foreach (var serviceDescription in serviceDescriptionList)
                    {
                        var service = (await DataManager.Default.ServiceTable.Where(s => s.ID == serviceDescription.ServiceID).Take(1).ToListAsync()).First();

                        Page.ServicesStack.Children.Add(new Label()
                        {
                            Text = $"{service.KurdishName}: {serviceDescription.KurdishDescription}"
                        });
                    }
                }
                catch (System.Net.Http.HttpRequestException)
                {
                    CanValidate = false;
                    await Page.DisplayAlert("Error", $"Couldn't resolve host.{Environment.NewLine}Please check your internet connection and try again.", "OK");
                }
                catch (Exception e)
                {
                    CanValidate = false;
                    await Page.DisplayAlert("Error", e.ToString(), "OK");
                    Crashes.TrackError(e);
                }
                finally
                {
                    IsBusy = false;
                }
            else
            {
                CanValidate =
                IsBusy = false;
            }
        }
        private async Task<bool> UpdateUserDetails()
        {
            try
            {
                var user = await DataManager.Default.UserTable.LookupAsync(Shop.UserID);
                if (user != null)
                {
                    UserDetails = $"{user.Username} ({user.PhoneNumber})";
                    return true;
                }
                return false;
            }
            catch { return false; }
        }
        private async void UpdateCategoryDetails()
        {
            if (Categories_API.Categories.Count == 0)
                await Categories_API.LoadCategories();

            var categoryPairs = new Dictionary<Categories_API.Category_API, List<Subcategories_API.Subcategory_API>>();
            var subcategoryIDs = Shop.SubcategoryID.Split(',');
            var keywordIDs = Shop.KeywordID.Split(',');

            foreach (string subCatID in subcategoryIDs)
            {
                if (int.TryParse(subCatID, out int id))
                {
                    var subCat = Subcategories_API.Subcategories.FirstOrDefault(s => s.id == id);
                    if (subCat != null)
                    {
                        var cat = Categories_API.Categories.First(cw => cw.id == subCat.category_id);
                        if (!categoryPairs.ContainsKey(cat))
                            categoryPairs.Add(cat, new List<Subcategories_API.Subcategory_API>() { subCat });
                        else
                            categoryPairs[cat].Add(subCat);
                    }
                }
            }

            var formattedText = new FormattedString();
            string newLine = String.Empty;
            foreach (var keyValuePair in categoryPairs)
            {
                var key = keyValuePair.Key;
                var list = keyValuePair.Value;

                formattedText.Spans.Add(new Span() { Text = $"{newLine}{key.name_ku}", FontAttributes = FontAttributes.Bold });
                newLine = Environment.NewLine;

                foreach (var subCat in list)
                    formattedText.Spans.Add(new Span() { Text = $"{Environment.NewLine}     {subCat.name_ku}" });
            }
            if (keywordIDs.Length > 0)
            {
                formattedText.Spans.Add(new Span() { Text = $"{newLine}کیوۆردەکان", FontAttributes = FontAttributes.Bold });
                foreach (var keywordID in keywordIDs)
                {
                    if (int.TryParse(keywordID, out int id))
                    {
                        var keyword = Keywords_API.Keywords.First(k => k.id == id);
                        if (keyword != null)
                            formattedText.Spans.Add(new Span() { Text = $"{Environment.NewLine}     {keyword.name}" });
                    }
                }
            }
            Page.SubcategoriesLabel.FormattedText = formattedText;
        }

        public async void ApproveAsync()
        {
            if (IsBusy)
                return;
            IsBusy = true;
            if (await Page.DisplayAlert("دڵنیابوونەوە", "ئایا دڵنیایت لە پەسەندکردنی ئەم تۆمارە؟", "بەڵێ", "نەخێر"))
            {
                Shop.Status = ShopStatus.Approved;
                await ShopAPI.UpdateAsync(Shop, true);
                CancelAsync();
            }
            IsBusy = false;
        }
        public async void RejectAsync()
        {
            if (IsBusy)
                return;
            IsBusy = true;
            if (await Page.DisplayAlert("دڵنیابوونەوە", "ئایا دڵنیایت لە ڕەتکردنەوەی ئەم تۆمارە؟", "بەڵێ", "نەخێر"))
            {
                Shop.Status = ShopStatus.Declined;
                await ShopAPI.UpdateAsync(Shop, true);
                CancelAsync();
            }
            IsBusy = false;
        }
        public async void CancelAsync()
        {
            await Page.Navigation.PopAsync();
        }
        public async void ShowImageAsync(object obj)
        {
            object[] objArray = obj as object[];
            Album album = objArray[0] as Album;
            AlbumImage albumImage = objArray[1] as AlbumImage;
            var viewer = new PhotoViewerPage(albumImage) { Title = album.KurdishName };
            await Page.Navigation.PushAsync(viewer);
        }
        public async void ShowShopImageAsync()
        {
            var imgSource = Shop.ImageLink;
            var viewer = new PhotoViewerPage(imgSource);
            await Page.Navigation.PushAsync(viewer);
        }
        public async void ShowShopCoverAsync()
        {
            var viewer = new PhotoViewerPage((ImageSource)(Shop.CoverImageLink));
            await Page.Navigation.PushAsync(viewer);
        }
    }
}
