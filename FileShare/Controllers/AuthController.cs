using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FileShare.Models;
using FileShare.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace FileShare.Controllers
{
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;

        private readonly UserManager<ApplicationIdentityUser> _userManager;
        private readonly SignInManager<ApplicationIdentityUser> _signInManager;

        public AuthController(ILogger<AuthController> logger, SignInManager<ApplicationIdentityUser> signInManager, UserManager<ApplicationIdentityUser> userManager)
        {
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public ActionResult Denied()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Index()
        {
            return RedirectToAction("Login", "Auth");
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl = "")
        {
            var model = new LoginModel { ReturnUrl = returnUrl };
            return View(model);
        }

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
                    var user = await _userManager.FindByEmailAsync(u.Username);

                    var authProperties = new AuthenticationProperties
                    {
                        AllowRefresh = true,
                        IsPersistent = u.RememberMe,
                        IssuedUtc = DateTime.UtcNow,
                        ExpiresUtc = (u.RememberMe ? DateTime.UtcNow.AddMonths(3) : DateTime.UtcNow.AddMinutes(30))
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
                    ModelState.AddModelError("", "User or Password is invalid!");
                }
            }
            return View(u);
        }

        public async Task<ActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth");
        }

        [Authorize]
        public async Task<string> GenerateToken()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("Senha#2021Afonsoft");

            var user = await _userManager.GetUserAsync(HttpContext.User);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.UserName.ToString()),
                    new Claim(ClaimTypes.Email, user.Email.ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        
        [AllowAnonymous]
        public ActionResult Create(string returnUrl = "")
        {
            return View(new CreateModel {  ReturnUrl = returnUrl });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateModel create)
        {
            if (ModelState.IsValid)
            {
                if (create.Password != create.Password2)
                {
                    ModelState.AddModelError("", "passwords not equal!");
                }
                else
                {
                    var user = await _userManager.FindByEmailAsync(create.Username);
                    if (user != null)
                    {
                        ModelState.AddModelError("", "This e-mail already registered!");
                    }
                    else
                    {
                        user = new ApplicationIdentityUser
                        {
                            Email = create.Username,
                            UserName = create.Username,
                            LockoutEnabled = false
                        };

                        var result = await _userManager.CreateAsync(user, create.Password);
                        if (result.Succeeded)
                        {
                            result = await _userManager.AddToRolesAsync(user, new string[] { "User" });
                            if (result.Succeeded)
                            {
                                return await Login(new LoginModel
                                {
                                    Password = create.Password,
                                    Username = create.Username,
                                    RememberMe = false,
                                    Id = 0,
                                    ReturnUrl = create.ReturnUrl
                                });
                            }
                        }
                        else
                        {
                            foreach (var e in result.Errors)
                            {
                                ModelState.AddModelError("", e.Description);
                            }
                        }
                    }
                }
            }
            return View(create);
        }


        public ActionResult Forgot()
        {
            return View();
        }
    }
}