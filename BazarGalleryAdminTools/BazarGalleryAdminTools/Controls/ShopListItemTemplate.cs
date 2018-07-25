using BazarGalleryAdminTools.Pages;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace BazarGalleryAdminTools.Controls
{
    public class ShopListItemTemplate
    {
        public static DataTemplate GetTemplate(Color statusColor, Action<object, EventArgs> editAction = null, Action<object, EventArgs> deleteAction = null)
        {
            return new DataTemplate(() =>
            {
                var viewCell = new ViewCell();
                var stack = new StackLayout() { FlowDirection = FlowDirection.RightToLeft };

                switch (Device.RuntimePlatform)
                {
                    case Device.UWP:
                        stack.Margin = new Thickness(0, 0);
                        break;

                    default:
                        stack.Margin = new Thickness(10, 0);
                        break;
                }
                var shopCreatedTime = new Label();

                var nameSpan = new Span() { TextColor = Color.Accent, FontAttributes = FontAttributes.Bold };
                var statusSpan = new Span() { TextColor = statusColor };
                var shopNameLabel = new Label()
                {
                    FormattedText = new FormattedString()
                    {
                        Spans =
                        {
                            nameSpan,
                            statusSpan
                        }
                    }
                };

                nameSpan.SetBinding(Span.TextProperty, "KurdishName");
                statusSpan.SetBinding(Span.TextProperty, "KurdishStatus", BindingMode.Default, null, "  ({0})");
                shopCreatedTime.SetBinding(Label.TextProperty, "CreatedAt");

                stack.Children.Add(shopNameLabel);
                stack.Children.Add(shopCreatedTime);

                if (editAction != null)
                {
                    var editMenuItem = new MenuItem { Text = "Edit", IsDestructive = false };
                    editMenuItem.SetBinding(MenuItem.CommandParameterProperty, new Binding("."));
                    editMenuItem.Clicked += editAction.Invoke;
                    viewCell.ContextActions.Add(editMenuItem);
                }
                if (deleteAction != null)
                {
                    var deleteMenuItem = new MenuItem { Text = "Delete", IsDestructive = true };
                    deleteMenuItem.SetBinding(MenuItem.CommandParameterProperty, new Binding("."));
                    deleteMenuItem.Clicked += deleteAction.Invoke;
                    viewCell.ContextActions.Add(deleteMenuItem);
                }
                viewCell.View = stack;

                return viewCell;
            });
        }
    }
}
