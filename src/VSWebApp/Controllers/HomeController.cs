using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VSWebApp.Models;

namespace VSWebApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Search()
        {
            return View();
        }
    }
}
