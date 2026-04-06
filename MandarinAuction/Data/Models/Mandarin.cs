using System.ComponentModel.DataAnnotations.Schema;

namespace MandarinAuction.Data.Models
{
    public class Mandarin
    {
        public int Id { get; set; }

        public string ImagePath { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public decimal CurrentBid { get; set; } = 0;

        public string? LastBidUserId { get; set; }

        public virtual ApplicationUser? LastBidUser { get; set; }

        public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();
    }
}
