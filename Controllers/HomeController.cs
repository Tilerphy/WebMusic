using FM89.Logical;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FM89.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            string white= ConfigurationManager.AppSettings["WhiteIps"];
            string[] whites = white.Split(';');
            string ip = HttpContext.Request.UserHostAddress;
            int whiteCount = whites.Count(a =>  ip.StartsWith(a, StringComparison.OrdinalIgnoreCase)||ip.Equals(a, StringComparison.OrdinalIgnoreCase));
            if (whiteCount > 0)
            {


                ViewBag.hasSalt = string.IsNullOrEmpty(SaltManager.CurrentSalt) ? 1 : 0;

                return View();
            }
            else 
            {
                Response.Write("Sorry, only Changchun Office, China could visit this site.");
                return null;
            }
        }
    }
}
