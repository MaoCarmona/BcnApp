using DotNetBcnModule.Presentation.App_Start;
using DotNetBcnModule.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;
using NetBcnModule.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace NetBcnModule.Presentation.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        {
        }

        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        /// <summary>
        /// Integration module interface
        /// </summary>
        public ActionResult Integration()
        {
            return View();
        }
    }
}
