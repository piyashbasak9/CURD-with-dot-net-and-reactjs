
namespace TaskManager.Infrastructure.Email;

public class GmailSmtpOptions
{
    public string Host { get; set; } = "smtp.gmail.com";

    public int Port { get; set; } = 587;

    public string SenderName { get; set; } = "Task Manager";

    public string SenderEmail { get; set; } = string.Empty;

    public string AppPassword { get; set; } = string.Empty;
}
