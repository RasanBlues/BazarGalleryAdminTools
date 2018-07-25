using BazarGallery.Models;
using BazarGalleryAdminTools.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using BazarGallery;
using BazarGallery.Controls;
using Xamarin.Forms.Maps;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using BazarGalleryAdminTools.Controls;
using Microsoft.AppCenter.Crashes;

namespace BazarGalleryAdminTools.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ShopEditorPage : TabbedPage
    {
        private Dictionary<Switch, Categories_API.Category_API> CategoryStacks;
        private Dictionary<Switch, StackLayout> SubcategoryStacks = new Dictionary<Switch, StackLayout>();
        public Dictionary<Switch, Subcategories_API.Subcategory_API> SubcategorySwitches = new Dictionary<Switch, Subcategories_API.Subcategory_API>();
        private Dictionary<Switch, List<StackLayout>> KeywordStacks = new Dictionary<Switch, List<StackLayout>>();
        public Dictionary<Switch, Keywords_API.Keyword_API> KeywordSwitches = new Dictionary<Switch, Keywords_API.Keyword_API>();
        private Dictionary<Switch, List<View>> TimeSwitchDictinary;
        private List<Tuple<Picker, Picker, Switch>> TimePickerList;
        public Dictionary<Switch, Tuple<Service, Label, Entry>> ServiceSwitches { get; private set; } = new Dictionary<Switch, Tuple<Service, Label, Entry>>();

        public ShopEditorViewModel ViewModel { get; private set; }
        public Map Map => UIMapView;
        public bool IsUsingCustomLocation => UIIsUsingCustomLocation.IsToggled;
        public bool ShopIsSponsored => UISponsoredSwitch.IsToggled;

        public int ShopPriority
        {
            get
            {
                if (int.TryParse(UIShopPriority.Text, out int priority))
                    return priority;
                return 0;
            }
        }
        public double CustomLatitude
        {
            get
            {
                if (double.TryParse(UILocation_Lat.Text, out double latitude))
                {
                    var clamped = Math.Max(Math.Min(latitude, 89.9), 0.0);
                    CustomLatitude = clamped;
                    return clamped;
                }

                return 0.0;
            }
            private set => UILocation_Lat.Text = value.ToString();
        }
        public double CustomLongitude
        {
            get
            {
                if (double.TryParse(UILocation_Long.Text, out double longitude))
                {
                    var clamped = Math.Max(Math.Min(longitude, 179.9), 0.0);
                    CustomLongitude = clamped;
                    return clamped;
                }

                return 0.0;
            }
            private set => UILocation_Long.Text = value.ToString();
        }
        public string ProviderPhoneNumber
        {
            get => UIProviderPhoneEntry.Text;
            set => UIProviderPhoneEntry.Text = value;
        }

        public ShopEditorPage(Action onSaved = null)
        {
            InitializeComponent();

            BindingContext =
            ViewModel = new ShopEditorViewModel(this, new Shop(), onSaved);

            InitAsync();
        }
        public ShopEditorPage(Shop shop, bool canEditData, Action onSaved)
        {
            InitializeComponent();


            BindingContext =
            ViewModel = new ShopEditorViewModel(this, shop, onSaved);
            InitAsync();
        }
        protected override bool OnBackButtonPressed()
        {
            ViewModel.CancelCommand.Execute(null);
            return true;
        }
        internal async void InitAsync()
        {
            if (Device.RuntimePlatform == Device.UWP)
            {
                UIIsUsingCustomLocation.IsToggled = true;
                UIIsUsingCustomLocation.IsVisible = false;
            }
            else if (await Location.CheckPermissionsAsync())
                UIMapView.IsShowingUser = true;

            try
            {
                InitCategoriesLayoutAsync();
                InitServicesStackAsync();
                if (!string.IsNullOrEmpty(ViewModel.Shop.ID))
                {
                    var user = (await DataManager.Default.UserTable.Where(u => u.ID == ViewModel.Shop.UserID).Take(1).ToListAsync()).FirstOrDefault();
                    if (user != null)
                        UIProviderPhoneEntry.Text = user.PhoneNumber;
                }
            }
            catch (System.Net.Http.HttpRequestException)
            {
                await DisplayAlert("Error", $"Couldn't resolve host.{Environment.NewLine}Please check your internet connection and try again.", "OK");
            }
            catch (Exception e)
            {
                await DisplayAlert("Error", e.ToString(), "OK");
                Crashes.TrackError(e);
            }
            if (!string.IsNullOrEmpty(ViewModel.Shop.ID))
            {
                UISponsoredSwitch.IsToggled = ViewModel.Shop.IsSponsored;
                UIShopPriority.Text = (ViewModel.Shop.Priority > 0) ? $"{ViewModel.Shop.Priority}" : String.Empty;
                UIIsUsingCustomLocation.IsToggled = true;

                CustomLatitude = ViewModel.Shop.Latitude;
                CustomLongitude = ViewModel.Shop.Longitude;
                UILocation_Completed(null, null);

            }
        }
        internal async void InitCategoriesLayoutAsync()
        {
            CategoryStacks = new Dictionary<Switch, Categories_API.Category_API>();
            List<Categories_API.Category_API> list = Categories_API.Categories;
            if (Categories_API.Categories.Count == 0)
                list = await Categories_API.LoadCategories();

            int itemsCount = list.Count;
            int rowCount = (int)Math.Ceiling((double)itemsCount / 2);

            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < 2; columnIndex++)
                {
                    int index = columnIndex + (rowIndex * 2);
                    if (index < itemsCount)
                    {
                        StackLayout stackLayout = new StackLayout() { Orientation = StackOrientation.Horizontal, Margin = new Thickness(5) };
                        Label label = new Label() { Text = list[index].name, VerticalTextAlignment = TextAlignment.Center };
                        Switch switchBox = new Switch();

                        switchBox.Toggled += CategorySwitchBox_Toggled;
                        stackLayout.Children.Add(switchBox);
                        stackLayout.Children.Add(label);

                        CategoryStacks.Add(switchBox, list[index]);
                        UICategoryGrid.Children.Add(stackLayout, columnIndex, rowIndex);
                    }
                }
            }

            if (!string.IsNullOrEmpty(ViewModel.Shop.ID))
            {
                var st = ViewModel.Shop.SubcategoryID.Split(',');
                foreach (var subcatID_str in st)
                {
                    if (int.TryParse(subcatID_str, out int subcatID))
                    {
                        var subCat = Subcategories_API.Subcategories.First(s => s.id == subcatID);
                        CategoryStacks.First(c => c.Value.id == subCat.category_id).Key.IsToggled = true;
                        SubcategorySwitches.First(sc => sc.Value.id == subcatID).Key.IsToggled = true;
                    }
                }

                st = ViewModel.Shop.KeywordID.Split(',');
                foreach (var keywordID_str in st)
                {
                    if (int.TryParse(keywordID_str, out int keywordID))
                        KeywordSwitches.First(k => k.Value.id == keywordID).Key.IsToggled = true;
                }
            }
        }
        internal async void InitServicesStackAsync()
        {
            ServiceSwitches.Clear();
            UIServicesLayout.Children.Clear();
            var serviceList = await DataManager.Default.ServiceTable.ToListAsync();
            foreach (var service in serviceList)
            {
                Switch uiSwitch = new Switch();
                Label uiLabel = new Label() { Text = service.KurdishName, VerticalTextAlignment = TextAlignment.Center, IsEnabled = false };
                Entry uiEntry = new Entry() { Placeholder = "وردەکاری", PlaceholderColor = Color.LightGray, IsEnabled = false, HorizontalOptions = new LayoutOptions(LayoutAlignment.Fill, true) };

                StackLayout stackLayout = new StackLayout()
                {
                    Orientation = StackOrientation.Horizontal,
                    Children =
                    {
                        uiSwitch,
                        uiLabel,
                        uiEntry
                    }
                };

                uiSwitch.Toggled += ServiceSwitch_Toggled;
                ServiceSwitches.Add(uiSwitch, Tuple.Create(service, uiLabel, uiEntry));

                if (ViewModel.Shop.HasID)
                {
                    var serviceDesc = (await DataManager.Default.ServiceDescriptionTable.Where(sd => (sd.ServiceID == service.ID) && (sd.ShopID == ViewModel.Shop.ID)).Take(1).ToListAsync()).FirstOrDefault();
                    if (serviceDesc != null)
                    {
                        uiSwitch.IsToggled = serviceDesc.IsActive;
                        uiEntry.Text = serviceDesc.KurdishDescription;
                    }
                }

                UIServicesLayout.Children.Add(stackLayout);
            }
        }

        internal void InitTimeGrid()
        {
            TimeSwitchDictinary = new Dictionary<Switch, List<View>>();
            TimePickerList = new List<Tuple<Picker, Picker, Switch>>();
            UITimeGrid.Children.Clear();

            for (int rowIndex = 0; rowIndex < 7; rowIndex++)
            {
                Switch uiSwitch = new Switch() { IsToggled = true };
                Label uiLabel = new Label() { Text = Constants.Days[rowIndex], VerticalTextAlignment = TextAlignment.Center };
                Picker uiPickerFrom = new Picker();
                Picker uiPickerTo = new Picker();

                uiSwitch.Toggled += TimeSwitch_Toggled;

                uiPickerFrom.ItemsSource =
                uiPickerTo.ItemsSource = Constants.TimeRanges;

                int from = ViewModel.WorkingHours[(rowIndex + 1) * -1];
                int to = ViewModel.WorkingHours[(rowIndex + 1)];
                if (from != to)
                {
                    uiPickerFrom.SelectedIndex = from;
                    uiPickerTo.SelectedIndex = to;
                }
                else
                {
                    uiPickerFrom.SelectedIndex = 16;
                    uiPickerTo.SelectedIndex = 32;
                }

                UITimeGrid.Children.Add(uiSwitch, 0, rowIndex);
                UITimeGrid.Children.Add(uiLabel, 1, rowIndex);
                UITimeGrid.Children.Add(uiPickerFrom, 2, rowIndex);
                UITimeGrid.Children.Add(uiPickerTo, 3, rowIndex);
                if (rowIndex > 0 && false)
                {
                    Button uiDropButton = new Button() { Text = "↓", BackgroundColor = Color.Accent, TextColor = Color.White, HeightRequest = 34 };
                    UITimeGrid.Children.Add(uiDropButton, 4, rowIndex);
                    TimeSwitchDictinary.Add(uiSwitch, new List<View>() { uiLabel, uiPickerFrom, uiPickerTo, uiDropButton });
                }
                else
                {
                    TimeSwitchDictinary.Add(uiSwitch, new List<View>() { uiLabel, uiPickerFrom, uiPickerTo });
                }

                TimePickerList.Add(Tuple.Create(uiPickerFrom, uiPickerTo, uiSwitch));

                uiSwitch.IsToggled &= (from != to || from != 0);
            }
        }
        public void CopyTime()
        {
            int timeFrom = ((Picker)(TimeSwitchDictinary.Values.ElementAt(0)[1])).SelectedIndex;
            int timeTo = ((Picker)(TimeSwitchDictinary.Values.ElementAt(0)[2])).SelectedIndex;
            foreach (var viewList in TimeSwitchDictinary.Values)
            {
                (viewList[1] as Picker).SelectedIndex = timeFrom;
                (viewList[2] as Picker).SelectedIndex = timeTo;
            }
        }
        public void RefreshAlbumsLayout()
        {
            UIAlbumsStack.Children.Clear();
            int albumCount = ViewModel.Albums.Count;
            for (int i = 0; i < albumCount; i++)
            {
                Album album = ViewModel.Albums[i];
                StackLayout layout = null;
                if (album.KurdishName == Constants.KurdishGeneralAlbumName || album.KurdishName == Constants.KurdishAddressAlbumName)
                    layout = AlbumLayout.GetAlbumLayout(album, 80, false, null, ViewModel.AddAlbumImageCommand, ViewModel.EditAlbumImageCommand);
                else
                    layout = AlbumLayout.GetAlbumLayout(album, 80, true, ViewModel.DeleteAlbumCommand, ViewModel.AddAlbumImageCommand, ViewModel.EditAlbumImageCommand);
                UIAlbumsStack.Children.Add(layout);
            }

            Entry entry = new Entry() { Placeholder = "ناوی ئەلبوم", HorizontalOptions = new LayoutOptions(LayoutAlignment.Fill, true) };
            StackLayout stack = new StackLayout()
            {
                Orientation = StackOrientation.Horizontal,
                Margin = new Thickness(0, 5, 0, 20),
                Children =
                {
                    entry,
                    new Button() { Text = "ئەلبووم زیادکە", Command = ViewModel.AddAlbumCommand, CommandParameter = entry}
                }
            };

            UIAlbumsStack.Children.Add(stack);
        }

        private void ServiceSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            var switchObj = sender as Switch;
            var tuple = ServiceSwitches[switchObj];

            tuple.Item2.IsEnabled =
            tuple.Item3.IsEnabled = e.Value;
        }
        private void TimeSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            var switchObj = sender as Switch;
            var list = TimeSwitchDictinary[switchObj];
            foreach (var view in list)
                view.IsEnabled = e.Value;
        }
        private void CategorySwitchBox_Toggled(object sender, ToggledEventArgs e)
        {
            var switchObj = sender as Switch;
            var cat = CategoryStacks[switchObj];
            if (e.Value)
            {
                var layout = GetSubcategoryListingLayout(cat);
                SubcategoryStacks.Add(switchObj, layout);
                UISubcategoryStack.Children.Add(layout);
            }
            else
            {
                UISubcategoryStack.Children.Remove(SubcategoryStacks[switchObj]);
                SubcategoryStacks.Remove(switchObj);
                foreach (var subCat in SubcategorySwitches.Where(key => key.Value.category_id == cat.id).ToList())
                {
                    SubcategorySwitches.Remove(subCat.Key);
                }
            }
        }
        private void SubcategorySwitchBox_Toggled(object sender, ToggledEventArgs e)
        {
            var switchObj = sender as Switch;
            var subCat = SubcategorySwitches[switchObj];
            if (e.Value)
            {
                var layout = GetKeywordListingLayout(subCat);
                KeywordStacks.Add(switchObj, layout);
                foreach (var switchStack in layout)
                {
                    int index = UIKeywordsGrid.Children.Count;
                    int column = index % 3;
                    int row = index / 3;

                    UIKeywordsGrid.Children.Add(switchStack, column, row);
                }
            }
            else
            {
                foreach (var switchStack in KeywordStacks[switchObj])
                {
                    UIKeywordsGrid.Children.Remove(switchStack);
                }

                KeywordStacks.Remove(switchObj);
                foreach (var keyWord in KeywordSwitches.Where(key => subCat.keywords.Contains(key.Value)).ToList())
                {
                    KeywordSwitches.Remove(keyWord.Key);
                }
            }
        }

        private StackLayout GetSubcategoryListingLayout(Categories_API.Category_API category_API)
        {
            StackLayout stackLayout = new StackLayout() { Margin = new Thickness(0, 5) };

            Label label = new Label() { Text = category_API.name, TextColor = Color.White, Margin = new Thickness(5, 0) };
            Grid grid = new Grid();

            var list = category_API.subcategories;
            int itemsCount = list.Count;
            int rowCount = (int)Math.Ceiling((double)itemsCount / 2);

            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < 2; columnIndex++)
                {
                    int index = columnIndex + (rowIndex * 2);
                    if (index < itemsCount)
                    {
                        StackLayout switchStack = new StackLayout() { Orientation = StackOrientation.Horizontal, Margin = new Thickness(5) };
                        Label switchLabel = new Label() { Text = list[index].name, VerticalTextAlignment = TextAlignment.Center };
                        Switch switchBox = new Switch();
                        switchBox.Toggled += SubcategorySwitchBox_Toggled;
                        switchStack.Children.Add(switchBox);
                        switchStack.Children.Add(switchLabel);
                        grid.Children.Add(switchStack, columnIndex, rowIndex);
                        SubcategorySwitches.Add(switchBox, list[index]);
                    }
                }
            }

            Frame frame = new Frame() { BackgroundColor = Color.Accent, Padding = new Thickness(6, 4), Content = label };
            stackLayout.Children.Add(frame);
            stackLayout.Children.Add(grid);
            return stackLayout;
        }
        private List<StackLayout> GetKeywordListingLayout(Subcategories_API.Subcategory_API subcategory_API)
        {
            var stackList = new List<StackLayout>();
            var list = subcategory_API.keywords;
            int itemsCount = list.Count;
            int rowCount = (int)Math.Ceiling((double)itemsCount / 2);

            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < 2; columnIndex++)
                {
                    int index = columnIndex + (rowIndex * 2);
                    if (index < itemsCount)
                    {
                        StackLayout switchStack = new StackLayout() { Orientation = StackOrientation.Horizontal, Margin = new Thickness(5) };
                        Label switchLabel = new Label() { Text = list[index].name, VerticalTextAlignment = TextAlignment.Center };
                        Switch switchBox = new Switch();

                        switchStack.Children.Add(switchBox);
                        switchStack.Children.Add(switchLabel);

                        stackList.Add(switchStack);
                        KeywordSwitches.Add(switchBox, list[index]);
                    }
                }
            }

            return stackList;
        }

        public void LoadWorkingHours(ShopWorkingHours workingHours)
        {
            var satFrom = 0;
            var satTo = 0;

            var sunFrom = 0;
            var sunTo = 0;

            var monFrom = 0;
            var monTo = 0;

            var tueFrom = 0;
            var tueTo = 0;

            var wedFrom = 0;
            var wedTo = 0;

            var thuFrom = 0;
            var thuTo = 0;

            var friFrom = 0;
            var friTo = 0;

            if (TimePickerList[0].Item3.IsToggled)
            {
                satFrom = TimePickerList[0].Item1.SelectedIndex;
                satTo = TimePickerList[0].Item2.SelectedIndex;
            }
            if (TimePickerList[1].Item3.IsToggled)
            {
                sunFrom = TimePickerList[1].Item1.SelectedIndex;
                sunTo = TimePickerList[1].Item2.SelectedIndex;
            }
            if (TimePickerList[2].Item3.IsToggled)
            {
                monFrom = TimePickerList[2].Item1.SelectedIndex;
                monTo = TimePickerList[2].Item2.SelectedIndex;
            }
            if (TimePickerList[3].Item3.IsToggled)
            {
                tueFrom = TimePickerList[3].Item1.SelectedIndex;
                tueTo = TimePickerList[3].Item2.SelectedIndex;
            }
            if (TimePickerList[4].Item3.IsToggled)
            {
                wedFrom = TimePickerList[4].Item1.SelectedIndex;
                wedTo = TimePickerList[4].Item2.SelectedIndex;
            }
            if (TimePickerList[5].Item3.IsToggled)
            {
                thuFrom = TimePickerList[5].Item1.SelectedIndex;
                thuTo = TimePickerList[5].Item2.SelectedIndex;
            }
            if (TimePickerList[6].Item3.IsToggled)
            {
                friFrom = TimePickerList[6].Item1.SelectedIndex;
                friTo = TimePickerList[6].Item2.SelectedIndex;
            }

            workingHours.SatFrom = new TimeSpan(satFrom / 2, (satFrom % 2) * 30, 0).ToString(@"hh\:mm\:ss");
            workingHours.SatTo = new TimeSpan(satTo / 2, (satTo % 2) * 30, 0).ToString(@"hh\:mm\:ss");

            workingHours.SunFrom = new TimeSpan(sunFrom / 2, (sunFrom % 2) * 30, 0).ToString(@"hh\:mm\:ss");
            workingHours.SunTo = new TimeSpan(sunTo / 2, (sunTo % 2) * 30, 0).ToString(@"hh\:mm\:ss");

            workingHours.MonFrom = new TimeSpan(monFrom / 2, (monFrom % 2) * 30, 0).ToString(@"hh\:mm\:ss");
            workingHours.MonTo = new TimeSpan(monTo / 2, (monTo % 2) * 30, 0).ToString(@"hh\:mm\:ss");

            workingHours.TueFrom = new TimeSpan(tueFrom / 2, (tueFrom % 2) * 30, 0).ToString(@"hh\:mm\:ss");
            workingHours.TueTo = new TimeSpan(tueTo / 2, (tueTo % 2) * 30, 0).ToString(@"hh\:mm\:ss");

            workingHours.WedFrom = new TimeSpan(wedFrom / 2, (wedFrom % 2) * 30, 0).ToString(@"hh\:mm\:ss");
            workingHours.WedTo = new TimeSpan(wedTo / 2, (wedTo % 2) * 30, 0).ToString(@"hh\:mm\:ss");

            workingHours.ThuFrom = new TimeSpan(thuFrom / 2, (thuFrom % 2) * 30, 0).ToString(@"hh\:mm\:ss");
            workingHours.ThuTo = new TimeSpan(thuTo / 2, (thuTo % 2) * 30, 0).ToString(@"hh\:mm\:ss");

            workingHours.FriFrom = new TimeSpan(friFrom / 2, (friFrom % 2) * 30, 0).ToString(@"hh\:mm\:ss");
            workingHours.FriTo = new TimeSpan(friTo / 2, (friTo % 2) * 30, 0).ToString(@"hh\:mm\:ss");
        }

        private async void PhoneEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.NewTextValue.Length >= 10)
            {
                UpdateProvider((await DataManager.Default.UserTable.Where(u => u.PhoneNumber == e.NewTextValue).Take(1).ToListAsync()).FirstOrDefault());
            }
            else
                UpdateProvider(null);
        }
        public void UpdateProvider(User user)
        {
            if (user != null)
            {
                UIProviderNameLabel.Text = $"خاوەنی : {user.Username}";
                ViewModel.Shop.UserID = user.ID;
                UIAddProviderButton.IsVisible = false;
            }
            else
            {
                UIProviderNameLabel.Text = $"خاوەنی :";
                ViewModel.Shop.UserID = String.Empty;
                UIAddProviderButton.IsVisible = true;
            }
        }

        private async Task UIIsUsingCustomLocation_ToggledAsync(object sender, ToggledEventArgs e)
        {
            if (Device.RuntimePlatform == Device.UWP)
                return;

            if (IsBusy)
                await Task.Delay(500);

            IsBusy = true;

            if (e.Value)
                UIMapView.IsShowingUser = false;
            else if (await Location.CheckPermissionsAsync())
                UIMapView.IsShowingUser = true;

            IsBusy = false;
        }

        private void UILocation_Completed(object sender, EventArgs e)
        {
            UIMapView.Pins.Clear();
            var CustomLocationPin = new Pin
            {
                Label = $"{CustomLatitude}, {CustomLongitude}",
                Position = new Position(CustomLatitude, CustomLongitude)
            };
            UIMapView.Pins.Add(CustomLocationPin);
            UIMapView.MoveToRegion(MapSpan.FromCenterAndRadius(CustomLocationPin.Position, new Distance(200)));
        }
    }
}