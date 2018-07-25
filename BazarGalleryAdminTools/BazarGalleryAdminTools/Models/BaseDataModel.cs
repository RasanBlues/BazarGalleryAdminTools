using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace BazarGallery.Models
{
    public class BaseDataModel : INotifyPropertyChanged
    {
        [JsonIgnore]
        public string Name => KurdishName;
        [JsonIgnore]
        public bool IsChanged { get; set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:BazarGallery.Models.BaseDataModel"/> is uploaded to the server and has an ID assigned.
        /// </summary>
        /// <value><c>true</c> if an ID is assigned by server; otherwise, <c>false</c>.</value>
        public bool HasID
        {
            get => !string.IsNullOrEmpty(ID);
        }
        [JsonProperty("Id")]
        public string ID { get; set; }
        [Microsoft.WindowsAzure.MobileServices.Version]
        public string AzureVersion { get; set; }
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }
        [JsonProperty("updatedAt")]
        public DateTime UpdateAt { get; set; }

        public string EnglishName
        {
            get => _EnglishName;
            set
            {
                if (_EnglishName != value)
                    IsChanged = true;

                _EnglishName = value;
                OnPropertyChanged();
            }
        }
        public string KurdishName
        {
            get => _KurdishName;
            set
            {
                if (_KurdishName != value)
                    IsChanged = true;

                _KurdishName = value;
                OnPropertyChanged();
            }
        }
        public string ArabicName
        {
            get => _ArabicName;
            set
            {
                if (_ArabicName != value)
                    IsChanged = true;

                _ArabicName = value;
                OnPropertyChanged();
            }
        }
        public string ImageLink
        {
            get => _ImageLink;
            set
            {
                if (_ImageLink != value)
                    IsChanged = true;

                _ImageLink = value;
                OnPropertyChanged();
            }
        }

        private string _EnglishName;
        private string _KurdishName;
        private string _ArabicName;
        private string _ImageLink;

        public override string ToString()
        {
            return $"{Name} {{{ID}}}";
        }

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
