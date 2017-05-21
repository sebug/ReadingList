using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.Authentication;

namespace ReadingList.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public async Task Login()
        {
            if (HttpContext.User == null || !HttpContext.User.Identity.IsAuthenticated)
            {
                await HttpContext.Authentication.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme,
                                                                new AuthenticationProperties { RedirectUri = "/" });
            }
        }

		[HttpGet]
		public async Task LogOff()
		{
			if (HttpContext.User.Identity.IsAuthenticated)
			{
				await HttpContext.Authentication.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
				await HttpContext.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			}
		}
    }
}
