using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pokedex.Models;

namespace Pokedex.Data
{
    // IdentityDbContext = DbContext + mga table para sa users (AspNetUsers, AspNetRoles, etc.)
    // Dating: DbContext. Ngayon: IdentityDbContext — para ma-store ang accounts sa parehong database.
    public class PokedexDbContext : IdentityDbContext
    {
        public PokedexDbContext(DbContextOptions<PokedexDbContext> options) : base(options) { }

        public DbSet<Pokemon> Pokemons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // IMPORTANTE: dapat UNA ito. Kung wala, hindi ma-map ang Identity tables.
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Pokemon>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Type).IsRequired().HasMaxLength(50);
                entity.Property(p => p.Type2).HasMaxLength(50);
                entity.Property(p => p.Description).HasMaxLength(500);
            });
        }
    }
}