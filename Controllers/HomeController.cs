using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using mvc_user_profile.Models;
using Microsoft.AspNetCore.Http;

namespace mvc_user_profile.Controllers
{
    public class HomeController : Controller
    {
        private string sessionKey = "token";
        public IActionResult Index()
        { 
            try {
                var token = HttpContext.Session.GetString(sessionKey);
                if (!string.IsNullOrEmpty(token)) {
                    ViewData["loggedin"] = true;
                }
                
                bool removeJWT = (bool) TempData["removeJWT"];                
                if (removeJWT) {
                    ViewData["removeJWT"] = "true";
                }
            } catch (Exception ex) {
                // do nothing
            }

            return View();
        }

        public IActionResult About()
        {
            try {
                var token = HttpContext.Session.GetString(sessionKey);
                if (!string.IsNullOrEmpty(token)) {
                    ViewData["loggedin"] = true;
                }

            } catch (Exception ex) {
                // do nothing
            }            
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        
    }
}
