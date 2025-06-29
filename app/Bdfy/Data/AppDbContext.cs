using Microsoft.EntityFrameworkCore;
using BDfy.Models;

namespace BDfy.Data
{
    public class BDfyDbContext : DbContext
    {
        public BDfyDbContext(DbContextOptions<BDfyDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserDetails> UserDetails { get; set; }
        public DbSet<AuctioneerDetails> AuctioneerDetails { get; set; }
        public DbSet<Auction> Auctions { get; set; }
        public DbSet<Lot> Lots { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<AutoBidConfig> AutoBidConfigs { get; set; }
        public DbSet<AuctionLot> AuctionLots { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<UserDetails>().ToTable("userdetails");
            modelBuilder.Entity<AuctioneerDetails>().ToTable("auctioneerdetails");
            modelBuilder.Entity<Auction>().ToTable("auctions");
            modelBuilder.Entity<Lot>().ToTable("lots");
            modelBuilder.Entity<Bid>().ToTable("bids");
            modelBuilder.Entity<AutoBidConfig>().ToTable("autobidconfig");
            modelBuilder.Entity<AuctionLot>().ToTable("auctionlot");

            //  UserDetails <-> User (1:1)
            modelBuilder.Entity<User>()
                .HasOne(u => u.UserDetails)
                .WithOne(ud => ud.User)
                .HasForeignKey<UserDetails>(ud => ud.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // AuctioneerDetails <-> User (1:1)
            modelBuilder.Entity<User>()
                .HasOne(u => u.AuctioneerDetails)
                .WithOne(ad => ad.User)
                .HasForeignKey<AuctioneerDetails>(ad => ad.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .OwnsOne(u => u.Direction, dir =>
                {
                    dir.Property(d => d.Street)
                        .HasColumnName("direction_street");
                    dir.Property(d => d.StreetNumber)
                        .HasColumnName("direction_street_number");
                    dir.Property(d => d.Corner)
                        .HasColumnName("direction_corner");
                    dir.Property(d => d.ZipCode)
                        .HasColumnName("direction_zip_code");
                    dir.Property(d => d.Department)
                        .HasColumnName("direction_department");
                });

            modelBuilder.Entity<Auction>()
            .OwnsOne(a => a.Direction, dir =>
            {
                dir.Property(d => d.Street)
                    .HasColumnName("direction_street");
                dir.Property(d => d.StreetNumber)
                    .HasColumnName("direction_street_number");
                dir.Property(d => d.Corner)
                    .HasColumnName("direction_corner");
                dir.Property(d => d.ZipCode)
                    .HasColumnName("direction_zip_code");
                dir.Property(d => d.Department)
                    .HasColumnName("direction_department");
            });

            // Auction --> AuctioneerDetails
            modelBuilder.Entity<Auction>(entity =>
            {
                entity.HasOne(a => a.Auctioneer) // Auctioneer es AuctioneerDetails
                        .WithMany(ad => ad.Auctions)
                        .HasForeignKey(a => a.AuctioneerId)
                        .OnDelete(DeleteBehavior.Restrict);
            });

            //  Lot --> Winner (User)
            modelBuilder.Entity<Lot>(entity =>
            {
                entity.HasOne(l => l.Winner)
                    .WithMany(ud => ud.Lots)
                    .HasForeignKey(l => l.WinnerId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Lot --> Bid
                entity.HasMany(l => l.BiddingHistory)
                      .WithOne(b => b.Lot)
                      .HasForeignKey(b => b.LotId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Bid --> User (Buyer)
            modelBuilder.Entity<Bid>(entity =>
            {
                entity.HasOne(b => b.Buyer) // User details
                      .WithMany(ud => ud.Bids)
                      .HasForeignKey(b => b.BuyerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AutoBidConfig>(entity =>
            {
                // AutoBidConfig --> UserDetails (Buyer)
                entity.HasOne(ab => ab.Buyer)
                    .WithMany(ud => ud.AutoBidConfigs)
                    .HasForeignKey(ab => ab.BuyerId)
                    .OnDelete(DeleteBehavior.Cascade);

                // AutoBidConfig --> Lot
                entity.HasOne(ab => ab.Lot)
                    .WithMany(l => l.AutoBidHistory)
                    .HasForeignKey(ab => ab.LotId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<AuctionLot>()
                .Property(al => al.IsOriginalAuction)
                .IsRequired();

            // Auction <--> Lot entidad intermedia AuctionLot
            modelBuilder.Entity<AuctionLot>(entity =>
            {
                entity.HasKey(al => new { al.AuctionId, al.LotId });

                entity.HasOne(al => al.Auction)
                    .WithMany(a => a.AuctionLots)
                    .HasForeignKey(al => al.AuctionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(al => al.Lot)
                    .WithMany(l => l.AuctionLots)
                    .HasForeignKey(al => al.LotId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(al => al.IsOriginalAuction)
                    .HasDefaultValue(true);
            });
        }
    }
}