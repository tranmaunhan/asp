using busline_project.Dtos;
using busline_project.Models;
using Microsoft.EntityFrameworkCore;

namespace busline_project.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // ────────────────────────────────────────────────
        // DbSets - Phần Người dùng & Phân quyền
        // ────────────────────────────────────────────────
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<UserRole> UserRoles { get; set; } = null!;
        public DbSet<VehicleSeatDto> VehicleSeatDtos { get; set; }

        // ────────────────────────────────────────────────
        // DbSets - Phần Xe & Ghế
        // ────────────────────────────────────────────────
        public DbSet<VehicleType> VehicleTypes { get; set; } = null!;
        public DbSet<Vehicle> Vehicles { get; set; } = null!;
        public DbSet<SeatTemplate> SeatTemplates { get; set; } = null!;

        // ────────────────────────────────────────────────
        // DbSets - Phần Tuyến đường & Địa điểm
        // ────────────────────────────────────────────────
        public DbSet<Location> Locations { get; set; } = null!;
        public DbSet<busline_project.Models.Route> Routes { get; set; } = null!;
        public DbSet<RouteStop> RouteStops { get; set; } = null!;

        // ────────────────────────────────────────────────
        // DbSets - Phần Chuyến xe, Ghế chuyến, Đặt vé & Vé
        // ────────────────────────────────────────────────
        public DbSet<Trip> Trips { get; set; } = null!;
        public DbSet<TripSeat> TripSeats { get; set; } = null!;
        public DbSet<Booking> Bookings { get; set; } = null!;
        public DbSet<Ticket> Tickets { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<VehicleSeatDto>().HasNoKey();
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username).IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email).IsUnique();

            modelBuilder.Entity<Role>()
                .HasIndex(r => r.RoleName).IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.Status)
                .HasConversion<string>();

            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("UTC_TIMESTAMP()");

            modelBuilder.Entity<User>()
                .Property(u => u.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("UTC_TIMESTAMP()");

            // ────────────────────────────────────────────────
            // 2. VehicleType - Vehicle - SeatTemplate
            // ────────────────────────────────────────────────
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.VehicleType)
                .WithMany(t => t.Vehicles)
                .HasForeignKey(v => v.VehicleTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SeatTemplate>()
                .HasOne(st => st.VehicleType)
                .WithMany(t => t.SeatTemplates)
                .HasForeignKey(st => st.VehicleTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => v.LicensePlate).IsUnique();

            modelBuilder.Entity<SeatTemplate>()
                .HasIndex(st => new { st.VehicleTypeId, st.SeatCode })
                .IsUnique()
                .HasDatabaseName("IX_SeatTemplate_VehicleTypeId_SeatCode");

            modelBuilder.Entity<Vehicle>()
                .Property(v => v.Status)
                .HasConversion<string>()
                .HasDefaultValue(VehicleStatus.Active);

            // ────────────────────────────────────────────────
            // 3. Location - Route - RouteStop
            // ────────────────────────────────────────────────
            modelBuilder.Entity<busline_project.Models.Route>()
                .HasOne(r => r.Origin)
                .WithMany(l => l.OriginRoutes)
                .HasForeignKey(r => r.OriginId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<busline_project.Models.Route>()
                .HasOne(r => r.Destination)
                .WithMany(l => l.DestinationRoutes)
                .HasForeignKey(r => r.DestinationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RouteStop>()
                .HasOne(rs => rs.Route)
                .WithMany(r => r.RouteStops)
                .HasForeignKey(rs => rs.RouteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RouteStop>()
                .HasOne(rs => rs.Location)
                .WithMany(l => l.RouteStops)
                .HasForeignKey(rs => rs.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RouteStop>()
                .HasIndex(rs => new { rs.RouteId, rs.StopOrder })
                .IsUnique()
                .HasDatabaseName("IX_RouteStop_RouteId_StopOrder_Unique");

            modelBuilder.Entity<Location>()
                .Property(l => l.Type)
                .HasConversion<string>();

            modelBuilder.Entity<RouteStop>()
                .Property(rs => rs.StopOrder)
                .HasDefaultValue(1);

            // ────────────────────────────────────────────────
            // 4. Trip - TripSeat - Booking - Ticket
            // ────────────────────────────────────────────────
            // Trip
            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Route)
                .WithMany(r => r.Trips)  // cần thêm ICollection<Trip> Trips vào Route model
                .HasForeignKey(t => t.RouteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Vehicle)
                .WithMany(v => v.Trips)  // cần thêm ICollection<Trip> Trips vào Vehicle model
                .HasForeignKey(t => t.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Trip>()
                .HasIndex(t => new { t.RouteId, t.DepartureTime, t.Status });

            // TripSeat
            modelBuilder.Entity<TripSeat>()
                .HasOne(ts => ts.Trip)
                .WithMany(t => t.TripSeats)
                .HasForeignKey(ts => ts.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TripSeat>()
                .HasOne(ts => ts.SeatTemplate)
                .WithMany()
                .HasForeignKey(ts => ts.SeatTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TripSeat>()
                .HasIndex(ts => new { ts.TripId, ts.SeatTemplateId })
                .IsUnique();

            modelBuilder.Entity<TripSeat>()
                .HasIndex(ts => new { ts.TripId, ts.Status });

            // Booking
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)  // cần thêm ICollection<Booking> Bookings vào User model
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .Property(b => b.BookingTime)
                .HasDefaultValueSql("UTC_TIMESTAMP()");

            // Ticket
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Booking)
                .WithMany(b => b.Tickets)
                .HasForeignKey(t => t.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Trip)
                .WithMany(t => t.Tickets)
                .HasForeignKey(t => t.TripId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.TripSeat)
                .WithMany(ts => ts.Tickets)
                .HasForeignKey(t => t.TripSeatId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.PickupStop)
                .WithMany()
                .HasForeignKey(t => t.PickupStopId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.DropoffStop)
                .WithMany()
                .HasForeignKey(t => t.DropoffStopId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasIndex(t => new { t.TripId, t.TripSeatId });
        }
    }
}