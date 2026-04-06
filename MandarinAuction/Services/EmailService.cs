using MailKit.Net.Smtp;
using MailKit.Security;
using MandarinAuction.Data.Models;
using Microsoft.Extensions.Options;
using MimeKit;

namespace MandarinAuction.Services
{
    public class EmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task<bool> SendOutbidNotificationAsync(string toEmail, string toUserName, decimal yourBid, decimal newBid, string mandarinId)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Мандариновый аукцион", _emailSettings.FromEmail));
                message.To.Add(new MailboxAddress(toUserName, toEmail));
                message.Subject = "Вашу ставку перебили!";

                var body = $@"
                    <html>
                    <body>
                        <h2>Вашу ставку перебили!</h2>
                        <p>Здравствуйте, {toUserName}!</p>
                        <p>Ваша ставка в размере <strong>{yourBid} ₽</strong> на мандарин #{mandarinId} была перебита.</p>
                        <p>Новая текущая ставка: <strong style='color: #ff6b00;'>{newBid} ₽</strong></p>
                        <p>Вы можете сделать новую ставку, перейдя по ссылке:</p>
                        <p><a href='{_emailSettings.SiteUrl}' style='background-color: #ff6b00; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Сделать новую ставку</a></p>
                        <hr>
                        <p style='color: #666; font-size: 12px;'>Это автоматическое сообщение, пожалуйста, не отвечайте на него.</p>
                    </body>
                    </html>
                ";

                message.Body = new TextPart("html") { Text = body };

                using var client = new SmtpClient();

                // Подключаемся к SMTP серверу
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);

                // Аутентификация
                await client.AuthenticateAsync(_emailSettings.SmtpUser, _emailSettings.SmtpPassword);

                // Отправка
                await client.SendAsync(message);

                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка отправки email: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        // Отправка чека победителю
        public async Task SendWinnerReceiptAsync(string toEmail, string toUserName, Mandarin mandarin)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Мандариновый аукцион", _emailSettings.FromEmail));
                message.To.Add(new MailboxAddress(toUserName, toEmail));
                message.Subject = $"Ваш мандарин #{mandarin.Id} отправлен!";
                var body = $@"
                <html>
                <body>
                <h2>Поздравляем с победой! 🍊</h2>
                <p>Здравствуйте, {toUserName}!</p>
                <p>Вы выиграли мандарин <strong>#{mandarin.Id}</strong> на аукционе.</p>
                <h3>Детали вашей покупки:</h3>
                <ul>
                    <li><strong>Номер лота:</strong> #{mandarin.Id}</li>
                    <li><strong>Победитель:</strong> {toUserName}</li>
                    <li><strong>Дата победы:</strong> {DateTime.Now:dd.MM.yyyy HH:mm:ss}</li>
                    <li><strong>Создан:</strong> {mandarin.CreatedAt:dd.MM.yyyy HH:mm}</li>
                    <li><strong>Количество ставок:</strong> {mandarin.Bids.Count}</li>
                    <li><strong>Итоговая сумма:</strong> <span style='color: #ff6b00; font-size: 18px; font-weight: bold;'>{mandarin.CurrentBid:N2} ₽</span></li>
                </ul>
                <p><strong>Мандарин отправлен!</strong><br>
                <p style='color: #666; font-size: 12px;'>Спасибо за участие в нашем аукционе!<br>
                Ваш мандарин уже в пути<br>
                © 2026 Мандариновый аукцион</p>
            </body>
            </html>
        ";

                message.Body = new TextPart("html") { Text = body };

                using var client = new SmtpClient();
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_emailSettings.SmtpUser, _emailSettings.SmtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка отправки чека: {ex.Message}");
            }
        }
    }

    public class EmailSettings
    {
        public string SmtpServer { get; set; } = "smtp.gmail.com";
        public int SmtpPort { get; set; } = 587;
        public string SmtpUser { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string SiteUrl { get; set; } = "https://localhost:5000";
    }
}