namespace XeroApp.Models.BusinessModels
{
	public class XeroTokenModel
	{
		public string UserName { get; set; }
		public string AccessToken { get; set; }
		public string RefreshToken { get; set; }
		public string IdToken { get; set; }
		public DateTime ExpiresAtUtc { get; set; }
	}
}