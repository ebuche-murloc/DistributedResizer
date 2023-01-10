using SkiaSharp;

namespace ResizerASPNET.MinIO
{
    public interface IMinIOProvider
    {
        public void UploadFile(IFormFile stream, string fileName);

        public string GetDownloadLink(string fileName);

        public SKBitmap DownloadImage(string fileName, string tmpPath);
    }
}
