using Microsoft.AspNetCore.Mvc;
using ResizerASPNET.Resizer;
using ResizerASPNET.MinIO;
using ResizerASPNET.Models;
using System.Diagnostics;

namespace ResizerASPNET.Controllers
{
    public class HomeController : Controller
    {
        readonly IResizerAPI _resizerAPI;
        private readonly IMinIOProvider _minIOProvider;

        public HomeController(IResizerAPI resizerAPI, IMinIOProvider minIOProvider)
        {
            _resizerAPI = resizerAPI;
            _minIOProvider = minIOProvider;
        }

        [HttpPost]
        public IActionResult Resize(IFormCollection data, IFormFile file)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            ResizeStatusInfo resizeStatusInfo = new ResizeStatusInfo(false, "", true);

            if (!(file.Length > 0) || !file.ContentType.Contains("image"))
                return PartialView("ApiResult", resizeStatusInfo);

            var height = int.Parse(data["height"]);
            var width = int.Parse(data["width"]);
            var uploadFolder = "cropprocess";
            var minioFilename = uploadFolder + '/' + DateTime.Now.ToString("yyyyMMddHHmmss") + height + width + Path.GetExtension(file.FileName);

            using (var ms = new MemoryStream())
            {
                file.CopyToAsync(ms).Wait();
                
                _minIOProvider.UploadFile(file, minioFilename);

                resizeStatusInfo = _resizerAPI.Resize(height, width, minioFilename);
                stopwatch.Stop();
                Console.WriteLine(stopwatch.ElapsedMilliseconds);
                return PartialView("ApiResult", resizeStatusInfo);
            }
        }


        public IActionResult Index()
        {
            return View(new IndexViewModel());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}