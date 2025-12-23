namespace Infrastructure.Email;

public interface IEmailService
{
    public Task SendSuccessfulEmailAsync(string email, string message, string subject);

    public Task SendErrorEmailAsync(string email, string message, string subject);
}