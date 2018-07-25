using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BazarGallery.Models;
using Microsoft.AppCenter.Crashes;
using System.Linq;
using Newtonsoft.Json;
using BazarGallery;

namespace BazarGalleryAdminTools
{
    public class ShopAPI
    {
        public const string URL = "http://bazargallery.com/api/apis/addUpdateShop";
        public const string DeleteURL = "http://bazargallery.com/api/apis/deleteShop";
        public const string UpdateAlbumURL = "http://bazargallery.com/api/apis/addAlbum";
        public const string DeleteAlbumURL = "http://bazargallery.com/api/apis/deleteAlbum";
        public const string UpdateAlbumImageURL = "http://bazargallery.com/api/apis/addGallery";
        public const string UpdateServiceDescription = "http://bazargallery.com/api/apis/addUpdateService";

        public static async Task InsertAsync(Shop shop)
        {
            try
            {
                if (!string.IsNullOrEmpty(shop.ID))
                    throw new InvalidOperationException($"Shop already exists! consider using ShopAPI.UpdateAsync(Shop).");

                await DataManager.Default.ShopTable.InsertAsync(shop);
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                Console.WriteLine(e.ToString());
            }
        }
        public static async Task UpdateAsync(Shop shop, bool pushToOldServer)
        {
            try
            {
                if (string.IsNullOrEmpty(shop.ID))
                    throw new InvalidOperationException($"Shop doesnt exists!");

                if (pushToOldServer)
                {
                    if (shop.Status == ShopStatus.Approved)
                    {
                        var values = await GetValueDictionaryAsync(shop);

                        ShopWorkingHours workingHours = await DataManager.GetShopWorkingHoursAsync(shop);
                        string timeJson = workingHours.ToJson();

                        values.Add("time", timeJson);

                        var response = await DataManager.HttpClient.PostAsync(URL, new FormUrlEncodedContent(values));
                        var responseString = await response.Content.ReadAsStringAsync();

                        var jsonObj = JsonConvert.DeserializeObject<JsonObject>(responseString);
                        shop.OldID = Convert.ToInt32(jsonObj.data.id);
                        workingHours.OldID = Convert.ToInt32(jsonObj.data.workingHours.id);

                        await DataManager.Default.ShopWorkingHoursTable.UpdateAsync(workingHours);


                        var serviceDescriptions = await DataManager.Default.ServiceDescriptionTable.Where(sd => sd.ShopID == shop.ID).ToListAsync();
                        if (serviceDescriptions.Count > 0)
                        {
                            List<JsonObject.ServiceDescription> oldServiceDescriptions = new List<JsonObject.ServiceDescription>();
                            foreach (var sd in serviceDescriptions)
                            {
                                var oldSD = new JsonObject.ServiceDescription() { desc = sd.KurdishDescription };

                                oldSD.id = (await DataManager.Default.ServiceTable.Where(s => s.ID == sd.ServiceID).Take(1).ToListAsync()).FirstOrDefault().OldID.ToString();
                                if (sd.OldID > 0)
                                    oldSD.desc_id = sd.OldID.ToString();
                                else
                                    oldSD.desc_id = "";

                                oldServiceDescriptions.Add(oldSD);
                            }
                            var sdJson = JsonConvert.SerializeObject(oldServiceDescriptions);

                            values = new Dictionary<string, string>
                        {
                            {"lang","ku" },
                            {"shop_id", shop.OldID.ToString() },
                            {"services",sdJson }
                        };

                            response = await DataManager.HttpClient.PostAsync(UpdateServiceDescription, new FormUrlEncodedContent(values));
                            responseString = await response.Content.ReadAsStringAsync();
                        }
                        var albums = await DataManager.Default.AlbumTable.Where(a => a.ShopID == shop.ID).ToListAsync();
                        foreach (var album in albums)
                        {
                            album.Images = await DataManager.Default.AlbumImageTable.Where(i => i.AlbumID == album.ID).ToListAsync();

                            var galleryAlbumAPI = new GalleryAlbumAPI(album);
                            values = new Dictionary<string, string>()
                            {
                                {"lang", "ku" },
                                {"shop_id", shop.OldID.ToString() },
                                {"title",album.KurdishName },
                                {"descripition", "" }
                            };

                            if (album.OldID > 0)
                                values.Add("id", album.OldID.ToString());

                            response = await DataManager.HttpClient.PostAsync(UpdateAlbumURL, new FormUrlEncodedContent(values));
                            responseString = await response.Content.ReadAsStringAsync();

                            if (album.OldID <= 0)
                                album.OldID = Convert.ToInt32(JsonConvert.DeserializeObject<JsonObject.AlbumResponse>(responseString).data);

                            await DataManager.Default.AlbumTable.UpdateAsync(album);

                            foreach (var image in album.Images)
                            {
                                values = new Dictionary<string, string>()
                                {
                                    {"lang","ku" },
                                    {"currency", (image.Currency == BazarGallery.Currency.IQD) ? "IQD" : "$"},
                                    {"price", image.Price.ToString() },
                                    {"desc",image.KurdishDescription },
                                    {"title",image.KurdishName },
                                    {"album_id",album.OldID.ToString() },
                                    {"shop_id",shop.OldID.ToString() },
                                    {"image", image.ImageLink }
                                };

                                if (image.OldID > 0)
                                    values.Add("id", image.OldID.ToString());

                                response = await DataManager.HttpClient.PostAsync(UpdateAlbumImageURL, new FormUrlEncodedContent(values));
                                responseString = await response.Content.ReadAsStringAsync();
                                if (image.OldID <= 0)
                                    image.OldID = Convert.ToInt32(JsonConvert.DeserializeObject<JsonObject.AlbumResponse>(responseString).data);
                                await DataManager.Default.AlbumImageTable.UpdateAsync(image);
                            }
                        }
                    }
                    else if (shop.OldID > 0)
                    {
                        var values = new Dictionary<string, string>
                        {
                            {"lang","ku" },
                            {"id", shop.OldID.ToString() }
                        };

                        var content = new FormUrlEncodedContent(values);
                        var response = await DataManager.HttpClient.PostAsync(DeleteURL, content);
                        var responseString = await response.Content.ReadAsStringAsync();
                        shop.OldID = 0;
                    }
                }

                await DataManager.Default.ShopTable.UpdateAsync(shop);
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                Console.WriteLine(e.ToString());
            }
        }
        public static async Task DeleteShopAlbum(Album album)
        {
            if (album.OldID > 0)
            {
                var values = new Dictionary<string, string>()
                {
                    {"lang","ku" },
                    {"id", album.OldID.ToString() }
                };

                var response = await DataManager.HttpClient.PostAsync(DeleteAlbumURL, new FormUrlEncodedContent(values));
                //var responseString = await response.Content.ReadAsStringAsync();
            }
            if (album.Images.Count > 0)
                foreach (var image in album.Images)
                {
                    if (!string.IsNullOrEmpty(image.ID))
                        await DataManager.Default.AlbumImageTable.DeleteAsync(image);
                }

            await DataManager.Default.AlbumTable.DeleteAsync(album);
        }
        public static async Task<Dictionary<string, string>> GetValueDictionaryAsync(Shop shop)
        {
            var dictionary = new Dictionary<string, string>
                    {
                        {"user_id", (await DataManager.Default.UserTable.Where(u=>u.ID==shop.UserID).Take(1).ToListAsync()).FirstOrDefault().OldID.ToString()},
                        {"name_en", shop.EnglishName},
                        {"name_ku", shop.KurdishName},
                        {"name_ar", shop.ArabicName},
                        {"address_en", shop.EnglishAddress},
                        {"address_ku", shop.KurdishAddress},
                        {"address_ar", shop.ArabicAddress},
                        {"desc_en", shop.EnglishDescription},
                        {"desc_ku", shop.KurdishDescription},
                        {"desc_ar", shop.ArabicDescription},
                        {"phone1", shop.Phone1},
                        {"phone1owner", shop.Phone1_Owner},
                        {"phone2", shop.Phone2},
                        {"phone2owner", shop.Phone2_Owner},
                        {"phone3", shop.Phone3},
                        {"phone3owner", shop.Phone3_Owner},
                        {"email", shop.Email},
                        {"website", shop.Website},
                        {"facebook", string.Format(Constants.FacebookURL, shop.Facebook)},
                        {"twitter", string.Format(Constants.TwitterURL, shop.Twitter)},
                        {"snapchat", string.Format(Constants.SnapchatURL, shop.Snapchat)},
                        {"youtube", string.Format(Constants.YouTubeURL, shop.YouTube)},
                        {"instagram", string.Format(Constants.InstagramURL, shop.Instagram)},
                        {"lat", shop.Latitude.ToString()},
                        {"lng", shop.Longitude.ToString()},
                        {"image", shop.ImageLink},
                        {"cover_image", shop.CoverImageLink},
                        {"lang", "ku"},
                        {"sub_cat", shop.SubcategoryID },
                        {"open", shop.IsOpen ? "1" : "0" },
                        {"recommend", shop.IsSponsored ? "1" : "0" },
                        {"chat", shop.IsChatAvailable ? "1" : "0" },
                        {"online", "1"  },
                        {"city_id","1" }
                    };
            if (shop.OldID > 0)
                dictionary.Add("shop_id", shop.OldID.ToString());
            return dictionary;
        }

        public class JsonObject
        {
            public Data data { get; set; }
            public string mesg { get; set; }

            public class Data
            {
                public string id { get; set; }
                public string user_id { get; set; }
                public WorkingHours workingHours { get; set; }

                public class WorkingHours
                {
                    public string id { get; set; }
                }
            }

            public class AlbumResponse
            {
                public string data { get; set; }
                public string mesg { get; set; }
            }

            public class ServiceDescription
            {
                public string id { get; set; }
                public string desc { get; set; }
                public string desc_id { get; set; }
            }
        }
    }
}
