using Microsoft.EntityFrameworkCore;
using Models;

namespace Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Pessoa> Pessoas { get; set; } = null!;
        public DbSet<Transacao> Transacoes { get; set;} = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Pessoa>(entity =>
            {
                entity.HasKey(pessoa => pessoa.Id);

                entity.Property(pessoa => pessoa.Nome)
                    .IsRequired()
                    .HasMaxLength(120);

                entity.Property(pessoa => pessoa.Idade)
                    .IsRequired();

                entity.HasMany(pessoa => pessoa.Transacoes)
                    .WithOne(transacao => transacao.Pessoa)
                    .HasForeignKey(transacao => transacao.PessoaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Transacao>(entity =>
            {
                entity.HasKey(transacao => transacao.Id);
                
                entity.Property(transacao => transacao.Descricao)
                    .IsRequired()
                    .HasMaxLength(200);
                
                entity.Property(transacao => transacao.Valor)
                    .IsRequired()
                    .HasPrecision(10, 2);
                
                entity.Property(transacao => transacao.Tipo)
                    .IsRequired()
                    .HasMaxLength(10);
            });
        }
    }
}

