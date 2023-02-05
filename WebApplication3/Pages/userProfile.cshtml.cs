

using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication3.Services;
using WebApplication3.ViewModels;

namespace WebApplication3.Pages
{
    public class userProfileModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        private readonly UserService _userService;

        public userProfileModel(ILogger<IndexModel> logger, UserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        public IEnumerable<Register> myprofiles { get; set; }
        public List<Register> profile { get; set; }


        public void OnGet()
        {
            if (HttpContext.Session.GetString("userId") != null)
            {
                profile = _userService.GetAll();
                myprofiles = from pro in profile where pro.Email == HttpContext.Session.GetString("userId") select pro;

                var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
                var protector = dataProtectionProvider.CreateProtector("E546C8DF278CD5931069B522E695D4F2");

                foreach (var pro in myprofiles)
                {
                    pro.NRIC = protector.Unprotect(pro.NRIC);
                }
            }
            else
            {
                Response.Redirect("/Login");
            }
             
            





        }

        public IActionResult OnpostDownloadAsync(string? id)
        {
            //myprofile = from profile in profile where profile.Email == HttpContext.Session.GetString("userID") select profile;

            var myprofile = _userService.GetUserById(id);

            if (myprofile == null)
            {
                return Page();
            }
            if (myprofile.Resume == null)
            {
                return Page();
            }
            else
            {
                
                
                    byte[] byteArr = myprofile.Resume;
                    string mimetype = "application/pdf";
                    return new FileContentResult(byteArr, mimetype)
                    {
                        FileDownloadName = $"Resume.pdf"
                    };
                
                
            }
        }


    }
}
