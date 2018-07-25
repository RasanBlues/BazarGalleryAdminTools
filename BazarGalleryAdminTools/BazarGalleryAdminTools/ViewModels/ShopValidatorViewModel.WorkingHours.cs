using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using BazarGallery.Models;

namespace BazarGalleryAdminTools.ViewModels
{
    public partial class ShopValidatorViewModel : BaseViewModel
    {
        private ShopWorkingHours _WorkingHours;
        private Color _SatColor;
        private Color _SunColor;
        private Color _MonColor;
        private Color _TueColor;
        private Color _WedColor;
        private Color _ThuColor;
        private Color _FriColor;

        public ShopWorkingHours WorkingHours
        {
            get => _WorkingHours;
            set
            {
                _WorkingHours = value;
                SatColor = ColorOfDate(value.SatFrom ,value.SatTo);
                SunColor = ColorOfDate(value.SunFrom, value.SunTo);
                MonColor = ColorOfDate(value.MonFrom, value.MonTo);
                TueColor = ColorOfDate(value.TueFrom, value.TueTo);
                WedColor = ColorOfDate(value.WedFrom, value.WedTo);
                ThuColor = ColorOfDate(value.ThuFrom, value.ThuTo);
                FriColor = ColorOfDate(value.FriFrom, value.FriTo);

                OnPropertyChanged();
            }
        }
        public Color SatColor
        {
            get => _SatColor;
            set
            {
                _SatColor = value;
                OnPropertyChanged();
            }
        }
        public Color SunColor
        {
            get => _SunColor;
            set
            {
                _SunColor = value;
                OnPropertyChanged();
            }
        }
        public Color MonColor
        {
            get => _MonColor;
            set
            {
                _MonColor = value;
                OnPropertyChanged();
            }
        }
        public Color TueColor
        {
            get => _TueColor;
            set
            {
                _TueColor = value;
                OnPropertyChanged();
            }
        }
        public Color WedColor
        {
            get => _WedColor;
            set
            {
                _WedColor = value;
                OnPropertyChanged();
            }
        }
        public Color ThuColor
        {
            get => _ThuColor;
            set
            {
                _ThuColor = value;
                OnPropertyChanged();
            }
        }
        public Color FriColor
        {
            get => _FriColor;
            set
            {
                _FriColor = value;
                OnPropertyChanged();
            }
        }

        private Color ColorOfDate(string From, string To)
        {
            return From == "00:00:00" && To == "00:00:00" ? Color.DarkRed : Color.Default;
        }
    }
}
