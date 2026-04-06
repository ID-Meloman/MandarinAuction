using MandarinAuction.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MandarinAuction.Data.Configurations
{
    public class BidConfiguration : IEntityTypeConfiguration<Bid>
    {
        public void Configure(EntityTypeBuilder<Bid> builder)
        {
            // Первичный ключ с автоинкрементом
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).UseIdentityColumn();

            // Обязательные поля
            builder.Property(x => x.Amount)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Внешние ключи
            builder.HasOne(b => b.Mandarin)
                .WithMany(m => m.Bids)
                .HasForeignKey(b => b.MandarinId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(b => b.User)
                .WithMany(u => u.Bids)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Индексы
            builder.HasIndex(b => b.MandarinId);
            builder.HasIndex(b => b.UserId);
            builder.HasIndex(b => b.CreatedAt);
        }
    }
}
