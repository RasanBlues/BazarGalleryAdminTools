using BazarGalleryAdminTools.Pages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace BazarGalleryAdminTools.ViewModels
{
    public abstract class BaseListViewModel : BaseViewModel
    {
        private IList _BaseItems;
        private IList _Items;
        private bool _IsBusy;

        public ListViewPage Page { get; protected set; }
        public int CurrentIndex { get; protected set; }
        public int ItemsPerPage { get; set; } = 50;
        public long TotalItemsCount { get; set; }

        /// <summary>
        /// Base items that is bound to the model
        /// </summary>
        public IList BaseItems
        {
            get => _BaseItems;
            set
            {
                _BaseItems =
                Items = value;
                Page.SearchText = String.Empty;
            }
        }

        /// <summary>
        /// list containing items that match the search criteria
        /// </summary>
        public IList Items
        {
            get => _Items;
            set
            {
                _Items = value;
                OnPropertyChanged();
            }
        }

        public bool IsBusy
        {
            get => _IsBusy;
            set
            {
                _IsBusy = value;
                Page.IsBusy = _IsBusy;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Command to run when the user refreshes the list
        /// </summary>
        public abstract ICommand OnRefresh { get; set; }

        /// <summary>
        /// Command to run when the user searches for a keyword
        /// </summary>
        public abstract ICommand OnSearch { get; set; }
        public abstract ICommand OnItemSelected { get; set; }
        public abstract ICommand OnItemAdded { get; set; }

        protected abstract void OnSearchRequested(string searchText);
        protected abstract void OnSelectionRequest(object item);
        protected virtual void OnItemEditedAsync(object obj) { throw new NotImplementedException(); }
        protected virtual void OnItemDeletedAsync(object obj) { throw new NotImplementedException(); }
    }
}
