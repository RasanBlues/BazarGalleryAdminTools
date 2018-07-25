using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BazarGallery;
using BazarGallery.Models;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;

namespace BazarGalleryAdminTools
{
    public class Categories_API
    {
        public static List<Category_API> Categories { get; private set; } = new List<Category_API>();
        public List<Category_API> data { get; set; }

        public class Category_API
        {
            public int id { get; set; }
            public string image { get; set; }
            public string name { get; set; }
            public string name_en { get; set; }
            public string name_ku { get; set; }
            public string name_ar { get; set; }
            public List<Subcategories_API.Subcategory_API> subcategories { get; set; }
        }

        public static async Task<List<Category_API>> LoadCategories()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(Constants.GetCategoriesAPI);
                    var json = await response.Content.ReadAsStringAsync();
                    var obj = JsonConvert.DeserializeObject<Categories_API>(json);
                    int catCount = obj.data.Count;

                    var allSubCategories = await Subcategories_API.LoadSubategories();
                    for (int i = 0; i < catCount; i++)
                        obj.data[i].subcategories = allSubCategories.Where(subCat => subCat.category_id == obj.data[i].id).ToList();

                    Categories = obj.data;
                    return obj.data;
                }
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                return new List<Category_API>();
            }
        }
    }

    public class Subcategories_API
    {
        public static List<Subcategory_API> Subcategories { get; private set; } = new List<Subcategory_API>();
        public List<Subcategory_API> data { get; set; }

        public class Subcategory_API
        {
            public int id { get; set; }
            public string image { get; set; }
            public string keyword { get; set; }
            public int category_id { get; set; }
            public string name { get; set; }
            public string name_en { get; set; }
            public string name_ku { get; set; }
            public string name_ar { get; set; }
            public List<Keywords_API.Keyword_API> keywords { get; set; }
        }

        public static async Task<List<Subcategory_API>> LoadSubategories()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(Constants.GetAllSubcategoriesAPI);
                    var json = await response.Content.ReadAsStringAsync();
                    var obj = JsonConvert.DeserializeObject<Subcategories_API>(json);

                    if (Keywords_API.Keywords == null)
                    {
                        await Keywords_API.LoadKeywords();
                    }

                    foreach (var subCat in obj.data)
                    {
                        var st = $" {subCat.keyword} ".Replace(",", " ");
                        subCat.keywords = Keywords_API.Keywords.Where(k => st.Contains($" {k.id} ")).ToList();
                    }
                    Subcategories = obj.data;
                    return obj.data;
                }
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                return new List<Subcategory_API>();
            }
        }
    }

    public class Keywords_API
    {
        public static List<Keyword_API> Keywords { get; private set; }
        public List<Keyword_API> data { get; set; }
        public string mesg { get; set; }

        public class Keyword_API
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        public static async Task<List<Keyword_API>> LoadKeywords()
        {
            try
            {
                var response = await DataManager.HttpClient.GetAsync(Constants.GetAllKeywordsAPI);
                var json = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<Keywords_API>(json);
                Keywords = obj.data;
                return obj.data;
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                return new List<Keyword_API>();
            }
        }
    }

#if LOAD_SERVICES
    public class Services_API
    {
        public IList<Service_API> data { get; set; }
        public string mesg { get; set; }

        public class Service_API
        {
            public int id { get; set; }
            public string name_en { get; set; }
            public string name_ar { get; set; }
            public string name_ku { get; set; }
            public string image { get; set; }
            public string desc_en { get; set; }
            public string desc_ar { get; set; }
            public string desc_ku { get; set; }
            public string created { get; set; }
            public string modified { get; set; }
            public string name { get; set; }
        }

        public static async Task LoadServices()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync("http://bazargallery.com/api/apis/serviceList?lang=en");
                    var json = await response.Content.ReadAsStringAsync();
                    var obj = JsonConvert.DeserializeObject<Services_API>(json);
                    foreach(var oldService in obj.data)
                    {
                        Service service = new Service()
                        {
                            KurdishName = oldService.name_ku,
                            EnglishName = oldService.name_en,
                            ArabicName = oldService.name_ar,
                            OldID = oldService.id
                        };

                        await DataManager.Default.ServiceTable.InsertAsync(service);
                        Console.WriteLine("Done inserting service!");
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
#endif
}
