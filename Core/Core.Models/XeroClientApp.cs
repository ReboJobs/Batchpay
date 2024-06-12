namespace Core
{
	public class XeroClientApp : BaseEntity
	{
		public string UserName { get; set; }
		public string? ClientId { get; set; }
		public string? CallbackUri { get; set; }
		public string? Scope { get; set; }
		public string? State { get; set; }
	}
}