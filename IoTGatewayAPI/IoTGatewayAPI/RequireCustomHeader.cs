using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IoTGatewayAPI
{
	public class RequireCustomHeader : ActionFilterAttribute
	{
		private readonly string SecurityKey;
        string IoTGatewayKey = Environment.GetEnvironmentVariable("SecurityKey");

		public RequireCustomHeader(string _securityKey)
		{
			SecurityKey = _securityKey;
		}
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			if (!context.HttpContext.Request.Headers.ContainsKey(SecurityKey))
			{
				context.Result = new ContentResult()
				{
					StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
					Content = $"Missing required header - {SecurityKey}"
				};
			}
			var sKey = context.HttpContext.Request.Headers["SecurityKey"];

			if(IoTGatewayKey != sKey)
            {
				context.Result = new ContentResult()
				{
					StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
					Content = $"{SecurityKey} is not correct - try again"
				};
			}	

		}
			
		}
	}
