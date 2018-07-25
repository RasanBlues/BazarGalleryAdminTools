using BazarGallery.Models;
using BazarGalleryAdminTools.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace BazarGalleryAdminTools.Controls
{
    class ShopDataTemplateSelector : DataTemplateSelector
    {
        private DataTemplate _Approved;
        private DataTemplate _Declined;
        private DataTemplate _NotReviewed;

        public ShopDataTemplateSelector(Action<object, EventArgs> editAction, Action<object, EventArgs> deleteAction)
        {
            _Approved = ShopListItemTemplate.GetTemplate(Constants.ApproveColor, editAction, deleteAction);
            _Declined = ShopListItemTemplate.GetTemplate(Constants.DelcinedColor, editAction, deleteAction);
            _NotReviewed = ShopListItemTemplate.GetTemplate(Constants.NotReviewedColor, editAction, deleteAction);
        }
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            Shop shop = item as Shop;
            if (shop == null)
                return _NotReviewed;
            switch (shop.Status)
            {
                case BazarGallery.ShopStatus.Approved:
                    return _Approved;
                case BazarGallery.ShopStatus.Declined:
                    return _Declined;
                default:
                    return _NotReviewed;
            }
        }
    }
}
