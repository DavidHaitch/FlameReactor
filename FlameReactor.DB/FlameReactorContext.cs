using System.Collections.Generic;
using FlameReactor.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace FlameReactor.DB
{
    public class FlameReactorContext : DbContext
    {
        public FlameReactorContext() : this(new DbContextOptionsBuilder<FlameReactorContext>().UseSqlite(@"Data Source=.\FlameReactor.sqlite").Options)
        {
        }

        public FlameReactorContext(DbContextOptions<FlameReactorContext> options)
        : base(options)
        {
        }

        public DbSet<Flame> Flames { get; set; }
        public DbSet<Breeding> Breedings { get; set; }
        public DbSet<AccessEvent> AccessEvents { get; set; }
        public DbSet<InteractionEvent> InteractionEvents { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<TweetRecord> TweetRecord { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccessEvent>()
                .HasKey(ae => new { ae.Timestamp, ae.IPAddress });
            modelBuilder.Entity<InteractionEvent>()
                .HasKey(ie => new { ie.Timestamp, ie.IPAddress });
            modelBuilder.Entity<Vote>()
                .HasKey(v => new { v.IPAddress, v.FlameId });

            modelBuilder.Entity<Flame>()
                .HasOne(f => f.Birth)
                .WithOne(b => b.Child)
                .HasForeignKey<Breeding>(f => f.ChildID);

            modelBuilder.Entity<Breeding>()
                .HasOne(b => b.Child)
                .WithOne(f => f.Birth)
                .HasForeignKey<Flame>(f => f.BirthID);

            modelBuilder.Entity<Flame>()
                .HasMany(f => f.Breedings)
                .WithMany(b => b.Parents);
            modelBuilder.Entity<Flame>()
                .HasMany(f => f.Tweets)
                .WithOne(t => t.Owner);
        }
    }
}
