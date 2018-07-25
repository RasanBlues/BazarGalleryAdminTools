using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BazarGallery
{
    public static class Location
    {
        public static Task<bool> CheckPermissionsAsync()
        {
            return CheckPermissionsAsync(null);
        }
        public static async Task<bool> CheckPermissionsAsync(Page Page)
        {
            var locationStatus = PermissionStatus.Unknown;

            locationStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);

            if (locationStatus != PermissionStatus.Granted)
            {
                var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Location });
                locationStatus = results[Permission.Location];
            }

            if (locationStatus == PermissionStatus.Granted)
                return true;
            else if (Page != null)
            {
                await Page.DisplayAlert("Permissions Denied", "Unable to get location, location access denied.", "OK");
                //On iOS you may want to send your user to the settings screen.
                //CrossPermissions.Current.OpenAppSettings();
            }

            return false;
        }
    }
}
