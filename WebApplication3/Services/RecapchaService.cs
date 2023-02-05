using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WebApplication3.Services;


namespace WebApplication3.Services
{
	public class RecapchaService
	{
		private GoogleCaptchaConfig _googleCaptchaConfig;

		public RecapchaService(IOptions<GoogleCaptchaConfig> googleCaptchaConfig)
		{
			_googleCaptchaConfig = googleCaptchaConfig.Value;

		}

		public virtual async Task<GoogleREspo> Recaptchaver(string _Token)
		{
			ReCaptchaData _myData = new ReCaptchaData
			{
				response = _Token,
				secret = _googleCaptchaConfig.SecretKey,
			};
			HttpClient client = new HttpClient();
			var response = await client.GetStringAsync($"https://www.google.com/recaptcha/api/siteverify?secret={_myData.secret}&response={_myData.response}");
			var capresp=JsonConvert.DeserializeObject<GoogleREspo>(response);

			return capresp;


		}
	}
	public class ReCaptchaData
	{
		public string response { get; set; }
		public string secret { get; set; }
	}
	public class GoogleREspo
	{
		public bool success { get; set; }
		public double score { get; set; }
		public string action { get; set; }
		public DateTime challenge_ts { get; set; }
		public string hostname { get; set; }

	}
}
