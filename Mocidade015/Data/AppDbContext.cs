using Microsoft.EntityFrameworkCore;
using Mocidade015.Models;

namespace Mocidade015.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Onibus> Onibus { get; set; }
        public DbSet<Assento> Assentos { get; set; }
        public DbSet<Acompanhante> Acompanhantes { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<ListaEspera> ListaEspera { get; set; } // Ajustado para plural

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. CONFIGURAÇÃO GLOBAL UTC (Para o Postgres não reclamar das datas)
            var dateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
                v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc));

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(dateTimeConverter);
                    }
                }
            }

            // 2. MAPEAMENTO USUARIOS
            modelBuilder.Entity<Usuario>(entity => {
                entity.ToTable("Usuarios");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Nome).HasColumnName("nome");
                // Dentro do modelBuilder.Entity<Usuario>
                entity.Property(e => e.Rg).HasColumnName("rg");
                entity.Property(e => e.Email).HasColumnName("email");
                entity.Property(e => e.SenhaHash).HasColumnName("senhahash");
                entity.Property(e => e.Role).HasColumnName("role");
                entity.Property(e => e.DataCriacao).HasColumnName("datacriacao");
            });

            // 3. MAPEAMENTO ONIBUS
            modelBuilder.Entity<Onibus>(entity => {
                entity.ToTable("Onibus");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Numero).HasColumnName("numero");
                entity.Property(e => e.TerminalSaida).HasColumnName("terminalsaida");
                entity.Property(e => e.HorarioSaida).HasColumnName("horariosaida");
                entity.Property(e => e.DataViagem).HasColumnName("dataviagem");
                entity.Property(e => e.LotacaoMaxima).HasColumnName("lotacaomaxima");
            });

            // 4. MAPEAMENTO ASSENTOS
            modelBuilder.Entity<Assento>(entity => {
                entity.ToTable("Assentos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.OnibusId).HasColumnName("onibusid");
                entity.Property(e => e.Numero).HasColumnName("numero");
                entity.Property(e => e.Ocupado).HasColumnName("ocupado");

                entity.HasIndex(a => new { a.OnibusId, a.Numero }).IsUnique();
            });

            // 5. MAPEAMENTO ACOMPANHANTES
            modelBuilder.Entity<Acompanhante>(entity => {
                entity.ToTable("Acompanhantes");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UsuarioResponsavelId).HasColumnName("usuarioresponsavelid");
                entity.Property(e => e.Nome).HasColumnName("nome");
                entity.Property(e => e.RgCpf).HasColumnName("rgcpf");
            });

            // 6. MAPEAMENTO RESERVAS (AQUI ESTAVA O CONFLITO)
            modelBuilder.Entity<Reserva>(entity => {
                entity.ToTable("Reservas");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UsuarioId).HasColumnName("usuarioid");
                entity.Property(e => e.AcompanhanteId).HasColumnName("acompanhanteid");
                entity.Property(e => e.AssentoId).HasColumnName("assentoid"); // Mapeia para a coluna correta
                entity.Property(e => e.Valor).HasColumnName("valor");
                entity.Property(e => e.DataReserva).HasColumnName("datareserva");

                // CONFIGURAÇÃO EXPLÍCITA DOS RELACIONAMENTOS (Evita o "AssentoId1")
                entity.HasOne(r => r.Usuario)
                      .WithMany(u => u.Reservas)
                      .HasForeignKey(r => r.UsuarioId);

                entity.HasOne(r => r.Acompanhante)
                      .WithMany()
                      .HasForeignKey(r => r.AcompanhanteId);

                entity.HasOne(r => r.Assento)
                      .WithMany() // Se o Assento.cs não tiver ICollection<Reserva>, deixe vazio
                      .HasForeignKey(r => r.AssentoId)
                      .IsRequired(); // Garante que o EF use o AssentoId Guid que já existe

                entity.HasIndex(r => r.AssentoId).IsUnique();
            });

            // 7. MAPEAMENTO LISTA DE ESPERA
            modelBuilder.Entity<ListaEspera>(entity => {
                entity.ToTable("ListaEspera");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UsuarioId).HasColumnName("usuarioid");
                entity.Property(e => e.TerminalDesejado).HasColumnName("terminaldesejado");
                entity.Property(e => e.DataSolicitacao).HasColumnName("datasolicitacao");

                entity.HasOne(l => l.Usuario)
                      .WithMany()
                      .HasForeignKey(l => l.UsuarioId);
            });
        }
    }
}