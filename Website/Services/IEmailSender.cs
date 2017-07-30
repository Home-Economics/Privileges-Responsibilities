using System.Threading.Tasks;

namespace HomeEconomics.Website.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
