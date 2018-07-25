using System.Collections.ObjectModel;
using Newtonsoft.Json;
namespace BazarGallery.Models
{
    public class Subcategory : BaseDataModel
    {
        public string CategoryID { get; set; }
        public int OldID { get; set; }

        public int ShopCount { get; set; }

        public int ViewCount
        {
            get
            {
                return _ViewCount;
            }
            set
            {
                _ViewCount = value;
                OnPropertyChanged();
            }
        }
        private int _ViewCount;

        [JsonIgnore]
        public int NotificationCount { get; set; }
    }
}
