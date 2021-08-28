using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestAuthTokens.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ValuesController : ControllerBase
	{
		private readonly UserManager<IdentityUser> userManager;
		private readonly SignInManager<IdentityUser> signInManager;

		private static ModelStateDictionary AddIdentityErrors(ModelStateDictionary modelState, IdentityResult identityResult)
		{
			foreach (var error in identityResult.Errors)
			{
				modelState.AddModelError(error.Code, error.Description);
			}

			return modelState;
		}

		public ValuesController(
			UserManager<IdentityUser> userManager,
			SignInManager<IdentityUser> signInManager)
		{
			this.userManager = userManager;
			this.signInManager = signInManager;
		}

		[HttpGet]
		public async Task<IActionResult> Get(string userName)
		{
			var user = await userManager.FindByNameAsync(userName);
			if (user == null)
			{
				var createResult = await userManager.CreateAsync(new IdentityUser
				{
					UserName = userName,
				}, "123456");

				if (!createResult.Succeeded)
				{
					AddIdentityErrors(ModelState, createResult);
					return ValidationProblem();
				}

				user = await userManager.FindByNameAsync(userName);
			}

			var signInResult = await signInManager.CheckPasswordSignInAsync(user, "123456", false);
			if (!signInResult.Succeeded)
			{
				ModelState.AddModelError("SignIn", signInResult.ToString());
				return ValidationProblem();
			}

			var srcToken = Guid.NewGuid().ToString();
			var tokenSetResult = await userManager.SetAuthenticationTokenAsync(user, "Custom Provider", "Custom Token", srcToken);
			if (!tokenSetResult.Succeeded)
			{
				AddIdentityErrors(ModelState, tokenSetResult);
				return ValidationProblem();
			}

			var dbToken = await userManager.GetAuthenticationTokenAsync(user, "Custom Provider", "Custom Token");
			if (srcToken != dbToken)
			{
				ModelState.AddModelError("Tokens", $"They differ: {srcToken} / {dbToken}");
				return ValidationProblem();
			}

			return Ok(dbToken);
		}
	}
}
