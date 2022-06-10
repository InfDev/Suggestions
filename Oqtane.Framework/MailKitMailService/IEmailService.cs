using System.Threading.Tasks;

namespace Oqtane.Infrastructure
{
    public interface IEmailService
    {
        Task<string> Send(int siteId, string to, string subject, string html, string from = null);
    }
}
