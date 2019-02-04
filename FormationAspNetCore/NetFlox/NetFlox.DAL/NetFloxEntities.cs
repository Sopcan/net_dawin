using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NetFlox.DAL
{
    public class NetFloxEntities : DbContext
    {
        public DbSet<Role> Roles { get; set; }

        public DbSet<Celebrite> Celebrites { get; set; }
        public DbSet<Film> Films { get; set; }
        public DbSet<Utilisateur> Utilisateurs { get; set; }

        public DbSet<Favori> Favoris { get; set; }
        public DbSet<RoleCelebriteFilm> RoleCelebriteFilms { get; set; }


        public static string DbFilePath
        {
            get
            {
                var dbFolder = Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.ApplicationData,
                        Environment.SpecialFolderOption.DoNotVerify),
                    "NetFlox");
                return Path.Combine(dbFolder, "NetFlox.db");
            }
        }

        public IEnumerable<object> Celebrities { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(DbFilePath));
            var builder = new SqliteConnectionStringBuilder
            {
                DataSource = DbFilePath,
            };
            optionsBuilder.UseSqlite(builder.ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Relation Favori : Utlisateur - Film
            modelBuilder
                .Entity<Favori>()
                .HasKey(fav => new { fav.UtilisateurId, fav.FilmId });
            modelBuilder
                .Entity<Favori>()
                .HasOne(fav => fav.Utilisateur)
                .WithMany(u => u.Favoris)
                .HasForeignKey(f => f.UtilisateurId);
            modelBuilder
                .Entity<Favori>()
                .HasOne(fav => fav.Film)
                .WithMany(film => film.Favoris)
                .HasForeignKey(fav => fav.FilmId);

            // Relation RoleCelebriteFilm : Role - Celebrite - Film
            modelBuilder
                .Entity<RoleCelebriteFilm>()
                .HasOne(rcf => rcf.Role)
                .WithMany()
                .HasForeignKey(rcf => rcf.RoleId);
            modelBuilder
                .Entity<RoleCelebriteFilm>()
                .HasOne(rcf => rcf.Film)
                .WithMany(film => film.RoleCelebriteFilms)
                .HasForeignKey(rcf => rcf.FilmId);
            modelBuilder
                .Entity<RoleCelebriteFilm>()
                .HasOne(rcf => rcf.Celebrite)
                .WithMany(c => c.RoleCelebriteFilms)
                .HasForeignKey(rcf => rcf.CelebriteId);

            // Properties Utilisateur
            modelBuilder
                .Entity<Utilisateur>()
                .Property(u => u.Nom)
                .IsRequired()
                .HasColumnType("nvarchar(100)");
            modelBuilder
                .Entity<Utilisateur>()
                .Property(u => u.Prenom)
                .IsRequired()
                .HasColumnType("nvarchar(100)");
            modelBuilder
                .Entity<Utilisateur>()
                .Property(u => u.AdresseEmail)
                .IsRequired()
                .HasColumnType("varchar(255)");
            modelBuilder
                .Entity<Utilisateur>()
                .Property(u => u.DateInscription)
                .IsRequired()
                .HasColumnType("datetime");

            // Properties Celebrite
            modelBuilder
                .Entity<Celebrite>()
                .Property(c => c.Nom)
                .IsRequired()
                .HasColumnType("nvarchar(200)");
            //modelBuilder
            //    .Entity<Celebrite>()
            //    .Property(c => c.Prenom)
            //    .IsRequired()
            //    .HasColumnType("nvarchar(100)");
            modelBuilder
                .Entity<Celebrite>()
                .Property(c => c.DateNaissance)
                .HasColumnType("date");
            modelBuilder
                .Entity<Celebrite>()
                .Property(c => c.UrlPhoto)
                .HasColumnType("varchar(1000)");

            // Properties Role
            modelBuilder
                .Entity<Role>()
                .Property(r => r.Id)
                .ValueGeneratedNever();
            modelBuilder
                .Entity<Role>()
                .Property(r => r.Libelle)
                .IsRequired()
                .HasColumnType("varchar(50)");

            // Properties Film
            modelBuilder
                .Entity<Film>()
                .Property(f => f.Titre)
                .IsRequired()
                .HasColumnType("nvarchar(200)");
            modelBuilder
                .Entity<Film>()
                .Property(f => f.Description)
                .HasColumnType("nvarchar(1000)");
            modelBuilder
                .Entity<Film>()
                .Property(f => f.Pays)
                .HasColumnType("nvarchar(100)");
            modelBuilder
                .Entity<Film>()
                .Property(f => f.UrlAffiche)
                .HasColumnType("varchar(1000)");

            // Properties RoleCelebriteFilm
            modelBuilder
                .Entity<RoleCelebriteFilm>()
                .Property(rcf => rcf.NomPersonnage)
                .HasColumnType("nvarchar(100)");
        }
    }
}
