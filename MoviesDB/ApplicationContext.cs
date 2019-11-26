using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MoviesDB
{

    public class ApplicationContext : DbContext
    {
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Actor> Actors { get; set; }
        //public DbSet<Tag> Tags { get; set; }

        public ApplicationContext()
        {
            Database.EnsureDeleted(); //удаление бд
            Database.EnsureCreated();
            // myDbContext.Database.Migrate(); //нельзя использовать с EnsureCreated()
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MovieActor>()
                .HasKey(t => new { t.ActorNconstId, t.MovieTconstId });

            modelBuilder.Entity<MovieActor>()
                .HasOne(ma => ma.Actor)
                .WithMany(a => a.MovieActors)
                .HasForeignKey(ma => ma.ActorNconstId);

            modelBuilder.Entity<MovieActor>()
                .HasOne(ma => ma.Actor)
                .WithMany(m => m.MovieActors)
                .HasForeignKey(ma => ma.MovieTconstId);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=Movies.db");
            //optionsBuilder.UseLoggerFactory(MyLoggerFactory);
        }

        // устанавливаем фабрику логгера
        //public static readonly ILoggerFactory MyLoggerFactory = LoggerFactory.Create(builder =>
        //{
        //    builder.AddProvider(new MyLoggerFactory());    // указываем наш провайдер логгирования
        //});
    } 
    
}
