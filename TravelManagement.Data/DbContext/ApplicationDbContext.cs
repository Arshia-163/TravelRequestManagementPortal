using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using TravelManagement.Data.Entities;

namespace TravelManagement.Data.DbContext;

public class ApplicationDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }


    public DbSet<Department> Departments => Set<Department>();

    public DbSet<TravelRequest> TravelRequests => Set<TravelRequest>();

    public DbSet<TripExtension> TripExtensions => Set<TripExtension>();

    public DbSet<Booking> Bookings => Set<Booking>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();



    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);



        builder.Entity<ApplicationUser>(b =>
        {
          
            b.Property(u => u.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            b.Property(u => u.LastName)
                .HasMaxLength(100)
                .IsRequired();

            b.Property(u => u.Gender)
                .HasMaxLength(20)
                .IsRequired();


            
            b.Property(u => u.EmployeeCode)
                .HasMaxLength(20);

            b.HasIndex(u => u.EmployeeCode)
                .IsUnique()
                .HasFilter("[EmployeeCode] IS NOT NULL");


         
            b.HasOne(u => u.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        });


        
        builder.Entity<Department>(b =>
        {
           
            b.HasIndex(d => d.Name)
                .IsUnique();

            b.Property(d => d.Name)
                .HasMaxLength(200)
                .IsRequired();


          
            b.Property(d => d.TravelApprovalLimit)
                .HasColumnType("decimal(18,2)");



            b.HasOne(d => d.Manager)
                .WithMany(u => u.ManagedDepartments)
                .HasForeignKey(d => d.ManagerId);
          
            b.HasOne(d => d.DepartmentHead)
                .WithMany(u => u.HeadedDepartments)
                .HasForeignKey(d => d.DepartmentHeadId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        });



        builder.Entity<TravelRequest>(b =>
        {
          
            b.Property(r => r.Source)
                .HasMaxLength(300)
                .IsRequired();

            b.Property(r => r.Destination)
                .HasMaxLength(300)
                .IsRequired();


            b.Property(r => r.BusinessJustification)
                .HasMaxLength(2000)
                .IsRequired();

            b.Property(r => r.Currency)
                .HasMaxLength(10)
                .IsRequired();


            b.Property(r => r.HotelName)
                .HasMaxLength(300);

            b.Property(r => r.HotelCity)
                .HasMaxLength(300);


            b.Property(r => r.EstimatedCost)
                .HasColumnType("decimal(18,2)");


           
            b.Property(r => r.Status)
                .HasConversion<string>()
                .HasMaxLength(30);

            b.Property(r => r.TravelType)
                .HasConversion<string>()
                .HasMaxLength(20);

            b.HasOne(r => r.User)
                .WithMany(u => u.TravelRequests)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            b.HasOne(r => r.Department)
                .WithMany(d => d.TravelRequests)
                .HasForeignKey(r => r.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });



        builder.Entity<TripExtension>(b =>
        {

            b.Property(e => e.Reason)
                .HasMaxLength(1000)
                .IsRequired();


            b.Property(e => e.AdditionalEstimatedCost)
                .HasColumnType("decimal(18,2)");

            b.Property(e => e.TotalEstimatedCost)
                .HasColumnType("decimal(18,2)");


          
            b.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(30);


            b.HasOne(e => e.TravelRequest)
                .WithMany(r => r.TripExtensions)
                .HasForeignKey(e => e.TravelRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        });



        builder.Entity<Booking>(b =>
        {
           
            b.Property(bk => bk.BookingReference)
                .HasMaxLength(100);

            b.Property(bk => bk.Notes)
                .HasMaxLength(2000);

            b.Property(bk => bk.Provider)
                .HasMaxLength(200);

           
            b.Property(bk => bk.ActualCost)
                .HasColumnType("decimal(18,2)");


           
            b.Property(bk => bk.Type)
                .HasConversion<string>()
                .HasMaxLength(20);

            b.Property(bk => bk.Status)
                .HasConversion<string>()
                .HasMaxLength(20);


          
            b.HasOne(bk => bk.TravelRequest)
                .WithMany(r => r.Bookings)
                .HasForeignKey(bk => bk.TravelRequestId)
                .OnDelete(DeleteBehavior.Cascade);


            b.HasOne(bk => bk.BookedByUser)
                .WithMany()
                .HasForeignKey(bk => bk.BookedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });


        builder.Entity<AuditLog>(b =>
        {
           
            b.Property(a => a.Action)
                .HasConversion<string>()
                .HasMaxLength(50);

         
            b.Property(a => a.Comment)
                .HasMaxLength(2000);
            b.HasOne(a => a.TravelRequest)
                .WithMany(r => r.AuditLogs)
                .HasForeignKey(a => a.TravelRequestId)
                .OnDelete(DeleteBehavior.Cascade);


            
            b.HasOne(a => a.PerformedByUser)
                .WithMany()
                .HasForeignKey(a => a.PerformedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(a => new
            {
                a.PerformedByUserId,
                a.Action
            });
        });
    }
}
