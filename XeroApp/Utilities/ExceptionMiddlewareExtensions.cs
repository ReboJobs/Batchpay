using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Net;


namespace XeroApp.Utilities
{
    public static class ExceptionMiddlewareExtensions
    {
        public static void ConfigureExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(error =>
            {
                error.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        await context.Response.WriteAsync(new ErrorDetails
                        {
                            StatusCode = context.Response.StatusCode,
                            Message = "Custom "
                        }.ToString());
                    }
                });
            });
        }

        public static void ConfigureCustomExceptionMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionMiddleware>();
        }

    }
}
