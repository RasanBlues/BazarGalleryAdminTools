using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace BazarGallery.Controls
{
    public class ButtonWithPadding : Button
    {
        #region Padding

        public static BindableProperty PaddingProperty = BindableProperty.Create(nameof(Padding), typeof(Thickness), typeof(ButtonWithPadding), default(Thickness), defaultBindingMode: BindingMode.OneWay);

        public Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        #endregion Padding
        private bool _measured = false;
        private bool _self = false;

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            if (!_self) _measured = true;
            return base.OnMeasure(widthConstraint, heightConstraint);
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (_measured)
            {
                _measured = false;
                _self = true;

                double totalWidthPadding = 0;
                double totalHeightPadding = 0;
                if (Padding != null)
                {
                    totalWidthPadding = Padding.Left + Padding.Right;
                    totalHeightPadding = Padding.Top + Padding.Bottom;
                    WidthRequest = width + totalWidthPadding;
                    HeightRequest = height + totalHeightPadding;
                }
                else
                {
                    WidthRequest = width + 12;
                }
            }
            else
            {
                _self = false;
            }
        }
    }
}
