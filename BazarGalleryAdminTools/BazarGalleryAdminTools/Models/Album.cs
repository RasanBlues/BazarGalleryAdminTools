using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;

namespace BazarGallery.Models
{
    public class Album : INotifyPropertyChanged
    {
        [JsonIgnore]
        public string Name => KurdishName;

        public string ID { get; set; }
        public string EnglishName { get; set; }
        public string KurdishName { get; set; }
        public string ArabicName { get; set; }
        public string ShopID { get; set; }
        public int OldID { get; set; }
        [JsonIgnore]
        public List<AlbumImage> Images { get; set; } = new List<AlbumImage>();

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged == null)
                return;

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
