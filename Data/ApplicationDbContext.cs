using inmobiliariaApi.Models;
using Microsoft.EntityFrameworkCore;

namespace inmobiliariaApi.Data
{
    public class ApplicationDbContext : DbContext // Heredar de DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) // Constructor
            : base(options) { }

        // Mapeo de entidades con los nombres correctos según la base de datos
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<ImagenPropiedad> ImagenPropiedad { get; set; }
        public DbSet<Rol> Rol { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        // Nuevas entidades para el módulo de Leads
        public DbSet<Inmobiliaria> Inmobiliaria { get; set; }
        public DbSet<EstadoInmobiliaria> EstadoInmobiliaria { get; set; }
        public DbSet<EstadoUsuario> EstadoUsuario { get; set; }
        public DbSet<Plan> Plan { get; set; }
        public DbSet<Lead> Lead { get; set; }
        public DbSet<Propiedad> Propiedad { get; set; }
        public DbSet<FuenteContacto> FuenteContacto { get; set; }
        public DbSet<EstadoLead> EstadoLead { get; set; }
        public DbSet<EstadoPropiedadOperativo> EstadoPropiedadOperativo { get; set; }
        public DbSet<EstadoPropiedadActividad> EstadoPropiedadActividad { get; set; }
        public DbSet<LeadEstadoHistorial> LeadEstadoHistorial { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) // Mapeo de tablas y relaciones
        {
            base.OnModelCreating(modelBuilder);

            // Mapear tablas existentes
            modelBuilder.Entity<Usuario>().ToTable("usuarios");
            modelBuilder.Entity<ImagenPropiedad>().ToTable("imagenes_propiedades");
            modelBuilder.Entity<Rol>().ToTable("roles");

            // Mapeo de nuevas tablas para módulo de Leads - Ya configurado con [Table] attributes
            // Las siguientes líneas son opcionales ya que usamos [Table] en los modelos:
            // modelBuilder.Entity<Inmobiliaria>().ToTable("inmobiliarias");
            // modelBuilder.Entity<EstadoInmobiliaria>().ToTable("estados_inmobiliaria");
            // etc...

            // Configurar relaciones y restricciones

            // Usuario -> Inmobiliaria
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Inmobiliaria)
                .WithMany(i => i.Usuarios)
                .HasForeignKey(u => u.IdInmobiliaria)
                .OnDelete(DeleteBehavior.Restrict);

            // Usuario -> Rol
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(u => u.IdRol)
                .OnDelete(DeleteBehavior.Restrict);

            // Usuario -> EstadoUsuario
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Estado)
                .WithMany(e => e.Usuarios)
                .HasForeignKey(u => u.IdEstado)
                .OnDelete(DeleteBehavior.Restrict);

            // Inmobiliaria -> EstadoInmobiliaria
            modelBuilder.Entity<Inmobiliaria>()
                .HasOne(i => i.Estado)
                .WithMany(e => e.Inmobiliarias)
                .HasForeignKey(i => i.IdEstado)
                .OnDelete(DeleteBehavior.Restrict);

            // Inmobiliaria -> Plan
            modelBuilder.Entity<Inmobiliaria>()
                .HasOne(i => i.Plan)
                .WithMany(p => p.Inmobiliarias)
                .HasForeignKey(i => i.IdPlan)
                .OnDelete(DeleteBehavior.Restrict);

            // Lead -> Inmobiliaria
            modelBuilder.Entity<Lead>()
                .HasOne(l => l.Inmobiliaria)
                .WithMany(i => i.Leads)
                .HasForeignKey(l => l.IdInmobiliaria)
                .OnDelete(DeleteBehavior.Restrict);

            // Lead -> Propiedad
            modelBuilder.Entity<Lead>()
                .HasOne(l => l.Propiedad)
                .WithMany(p => p.Leads)
                .HasForeignKey(l => l.IdPropiedad)
                .OnDelete(DeleteBehavior.SetNull);

            // Lead -> FuenteContacto
            modelBuilder.Entity<Lead>()
                .HasOne(l => l.Fuente)
                .WithMany(f => f.Leads)
                .HasForeignKey(l => l.IdFuente)
                .OnDelete(DeleteBehavior.Restrict);

