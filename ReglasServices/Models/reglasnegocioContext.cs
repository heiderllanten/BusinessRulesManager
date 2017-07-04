using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ReglasServices.Models
{
    public partial class reglasnegocioContext : DbContext
    {
        public virtual DbSet<Reglas> Reglas { get; set; }

        public reglasnegocioContext(DbContextOptions<reglasnegocioContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Reglas>(entity =>
            {
                entity.ToTable("reglas");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedNever();

                entity.Property(e => e.Operador)
                    .IsRequired()
                    .HasColumnName("operador")
                    .HasMaxLength(20);

                entity.Property(e => e.Propiedad)
                    .IsRequired()
                    .HasColumnName("propiedad")
                    .HasMaxLength(20);

                entity.Property(e => e.ValorComparacion)
                    .IsRequired()
                    .HasColumnName("valor_comparacion")
                    .HasMaxLength(20);
            });
        }
    }
}