using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace jafleet.EF
{
    public partial class jafleetContext : DbContext
    {
        public jafleetContext()
        {
        }

        public jafleetContext(DbContextOptions<jafleetContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Aircraft> Aircraft { get; set; }
        public virtual DbSet<AircraftView> AircraftView { get; set; }
        public virtual DbSet<Airline> Airline { get; set; }
        public virtual DbSet<AirlineGroup> AirlineGroup { get; set; }
        public virtual DbSet<Code> Code { get; set; }
        public virtual DbSet<Maker> Maker { get; set; }
        public virtual DbSet<Type> Type { get; set; }

        /*public static readonly LoggerFactory MyLoggerFactory
= new LoggerFactory(new[]
{
            new ConsoleLoggerProvider((category, level)
                => category == DbLoggerCategory.Database.Command.Name
                && level == LogLevel.Information, true)
});*/

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source='../ja-fleet_db/ja-fleet.sqlite3'");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Aircraft>(entity =>
            {
                entity.HasKey(e => e.RegistrationNumber);

                entity.ToTable("AIRCRAFT");

                entity.Property(e => e.RegistrationNumber)
                    .HasColumnName("REGISTRATION_NUMBER")
                    .ValueGeneratedNever();

                entity.Property(e => e.Airline).HasColumnName("AIRLINE");

                entity.Property(e => e.CreationTime).HasColumnName("CREATION_TIME");

                entity.Property(e => e.OperationCode).HasColumnName("OPERATION_CODE");

                entity.Property(e => e.RegisterDate).HasColumnName("REGISTER_DATE");

                entity.Property(e => e.Remarks).HasColumnName("REMARKS");

                entity.Property(e => e.SerialNumber).HasColumnName("SERIAL_NUMBER");

                entity.Property(e => e.TypeCode).HasColumnName("TYPE_CODE");

                entity.Property(e => e.UpdateTime).HasColumnName("UPDATE_TIME");

                entity.Property(e => e.WifiCode).HasColumnName("WIFI_CODE");
            });

            modelBuilder.Entity<AircraftView>(entity =>
            {
                entity.HasKey(e => e.RegistrationNumber);

                entity.ToTable("aircraft_view");

                entity.Property(e => e.RegistrationNumber)
                    .HasColumnName("REGISTRATION_NUMBER")
                    .ValueGeneratedNever();

                entity.Property(e => e.Airline).HasColumnName("AIRLINE");

                entity.Property(e => e.AirlineGroupCode).HasColumnName("AIRLINE_GROUP_CODE");

                entity.Property(e => e.AirlineNameJpShort).HasColumnName("AIRLINE_NAME_JP_SHORT");

                entity.Property(e => e.CreationTime).HasColumnName("CREATION_TIME");

                entity.Property(e => e.DisplayOrder).HasColumnName("DISPLAY_ORDER");

                entity.Property(e => e.Operation).HasColumnName("OPERATION");

                entity.Property(e => e.OperationCode).HasColumnName("OPERATION_CODE");

                entity.Property(e => e.RegisterDate).HasColumnName("REGISTER_DATE");

                entity.Property(e => e.Remarks).HasColumnName("REMARKS");

                entity.Property(e => e.SerialNumber).HasColumnName("SERIAL_NUMBER");

                entity.Property(e => e.TypeCode).HasColumnName("TYPE_CODE");

                entity.Property(e => e.TypeName).HasColumnName("TYPE_NAME");

                entity.Property(e => e.UpdateTime).HasColumnName("UPDATE_TIME");

                entity.Property(e => e.Wifi).HasColumnName("WIFI");

                entity.Property(e => e.WifiCode).HasColumnName("WIFI_CODE");
            });

            modelBuilder.Entity<Airline>(entity =>
            {
                entity.HasKey(e => e.AirlineCode);

                entity.ToTable("AIRLINE");

                entity.Property(e => e.AirlineCode)
                    .HasColumnName("AIRLINE_CODE")
                    .ValueGeneratedNever();

                entity.Property(e => e.AirlineGroupCode).HasColumnName("AIRLINE_GROUP_CODE");

                entity.Property(e => e.AirlineNameEn).HasColumnName("AIRLINE_NAME_EN");

                entity.Property(e => e.AirlineNameEnShort).HasColumnName("AIRLINE_NAME_EN_SHORT");

                entity.Property(e => e.AirlineNameJp).HasColumnName("AIRLINE_NAME_JP");

                entity.Property(e => e.AirlineNameJpShort).HasColumnName("AIRLINE_NAME_JP_SHORT");

                entity.Property(e => e.DisplayOrder).HasColumnName("DISPLAY_ORDER");
            });

            modelBuilder.Entity<AirlineGroup>(entity =>
            {
                entity.HasKey(e => e.AirlineGroupCode);

                entity.ToTable("AIRLINE_GROUP");

                entity.Property(e => e.AirlineGroupCode)
                    .HasColumnName("AIRLINE_GROUP_CODE")
                    .ValueGeneratedNever();

                entity.Property(e => e.AirlineGroupName).HasColumnName("AIRLINE_GROUP_NAME");
            });

            modelBuilder.Entity<Code>(entity =>
            {
                entity.HasKey(e => new { e.CodeType, e.Key });

                entity.ToTable("CODE");

                entity.Property(e => e.CodeType).HasColumnName("CODE_TYPE");

                entity.Property(e => e.Key).HasColumnName("KEY");

                entity.Property(e => e.Value).HasColumnName("VALUE");
            });

            modelBuilder.Entity<Maker>(entity =>
            {
                entity.HasKey(e => e.MakerCode);

                entity.ToTable("MAKER");

                entity.Property(e => e.MakerCode)
                    .HasColumnName("MAKER_CODE")
                    .ValueGeneratedNever();

                entity.Property(e => e.MakerNameEn).HasColumnName("MAKER_NAME_EN");

                entity.Property(e => e.MakerNameJp).HasColumnName("MAKER_NAME_JP");
            });

            modelBuilder.Entity<Type>(entity =>
            {
                entity.HasKey(e => e.TypeCode);

                entity.ToTable("TYPE");

                entity.Property(e => e.TypeCode)
                    .HasColumnName("TYPE_CODE")
                    .ValueGeneratedNever();

                entity.Property(e => e.DisplayOrder).HasColumnName("DISPLAY_ORDER");

                entity.Property(e => e.MakerCode).HasColumnName("MAKER_CODE");

                entity.Property(e => e.TypeName).HasColumnName("TYPE_NAME");
            });
        }
    }
}
