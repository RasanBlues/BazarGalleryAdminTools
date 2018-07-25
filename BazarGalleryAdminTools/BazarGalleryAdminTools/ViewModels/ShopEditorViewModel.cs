using System;
using System.Collections.Generic;
using System.Windows.Input;
using BazarGallery;
using BazarGallery.Models;
using BazarGalleryAdminTools.Pages;
using Xamarin.Forms;
using System.Linq;
using Plugin.Media.Abstractions;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Microsoft.AppCenter.Crashes;
using System.IO;
using Microsoft.WindowsAzure.Storage.Core.Util;

namespace BazarGalleryAdminTools.ViewModels
{
    public class ShopEditorViewModel : BaseViewModel
    {
        private bool _IsBusy;
        private string _ProgressText;
        private string _CurrentOperation;
#if STORAGE_PROGRESS
        private StorageProgressHandler _StorageProgressHandler;
#endif
        public ShopEditorPage Page { get; private set; }
        public Shop Shop { get; private set; }
        public ShopWorkingHours WorkingHours { get; set; }
        public List<Album> Albums { get; private set; } = new List<Album>();
        public List<string> TimePickerItems => Constants.TimeRanges;

#if STORAGE_PROGRESS
        public StorageProgressHandler StorageProgressHandler
        {
            get
            {
                if (_StorageProgressHandler == null)
                {
                    _StorageProgressHandler = new StorageProgressHandler();
                    _StorageProgressHandler.OnProgressPercentChanged += OnProgressPercentChanged;
                }
                return _StorageProgressHandler;
            }
        }
#endif
        public bool IsBusy
        {
            get => _IsBusy;
            set
            {
                _IsBusy = value;
                OnPropertyChanged();
            }
        }
        public string ProgressText
        {
            get => _ProgressText;
            set
            {
                _ProgressText = value;
                OnPropertyChanged();
            }
        }
        public string CurrentOperation
        {
            get => _CurrentOperation;
            set
            {
                _CurrentOperation = value;
                ProgressText = $"{value}...";
            }
        }

        public ImageSource CoverImageSource
        {
            get => _CoverImageSource;
            set
            {
                _CoverImageSource = value;
                OnPropertyChanged();
            }
        }
        public ImageSource ThumbnailImageSource
        {
            get => _ThumbnailImageSource;
            set
            {
                _ThumbnailImageSource = value;
                OnPropertyChanged();
            }
        }

        public MediaFile CoverMediaFile { get; private set; }
        public MediaFile ThumbnailMediaFile { get; private set; }
        private ImageSource _CoverImageSource = Constants.EmptyImagePlaceholder;
        private ImageSource _ThumbnailImageSource = Constants.EmptyImagePlaceholder;

        public ICommand CoverImageTappedCommand { get; private set; }
        public ICommand ThumbnailImageTappedCommand { get; private set; }
        public ICommand AddProviderCommand { get; private set; }
        public ICommand AddAlbumCommand { get; private set; }
        public ICommand DeleteAlbumCommand { get; private set; }
        public ICommand AddAlbumImageCommand { get; set; }
        public ICommand EditAlbumImageCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand MoreTabCommand { get; set; }
        public ICommand GalleryTabCommand { get; set; }
        public ICommand CopyTimeCommand { get; private set; }

