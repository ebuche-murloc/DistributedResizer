using SkiaSharp;

namespace ResizerAPI.MinIO
{
    public interface IMinIOProvider
    {
        public void UploadFile(Stream stream, string fileName);

        public string GetDownloadLink(string fileName);

        public SKBitmap DownloadImage(string fileName, string tmpPath);
    }
}
