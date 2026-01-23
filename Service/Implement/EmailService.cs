using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Service.Interface;

namespace Service.Implement;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendOtpEmailAsync(string toEmail, string otpCode)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");
        var fromEmail = emailSettings["FromEmail"];
        var fromName = emailSettings["FromName"];
        var smtpServer = emailSettings["SmtpServer"];
        var smtpPort = int.Parse(emailSettings["SmtpPort"]!);
        var username = emailSettings["Username"];
        var password = emailSettings["Password"];

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(new MailboxAddress(toEmail, toEmail));
        message.Subject = "StudyMate - Email Verification OTP";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #4CAF50;'>Welcome to StudyMate!</h2>
                        <p>Thank you for registering. Your verification code is:</p>
                        <div style='background-color: #f4f4f4; padding: 15px; text-align: center; font-size: 24px; font-weight: bold; letter-spacing: 5px; margin: 20px 0;'>
                            {otpCode}
                        </div>
                        <p>This code will expire in 10 minutes.</p>
                        <p>If you didn't request this code, please ignore this email.</p>
                        <hr style='margin-top: 30px;'>
                        <p style='color: #666; font-size: 12px;'>This is an automated message, please do not reply.</p>
                    </div>
                </body>
                </html>
            "
        };

        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(username, password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    public async Task SendVerificationEmailAsync(string toEmail, string verificationLink)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");
        var fromEmail = emailSettings["FromEmail"];
        var fromName = emailSettings["FromName"];
        var smtpServer = emailSettings["SmtpServer"];
        var smtpPort = int.Parse(emailSettings["SmtpPort"]!);
        var username = emailSettings["Username"];
        var password = emailSettings["Password"];

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(new MailboxAddress(toEmail, toEmail));
        message.Subject = "StudyMate - Verify Your Email Address";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                        <div style='text-align: center; margin-bottom: 30px;'>
                            <h1 style='color: #4CAF50; margin-bottom: 10px;'>Welcome to StudyMate!</h1>
                            <p style='color: #666; font-size: 16px;'>Thank you for registering with us</p>
                        </div>
                        
                        <div style='background-color: #f9f9f9; padding: 20px; border-radius: 5px; margin-bottom: 20px;'>
                            <p style='margin: 0 0 15px 0; color: #333;'>
                                To complete your registration, please verify your email address by clicking the button below:
                            </p>
                            
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='{verificationLink}' 
                                   style='background-color: #4CAF50; 
                                          color: white; 
                                          padding: 15px 40px; 
                                          text-decoration: none; 
                                          border-radius: 5px; 
                                          font-size: 16px;
                                          font-weight: bold;
                                          display: inline-block;'>
                                    Verify Email Address
                                </a>
                            </div>
                            
                            <p style='margin: 15px 0 0 0; color: #666; font-size: 14px;'>
                                Or copy and paste this link into your browser:
                            </p>
                            <p style='word-break: break-all; color: #4CAF50; font-size: 12px;'>
                                {verificationLink}
                            </p>
                        </div>
                        
                        <div style='background-color: #fff3cd; padding: 15px; border-radius: 5px; border-left: 4px solid #ffc107; margin-bottom: 20px;'>
                            <p style='margin: 0; color: #856404; font-size: 14px;'>
                                <strong>?? Important:</strong> This verification link will expire in 24 hours.
                            </p>
                        </div>
                        
                        <div style='margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd;'>
                            <p style='color: #999; font-size: 12px; margin: 5px 0;'>
                                If you didn't create an account with StudyMate, please ignore this email.
                            </p>
                            <p style='color: #999; font-size: 12px; margin: 5px 0;'>
                                This is an automated message, please do not reply.
                            </p>
                        </div>
                    </div>
                </body>
                </html>
            "
        };

        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(username, password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
