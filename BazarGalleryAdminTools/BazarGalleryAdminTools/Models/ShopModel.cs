using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace BazarGallery.Models
{
    public class Shop : BaseDataModel
    {
        private ShopStatus _Status;

        public string CoverImageLink { get; set; }

        [JsonIgnore]
        public string Description => EnglishDescription;
        public string KurdishDescription { get; set; }
        public string ArabicDescription { get; set; }
        public string EnglishDescription { get; set; }

        [JsonIgnore]
        public string Address => EnglishAddress;
        public string KurdishAddress { get; set; }
        public string ArabicAddress { get; set; }
        public string EnglishAddress { get; set; }

        public ShopStatus Status {
            get => _Status;
            set {
                _Status = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(KurdishStatus));
                OnPropertyChanged(nameof(StatusColor));
            }
        }
        public string Data { get; set; }

        [JsonIgnore]
        public string KurdishStatus
        {
            get
            {
                if (Status == ShopStatus.Approved)
                    return "پەسەندکراوە";
                else if (Status == ShopStatus.Declined)
                    return $"پەسەندنەکراوە";
                return "سەیرنەکراوە";
            }
        }
        [JsonIgnore]
        public Color StatusColor
        {
            get
            {
                if (Status == ShopStatus.Approved)
                    return Color.DarkSeaGreen;
                else if (Status == ShopStatus.Declined)
                    return Color.IndianRed;
                return Color.Default;
            }
        }
        
        /// <summary>
        /// A set of comma seperated values that represent the shop
        /// </summary>
        public string Tags { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public string Phone1 { get; set; }
        public string Phone1_Owner { get; set; }
        public string Phone2 { get; set; }
        public string Phone2_Owner { get; set; }
        public string Phone3 { get; set; }
        public string Phone3_Owner { get; set; }

        public string Website { get; set; }
        public string Email { get; set; }
        public string Facebook { get; set; }
        public string Twitter { get; set; }
        public string Snapchat { get; set; }
        public string Instagram { get; set; }
        public string YouTube { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:BazarGallery.Models.Shop"/> is approved.
        /// </summary>
        /// <value><c>true</c> if approved by a moderator or admin; otherwise, <c>false</c>.</value>
        [JsonIgnore]
        public bool IsApproved => (Status == ShopStatus.Approved);

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:BazarGallery.Models.Shop"/>'s chat available.
        /// </summary>
        /// <value><c>true</c> if is chat available; otherwise, <c>false</c>.</value>
        public bool IsChatAvailable { get; set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:BazarGallery.Models.Shop"/> is open.
        /// </summary>
        /// <value><c>true</c> if is open; otherwise, <c>false</c>.</value>
        [JsonIgnore]
        public bool IsOpen
        {
            get
            {
                return !IsManuallyClosed;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:BazarGallery.Models.Shop"/> is manually closed.
        /// </summary>
        /// <value><c>true</c> if manually closed; otherwise, <c>false</c>.</value>
        public bool IsManuallyClosed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:BazarGallery.Models.Shop"/> is sponsored.
        /// </summary>
        /// /// <value><c>true</c> if sponsored; otherwise, <c>false</c>.</value>
        public bool IsSponsored { get; set; }

        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        /// <value>an integer value specifying how important is this shop, the higher the more important</value>
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets the rating.
        /// </summary>
        /// <value>The rating from 0 to 5.</value>
        public int Rating { get; set; }

        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>The user identifier.</value>
        public string UserID { get; set; }

        /// <summary>
        /// Gets or sets the salesman user identifier that recorded this shop.
        /// </summary>
        /// <value>The salesman user identifier.</value>
        public string SalesmenUserID { get; set; }

        /// <summary>
        /// Gets or sets the subcategory (or subcategories) identifier.
        /// </summary>
        /// <value>The subcategory identifier, different subcategories can be specified with a comma.</value>
        public string SubcategoryID { get; set; }

        public string KeywordID { get; set; }

        public int OldID { get; set; }
    }
}
