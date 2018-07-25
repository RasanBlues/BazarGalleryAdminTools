using System;
using System.IO;
using Microsoft.WindowsAzure.Storage.Core.Util;

#if STORAGE_PROGRESS
namespace BazarGalleryAdminTools
{
    public class StorageProgressHandler : IProgress<StorageProgress>
    {
        public event Action<int> OnProgressPercentChanged;

        public long Length { get; private set; }
        public long BytesTransferred { get; private set; }
        public long BytesRemaining => Length - BytesTransferred;
        public int PercentComplete => (int)((BytesTransferred / (double)Length) * 100);

        public StorageProgressHandler()
        {

        }
        public StorageProgressHandler(long streamLength)
        {
            Length = streamLength;
        }
        public StorageProgressHandler(Stream stream)
        {
            Length = stream.Length;
        }

        public void SetLength(long streamLength)
        {
            BytesTransferred = 0;
            Length = streamLength;
        }
        void IProgress<StorageProgress>.Report(StorageProgress value)
        {
            BytesTransferred = value.BytesTransferred;
            OnProgressPercentChanged?.Invoke(PercentComplete);
        }
    }
}
#endif