using System.Threading.Tasks;

namespace HomeEconomics.Website.Services
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }
}
