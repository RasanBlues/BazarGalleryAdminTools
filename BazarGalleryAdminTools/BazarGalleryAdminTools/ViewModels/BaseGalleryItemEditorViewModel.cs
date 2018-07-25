using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using BazarGallery;
using BazarGallery.Models;
using BazarGalleryAdminTools.Pages;
using Plugin.Media.Abstractions;
using Xamarin.Forms;

namespace BazarGalleryAdminTools.ViewModels
{
    public class BaseGalleryItemEditorViewModel : BaseViewModel
    {
        public BaseGalleryItemEditorPage Page { get; private set; }
        public AlbumImage Image { get; private set; }
        public MediaFile LoadedMediaFile { get; private set; }

        public bool CanSave
        {
            get => RequirementsFilled();
        }
        public bool CanCancel { get; private set; }
        public bool CanDelete { get; private set; }
        public bool HasMetaData { get; private set; }

        public ICommand ImageTappedCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand CancelCommand
        {
            get;
            private set;
        }


        private AlbumImage ImageBackup;

        public BaseGalleryItemEditorViewModel(BaseGalleryItemEditorPage page, AlbumImage image, Album album, Action saveAction, Action deleteAction = null, bool canCancel = true, bool canDelete = false, bool hasMetaData = true)
        {
            Page = page;
            Image = image;
            ImageBackup = Image.Clone();
            CanCancel = canCancel;
            CanDelete = canDelete;
            HasMetaData = hasMetaData;

            ImageTappedCommand = new Command(async () =>
            {
                var media = await Media.GetMediaFile(Page);
                if (media != null)
                {
                    var save = true;
                    if (IsDublicate(album, media))
                    {
                        save = await Page.DisplayAlert("Warning", "The image already exists in the album are you sure you want to save it", "Yes", "No");
                    }
                    if (save)
                    {
                        LoadedMediaFile = Image.MediaFile = media;
                        OnPropertyChanged(nameof(CanSave));
                    }
                }

            }
            );

            SaveCommand = new Command(saveAction);
            if (deleteAction != null)
                DeleteCommand = new Command(async () =>
                {
                    if (await Page.DisplayAlert("Delete Image", "Are you sure you want to delete the image?", "Yes", "No"))
                        deleteAction();
                }
                );
            CancelCommand = new Command(CancelAsync);
        }

        private bool IsDublicate(Album album, MediaFile media)
        {
            foreach (AlbumImage img in album.Images)
            {
                if (img.MediaFile.AlbumPath == media.AlbumPath)
                {
                    return true;
                }
            }
            return false;
        }

        private async void CancelAsync()
        {
            var cancel =
                    ImageBackup.KurdishName == Image.KurdishName &&
                    Image.Price == ImageBackup.Price &&
                    Image.Currency == ImageBackup.Currency &&
                    Image.KurdishDescription == ImageBackup.KurdishDescription;

            if (!cancel)
                cancel = await Page.DisplayAlert("Unsaved Changes", "Are you sure you want to cancel?", "Yes", "No");

            if (cancel)
            {
                ImageBackup.CopyTo(Image);
                await Page.Navigation.PopModalAsync();
            }
        }
        private bool RequirementsFilled()
        {
            return !string.IsNullOrEmpty(Image.ImageLink) && Image.ImageLink != Constants.EmptyImagePlaceholder;
        }
    }
}
