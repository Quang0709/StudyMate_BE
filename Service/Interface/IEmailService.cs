using System.Threading.Tasks;

namespace Service.Interface;

public interface IEmailService
{
    Task SendOtpEmailAsync(string toEmail, string otpCode);
}