            // Lead -> EstadoLead
            modelBuilder.Entity<Lead>()
                .HasOne(l => l.Estado)
                .WithMany(e => e.Leads)
                .HasForeignKey(l => l.IdEstado)
                .OnDelete(DeleteBehavior.Restrict);


            // Lead -> Usuario (Asignado)
            modelBuilder.Entity<Lead>()
                .HasOne(l => l.UsuarioAsignado)
                .WithMany(u => u.LeadsAsignados)
                .HasForeignKey(l => l.IdUsuarioAsignado)
                .OnDelete(DeleteBehavior.SetNull);

            // Propiedad -> Inmobiliaria
            modelBuilder.Entity<Propiedad>()
                .HasOne(p => p.Inmobiliaria)
                .WithMany(i => i.Propiedades)
                .HasForeignKey(p => p.IdInmobiliaria)
                .OnDelete(DeleteBehavior.Restrict);

            // Propiedad -> Usuario (Agente Responsable)
            modelBuilder.Entity<Propiedad>()
                .HasOne(p => p.AgenteResponsable)
                .WithMany(u => u.PropiedadesResponsable)
                .HasForeignKey(p => p.IdAgenteResponsable)
                .OnDelete(DeleteBehavior.SetNull);

            // Propiedad -> Estados
            modelBuilder.Entity<Propiedad>()
                .HasOne(p => p.EstadoOperativo)
                .WithMany(e => e.Propiedades)
                .HasForeignKey(p => p.IdEstadoOperativo)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Propiedad>()
                .HasOne(p => p.EstadoAdmin)
                .WithMany(e => e.Propiedades)
                .HasForeignKey(p => p.IdEstadoAdmin)
                .OnDelete(DeleteBehavior.Restrict);

            // ImagenPropiedad -> Propiedad
            modelBuilder.Entity<ImagenPropiedad>()
                .HasOne(i => i.Propiedad)
                .WithMany(p => p.Imagenes)
                .HasForeignKey(i => i.IdPropiedad)
                .OnDelete(DeleteBehavior.Cascade);

            // LeadEstadoHistorial -> Lead
            modelBuilder.Entity<LeadEstadoHistorial>()
                .HasOne(h => h.Lead)
                .WithMany(l => l.HistorialEstados)
                .HasForeignKey(h => h.IdLead)
                .OnDelete(DeleteBehavior.Cascade);

            // LeadEstadoHistorial -> EstadoLead (Anterior)
            modelBuilder.Entity<LeadEstadoHistorial>()
                .HasOne(h => h.EstadoAnterior)
                .WithMany(e => e.HistorialEstadosAnteriores)
                .HasForeignKey(h => h.IdEstadoAnterior)
                .OnDelete(DeleteBehavior.Restrict);

            // LeadEstadoHistorial -> EstadoLead (Nuevo)
            modelBuilder.Entity<LeadEstadoHistorial>()
                .HasOne(h => h.EstadoNuevo)
                .WithMany(e => e.HistorialEstadosNuevos)
                .HasForeignKey(h => h.IdEstadoNuevo)
                .OnDelete(DeleteBehavior.Restrict);

            // LeadEstadoHistorial -> Usuario
            modelBuilder.Entity<LeadEstadoHistorial>()
                .HasOne(h => h.Usuario)
                .WithMany(u => u.CambiosEstadoLead)
                .HasForeignKey(h => h.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            // Configurar índices únicos
            modelBuilder.Entity<Inmobiliaria>()
                .HasIndex(i => i.Subdominio)
                .IsUnique();

            modelBuilder.Entity<FuenteContacto>()
                .HasIndex(f => f.Nombre)
                .IsUnique();

            modelBuilder.Entity<EstadoLead>()
                .HasIndex(e => e.Nombre)
                .IsUnique();

            // Configurar valores por defecto
            modelBuilder.Entity<Inmobiliaria>()
                .Property(i => i.CreadoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<Lead>()
                .Property(l => l.CreadoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<Propiedad>()
                .Property(p => p.CreadoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<LeadEstadoHistorial>()
                .Property(h => h.CreadoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}
