namespace Core
{
	public class XeroToken : BaseEntity
	{
		public string UserName { get; set; }
		public string AccessToken { get; set; }
		public string RefreshToken { get; set; }
		public string IdToken { get; set; }
		public DateTime ExpiresAtUtc { get; set; }
		public string PayLoadData { get; set; }
		public string? CurrentTenantId { get; set; }
		public string? State { get; set; }

	}
}