using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using kokoni_transfer.Attributes;
using Microsoft.AspNetCore.Http;

namespace kokoni_transfer.Controllers
{
    public class DashboardController : Controller
    {
        [CheckSessionOut]
        public IActionResult Index()
        {
            return View();
        }

        [CheckSessionOut]
        public IActionResult About()
        {
            return View();
        }

        [CheckSessionOut]
        public IActionResult Error()
        {
            return View();
        }
    }
}