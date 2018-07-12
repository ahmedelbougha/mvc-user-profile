using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using mvc_user_profile.Auth;
using mvc_user_profile.Models;
using mvc_user_profile.ViewModels;
using Microsoft.AspNetCore.Http;

namespace mvc_user_profile.Controllers
{
    public class UserController : Controller
    {
        private string sessionKey = "token";

        [HttpGet] 
        public IActionResult Login() 
        { 
            var model = new LoginViewModel(); 

            try {
                var token = HttpContext.Session.GetString(sessionKey);
                if (!string.IsNullOrEmpty(token)) {
                    // redirect to user profile in case of the user token exists
                    return RedirectToAction("Profile", "User");
                }
            } catch (Exception ex) {
                return View(model);
            }

            return View(model);     
        }        
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            var auth = new AuhtenticationManager();
            var result = auth.CheckUser(model.Username, model.Password);

            if (result)
            {
                var token = AuhtenticationManager.GenerateToken(model.Username, 30);
                HttpContext.Session.SetString(sessionKey, token.Token);

                return RedirectToAction("Profile", "User");
            }
            return View(model);
        }
        
        public IActionResult Profile(ProfileViewModel model)
        {
            try {
                var token = HttpContext.Session.GetString(sessionKey);
                if (!string.IsNullOrEmpty(token)) {
                    var user = AuhtenticationManager.GetTokenData(token);
                    model.FirstName = user.FirstName;
                    model.LastName = user.LastName;
                    model.Email = user.Email;
                    model.Username = user.Username;
                    model.Token = token;
                    
                    ViewData["loggedin"] = true;
                    return View(model);
                }
            } catch (Exception ex) {
                return RedirectToAction("Login", "User");
            }            
            return RedirectToAction("Login", "User");
        }

        [HttpPost]
        public IActionResult ValidateJwt(string jwt)
        {
            if (Request.Headers["X-Requested-With"] != "XMLHttpRequest") {
                return BadRequest("Invalid Request");
            }

            if  (AuhtenticationManager.ValidateToken(jwt)) {
                HttpContext.Session.SetString(sessionKey, jwt);
                return Ok("valid JWT");
            }
            return BadRequest("Invalid JWT");
        }

        [HttpGet] 
        public IActionResult Logout() 
        { 
            HttpContext.Session.Remove(sessionKey);
            TempData["removeJWT"] = true;
            return RedirectToAction("Index", "Home"); 
        }         
    }
}