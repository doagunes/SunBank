using System.Net; //Ağ işlemleri (network operations) için gerekli sınıfları içerir.
using System.Net.Mail; //E-posta gönderimi için gerekli MailMessage ve SmtpClient sınıflarını içerir.

public class MailService
{
    private readonly string _smtpServer = "smtp.gmail.com";
    private readonly int _smtpPort = 587;
    private readonly string _fromEmail = "intbank0725@gmail.com"; 
    private readonly string _appPassword = "byds kjhk jmuu palq";  

    //toEmail: Alıcı e-posta adresi.
    //resetLink: Kullanıcının şifresini sıfırlaması için gönderilen bağlantı 
    public async Task SendResetPasswordEmailAsync(string toEmail, string resetLink)
    {
        var message = new MailMessage
        {
            //MailMessage sınıfı, gönderilecek e-postayı temsil eder.
            //From: Gönderen kişi ve adı ("Intern Bank Support").
            From = new MailAddress(_fromEmail, "Intern Bank Support"), 
            Subject = "Password Reset Link",
            Body = $"Hello!,\n\nYou can reset your password with click the link below:\n{resetLink}\n\nIf you not make this request you may not worry about that.",
            IsBodyHtml = false,
        };
        //message.To.Add(toEmail): Alıcının e-posta adresi eklenir.
        message.To.Add(toEmail);

        //SmtpClient: E-posta sunucusuna bağlanmak için kullanılır.
        //Credentials: Gmail’e bağlanmak için kimlik bilgileri (e-posta + uygulama şifresi).
        //EnableSsl = true: Güvenli bağlantı (TLS/SSL) kullanılır.
        using var smtpClient = new SmtpClient(_smtpServer, _smtpPort)
        {
            Credentials = new NetworkCredential(_fromEmail, _appPassword),
            EnableSsl = true,
        };
        //SendMailAsync: E-posta gönderme işlemi burada gerçekleşir.
        await smtpClient.SendMailAsync(message);
    }
}
