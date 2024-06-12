namespace XeroApp.Models.BusinessModels
{
	public class XeroClientAppModel
	{
		public string? ClientId { get; set; }
		public string? CallbackUri { get; set; }
		public string? Scope { get; set; }
		public string? State { get; set; }
		public string? UserName { get; set; }
		public DateTime DateCreated { get; set; }
	}
}