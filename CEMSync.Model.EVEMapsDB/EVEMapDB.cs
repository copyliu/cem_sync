using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CEMSync.Model.EVEMapsDB
{
    public partial class EVEMapDB : DbContext
    {
      
        public EVEMapDB(DbContextOptions<EVEMapDB> options)
            : base(options)
        {
        }

        public virtual DbSet<evetypes> evetypes { get; set; }
        public virtual DbSet<maps_alliance_history> maps_alliance_history { get; set; }
        public virtual DbSet<maps_alliance_statics> maps_alliance_statics { get; set; }
        public virtual DbSet<maps_alliances> maps_alliances { get; set; }
        public virtual DbSet<maps_campaignatk> maps_campaignatk { get; set; }
        public virtual DbSet<maps_campaigndef> maps_campaigndef { get; set; }
        public virtual DbSet<maps_campaigns> maps_campaigns { get; set; }
        public virtual DbSet<maps_campaigns_history> maps_campaigns_history { get; set; }
        public virtual DbSet<maps_constellationjumps> maps_constellationjumps { get; set; }
        public virtual DbSet<maps_constellations> maps_constellations { get; set; }
        public virtual DbSet<maps_corporation> maps_corporation { get; set; }
        public virtual DbSet<maps_corporation_history> maps_corporation_history { get; set; }
        public virtual DbSet<maps_corporation_statics> maps_corporation_statics { get; set; }
        public virtual DbSet<maps_eventhistory> maps_eventhistory { get; set; }
        public virtual DbSet<maps_factions> maps_factions { get; set; }
        public virtual DbSet<maps_jumpstatic> maps_jumpstatic { get; set; }
        public virtual DbSet<maps_killstatic> maps_killstatic { get; set; }
        public virtual DbSet<maps_outposts> maps_outposts { get; set; }
        public virtual DbSet<maps_outposts_history> maps_outposts_history { get; set; }
        public virtual DbSet<maps_publiccrestapicache> maps_publiccrestapicache { get; set; }
        public virtual DbSet<maps_regionjumps> maps_regionjumps { get; set; }
        public virtual DbSet<maps_regions> maps_regions { get; set; }
        public virtual DbSet<maps_serverstatus> maps_serverstatus { get; set; }
        public virtual DbSet<maps_sovereignty> maps_sovereignty { get; set; }
        public virtual DbSet<maps_sovereignty_history> maps_sovereignty_history { get; set; }
        public virtual DbSet<maps_stastationtypes> maps_stastationtypes { get; set; }
        public virtual DbSet<maps_struct> maps_struct { get; set; }
        public virtual DbSet<maps_struct_history> maps_struct_history { get; set; }
        public virtual DbSet<maps_systemgrid> maps_systemgrid { get; set; }
        public virtual DbSet<maps_systemjumps> maps_systemjumps { get; set; }
        public virtual DbSet<maps_systems> maps_systems { get; set; }
        public virtual DbSet<maps_universe> maps_universe { get; set; }
        public virtual DbSet<marketgroup> marketgroup { get; set; }
        public virtual DbSet<system_trn> system_trn { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseNpgsql("Host=localhost;Database=evemaps;Username=postgres;Password=");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
