using Newtonsoft.Json;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace BazarGallery.Models
{
    public class AlbumImage : BaseDataModel
    {
        [JsonIgnore]
        public ImageSource ImageSource
        {
            get
            {
                if (string.IsNullOrEmpty(ImageLink) || ImageLink == Constants.LocalImageLink)
                    return _ImageSource;
                else return ImageLink;
            }
            set
            {
                ImageLink = Constants.LocalImageLink;
                _ImageSource = value;
                OnPropertyChanged();
            }
        }
        [JsonIgnore]
        public MediaFile MediaFile
        {
            get => _MediaFile;
            set
            {
                _MediaFile = value;
                ImageSource = ImageSource.FromStream(_MediaFile.GetStream);
            }
        }
        [JsonIgnore]
        public bool HasMetaData
        {
            get
            {
                return !string.IsNullOrEmpty(Description + Name);
            }
        }

        [JsonIgnore]
        public string Description => KurdishDescription;
        public string KurdishDescription
        {
            get => _KurdishDescription;
            set
            {
                _KurdishDescription = value;
                OnPropertyChanged();
            }
        }
        public string ArabicDescription { get; set; }
        public string EnglishDescription { get; set; }

        private MediaFile _MediaFile;
        private ImageSource _ImageSource;
        private string _KurdishDescription;

        public string AlbumID { get; set; }
        public int OldID { get; set; }
        public double Price { get; set; }
        public Currency Currency { get; set; }

        public AlbumImage Clone()
        {
            return new AlbumImage()
            {
                KurdishName = KurdishName,
                EnglishName = EnglishName,
                ArabicName = ArabicName,
                ImageLink = ImageLink,

                KurdishDescription = KurdishDescription,
                EnglishDescription = EnglishDescription,
                ArabicDescription = ArabicDescription,
                AlbumID = AlbumID,
                Price = Price,
                Currency = Currency,
            };
        }
        public void CopyTo(AlbumImage image)
        {
            image.KurdishName = KurdishName;
            image.EnglishName = EnglishName;
            image.ArabicName = ArabicName;

            image.ImageLink = ImageLink;

            image.KurdishDescription = KurdishDescription;
            image.EnglishDescription = EnglishDescription;
            image.ArabicDescription = ArabicDescription;

            image.AlbumID = AlbumID;
            image.Price = Price;
            image.Currency = Currency;
        }
    }
}
