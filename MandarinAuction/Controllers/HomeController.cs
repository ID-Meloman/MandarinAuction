using MandarinAuction.Data;
using MandarinAuction.Data.Models;
using MandarinAuction.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MandarinAuction.Controllers
{
    public class HomeController : Controller
    {
        private readonly appDBContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EmailService _emailService;
        private readonly MandarinGenerator _generatorService;
        private readonly CleanupScheduler _cleanupService;
        public HomeController(
            appDBContext context,
            UserManager<ApplicationUser> userManager,
            MandarinGenerator generatorService,
            CleanupScheduler cleanupService,
            EmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _generatorService = generatorService;
            _cleanupService = cleanupService;
        }

        //Главная страница
        public async Task<IActionResult> Index()
        {
            var mandarins = await _context.Mandarins
                .Include(m => m.LastBidUser)
                .Include(m => m.Bids)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return View(mandarins);
        }

        //обработка ставки
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PlaceBid(int mandarinId, decimal amount)
        {
            var mandarin = await _context.Mandarins
                .Include(m => m.Bids)
                .FirstOrDefaultAsync(m => m.Id == mandarinId);

            if (mandarin == null)
            {
                TempData["Error"] = "Мандарин не найден";
                return RedirectToAction(nameof(Index));
            }

            if (amount <= mandarin.CurrentBid)
            {
                TempData["Error"] = $"Ставка должна быть больше {mandarin.CurrentBid} ₽";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var previousLeader = mandarin.LastBidUser;
            var previousBid = mandarin.CurrentBid;

            // Создаём ставку
            var bid = new Bid
            {
                Amount = amount,
                MandarinId = mandarinId,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            mandarin.CurrentBid = amount;
            mandarin.LastBidUserId = user.Id;
            mandarin.Bids.Add(bid);

            await _context.Bids.AddAsync(bid);
            await _context.SaveChangesAsync();

            if (previousLeader != null && previousLeader.Id != user.Id && previousBid > 0)
            {
                await _emailService.SendOutbidNotificationAsync(
                    previousLeader.UserName!,
                    previousLeader.UserName!.Split('@')[0],
                    previousBid,
                    amount,
                    mandarinId.ToString()
                );
            }

            TempData["Success"] = $"Ставка {amount} ₽ принята!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Test()
        {
            return Json(new { message = "Сервер работает", time = DateTime.Now });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
