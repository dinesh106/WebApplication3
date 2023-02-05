using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using WebApplication3.Services;
using WebApplication3.ViewModels;

namespace WebApplication3.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public Login LModel { get; set; }

        private readonly SignInManager<IdentityUser> signInManager;

        private readonly RecapchaService _recapchaService;
        private readonly IHttpContextAccessor contxt;

        public LoginModel(IHttpContextAccessor httpContextAccessor,SignInManager<IdentityUser> signInManager, RecapchaService recapchaService)
        {
            this.signInManager = signInManager;
            _recapchaService = recapchaService;
            contxt = httpContextAccessor;
        }
        public void OnGet()
        {
        }
        public async Task<IActionResult> OnPostAsync()
        {

            var _GoogleCaptcha = _recapchaService.Recaptchaver(LModel.Token);
            if (!_GoogleCaptcha.Result.success && _GoogleCaptcha.Result.score<= 0.5)
            {
                ModelState.AddModelError(string.Empty, "You are not human");
                return Page();
            }
            else
            {
                if (ModelState.IsValid)
                {

                    var identityResult = await signInManager.PasswordSignInAsync(LModel.Email, LModel.Password,
                    LModel.RememberMe, lockoutOnFailure: true);
                    if (identityResult.Succeeded)
                    {
                        var claims = new List<Claim> {
                            new Claim(ClaimTypes.Name, "c@c.com"),
                            new Claim(ClaimTypes.Email, "c@c.com")
                            };
                        var i = new ClaimsIdentity(claims, "MyCookieAuth");
                        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(i);
                        await HttpContext.SignInAsync("MyCookieAuth", claimsPrincipal);
                        HttpContext.Session.SetString("userId", LModel.Email);

                        return RedirectToPage("userProfile");
                    }
                    else if (identityResult.RequiresTwoFactor)
                    {
                        return RedirectToPage("mfa/LoginWith2fa", new { area = "auth" });

                    }
                    if (identityResult.IsLockedOut)
                    {

                        return RedirectToPage("/Lockout");
                    }



                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }

            
             
            

            return Page();
        }
    }
}
