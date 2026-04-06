using MandarinAuction.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MandarinAuction.Data.Configurations
{
    public class MandarinConfiguration : IEntityTypeConfiguration<Mandarin>
    {
        public void Configure(EntityTypeBuilder<Mandarin> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).UseIdentityColumn();

            builder.Property(x => x.ImagePath)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.CurrentBid)
                .IsRequired()
                .HasDefaultValue(0)
                .HasPrecision(18, 2);

            // Внешний ключ для последней ставки
            builder.HasOne(m => m.LastBidUser)
                .WithMany()
                .HasForeignKey(m => m.LastBidUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Индексы
            builder.HasIndex(m => m.CreatedAt);
            builder.HasIndex(m => m.CurrentBid);
        }
    }
}
