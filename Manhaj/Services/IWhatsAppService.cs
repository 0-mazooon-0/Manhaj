namespace Manhaj.Services
{
    public interface IWhatsAppService
    {
        Task<bool> SendMessageAsync(string to, string message);
    }
}