        public ShopEditorViewModel(ShopEditorPage page, Shop shop, Action onSaved = null)
        {
            Page = page;
            Shop = shop;


            CoverImageTappedCommand = new Command(async () =>
            {
                var mediaFile = await Media.GetMediaFile(Page);
                if (mediaFile != null)
                {
                    CoverMediaFile = mediaFile;
                    CoverImageSource = ImageSource.FromStream(CoverMediaFile.GetStream);
                }
            });
            ThumbnailImageTappedCommand = new Command(async () =>
            {
                var mediaFile = await Media.GetMediaFile(Page);
                if (mediaFile != null)
                {
                    ThumbnailMediaFile = mediaFile;
                    ThumbnailImageSource = ImageSource.FromStream(ThumbnailMediaFile.GetStream);
                }
            });
            AddAlbumCommand = new Command((object parameter) =>
            {
                Entry entry = parameter as Entry;
                if (!string.IsNullOrEmpty(entry.Text))
                {
                    Albums.Add(new Album() { KurdishName = entry.Text });
                    Page.RefreshAlbumsLayout();
                }
            });
            DeleteAlbumCommand = new Command(async (object parameter) =>
            {
                Album album = parameter as Album;
                bool shouldDelete = await Page.DisplayAlert($"سڕینەوەی ئەلبوم", $"ئایا دڵنیایت لە سڕینەوەی ئەلبومی '{album.KurdishName}'؟", "بەڵێ", "نەخێر");
                if (shouldDelete)
                {
                    if (!string.IsNullOrEmpty(album.ID))
                        await ShopAPI.DeleteShopAlbum(album);

                    Albums.Remove(album);
                    Page.RefreshAlbumsLayout();
                }

            });
            AddAlbumImageCommand = new Command(async (object parameter) =>
            {
                Album album = parameter as Album;
                AlbumImage image = new AlbumImage() { AlbumID = album.ID, ImageLink = Constants.EmptyImagePlaceholder };
                BaseGalleryItemEditorPage editorPage = new BaseGalleryItemEditorPage();
                BaseGalleryItemEditorViewModel viewModel = new BaseGalleryItemEditorViewModel(
                    page: editorPage,
                    image: image,
                    album: album,
                    saveAction: async () =>
                    {
                        image.Currency = editorPage.ItemCurrency;
                        album.Images.Add(image);
                        Page.RefreshAlbumsLayout();
                        await Page.Navigation.PopModalAsync();
                    },
                    deleteAction: null,
                    canCancel: true,
                    canDelete: false,
                    hasMetaData: (album.EnglishName == "Address" || album.EnglishName == "General") ? false : true);

                editorPage.BindingContext = viewModel;
                await Page.Navigation.PushModalAsync(editorPage);
            });
            EditAlbumImageCommand = new Command(async (object parameter) =>
            {
                object[] objArray = parameter as object[];
                Album album = (Album)objArray[0];
                AlbumImage image = (AlbumImage)objArray[1];
                BaseGalleryItemEditorPage editorPage = new BaseGalleryItemEditorPage();
                BaseGalleryItemEditorViewModel viewModel = new BaseGalleryItemEditorViewModel(
                    page: editorPage,
                    image: image,
                    album: album,
                    saveAction: async () =>
                    {
                        image.Currency = editorPage.ItemCurrency;
                        Page.RefreshAlbumsLayout();
                        await Page.Navigation.PopModalAsync();
                    },
                    deleteAction: async () =>
                    {
                        try
                        {
                            album.Images.Remove(image);
                            if (!string.IsNullOrEmpty(image.ID))
                            {
                                await DataManager.Default.AlbumImageTable.DeleteAsync(image);
                            }
                            Page.RefreshAlbumsLayout();
                            await Page.Navigation.PopModalAsync();
                        }
                        catch (Exception e)
                        {
                            Crashes.TrackError(e);
                            await Page.DisplayAlert("کێشەیەک ڕوویدا", e.ToString(), "باشە");
                        }
                    },
                    canCancel: true,
                    canDelete: true,
                    hasMetaData: (album.EnglishName == "Address" || album.EnglishName == "General") ? false : true);

                editorPage.BindingContext = viewModel;
                editorPage.ItemCurrency = image.Currency;
                await Page.Navigation.PushModalAsync(editorPage);
            });
            AddProviderCommand = new Command(async () =>
            {
                if (IsBusy)
                    return;
                IsBusy = true;

                var editorPage = new BaseMasterUserEditorPage(false, false);
                editorPage.PhoneNumber = Page.ProviderPhoneNumber;
                BaseMasterUserEditorPageViewModel viewModel = null;
                viewModel = new BaseMasterUserEditorPageViewModel(page: editorPage,
                save: async () =>
                {
                    viewModel.IsBusy = true;

                    if ((await DataManager.Default.UserTable.Where(u => u.PhoneNumber == editorPage.PhoneNumber).ToListAsync()).FirstOrDefault() != null)
                    {
                        await Page.DisplayAlert("Error", "Phone number is associated with other account", "OK");
                        viewModel.IsBusy = false;
                        return;
                    }

                    User user = new User()
                    {
                        Username = editorPage.Username,
                        PhoneNumber = editorPage.PhoneNumber,
                        Password = editorPage.Password,
                        MasterUserID = User.Current.ID,
                        UserType = UserType.User,
                    };

                    await RegisterAPI.InsertAsync(user);

                    viewModel.IsBusy = false;

                    if (Page.ProviderPhoneNumber != user.PhoneNumber)
                        Page.ProviderPhoneNumber = user.PhoneNumber;

                    Page.UpdateProvider(user);
                    await Page.Navigation.PopModalAsync();
                });
                editorPage.ViewModel = viewModel;


                await Page.Navigation.PushModalAsync(editorPage);

                IsBusy = false;
            });

            SaveCommand = new Command(async () =>
            {
                await SaveAsync();
                onSaved?.Invoke();
            });

            CancelCommand = new Command(CancelAsync);
            MoreTabCommand = new Command(MoreTab);
            GalleryTabCommand = new Command(GalleryTab);

            CopyTimeCommand = new Command(Page.CopyTime);


            Load();

            if (!string.IsNullOrEmpty(Shop.CoverImageLink))
                CoverImageSource = Shop.CoverImageLink;
            if (!string.IsNullOrEmpty(Shop.ImageLink))
                ThumbnailImageSource = Shop.ImageLink;
        }

