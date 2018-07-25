using BazarGallery.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BazarGalleryAdminTools
{
    public class GalleryAlbumAPI
    {
        public string title_en { get; set; }
        public string title_ar { get; set; }
        public string title_ku { get; set; }
        public string descripition_en { get; set; }
        public string descripition_ar { get; set; }
        public string descripition_ku { get; set; }
        public string image { get; set; }
        public List<Gallery> gallery { get; set; }

        public class Gallery
        {
            public string price { get; set; }
            public string currency { get; set; }
            public string title_en { get; set; }
            public string title_ar { get; set; }
            public string title_ku { get; set; }
            public string desc_en { get; set; }
            public string desc_ar { get; set; }
            public string desc_ku { get; set; }
            public string image { get; set; }

            public Gallery(AlbumImage albumImage)
            {
                price = albumImage.Price.ToString();
                currency = (albumImage.Currency == BazarGallery.Currency.IQD) ? "IQD" : "$";

                title_en = albumImage.EnglishName;
                title_ku = albumImage.KurdishName;
                title_ar = albumImage.ArabicName;

                desc_en = albumImage.EnglishDescription;
                desc_ku = albumImage.KurdishDescription;
                desc_ar = albumImage.ArabicDescription;

                image = albumImage.ImageLink;
            }
        }

        public GalleryAlbumAPI(Album album)
        {
            title_en = album.EnglishName;
            title_ku = album.KurdishName;
            title_ar = album.ArabicName;

            gallery = new List<Gallery>(album.Images.Count);
            foreach (var albumImage in album.Images)
                gallery.Add(new Gallery(albumImage));
        }
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

}
