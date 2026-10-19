using Microsoft.Extensions.Options;
using MimeKit;
using Web.Application.Configurations;
using Web.Application.Interfaces.Repository;
using Web.Application.Interfaces.Service;
using Web.Infrastructure.Repositories;

namespace Web.Infrastructure.Notifications;

public class EmailNotificationChannel (
    IOptions<MailSettings> mailSettingsOptions, 
    IUserRepository userRepository
    ) : INotificationChannel
{
    public string Name => "email";

    private readonly MailSettings _mailSettings = mailSettingsOptions.Value;

    public async Task SendAsync(Guid userId, string message, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new InvalidOperationException($"Пользователь {userId} не найден");

        var email = new MimeMessage();

        email.From.Add(new MailboxAddress(_mailSettings.SenderName, _mailSettings.SenderEmail));
        email.To.Add(new MailboxAddress(user.FirstName, user.Email));
        email.Subject = "Обновление по заказу";
        email.Body = new TextPart("plain") { Text = message };

        using var client = new MailKit.Net.Smtp.SmtpClient();
        await client.ConnectAsync(_mailSettings.SmtpServer, _mailSettings.Port,
            MailKit.Security.SecureSocketOptions.StartTlsWhenAvailable, cancellationToken);

        if (!string.IsNullOrEmpty(_mailSettings.UserName))
            await client.AuthenticateAsync(_mailSettings.UserName, _mailSettings.Password, cancellationToken);

        await client.SendAsync(email, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
