using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.Net.Http.Headers;

namespace kokoni_transfer.Controllers
{
    public class UploadController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Upload()
        {
            var files = HttpContext.Request.Form.Files;
            //var uploads = Path.Combine(_environment.WebRootPath, "uploads");
            //foreach (var file in files)
            //{
            //    if (file.Length > 0)
            //    {
            //        var fileName = ContentDispositionHeaderValue.Parse
            //            (file.ContentDisposition).FileName.Trim('"');
            //        System.Console.WriteLine(fileName);
            //        file.SaveAs(Path.Combine(uploads, fileName));
            //    }
            //}

            return Ok();
        }

    }

}