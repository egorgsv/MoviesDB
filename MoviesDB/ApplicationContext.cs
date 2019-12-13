using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MoviesDB
{

    public class ApplicationContext : DbContext
    {
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<Tag> Tags { get; set; }
        
        public DbSet<links_IMDB_MovieLens> links_IMDB_MovieLens { get; set; }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        { 
            modelBuilder.Entity<MovieActor>()
                .HasKey(t => new { t.ActorNconstId, t.MovieTconstId });

            modelBuilder.Entity<MovieActor>()
                .HasOne(ma => ma.Actor)
                .WithMany(a => a.MovieActors)
                .HasForeignKey(ma => ma.ActorNconstId);

            modelBuilder.Entity<MovieActor>()
                .HasOne(ma => ma.Movie)
                .WithMany(m => m.MovieActors)
                .HasForeignKey(ma => ma.MovieTconstId);

            modelBuilder.Entity<MovieTag>()
                .HasKey(t => new { t.TagId, t.MovieTconstId });

            modelBuilder.Entity<MovieTag>()
                .HasOne(mt => mt.Tag)
                .WithMany(t => t.MovieTags)
                .HasForeignKey(mt => mt.TagId);

            modelBuilder.Entity<MovieTag>()
                .HasOne(mt => mt.Movie)
                .WithMany(m => m.MovieTags)
                .HasForeignKey(mt => mt.MovieTconstId);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=Movies.db");
            
        }

    } 
    
}
