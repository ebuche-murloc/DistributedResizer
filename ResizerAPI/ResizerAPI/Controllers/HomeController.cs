using Microsoft.AspNetCore.Mvc;
using ResizerAPI.Models;
using ResizerAPI.ImageResize;
using Newtonsoft.Json;
using System.Diagnostics;

namespace ResizerAPI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IResizer _resizer;

        public HomeController(IResizer resizer)
        {
            _resizer = resizer;
        }

        [HttpGet]
        public string Resize(int height, int width, string file)
        {
            string errorMessage = JsonConvert.SerializeObject(new ResizeStatusInfo(false, "", true));

            if (file is null )
            {
                return errorMessage;
            }


            
            
            var resizeStatusInfo = _resizer.Resize(height, width, file);
            string json = JsonConvert.SerializeObject(resizeStatusInfo);
            return json;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}