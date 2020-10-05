using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileShare.Models;
using FileShare.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FileShare.Controllers
{
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly SignInManager<ApplicationIdentityUser> _signInManager;

        /// <summary>
        /// AuthController
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="signInManager"></param>
        /// <param name="userManager"></param>
        public AuthController(ILogger<AuthController> logger, SignInManager<ApplicationIdentityUser> signInManager)
        {
            _logger = logger;
            _signInManager = signInManager;
            // _userManager = userManager; UserManager<ApplicationIdentityUser> userManager
        }

        /// <summary>
        /// Denied
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult Denied()
        {
            return View();
        }

        /// <summary>
        /// Index
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult Index()
        {
            return RedirectToAction("Login", "Auth");
        }

        /// <summary>
        /// Login
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult Login(string returnUrl = "")
        {
            var model = new LoginModel { ReturnUrl = returnUrl };
            return View(model);
        }

        /// <summary>
        /// Login
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginModel u)
        {
            if (ModelState.IsValid) //verifica se é válido
            {
                var result = await _signInManager.PasswordSignInAsync(u.Username, u.Password, u.RememberMe, false);

                if (result.Succeeded)
                {
                    var user = await _signInManager.UserManager.FindByEmailAsync(u.Username);

                    var authProperties = new AuthenticationProperties
                    {
                        AllowRefresh = true,
                        IsPersistent = u.RememberMe,
                        IssuedUtc = DateTime.UtcNow,
                        ExpiresUtc = (u.RememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddMinutes(30))
                    };

                    authProperties.Items.Add("Username", u.Username);
                    authProperties.SetString(".persistent", u.RememberMe.ToString());
                    authProperties.Items.Add("Id", user.Id.ToString());
                    authProperties.Items.Add("Email", user.Email);

                    await _signInManager.SignInAsync(user, authProperties, CookieAuthenticationDefaults.AuthenticationScheme);

                    _logger.LogInformation($"Usuário {u.Username} efetuou o login com sucesso.");

                    if (!string.IsNullOrEmpty(u.ReturnUrl) && Url.IsLocalUrl(u.ReturnUrl))
                    {
                        return Redirect(u.ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    _logger.LogInformation($"Usuário {u.Username} Usuário ou Senha Inválidos.");
                    ModelState.AddModelError("", "Usuário ou Senha Inválidos!");
                }
            }
            return View(u);
        }

        /// <summary>
        /// Logout
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth");
        }
    }
}