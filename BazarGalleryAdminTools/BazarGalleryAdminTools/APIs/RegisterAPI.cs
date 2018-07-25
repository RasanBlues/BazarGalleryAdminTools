using BazarGallery;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BazarGalleryAdminTools
{
    public static class RegisterAPI
    {
        public const string URL = "http://bazargallery.com/api/apis/registerUser";

        public static async Task InsertAsync(User user)
        {
            try
            {
                if (!string.IsNullOrEmpty(user.ID))
                    throw new InvalidOperationException($"User already exists! cannot register another user with the same ID.");

                var values = new Dictionary<string, string>
                {
                   { "name", user.Username },
                   { "password", user.Password },
                   { "phone", user.PhoneNumber },
                   { "deviceType", Device.RuntimePlatform.ToLower() },
                   { "deviceToken", "" },
                   { "userType", "2" },
                   { "image", user.ImageLink },
                   { "lang", "en" },
                };

                var content = new FormUrlEncodedContent(values);
                var response = await DataManager.HttpClient.PostAsync(URL, content);
                var responseString = await response.Content.ReadAsStringAsync();
                var jsonObj = JsonConvert.DeserializeObject<JsonObject>(responseString);
                user.OldID = Convert.ToInt32(jsonObj.data.id);
                await DataManager.Default.UserTable.InsertAsync(user);
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                Console.WriteLine(e.ToString());
            }
        }
        public static async Task UpdateAsync(User user)
        {
            try
            {
                if (string.IsNullOrEmpty(user.ID))
                    throw new InvalidOperationException($"User doesn't exists! cannot update user.");

                var values = new Dictionary<string, string>
                {
                   {"id",user.OldID.ToString() },
                   { "name", user.Username },
                   { "device", user.Password },
                   { "phone", user.PhoneNumber },
                   { "deviceType", Device.RuntimePlatform.ToLower() },
                   { "deviceToken", "" },
                   { "userType", "2" },
                   { "image", user.ImageLink },
                   { "lang", "en" },
                };

                var content = new FormUrlEncodedContent(values);
                var response = await DataManager.HttpClient.PostAsync(URL, content);
                var responseString = await response.Content.ReadAsStringAsync();

                await DataManager.Default.UserTable.UpdateAsync(user);
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                Console.WriteLine(e.ToString());
            }
        }

        private class JsonObject
        {
            public JsonObjectData data { get; set; }
            public string mesg { get; set; }

            public class JsonObjectData
            {
                public string id { get; set; }
                public string name { get; set; }
                public string email { get; set; }
                public string deviceType { get; set; }
                public string deviceToken { get; set; }
                public string userType { get; set; }
                public string image { get; set; }
                public string phone { get; set; }
                public string social_id { get; set; }
                public string notify_status { get; set; }
                public string lang { get; set; }
                public string city { get; set; }
                public int notifyCount { get; set; }
            }
        }
    }
}