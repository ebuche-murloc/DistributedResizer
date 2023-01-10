using Minio;
using ResizerAPI.MinIO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using SkiaSharp;

namespace ResizerAPI.ImageResize
{
    public class Resizer : IResizer
    {
        private readonly IMinIOProvider _minIOProvider;

        public Resizer(IMinIOProvider minIOProvider)
        {
            _minIOProvider = minIOProvider;
        }

        public static byte[] GetHash(string inputString)
        {
            using (HashAlgorithm algorithm = SHA256.Create())
                return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        public ResizeStatusInfo Resize(int height, int width, string dowloadFilename, [CallerFilePath] string tmpPath = "")
        {
            Console.WriteLine(Environment.CurrentDirectory);
            var filename = dowloadFilename.Split('/')[1];
            var downloadPath = Path.Combine(Environment.CurrentDirectory, "tmp", filename);
            var imgToResize = _minIOProvider.DownloadImage(dowloadFilename, downloadPath);
            var isResized = false;
            //var resizedImage = ResizeImage(imgToResize, new Size(width, height));
            var resizedImageBitmap = imgToResize.Resize(new SKImageInfo(width, height), SKFilterQuality.High);
            var resizedImage = SKImage.FromPixels(resizedImageBitmap.PeekPixels());
            var uploadFilename = "cropresult" + '/' + filename;
            using (var m = resizedImage.Encode().AsStream())
            {
                //resizedImage.Save(m, imgToResize.RawFormat);

                _minIOProvider.UploadFile(m, uploadFilename);

                isResized = true;
            }

            ClearTmp(downloadPath, imgToResize, resizedImage);
            
            var downloadLink = _minIOProvider.GetDownloadLink(uploadFilename);

            var result = new ResizeStatusInfo(true, downloadLink, false);

            return  isResized ? result : new ResizeStatusInfo(false, "", true);
        }

        private static void ClearTmp(string downloadPath, SKBitmap imgToResize, SKImage resizedImage)
        {
            imgToResize.Dispose();
            resizedImage.Dispose();
            File.Delete(downloadPath);
        }

        private static Image ResizeImage(Image imgToResize, Size size)
        {
            //Get the image current width  
            int sourceWidth = imgToResize.Width;
            //Get the image current height  
            int sourceHeight = imgToResize.Height;
            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;
            //Calulate  width with new desired size  
            nPercentW = ((float)size.Width / (float)sourceWidth);
            //Calculate height with new desired size  
            nPercentH = ((float)size.Height / (float)sourceHeight);
            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;
            //New Width  
            int destWidth = (int)(sourceWidth * nPercent);
            //New Height  
            int destHeight = (int)(sourceHeight * nPercent);
            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage((Image)b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            // Draw image with new width and height  
            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();
            return (Image)b;
        }
    }
}
