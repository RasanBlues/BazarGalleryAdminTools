using BazarGallery;
using BazarGallery.Models;
using FFImageLoading.Forms;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace BazarGalleryAdminTools.Controls
{
    public static class AlbumLayout
    {
        public static StackLayout GetAlbumLayout(Album album, int itemSize)
        {
            return GetAlbumLayout(album, itemSize, false, null, null, null);
        }
        public static StackLayout GetAlbumLayout(Album album, int itemSize, bool canRename, ICommand deleteAlbumCommand, ICommand addAlbumImageCommand, ICommand albumImageClickedCommand)
        {
            Thickness itemMargin = new Thickness(5);
            Frame frame = new Frame()
            {
                BackgroundColor = Color.Accent,
                Padding = new Thickness(6, 0, 0, 0),
            };

            if (addAlbumImageCommand != null)
            {

                View textView = null;
                if (canRename)
                {
                    Entry entryTitle = new Entry()
                    {
                        Text = album.Name,
                        TextColor = Color.White,
                        BackgroundColor = Color.Transparent,
                        Margin = new Thickness(5, 2),
                        HorizontalOptions = new LayoutOptions(LayoutAlignment.Fill, true)
                    };
                    textView = entryTitle;
                    entryTitle.BindingContext = album;
                    entryTitle.SetBinding(Entry.TextProperty, new Binding("KurdishName"));
                }
                else
                {
                    Label labelTitle = new Label()
                    {
                        Text = album.Name,
                        TextColor = Color.White,
                        Margin = itemMargin,
                        HorizontalOptions = new LayoutOptions(LayoutAlignment.Fill, true)
                    };

                    textView = labelTitle;
                }
                if (deleteAlbumCommand != null)
                {
                    Button deleteButton = new Button()
                    {
                        Text = "سڕینەوە",
                        TextColor = Color.White,
                        BackgroundColor = Color.IndianRed,
                        HorizontalOptions = new LayoutOptions(LayoutAlignment.End, false)
                    };

                    deleteButton.Command = deleteAlbumCommand;
                    deleteButton.CommandParameter = album;

                    frame.Content = new StackLayout()
                    {
                        Orientation = StackOrientation.Horizontal,
                        Children =
                    {
                        textView,
                        deleteButton
                    }
                    };
                }
                else frame.Content = textView;
            }
            else
            {
                Label labelTitle = new Label()
                {
                    Text = album.Name,
                    TextColor = Color.White,
                    Margin = itemMargin
                };

                frame.Content = labelTitle;
            }

            FlexLayout flexLayout = new FlexLayout()
            {
                Wrap = FlexWrap.Wrap,
                JustifyContent = FlexJustify.Start
            };

            for (int i = 0; i < album.Images.Count; i++)
            {
                CachedImage image = new CachedImage()
                {
                    Source = album.Images[i].ImageSource,
                    ErrorPlaceholder = Constants.EmptyImagePlaceholder,
                    LoadingPlaceholder = Constants.DownloadingPlaceholderImage,
                    DownsampleWidth = 256,
                    RetryDelay = 1000,
                    RetryCount = 5,
                    CacheDuration = new TimeSpan(1, 12, 0, 0)
                };
                Frame imageFrame = new Frame()
                {
                    Padding = new Thickness(0),
                    Margin = itemMargin,
                    HeightRequest = itemSize,
                    WidthRequest = itemSize,
                    Content = image
                };

                if (albumImageClickedCommand != null)
                {
                    imageFrame.GestureRecognizers.Add(new TapGestureRecognizer()
                    {
                        Command = albumImageClickedCommand,
                        CommandParameter = new object[] { album, album.Images[i] }
                    });
                }

                flexLayout.Children.Add(imageFrame);
            }

            if (addAlbumImageCommand != null)
            {
                Button addButton = new Button()
                {
                    Text = "+",
                    Margin = itemMargin,
                    BackgroundColor = Color.Accent,
                    TextColor = Color.White,
                    WidthRequest = itemSize,
                    HeightRequest = itemSize,
                    HorizontalOptions = new LayoutOptions(LayoutAlignment.End, true),
                    VerticalOptions = new LayoutOptions(LayoutAlignment.Start, false),
                    Command = addAlbumImageCommand,
                    CommandParameter = album,
                };
                flexLayout.Children.Add(addButton);
            }

            StackLayout stackLayout = new StackLayout()
            {
                Margin = new Thickness(0, 5),
                Children =
                {
                    frame,
                    flexLayout
                }
            };

            return stackLayout;
        }

    }
}
