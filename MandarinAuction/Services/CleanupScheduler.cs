using Microsoft.EntityFrameworkCore;
using MandarinAuction.Data;

namespace MandarinAuction.Services
{
    public class CleanupScheduler
    {
        private readonly IServiceProvider _services;
        private Timer? _timer;

        public CleanupScheduler(IServiceProvider services)
        {
            _services = services;
            ScheduleCleanup();
        }

        private void ScheduleCleanup()
        {
            var now = DateTime.Now;
            var nextRun = now.Date.AddDays(1);
            //var nextRun = now.AddMinutes(1); // Тестовый режим
            var firstDelay = nextRun - now;

            _timer = new Timer(CleanOldMandarins, null, firstDelay, TimeSpan.FromDays(1));
        }

        private async void CleanOldMandarins(object? state)
        {
            using var scope = _services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<appDBContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<CleanupScheduler>>();
            var twelveHoursAgo = DateTime.UtcNow.AddHours(-12);
            //var twelveHoursAgo = DateTime.UtcNow.AddMinutes(-7); // Тестовый режим

            var oldMandarins = await context.Mandarins
                .Include(m => m.LastBidUser)
                .Include(m => m.Bids)
                .ThenInclude(b => b.User)
                .Where(m => m.CreatedAt < twelveHoursAgo)
                .ToListAsync();

            if (oldMandarins.Any())
            {
                // Отправляем чеки победителям
                foreach (var mandarin in oldMandarins)
                {
                    if (mandarin.LastBidUser != null && mandarin.CurrentBid > 0)
                    {
                        try
                        {
                            await emailService.SendWinnerReceiptAsync(
                                mandarin.LastBidUser.Email!,
                                mandarin.LastBidUser.Email!.Split('@')[0],
                                mandarin
                            );
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, $"Ошибка отправки чека для мандарина #{mandarin.Id}");
                        }
                    }
                    else
                    {
                        logger.LogInformation($"Мандарин #{mandarin.Id} удалён без победителя (ставок не было)");
                    }
                }

                // Удаляем старые мандарины
                context.Mandarins.RemoveRange(oldMandarins);
                await context.SaveChangesAsync();

                logger.LogInformation($"Удалено {oldMandarins.Count} старых мандаринов");
            }

            // Перепланируем на следующий день
            ScheduleCleanup();
        }
    }
}
