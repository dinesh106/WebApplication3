using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.DataProtection;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using WebApplication3.Services;
using WebApplication3.ViewModels;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace WebApplication3.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly UserService _userService;
        private readonly EncryptDataService _encryptDataService;
        private IWebHostEnvironment _environment;

        




        private UserManager<IdentityUser> userManager { get; }
        private SignInManager<IdentityUser> signInManager { get; }

        [BindProperty]
        public Register RModel { get; set; }

        [BindProperty]
        public IFormFile? Upload { get; set; }

        [BindProperty]
        public string? Nric { get; set; }

        static string finalHash;
        static string salt;
        byte[] Key;
        byte[] IV;



        public RegisterModel(UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager, UserService userService, EncryptDataService encryptDataService, IWebHostEnvironment environment)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            _userService = userService;
            _encryptDataService = encryptDataService;
            _environment = environment;
        }



        public void OnGet()
        {
        }


        public async Task<IActionResult> OnPostAsync()
        {
            
                var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
                var protector = dataProtectionProvider.CreateProtector("E546C8DF278CD5931069B522E695D4F2");

                var supportedTypes = new[] { "docx", "pdf" };
                if (Upload != null)
                {
                    /*var uploadsFolder = "uploads";
                    var imageFile = Guid.NewGuid() + Path.GetExtension(Upload.FileName);
                    var imagePath = Path.Combine(_environment.ContentRootPath, "wwwroot", uploadsFolder, imageFile);
                    using var fileStream = new FileStream(imagePath, FileMode.Create);
                    await Upload.CopyToAsync(fileStream);
                    RModel.Resume = string.Format("/{0}/{1}", uploadsFolder, imageFile);*/
                    if (Upload.Length > 2 * 1024 * 1024)
                    {
                        ModelState.AddModelError("Upload", "File size cannot exceed 2MB.");
                        return Page();
                    }

                    using (var target = new MemoryStream())
                    {
                        Upload.CopyTo(target);
                        RModel.Resume = target.ToArray();

                    }


                    /*var nric = EncryptString(Nric, key).ToString();

                    RModel.NRIC = nric;*/


                    var user = new IdentityUser()
                    {
                        UserName = HttpUtility.HtmlEncode(RModel.Email),
                        Email = HttpUtility.HtmlEncode(RModel.Email),
                        /*First_Name = RModel.FirstName,
                        Last Name
                        Gender

                        Password
                        Confirm Password
                        Date of Birth
                        Resume(.docx or.pdf file)
                        WhoamI(allow all special chars)*/
                    };
                    var result = await userManager.CreateAsync(user, RModel.Password);
                    if (result.Succeeded)
                    {



                        /*RModel.NRIC = protector.Protect(RModel.NRIC);*/


                        string pwd = RModel.Password.ToString().Trim(); ;
                        //Generate random "salt"
                        RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                        byte[] saltByte = new byte[8];
                        //Fills array of bytes with a cryptographically strong sequence of random values.
                        rng.GetBytes(saltByte);
                        salt = Convert.ToBase64String(saltByte);
                        SHA512Managed hashing = new SHA512Managed();
                        string pwdWithSalt = pwd + salt;
                        byte[] plainHash = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwd));
                        byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));
                        finalHash = Convert.ToBase64String(hashWithSalt);
                        RijndaelManaged cipher = new RijndaelManaged();
                        cipher.GenerateKey();
                        Key = cipher.Key;
                        IV = cipher.IV;

                        RModel.Password = finalHash;
                        RModel.ConfirmPassword = finalHash;
                        RModel.PasswordSalt = protector.Protect(salt);
                        RModel.NRIC = protector.Protect(RModel.NRIC);
                        RModel.WhoamI = HttpUtility.HtmlEncode(RModel.WhoamI);
                        RModel.FirstName = HttpUtility.HtmlEncode(RModel.FirstName);
                        RModel.LastName = HttpUtility.HtmlEncode(RModel.LastName);





                        _userService.AddUser(RModel);
                        await signInManager.SignInAsync(user, false);
                        HttpContext.Session.SetString("userId", RModel.Email);
                        return RedirectToPage("Index");
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                }
            else
            {
                ModelState.AddModelError("Upload", "Enter a valid file");
            }



            


                return Page();
            
        }
        /*public static string EncryptString(string text, string keyString)
        {
            var key = Encoding.UTF8.GetBytes(keyString);

            using (var aesAlg = Aes.Create())
            {
                using (var encryptor = aesAlg.CreateEncryptor(key, aesAlg.IV))
                {
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(text);
                        }

                        var iv = aesAlg.IV;

                        var decryptedContent = msEncrypt.ToArray();

                        var result = new byte[iv.Length + decryptedContent.Length];

                        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                        Buffer.BlockCopy(decryptedContent, 0, result, iv.Length, decryptedContent.Length);

                        return Convert.ToBase64String(result);
                    }
                }
            }
        }*/
        /*public static string DecryptString(string cipherText, string keyString)
        {
            var fullCipher = Convert.FromBase64String(cipherText);

            var iv = new byte[16];
            var cipher = new byte[16];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, iv.Length);
            var key = Encoding.UTF8.GetBytes(keyString);

            using (var aesAlg = Aes.Create())
            {
                using (var decryptor = aesAlg.CreateDecryptor(key, iv))
                {
                    string result;
                    using (var msDecrypt = new MemoryStream(cipher))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                result = srDecrypt.ReadToEnd();
                            }
                        }
                    }

                    return result;
                }
            }
        }*/







    }
}
