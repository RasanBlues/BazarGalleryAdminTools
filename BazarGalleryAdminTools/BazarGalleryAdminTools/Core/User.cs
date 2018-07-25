using BazarGalleryAdminTools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace BazarGallery
{
    public class User : INotifyPropertyChanged
    {
        public static User Current { get; private set; }
        public static bool IsLoggedIn
        {
            get
            {
                return Current != null;
            }
        }

        private string _PhoneNumber;

        public string ID { get; set; }
        public string MasterUserID { get; set; }
        public int OldID { get; set; }
        public UserType UserType { get; set; }
        public string Username { get; set; }
        public string PhoneNumber
        {
            get => _PhoneNumber;
            set
            {
                _PhoneNumber = value;
                OnPropertyChanged();
            }
        }
        //public string Email { get; set; }
        public string ImageLink { get; set; }
        public string Data { get; set; }
        public string Password { get; set; }
        public string AuthID { get; set; }

        public static async Task Login(User user, bool updateAuthID = true)
        {
            if (updateAuthID)
            {
                user.AuthID = Guid.NewGuid().ToString();
                await DataManager.Default.UserTable.UpdateAsync(user);
            }
           
            Preferences.Set(Constants.OAuthPhoneNumber, user.PhoneNumber);
            Preferences.Set(Constants.OAuthProperty, user.AuthID);

            Current = user;
        }
        public static void Logout()
        {
            if (Current == null)
                throw new InvalidOperationException("Cannot logout when user is not logged in");

            Preferences.Clear();

            Current = null;
        }

        public static string EncryptPassword(string password)
        {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 1000);
            byte[] hash = pbkdf2.GetBytes(20);
            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);
            return Convert.ToBase64String(hashBytes);
        }
        public static bool IsPasswordMatch(string input, string savedPasswordHash)
        {

            byte[] hashBytes = Convert.FromBase64String(savedPasswordHash);
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);
            var pbkdf2 = new Rfc2898DeriveBytes(input, salt, 1000);
            byte[] hash = pbkdf2.GetBytes(20);
            /* Compare the results */
            for (int i = 0; i < 20; i++)
                if (hashBytes[i + 16] != hash[i])
                    return false;
            return true;
        }
        public static bool IsPhoneNumberValid(string phoneNumber, out string fixedPhoneNumber)
        {
            fixedPhoneNumber = null;
            if (IsPhoneNumberValid(phoneNumber))
            {
                if (phoneNumber.Length == 10 && phoneNumber.StartsWith("7", StringComparison.OrdinalIgnoreCase))
                {
                    fixedPhoneNumber = $"0{phoneNumber}";
                    return true;
                }

                fixedPhoneNumber = phoneNumber;
                return true;
            }

            return false;
        }
        /// <summary>
        /// Checks if the phone number is starting with 7 or 07 and its length 
        /// returns true if it was valid
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        /// <value><c>true</c> if it starts with 7 or 07 with 10 or 11 length; otherwise, <c>false</c>.</value>
        public static bool IsPhoneNumberValid(string phoneNumber)
        {
            bool valid = false;
            if (phoneNumber.Length == 10)
            {
                valid = phoneNumber.StartsWith("053", StringComparison.OrdinalIgnoreCase) || phoneNumber.StartsWith("7", StringComparison.OrdinalIgnoreCase);
            }
            else if (phoneNumber.Length == 11)
            {
                valid = phoneNumber.StartsWith("07", StringComparison.OrdinalIgnoreCase);
            }

            return valid;
        }
        public override string ToString()
        {
            return $"{Username} ({PhoneNumber})";
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
