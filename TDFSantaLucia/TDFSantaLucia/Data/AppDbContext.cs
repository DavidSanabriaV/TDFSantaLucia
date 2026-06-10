using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TDFSantaLucia.Models;

namespace TDFSantaLucia.Data
{
    public class AppDbContext : IdentityDbContext<Usuario>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Inventario> Inventarios { get; set; }
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<HorarioEmpleado> HorariosEmpleados { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Cita> Citas { get; set; }
        public DbSet<Expediente> Expedientes { get; set; }
        public DbSet<RecetaMedica> RecetasMedicas { get; set; }
        public DbSet<Tratamiento> Tratamientos { get; set; }
        public DbSet<RecordatorioTratamiento> RecordatoriosTratamiento { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<DetallePedido> DetallesPedido { get; set; }
        public DbSet<Factura> Facturas { get; set; }
        public DbSet<DetalleFactura> DetallesFactura { get; set; }
        public DbSet<Cupon> Cupones { get; set; }
        public DbSet<ClienteCupon> ClientesCupones { get; set; }
        public DbSet<CarritoItemDb> CarritoItems { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Empleado -> Usuario (1 a 1)
            modelBuilder.Entity<Empleado>()
                .HasOne(e => e.Usuario)
                .WithOne(u => u.Empleado)
                .HasForeignKey<Empleado>(e => e.Usuario_ID)
                .OnDelete(DeleteBehavior.Restrict);

            // Cliente -> Usuario (1 a 1)
            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.Usuario)
                .WithOne(u => u.Cliente)
                .HasForeignKey<Cliente>(c => c.Usuario_ID)
                .OnDelete(DeleteBehavior.Restrict);

            // Producto -> Categoria
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.Categoria_Id)
                .OnDelete(DeleteBehavior.Restrict);

            // Inventario -> Producto
            modelBuilder.Entity<Inventario>()
                .HasOne(i => i.Producto)
                .WithMany(p => p.Inventarios)
                .HasForeignKey(i => i.Producto_Id)
                .OnDelete(DeleteBehavior.Restrict);

            // HorarioEmpleado -> Empleado
            modelBuilder.Entity<HorarioEmpleado>()
                .HasOne(h => h.Empleado)
                .WithMany(e => e.Horarios)
                .HasForeignKey(h => h.Empleado_Id)
                .OnDelete(DeleteBehavior.Restrict);

            // Cita -> Cliente
            modelBuilder.Entity<Cita>()
                .HasOne(c => c.Cliente)
                .WithMany(cl => cl.Citas)
                .HasForeignKey(c => c.Cliente_Id)
                .OnDelete(DeleteBehavior.Restrict);

            // Cita -> Empleado (opcional)
            modelBuilder.Entity<Cita>()
                .HasOne(c => c.Empleado)
                .WithMany(e => e.Citas)
                .HasForeignKey(c => c.Empleado_Id)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Expediente -> Cliente
            modelBuilder.Entity<Expediente>()
                .HasOne(e => e.Cliente)
                .WithMany(c => c.Expedientes)
                .HasForeignKey(e => e.Cliente_Id)
                .OnDelete(DeleteBehavior.Restrict);

            // Expediente -> Empleado
            modelBuilder.Entity<Expediente>()
                .HasOne(e => e.Empleado)
                .WithMany(emp => emp.Expedientes)
                .HasForeignKey(e => e.Empleado_Id)
                .OnDelete(DeleteBehavior.Restrict);

            // RecetaMedica -> Producto
            modelBuilder.Entity<RecetaMedica>()
                .HasOne(r => r.Producto)
                .WithMany()
                .HasForeignKey(r => r.Producto_Id)
                .OnDelete(DeleteBehavior.Restrict);

            // RecetaMedica -> Expediente
            modelBuilder.Entity<RecetaMedica>()
                .HasOne(r => r.Expediente)
                .WithMany(e => e.RecetasMedicas)
                .HasForeignKey(r => r.Expediente_Id)
                .OnDelete(DeleteBehavior.Restrict);

            // Tratamiento -> Cliente
            modelBuilder.Entity<Tratamiento>()
                .HasOne(t => t.Cliente)
                .WithMany(c => c.Tratamientos)
                .HasForeignKey(t => t.Cliente_Id)
                .OnDelete(DeleteBehavior.Restrict);

            // RecordatorioTratamiento -> Tratamiento
            modelBuilder.Entity<RecordatorioTratamiento>()
                .HasOne(r => r.Tratamiento)
                .WithMany(t => t.Recordatorios)
                .HasForeignKey(r => r.Tratamiento_Id)
                .OnDelete(DeleteBehavior.Restrict);

            // Pedido -> Cliente
            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Cliente)
                .WithMany(c => c.Pedidos)
                .HasForeignKey(p => p.Cliente_Id)
                .OnDelete(DeleteBehavior.Restrict);

