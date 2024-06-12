using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Diagnostics;
using XeroApp.Extensions;
using XeroApp.Controllers;
using XeroApp.Models.BusinessModels;
using XeroApp.Utilities;
using XeroApp.Services.ReportBugService;

namespace XeroApp.Filters
{
    public class JsonExceptionFilter : ExceptionFilterAttribute
    {
        public override Task OnExceptionAsync(ExceptionContext context)
        {
            ReportBugModel reportBugModel = new ReportBugModel();
            var emailAddress = context.HttpContext.Session.GetString("EmailAddress");
            var svc = context.HttpContext.RequestServices;
            var _reportBugService = svc.GetService<IReportBugService>();
            string message = string.Empty;
            string innerMessage = string.Empty;


             if (context.HttpContext.Session.GetString("xero_userid") == null ||
               context.HttpContext.Session.GetString("EmailAddress") == null ||
               context.HttpContext.Session.GetString("given_name") == null ||
               context.HttpContext.Session.GetString("family_name") == null)
            {
                message = "Session has timeout";
                innerMessage = "User must reauthorize xero";
                context.Result = new JsonResult(new { Message = "Session has timeout", InnerMessage = "User must reauthorize xero", IsError = true, sessionTimeOut = true, ErrorCode = 1 });
                reportBugModel.ReportedBy = emailAddress;
                reportBugModel.Title = "Session has timeout";
                reportBugModel.ReportBugStatus = Enums.EnumReportBugStatus.Open;
                reportBugModel.Description = innerMessage;
                reportBugModel.ErrorPath = message;
                return base.OnExceptionAsync(context);
            }

            var result = context.Exception as Xero.NetStandard.OAuth2.Client.ApiException;
   
            if (result != null)
            {
                var ErrorCode = ((Xero.NetStandard.OAuth2.Client.ApiException)context.Exception).ErrorCode;
                if (ErrorCode == 401)
                {
                    message = "Unauthorized";
                    innerMessage = "User must reauthorize xero";
                    context.Result = new JsonResult(new { Message = message, InnerMessage = innerMessage, IsError = true, ErrorCode = ErrorCode });
                }

                if (ErrorCode == 503)
                {
                    message = "Organisation offline";
                    innerMessage = "The organisation temporarily cannot be connected to.";
                    context.Result = new JsonResult(new { Message = message, InnerMessage = innerMessage, IsError = true, ErrorCode = ErrorCode });
                }
                    

                reportBugModel.ReportedBy = emailAddress;
                reportBugModel.Title = "Xero APIException response code: " + ErrorCode;
                reportBugModel.ReportBugStatus = Enums.EnumReportBugStatus.Open;
                reportBugModel.Description = innerMessage;
                reportBugModel.ErrorPath = message;
                //reportBugModel.StackTrace = context.Exception.StackTrace;
                reportBugModel = _reportBugService.UpsertReportBugAsync(reportBugModel).Result;
                return base.OnExceptionAsync(context);
            }

            var resultInnerException = context.Exception.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
            if (resultInnerException != null)
            {
                var ErrorCode = ((Xero.NetStandard.OAuth2.Client.ApiException)context.Exception.InnerException).ErrorCode;
                if (ErrorCode == 400)
                {
                    var ErrorContent = ((Xero.NetStandard.OAuth2.Client.ApiException)context.Exception.InnerException).ErrorContent;
                    message = "Bad Request - A validation exception has occurred";
                    innerMessage = ErrorContent;
                    context.Result = new JsonResult(new { Message = message, InnerMessage = ErrorContent, IsError = true, ErrorCode = ErrorCode });
                }

                if (ErrorCode == 429)
                {
                    var ErrorContent = ((Xero.NetStandard.OAuth2.Client.ApiException)context.Exception.InnerException).ErrorContent;
                    message = "Rate Limit Exceeded - The API rate limit for your organisation/application pairing has been exceeded";
                    innerMessage = "Please wait for a few minutes before trying again";
                    context.Result = new JsonResult(new { Message = message, InnerMessage = ErrorContent, IsError = true, ErrorCode = ErrorCode });
                }

                reportBugModel.ReportedBy = emailAddress;
                reportBugModel.Title = "Xero APIException response code: " + ErrorCode;
                reportBugModel.ReportBugStatus = Enums.EnumReportBugStatus.Open;
                reportBugModel.Description = innerMessage;
                reportBugModel.ErrorPath = message;
                //reportBugModel.StackTrace = context.Exception.StackTrace;
                reportBugModel = _reportBugService.UpsertReportBugAsync(reportBugModel).Result;
                return base.OnExceptionAsync(context);
            }

            reportBugModel.ReportedBy = emailAddress;
            reportBugModel.Title = ((Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)context.ActionDescriptor).ActionName;
            reportBugModel.ReportBugStatus = Enums.EnumReportBugStatus.Open;
            reportBugModel.Description = context.Exception.Message;
            reportBugModel.ErrorPath = context.ActionDescriptor.DisplayName;
            reportBugModel.StackTrace = context.Exception.StackTrace;
            reportBugModel = _reportBugService.UpsertReportBugAsync(reportBugModel).Result;
            var actionName = ((Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)context.ActionDescriptor).ActionName;
            context.Result = new JsonResult(new { IsError = true, ErrorCode = 1, ExceptionPath = actionName, ExceptionMessage = context.Exception.Message, Stacktrace = context.Exception.StackTrace });
            return base.OnExceptionAsync(context);
        }

    }
}
