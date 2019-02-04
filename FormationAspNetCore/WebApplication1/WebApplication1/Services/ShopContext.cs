using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class ShopContext : DbContext
    {
        public DbSet<Client> Clients { get; set; }
        public DbSet<Produit> Produits { get; set; }
        public DbSet<Commande> Commandes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=C:\\dev\\shop.db");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Client>()
                .Property(c => c.Nom)
                .IsRequired()
                .HasColumnType("nvarchar(100)");
            modelBuilder
                .Entity<Client>()
                .Property(c => c.Prenom)
                .IsRequired()
                .HasColumnType("nvarchar(150)");
            modelBuilder
                .Entity<Client>()
                .Property(c => c.DateInscription)
                .IsRequired()
                .HasColumnType("datetime");
        }
    }
}
