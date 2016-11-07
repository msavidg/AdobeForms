using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AdobeForms.Web.Models;

namespace AdobeForms.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            IndexViewModel indexViewModel = new IndexViewModel();

            return View(indexViewModel);
        }
    }
}
