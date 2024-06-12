using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace XeroApp.Utilities
{
	public class XeroAuthenticationAttribute : AuthorizeAttribute, IAuthorizationFilter
	{
		public void OnAuthorization(AuthorizationFilterContext context)
		{
		}
	}
}