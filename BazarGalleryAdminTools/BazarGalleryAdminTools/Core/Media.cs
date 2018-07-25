using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BazarGallery
{
    public static class Media
    {
        public static async Task<MediaFile> GetMediaFile(Page Page)
        {
            bool hasPermissions = await CheckCameraPermissionsAsync(Page);
            if (hasPermissions)
            {
                if (Device.RuntimePlatform == Device.UWP)
                {
                    return await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions() { CompressionQuality = 90, PhotoSize = PhotoSize.Large });
                }
                else
                {
                    var selectedOption = await Page.DisplayActionSheet("ڕەسمی کەڤەر", "گەڕانەوە", null, "کامێرا", "گەلەری");
                    if (selectedOption == "کامێرا")
                        return await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions() { CompressionQuality = 90, PhotoSize = PhotoSize.Large });
                    else if (selectedOption == "گەلەری")
                        return await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions() { CompressionQuality = 90, PhotoSize = PhotoSize.Large });
                }
            }
            return null;
        }
        public static async Task<bool> CheckCameraPermissionsAsync(Page Page)
        {
            var cameraStatus = PermissionStatus.Unknown;
            var storageStatus = PermissionStatus.Unknown;
            var photoStatus = PermissionStatus.Unknown;

            cameraStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera);
            storageStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
            photoStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Photos);

            if (cameraStatus != PermissionStatus.Granted || storageStatus != PermissionStatus.Granted || photoStatus != PermissionStatus.Granted)
            {
                var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Camera, Permission.Storage, Permission.Photos });
                cameraStatus = results[Permission.Camera];
                storageStatus = results[Permission.Storage];
                photoStatus = results[Permission.Photos];
            }

            if (cameraStatus == PermissionStatus.Granted && storageStatus == PermissionStatus.Granted && photoStatus == PermissionStatus.Granted)
                return true;
            else
            {
                if (cameraStatus != PermissionStatus.Granted)
                    await Page.DisplayAlert("Permissions Denied", "Unable to take photos, need camera permission.", "OK");
                else if (storageStatus != PermissionStatus.Granted)
                    await Page.DisplayAlert("Permissions Denied", "Unable to take photos, need storage permission.", "OK");
                else if (photoStatus != PermissionStatus.Granted)
                    await Page.DisplayAlert("Permissions Denied", "Unable to take photos, need photos permission.", "OK");
                //On iOS you may want to send your user to the settings screen.
                //CrossPermissions.Current.OpenAppSettings();
            }

            return false;
        }
    }
}
