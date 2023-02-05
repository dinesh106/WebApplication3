using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication3.Services;
using WebApplication3.ViewModels;

namespace WebApplication3.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        private readonly UserService _userService;

        public IndexModel(ILogger<IndexModel> logger, UserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        public IEnumerable<Register> myprofile { get; set; }
        public List<Register> profile { get; set; }    

        public void OnGet()
        {
            profile = _userService.GetAll();

        }

        public IActionResult OnpostDownloadAsync()
        {
            //myprofile = from profile in profile where profile.Email == HttpContext.Session.GetString("userID") select profile;

            var myprof =  _userService.GetAll().FirstOrDefault(x => x.Email == HttpContext.Session.GetString("userID"));
            
            if(myprof == null)
            {
                return Page();
            }
            if(myprof.Resume == null)
            {
                return Page();
            }
            else
            {
                byte[] byteArr = myprof.Resume;
                string mimetype = "application.pdf";
                return new FileContentResult(byteArr, mimetype)
                {
                    FileDownloadName = $"Invoice number {myprof.FirstName}"
                };
            }
        }


    }
}