using XeroApp.Models.BusinessModels;

namespace XeroApp.Utilities
{
    public class Global
    {
        public static string GlobalBrowser { get; set; } = string.Empty;

        public static string UploadUrl { get; set; } = string.Empty;

        public static List<IdeaModel> globallistIdeas { get; set; } = new List<IdeaModel>();

        public static List<TenantLog> globallistTenantLog { get; set; } = new List<TenantLog>();

    }
}
