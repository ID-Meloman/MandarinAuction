using Microsoft.AspNetCore.Identity;

namespace MandarinAuction.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Навигационные свойства
        public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();
    }
}
