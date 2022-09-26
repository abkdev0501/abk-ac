using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace Arity.Service.Helpers
{
    // Sendgrid implementation
    public class EmailSender : IEmailSender
    {
        public void SendEmail(string subject, string messageBody, string path)
        {
            var apiKey = ConfigurationManager.AppSettings["SendGridKey"];
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("ahir.prakash1992@outlook.com", "RMN Notification");

            var toEmails = new List<EmailAddress>()
            {
                new EmailAddress("ahir.prakash1992@gmail.com", "RMN Notification"),
                new EmailAddress("mrnarshana@gmail.com", "RMN Notification"),
                new EmailAddress("niravshah.apr22@gmail.com", "RMN Notification")
            };

            var plainTextContent = messageBody;
            var htmlContent = $"<strong>{messageBody}</strong>";

            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, toEmails, subject, plainTextContent, htmlContent);

            var bytes = File.ReadAllBytes(path);
            var file = Convert.ToBase64String(bytes);
            msg.AddAttachment("backup.sql", file);
            client.SendEmailAsync(msg).Wait();
        }
    }
}