        private void OnProgressPercentChanged(int progress)
        {
            ProgressText = $"{CurrentOperation}... {progress}%";
        }

        public async void Load()
        {
            WorkingHours = (await DataManager.Default.ShopWorkingHoursTable.Where(s => s.ShopID == Shop.ID).Take(1).ToListAsync()).FirstOrDefault();
            if (WorkingHours == null)
                WorkingHours = new ShopWorkingHours();

            Page.InitTimeGrid();

            Albums = await DataManager.Default.AlbumTable.Where(a => a.ShopID == Shop.ID).ToListAsync();
            if (Albums.Count == 0)
            {
                Albums.Add(new Album() { KurdishName = Constants.KurdishAddressAlbumName, EnglishName = "Address", ArabicName = "عُنوان" });
                Albums.Add(new Album() { KurdishName = Constants.KurdishGeneralAlbumName, EnglishName = "General", ArabicName = "عام" });
            }
            else
            {
                foreach (var album in Albums)
                    album.Images = await DataManager.Default.AlbumImageTable.Where(i => i.AlbumID == album.ID).ToListAsync();
            }
            Page.RefreshAlbumsLayout();
        }

        public async void CancelAsync()
        {
            var cancel = string.IsNullOrWhiteSpace(Shop.KurdishName);
            if (!cancel)
                cancel = await Page.DisplayAlert("Unsaved Changes", "Are you sure you want to cancel?", "Yes", "No");

            if (cancel)
                await Page.Navigation.PopModalAsync();
        }

        public async Task SaveAsync()
        {
            if (string.IsNullOrEmpty(Shop.KurdishName))
                await Page.DisplayAlert("Error", "تکایە ناوی دوکانەکە پڕ بکەرەوە", "باشە");
            else if (string.IsNullOrEmpty(Shop.UserID))
                await Page.DisplayAlert("Error", "تکایە خاوەن دوکانەکە پڕ بکەرەوە", "باشە");
            else if (await Page.DisplayAlert("تۆمارکردنی دوکان", "ئایا دڵنیایت لە تۆمارکردنی ئەم دوکانە؟", "بەڵێ", "نەخێر"))
            {
                IsBusy = true;

                if (Page.IsUsingCustomLocation)
                {
                    Shop.Latitude = Page.CustomLatitude;
                    Shop.Longitude = Page.CustomLongitude;
                }
                else if (!await BazarGallery.Location.CheckPermissionsAsync())
                {
                    await Page.DisplayAlert("کێشەیەک ڕوویدا", "پێویستمان بەڕەزامەندیت هەیە لە وەرگرتنی لۆکەیشنی مۆبایلەکەت، تکایە هەوڵبدەرەوە ", "باشە");
                    IsBusy = false;
                    return;
                }
                else
                {
                    CurrentOperation = "Getting Location Coordinates";
                    var location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best, new TimeSpan(0, 0, 20)));

                    if (location != null)
                    {
                        Shop.Latitude = location.Latitude;
                        Shop.Longitude = location.Longitude;
                    }
                    else
                    {
                        await Page.DisplayAlert("کێشەیەک ڕوویدا", "نەمانتوانی لۆکەیشن دیاری بکەین، دڵنیابەرەوە لەوەی کە لۆکەیشنی مۆبایلەکەت کارایە", "باشە");
                        IsBusy = false;
                        return;
                    }
                }

