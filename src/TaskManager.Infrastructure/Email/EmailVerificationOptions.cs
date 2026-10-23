
namespace TaskManager.Infrastructure.Email;

public class EmailVerificationOptions
{
    public string HmacKey { get; set; } = string.Empty;

    public int CodeLifetimeMinutes { get; set; } = 10;
}
