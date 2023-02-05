using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace WebApplication3.Pages.Mfa
{
    public class EnableAuthenticatorModel : PageModel
    {
        private readonly UserManager<IdentityUser> _um;
        public EnableAuthenticatorModel(UserManager<IdentityUser> um)
        {
            _um = um;
        }

        public string? SharedKey { get; set; }
        public async Task<IActionResult>OnGetAsync()
        {
            IdentityUser user = await _um.GetUserAsync(User);
            if(null == user)
            {
                return NotFound("problem with the user account");
            }

            var authKey = await _um.GetAuthenticatorKeyAsync(user);
            if(string.IsNullOrEmpty(authKey))
            {
                await _um.ResetAuthenticatorKeyAsync(user);

                return RedirectToPage("./EnableAuthenticator");
            }
            StringBuilder sbkey = new();

            for(int i = 0; i < authKey.Length; i++)
            {
                if(0 == (i % 4))
                {
                    sbkey.Append(' ');
                }
                sbkey.Append(authKey[i]);
            }
            SharedKey = sbkey.ToString();
            return Page();
               
        }

        [BindProperty]
        [Required]
        public string? Code { get; set; }

        [TempData]
        public string? Message {get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                string token = Code!.Replace(" ", string.Empty).Replace("-", string.Empty);

                var user = await _um.GetUserAsync(User);

                var istokenValid = await _um.VerifyTwoFactorTokenAsync(user, _um.Options.Tokens.AuthenticatorTokenProvider, token);
                if (istokenValid)
                {
                    var res = await _um.SetTwoFactorEnabledAsync(user, true);
                    if (res.Succeeded)
                    {
                        Message = "Authenticator App is valid!";
                        return RedirectToPage("./Fa2s");

                    }
                    ModelState.AddModelError(string.Empty, "Invalid Code");
                }
               
            }
            return await OnGetAsync();
        }

    }
}
