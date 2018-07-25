using BazarGallery.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace BazarGalleryAdminTools.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PhotoViewerPage : ContentPage
    {
        private AlbumImage _AlbumImage;
        public AlbumImage AlbumImage {
            get => _AlbumImage;
            set
            {
                _AlbumImage = value;
            }
        }
        public PhotoViewerPage(AlbumImage image)
        {
            InitializeComponent();

            AlbumImage = image;

            BindingContext = this;
            
        }
        public PhotoViewerPage(ImageSource image)
        {
            InitializeComponent();
            var album = new AlbumImage() { ImageSource = image};
            AlbumImage = album;

            BindingContext = this;

        }
    }
}
