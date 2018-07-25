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
    public partial class ListViewPage : ContentPage
    {
        public event Action<string> OnSeachTextChanged;
        public event Action<object> OnItemDeleted;
        public event Action<object> OnItemEdited;
        public event Action<object> OnItemSelected;

        public ListView ListView => UIListView;
        public bool IsRefreshing
        {
            get => UIListView.IsRefreshing;
            set => UIListView.IsRefreshing = value;
        }
        public string SearchText
        {
            get => UISearchBar.Text;
            set => UISearchBar.Text = value;
        }

        public ListViewPage()
        {
            InitializeComponent();
        }

        public ListViewPage(bool hideFooter)
        {
            InitializeComponent();
            if (hideFooter)
                ListView.Footer = null;
        }

        internal void Delete_Clicked(object sender, EventArgs e)
        {
            OnItemDeleted?.Invoke((sender as MenuItem).CommandParameter);
        }
        internal void Edit_Clicked(object sender, EventArgs e)
        {
            OnItemEdited?.Invoke((sender as MenuItem).CommandParameter);
        }
        private void UISearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            OnSeachTextChanged?.Invoke(e.NewTextValue);
        }
        private void UIListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            OnItemSelected?.Invoke(e.SelectedItem);
        }
    }
}