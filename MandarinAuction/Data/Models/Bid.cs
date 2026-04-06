using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MandarinAuction.Data.Models
{
    public class Bid
    {
        public int Id { get; set; }

        public decimal Amount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int MandarinId { get; set; }

        public string UserId { get; set; } = string.Empty;

        public virtual Mandarin Mandarin { get; set; } = null!;

        public virtual ApplicationUser User { get; set; } = null!;
    }
}