                try
                {
                    Shop.IsSponsored = Page.ShopIsSponsored;
                    Shop.Priority = Page.ShopPriority;
                    Shop.Status = ShopStatus.NotReviewed;

                    string textID = "";
                    int count = Page.SubcategorySwitches.Count;
                    for (int i = 0; i < count; i++)
                    {
                        var keyValuePair = Page.SubcategorySwitches.ElementAt(i);
                        if (keyValuePair.Key.IsToggled)
                        {
                            var subcatAPI = keyValuePair.Value;
                            #region Expiremental
                            //var subCat = (await DataManager.Default.SubcategoryTable.Where(subC => subC.OldID == subcatAPI.id).Take(1).ToListAsync()).FirstOrDefault();
                            //if (subCat == null)
                            //{
                            //    var cat = (await DataManager.Default.CategoryTable.Where(c => c.OldID == subcatAPI.category_id).Take(1).ToListAsync()).FirstOrDefault();
                            //    if (cat == null)
                            //    {
                            //        var catAPI = Categories_API.Categories.Where(c => c.id == subcatAPI.category_id).FirstOrDefault();
                            //        cat = new Category()
                            //        {
                            //            KurdishName = catAPI.name_ku,
                            //            EnglishName = catAPI.name_en,
                            //            ArabicName = catAPI.name_ar,
                            //            OldID = catAPI.id
                            //        };
                            //        await DataManager.Default.CategoryTable.InsertAsync(cat);
                            //    }

                            //    subCat = new Subcategory()
                            //    {
                            //        KurdishName = subcatAPI.name_ku,
                            //        EnglishName = subcatAPI.name_en,
                            //        ArabicName = subcatAPI.name_ar,
                            //        OldID = subcatAPI.id
                            //    };

                            //    await DataManager.Default.SubcategoryTable.InsertAsync(subCat);
                            //}

                            //var shopSubCat = (await DataManager.Default.ShopSubcategoryTable.Where(c => c.SubcategoryID == subCat.ID).Take(1).ToListAsync()).FirstOrDefault();

                            //if (shopSubCat == null && !isOn)
                            //    continue;

                            //shopSubCat = new ShopSubcategory()
                            //{
                            //    ShopID = Shop.ID,
                            //    SubcategoryID = subCat.ID
                            //};
                            #endregion
                            textID += $"{subcatAPI.id}" + ((i < count - 1) ? "," : "");
                        }
                    }
                    Shop.SubcategoryID = textID;

                    textID = "";
                    count = Page.KeywordSwitches.Count;
                    for (int i = 0; i < count; i++)
                    {
                        var keyValuePair = Page.KeywordSwitches.ElementAt(i);
                        if (keyValuePair.Key.IsToggled)
                        {
                            var keywordAPI = keyValuePair.Value;
                            textID += $"{keywordAPI.id}" + ((i < count - 1) ? "," : "");
                        }
                    }
                    Shop.KeywordID = textID;


                    if (ThumbnailMediaFile != null)
                    {
                        CurrentOperation = "Uploading Thumbnail";
                        if (!string.IsNullOrEmpty(Shop.ImageLink))
                        {
                            string filename = Path.GetFileName(Shop.ImageLink);
                            var blockRef = DataManager.Default.ShopContentContainer.GetBlockBlobReference(filename);
                            await blockRef.DeleteIfExistsAsync(Microsoft.WindowsAzure.Storage.Blob.DeleteSnapshotsOption.IncludeSnapshots, null, null, null);
                        }
                        var stream = ThumbnailMediaFile.GetStream();
                        var blockBlob = DataManager.Default.ShopContentContainer.GetBlockBlobReference($"thumb-{Guid.NewGuid()}.jpg");

#if STORAGE_PROGRESS
                        if (Device.RuntimePlatform != Device.UWP)
                        {
                            StorageProgressHandler.SetLength(stream.Length);
                            await blockBlob.UploadFromStreamAsync(stream, new Microsoft.WindowsAzure.Storage.AccessCondition(), new Microsoft.WindowsAzure.Storage.Blob.BlobRequestOptions(), null, StorageProgressHandler, new System.Threading.CancellationToken());
                        }
                        else
#endif
                        await blockBlob.UploadFromStreamAsync(stream);

                        Shop.ImageLink = blockBlob.Uri.OriginalString;
                    }
                    if (CoverMediaFile != null)
                    {
                        CurrentOperation = "Uploading Cover";
                        if (!string.IsNullOrEmpty(Shop.CoverImageLink))
                        {
                            string filename = Path.GetFileName(Shop.CoverImageLink);
                            var blockRef = DataManager.Default.ShopContentContainer.GetBlockBlobReference(filename);
                            await blockRef.DeleteIfExistsAsync(Microsoft.WindowsAzure.Storage.Blob.DeleteSnapshotsOption.IncludeSnapshots, null, null, null);
                        }
                        var stream = CoverMediaFile.GetStream();
                        var blockBlob = DataManager.Default.ShopContentContainer.GetBlockBlobReference($"cover-{Guid.NewGuid()}.jpg");
#if STORAGE_PROGRESS
                        if (Device.RuntimePlatform != Device.UWP)
                        {
                            StorageProgressHandler.SetLength(stream.Length);
                            await blockBlob.UploadFromStreamAsync(stream, new Microsoft.WindowsAzure.Storage.AccessCondition(), new Microsoft.WindowsAzure.Storage.Blob.BlobRequestOptions(), null, StorageProgressHandler, new System.Threading.CancellationToken());
                        }
                        else
#endif
                        await blockBlob.UploadFromStreamAsync(stream);

                        Shop.CoverImageLink = blockBlob.Uri.OriginalString;
                    }

                    CurrentOperation = "Updating Shop";
                    if (string.IsNullOrEmpty(Shop.ID))
                    {
                        Shop.SalesmenUserID = User.Current.ID;
                        await ShopAPI.InsertAsync(Shop);
                    }
                    else
                        await ShopAPI.UpdateAsync(Shop, false);

                    CurrentOperation = "Updating Working Times";
                    WorkingHours.ShopID = Shop.ID;
                    Page.LoadWorkingHours(WorkingHours);

                    if (string.IsNullOrEmpty(WorkingHours.ID))
                        await DataManager.Default.ShopWorkingHoursTable.InsertAsync(WorkingHours);
                    else
                        await DataManager.Default.ShopWorkingHoursTable.UpdateAsync(WorkingHours);


                    CurrentOperation = "Updating Services";
                    foreach (var serviceKey in Page.ServiceSwitches.Keys)
                    {
                        var tuple = Page.ServiceSwitches[serviceKey];
                        var service = tuple.Item1;
                        var serviceDesc = (await DataManager.Default.ServiceDescriptionTable.Where(sd => (sd.ServiceID == service.ID) && (sd.ShopID == Shop.ID)).Take(1).ToListAsync()).FirstOrDefault();
                        if (serviceDesc == null && !serviceKey.IsToggled)
                            continue;

                        if (serviceDesc == null)
                            serviceDesc = new ServiceDescription();

                        serviceDesc.ServiceID = service.ID;
                        serviceDesc.ShopID = Shop.ID;
                        serviceDesc.KurdishDescription = tuple.Item3.Text;
                        serviceDesc.IsActive = serviceKey.IsToggled;

                        if (string.IsNullOrEmpty(serviceDesc.ID))
                            await DataManager.Default.ServiceDescriptionTable.InsertAsync(serviceDesc);
                        else
                            await DataManager.Default.ServiceDescriptionTable.UpdateAsync(serviceDesc);
                    }

                    for (int i = 0; i < Albums.Count; i++)
                    {
                        CurrentOperation = $"Updating Albums ({i + 1}/{Albums.Count})";
                        Album album = Albums[i];
                        album.ShopID = Shop.ID;

                        if (string.IsNullOrEmpty(album.ID))
                            await DataManager.Default.AlbumTable.InsertAsync(album);
                        else
                            await DataManager.Default.AlbumTable.UpdateAsync(album);

                        foreach (var image in album.Images)
                        {
                            image.AlbumID = album.ID;

                            if (string.IsNullOrEmpty(image.ID))
                                await DataManager.Default.AlbumImageTable.InsertAsync(image);

                            if (image.ImageLink == Constants.LocalImageLink)
                            {

                                var stream = image.MediaFile.GetStream();
                                var blockBlob = DataManager.Default.AlbumContentContainer.GetBlockBlobReference($"{album.ID}_{image.ID}.jpg");
                                await blockBlob.UploadFromStreamAsync(stream);
                                image.ImageLink = blockBlob.Uri.OriginalString;
                                await DataManager.Default.AlbumImageTable.UpdateAsync(image);
                            }
                        }
                    }

                    await Page.Navigation.PopModalAsync();
                }
                catch (Exception e)
                {
                    Crashes.TrackError(e);
                    await Page.DisplayAlert("کێشەیەک ڕوویدا", e.ToString(), "باشە");
                }
                IsBusy = false;
            }
        }

        public void MoreTab()
        {
            Page.CurrentPage = Page.Children[1];
        }
        public void GalleryTab()
        {
            Page.CurrentPage = Page.Children[2];
        }
    }
}
