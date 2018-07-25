using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BazarGallery.Models
{
    public class ServiceDescription
    {
        public string ID { get; set; }

        [JsonIgnore]
        public string Description => EnglishDescription;
        public string KurdishDescription { get; set; }
        public string ArabicDescription { get; set; }
        public string EnglishDescription { get; set; }
        public bool IsActive { get; set; }

        public string ServiceID { get; set; }
        public string ShopID { get; set; }
        public int OldID { get; set; }
    }
}
