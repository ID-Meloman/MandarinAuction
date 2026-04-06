using MandarinAuction.Data.Models;
using MandarinAuction.Data;

namespace MandarinAuction.Services
{
    public class MandarinGenerator
    {
        private readonly IServiceProvider _services;
        private readonly IWebHostEnvironment _env;
        private readonly Timer _timer;
        private static readonly Random _random = new Random();

        public MandarinGenerator(IServiceProvider services, IWebHostEnvironment env)
        {
            _services = services;
            _env = env;
            _timer = new Timer(GenerateMandarin, null, TimeSpan.Zero, TimeSpan.FromHours(1));
        }

        private async void GenerateMandarin(object? state)
        {
            using var scope = _services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<appDBContext>();

            var imagesPath = Path.Combine(_env.WebRootPath, "images", "mandarins");

            if (!Directory.Exists(imagesPath))
            {
                Directory.CreateDirectory(imagesPath);
                return;
            }

            var imageFiles = Directory.GetFiles(imagesPath, "*.*")
                .Where(f => f.EndsWith(".jpg") || f.EndsWith(".png") || f.EndsWith(".jpeg"))
                .ToArray();

            if (imageFiles.Length == 0)
                return;

            var randomImage = imageFiles[_random.Next(imageFiles.Length)];
            var relativePath = Path.GetRelativePath(_env.WebRootPath, randomImage).Replace('\\', '/');
            relativePath = "/" + relativePath;

            var mandarin = new Mandarin
            {
                ImagePath = relativePath,
                CreatedAt = DateTime.UtcNow
            };

            await context.Mandarins.AddAsync(mandarin);
            await context.SaveChangesAsync();
        }
    }
}
