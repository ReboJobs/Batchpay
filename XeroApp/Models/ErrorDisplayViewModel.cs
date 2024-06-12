namespace XeroApp.Models
{
    public class ErrorDisplayViewModel
    {
        public int errorCode { get; set; }
        public string exceptionPath { get; set; }
        public string exceptionMessage { get; set; }
        public string stacktrace { get; set; }
    }
}
