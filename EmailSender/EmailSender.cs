using MimeKit;
using MailKit.Net.Smtp;

namespace EmailService;

public class EmailSender : IEmailSender
{
    private readonly EmailConfiguration emailConfig;

    public EmailSender(EmailConfiguration emailConfig)
    {
        this.emailConfig = emailConfig;
    }

    public async Task SendEmailAsync(Message message)
    {
        var emailMessage = CreateEmailMessage(message);

        await SendAsync(emailMessage);
    }

    private async Task SendAsync(MimeMessage emailMessage)
    {
        using var client = new SmtpClient();

        try
        {
            client.Connect(emailConfig.SmtpServer, emailConfig.Port, true);
            client.AuthenticationMechanisms.Remove("XOAUTH2");
            await client.AuthenticateAsync(emailConfig.Username, emailConfig.Password);

            await client.SendAsync(emailMessage);
        }
        finally
        {
            await client.DisconnectAsync(true);
            client.Dispose();
        }
    }

    private MimeMessage CreateEmailMessage(Message message)
    {
        var emailMessage = new MimeMessage();

        emailMessage.From.Add(new MailboxAddress(string.Empty, emailConfig.From));
        emailMessage.To.AddRange(message.To);
        emailMessage.Subject = message.Subject;
        emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = message.Content };

        return emailMessage;
    }
}
