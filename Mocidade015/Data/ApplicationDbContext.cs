using Microsoft.EntityFrameworkCore;
using Mocidade015.Models; // Puxa as classes Lote e Passageiro

namespace Mocidade015.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Estas serão as tabelas no Supabase
        public DbSet<Lote> Lotes { get; set; }
        public DbSet<Passageiro> Passageiros { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // REGRA DE OURO: Impede que o mesmo CPF seja cadastrado duas vezes no banco de dados.
            modelBuilder.Entity<Passageiro>()
                .HasIndex(p => p.Cpf)
                .IsUnique();
        }
    }
}