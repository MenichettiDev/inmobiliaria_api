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
        public DbSet<LeadEstadoHistorial> LeadEstadoHistorial { get; set; }
        public DbSet<UsoMensual> UsoMensual { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<PagoSuscripcion> PagosSuscripcion { get; set; }
        public DbSet<Suscripcion> Suscripciones { get; set; }
        public DbSet<Provincia> Provincias { get; set; }
        public DbSet<Localidad> Localidades { get; set; }
        public DbSet<UsuarioWeb> UsuariosWeb { get; set; }
        public DbSet<PropiedadFavorita> PropiedadesFavoritas { get; set; }
        public DbSet<TransaccionHistorial> TransaccionesHistorial { get; set; }
        public DbSet<TipoTransaccion> TiposTransaccion { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder) // Mapeo de tablas y relaciones
        {
            base.OnModelCreating(modelBuilder);

            // Mapear tablas existentes
            modelBuilder.Entity<Usuario>().ToTable("usuarios");
            modelBuilder.Entity<ImagenPropiedad>().ToTable("imagenes_propiedades");
            modelBuilder.Entity<Rol>().ToTable("roles");
            modelBuilder.Entity<Cliente>().ToTable("clientes");

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

            // Lead -> Cliente
            modelBuilder.Entity<Lead>()
                .HasOne(l => l.Cliente)
                .WithMany(c => c.Leads)
                .HasForeignKey(l => l.IdCliente)
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

            // Relación Cliente -> Inmobiliaria (sin asumir propiedad de navegación en Inmobiliaria)
            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.Inmobiliaria)
                .WithMany() // usar vacío para no depender de una colección en Inmobiliaria
                .HasForeignKey(c => c.IdInmobiliaria)
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

            modelBuilder.Entity<Cliente>()
                .Property(c => c.CreadoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<Cliente>()
                .Property(c => c.Activo)
                .HasDefaultValue(true);

            // RefreshToken -> Usuario
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.Usuario)
                .WithMany()
                .HasForeignKey(rt => rt.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.Token)
                .IsUnique();

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => new { rt.IdUsuario, rt.RevocadoEn });

            modelBuilder.Entity<RefreshToken>()
                .Property(rt => rt.CreadoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Suscripcion -> Plan
            modelBuilder.Entity<Suscripcion>()
                .HasOne(s => s.Plan)
                .WithMany()
                .HasForeignKey(s => s.IdPlan)
                .OnDelete(DeleteBehavior.Restrict);

            // PagoSuscripcion -> Inmobiliaria
            modelBuilder.Entity<PagoSuscripcion>()
                .HasOne(p => p.Inmobiliaria)
                .WithMany()
                .HasForeignKey(p => p.IdInmobiliaria)
                .OnDelete(DeleteBehavior.Restrict);

            // PagoSuscripcion -> Plan
            modelBuilder.Entity<PagoSuscripcion>()
                .HasOne(p => p.Plan)
                .WithMany()
                .HasForeignKey(p => p.IdPlan)
                .OnDelete(DeleteBehavior.Restrict);

            // PagoSuscripcion -> Suscripcion (nullable)
            modelBuilder.Entity<PagoSuscripcion>()
                .HasOne(p => p.Suscripcion)
                .WithMany()
                .HasForeignKey(p => p.IdSuscripcion)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<PagoSuscripcion>()
                .HasIndex(p => p.MpPreferenceId);

            modelBuilder.Entity<PagoSuscripcion>()
                .HasIndex(p => new { p.IdInmobiliaria, p.Estado });

            modelBuilder.Entity<PagoSuscripcion>()
                .Property(p => p.CreadoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<PagoSuscripcion>()
                .Property(p => p.Estado)
                .HasConversion<string>();

            // Provincia -> Localidades
            modelBuilder.Entity<Localidad>()
                .HasOne(l => l.Provincia)
                .WithMany(p => p.Localidades)
                .HasForeignKey(l => l.IdProvincia)
                .OnDelete(DeleteBehavior.Restrict);

            // Localidad -> Propiedades
            modelBuilder.Entity<Propiedad>()
                .HasOne(p => p.Localidad)
                .WithMany(l => l.Propiedades)
                .HasForeignKey(p => p.IdLocalidad)
                .OnDelete(DeleteBehavior.SetNull);

            // Provincia -> Inmobiliarias
            modelBuilder.Entity<Inmobiliaria>()
                .HasOne(i => i.Provincia)
                .WithMany(p => p.Inmobiliarias)
                .HasForeignKey(i => i.IdProvincia)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
