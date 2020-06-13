using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace EVEMarketSite.Model
{

    public class CNMarketDB : MarketDB
    {
        public CNMarketDB(DbContextOptions<CNMarketDB> options)
            : base(options)
        {
        }
    }

    public class TQMarketDB : MarketDB
    {

        public TQMarketDB(DbContextOptions<TQMarketDB> options)
            : base(options)
        {
        }
    }

    public abstract partial class MarketDB : DbContext
    {
        protected static DbContextOptions<T> ChangeOptionsType<T>(DbContextOptions options) where T : DbContext
        {
          

            var sqlExt = options.Extensions.FirstOrDefault(e => e is NpgsqlOptionsExtension);

            if (sqlExt == null)
                throw (new Exception("Failed to retrieve SQL connection string for base Context"));

            return new DbContextOptionsBuilder<T>()
                .UseNpgsql(((NpgsqlOptionsExtension)sqlExt).ConnectionString, builder => builder.UseNodaTime())
                .Options;
        }
        public static DateTimeZone ChinaTimeZone = DateTimeZoneProviders.Tzdb["Asia/Shanghai"];

        protected MarketDB()
        {
        }

        protected MarketDB(DbContextOptions options)
            : base(options)
        {
        }

        public virtual DbSet<constellations> constellations { get; set; }
        public virtual DbSet<crest_order> crest_order { get; set; }

        public virtual DbSet<current_market> current_market { get; set; }

         
        public virtual DbSet<evetypes> evetypes { get; set; }

        public virtual DbSet<market_markethistory> market_markethistory { get; set; }

        public virtual DbSet<market_markethistorybyday> market_markethistorybyday { get; set; }
        public virtual DbSet<market_realtimehistory> market_realtimehistory { get; set; }
        public virtual DbSet<market_systemstatus> market_systemstatus { get; set; }
        public virtual DbSet<marketgroup> marketgroup { get; set; }

        public virtual DbSet<regions> regions { get; set; }
        public virtual DbSet<stations> stations { get; set; }
        public virtual DbSet<systems> systems { get; set; }
        public virtual DbSet<dogma_attributes> dogma_attributes { get; set; }
        public virtual DbSet<type_attributes> type_attributes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=cevemarket;Username=copyliu;Password=");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<current_market>().HasKey(p => new {p.id, p.regionid});
            modelBuilder.Entity<type_attributes>().HasKey(p => new {p.type_id, p.attribute_id});
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
