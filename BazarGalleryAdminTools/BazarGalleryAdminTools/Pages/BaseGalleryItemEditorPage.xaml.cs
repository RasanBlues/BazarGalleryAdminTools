using BazarGallery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BazarGalleryAdminTools.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BaseGalleryItemEditorPage : ContentPage
    {
        public Currency ItemCurrency
        {
            get => (Currency)UICurrencyPicker.SelectedIndex;
            set => UICurrencyPicker.SelectedIndex = (int)value;
        }

        public BaseGalleryItemEditorPage()
        {
            InitializeComponent();
            UICurrencyPicker.SelectedIndex = 0;
        }
    }
}