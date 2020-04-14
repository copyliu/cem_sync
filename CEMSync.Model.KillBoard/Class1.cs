using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace CEMSync.Model.KillBoard
{
    public abstract class KillboardDB : DbContext
    {
        protected static DbContextOptions<T> ChangeOptionsType<T>(DbContextOptions options) where T : DbContext
        {

            var sqlExt = options.Extensions.FirstOrDefault(e => e is NpgsqlOptionsExtension);

            if (sqlExt == null)
                throw (new Exception("Failed to retrieve SQL connection string for base Context"));

            return new DbContextOptionsBuilder<T>()
                .UseNpgsql(((NpgsqlOptionsExtension)sqlExt).ConnectionString)
                .Options;
        }
        public virtual DbSet<killboard_waiting_api> killboard_waiting_api { get; set; }

        public virtual DbSet<killboard_war> killboard_war { get; set; }

        protected KillboardDB(DbContextOptions options)
            : base(options)
        {
        }
    }

    public class CNKillboardDB : KillboardDB
    {
        public CNKillboardDB(DbContextOptions<CNKillboardDB> options) : base(options)
        {
        }
    }
    public class TQKillboardDB : KillboardDB
    {
        public TQKillboardDB(DbContextOptions<TQKillboardDB> options) : base(options)
        {
        }
    }
    public class killboard_war
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public int warID { get; set; }

        [Column(TypeName = "jsonb")] public string rawdata { get; set; }

        public bool? finished { get; set; }
        public int? lastkm { get; set; }

        [Column(TypeName = "timestamp with time zone")]
        public DateTime? cacheutil { get; set; }
    }

    public class killboard_waiting_api
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public int killID { get; set; }

        [Required] [StringLength(64)] public string hash { get; set; }

        public bool? error { get; set; }
        public string traceback { get; set; }
        public bool? fromapi { get; set; }
    }
}
