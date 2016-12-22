using System.Net.Mail;

namespace DataRetention.Robot.Test1
{
    public class MailFunctions
    {
        public static void SendErrorEmail(string subject, string body)
        {
            using (var smtpClient = new SmtpClient())
            {
                using (var email = new MailMessage(ConfigOptions.EmailErrorsFrom, ConfigOptions.EmailErrorsFrom))
                {
                    email.Subject = subject;
                    email.Body = body;
                    smtpClient.Send(email);
                }
            }
        }
    }
}
