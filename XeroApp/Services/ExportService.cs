using System.Text;

namespace XeroApp.Services
{
    public interface IExportService
    {
        byte[] ExportToTextFile(string content);
    }

    public class ExportService : IExportService
    {
        public byte[] ExportToTextFile(string content) => Encoding.UTF8.GetBytes(content);
    }
}