            // Pedido -> Cupon (opcional)
            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Cupon)
                .WithMany(c => c.Pedidos)
                .HasForeignKey(p => p.Cupon_Id)
                .OnDelete(DeleteBehavior.Restrict);

            // DetallePedido -> Pedido
            modelBuilder.Entity<DetallePedido>()
                .HasOne(d => d.Pedido)
                .WithMany(p => p.DetallesPedido)
                .HasForeignKey(d => d.Pedido_Id)
                .OnDelete(DeleteBehavior.Restrict);

            // DetallePedido -> Producto
            modelBuilder.Entity<DetallePedido>()
                .HasOne(d => d.Producto)
                .WithMany(p => p.DetallesPedido)
                .HasForeignKey(d => d.Producto_Id)
                .OnDelete(DeleteBehavior.Restrict);

            // Factura -> Cliente
            modelBuilder.Entity<Factura>()
                .HasOne(f => f.Cliente)
                .WithMany(c => c.Facturas)
                .HasForeignKey(f => f.Cliente_Id)
                .OnDelete(DeleteBehavior.Restrict);

            // Factura -> Pedido
            modelBuilder.Entity<Factura>()
                .HasOne(f => f.Pedido)
                .WithMany(p => p.Facturas)
                .HasForeignKey(f => f.Pedido_Id)
                .OnDelete(DeleteBehavior.Restrict);

            // DetalleFactura -> Factura
            modelBuilder.Entity<DetalleFactura>()
                .HasOne(d => d.Factura)
                .WithMany(f => f.DetallesFactura)
                .HasForeignKey(d => d.Factura_Id)
                .OnDelete(DeleteBehavior.Restrict);

            // DetalleFactura -> Producto
            modelBuilder.Entity<DetalleFactura>()
                .HasOne(d => d.Producto)
                .WithMany(p => p.DetallesFactura)
                .HasForeignKey(d => d.Producto_Id)
                .OnDelete(DeleteBehavior.Restrict);

            // ClienteCupon -> Cliente
            modelBuilder.Entity<ClienteCupon>()
                .HasOne(cc => cc.Cliente)
                .WithMany(c => c.ClienteCupones)
                .HasForeignKey(cc => cc.Cliente_Id)
                .OnDelete(DeleteBehavior.Restrict);

            // ClienteCupon -> Cupon
            modelBuilder.Entity<ClienteCupon>()
                .HasOne(cc => cc.Cupon)
                .WithMany(c => c.ClienteCupones)
                .HasForeignKey(cc => cc.Cupon_Id)
                .OnDelete(DeleteBehavior.Restrict);

            // Cupon -> Usuario
            modelBuilder.Entity<Cupon>()
                .HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.Usuario_Id)
                .OnDelete(DeleteBehavior.Restrict);

            // CarritoItemDb -> Cliente
            modelBuilder.Entity<CarritoItemDb>()
                .HasOne(c => c.Cliente)
                .WithMany(c => c.CarritoItems)   
                .HasForeignKey(c => c.Cliente_Id)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}