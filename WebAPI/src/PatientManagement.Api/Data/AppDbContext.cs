using Microsoft.EntityFrameworkCore;
using PatientManagement.Api.Data.Entities;

namespace PatientManagement.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Patient> Patients => Set<Patient>();

    public DbSet<Appointment> Appointments => Set<Appointment>();

    public DbSet<Visit> Visits => Set<Visit>();

    public DbSet<Vitals> Vitals => Set<Vitals>();

    public DbSet<Medication> Medications => Set<Medication>();

    public DbSet<Doctor> Doctors => Set<Doctor>();

    public DbSet<ClinicSettings> ClinicSettings => Set<ClinicSettings>();

    public DbSet<DoctorAccount> DoctorAccounts => Set<DoctorAccount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.Property(p => p.Id).ValueGeneratedNever();
            entity.Property(p => p.FullName).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Gender).IsRequired().HasMaxLength(20);
            entity.Property(p => p.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(p => p.Address).HasMaxLength(300);

            entity.HasIndex(p => p.FullName);
            entity.HasIndex(p => p.PhoneNumber);
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.Property(a => a.Id).ValueGeneratedNever();
            entity.Property(a => a.Notes).HasMaxLength(1000);

            entity.HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(a => a.ScheduledAt);
        });

        modelBuilder.Entity<Visit>(entity =>
        {
            entity.Property(v => v.Id).ValueGeneratedNever();
            entity.Property(v => v.Complaints).IsRequired().HasMaxLength(2000);
            entity.Property(v => v.Diagnosis).IsRequired().HasMaxLength(2000);

            entity.HasOne(v => v.Patient)
                .WithMany()
                .HasForeignKey(v => v.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(v => v.Appointment)
                .WithOne()
                .HasForeignKey<Visit>(v => v.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(v => v.AppointmentId).IsUnique();
            entity.HasIndex(v => v.PatientId);
            entity.HasIndex(v => v.VisitDateUtc);
        });

        modelBuilder.Entity<Vitals>(entity =>
        {
            entity.HasKey(v => v.VisitId);

            entity.HasOne(v => v.Visit)
                .WithOne(v => v.Vitals)
                .HasForeignKey<Vitals>(v => v.VisitId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Medication>(entity =>
        {
            entity.Property(m => m.Id).ValueGeneratedNever();
            entity.Property(m => m.Name).IsRequired().HasMaxLength(200);
            entity.Property(m => m.Dosage).IsRequired().HasMaxLength(100);
            entity.Property(m => m.Frequency).IsRequired().HasMaxLength(100);
            entity.Property(m => m.Duration).IsRequired().HasMaxLength(100);
            entity.Property(m => m.Instructions).HasMaxLength(500);

            entity.HasOne(m => m.Visit)
                .WithMany(v => v.Medications)
                .HasForeignKey(m => m.VisitId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.Property(d => d.Id).ValueGeneratedNever();
            entity.Property(d => d.FullName).IsRequired().HasMaxLength(200);
            entity.Property(d => d.Specialization).IsRequired().HasMaxLength(150);
            entity.Property(d => d.RegistrationNumber).HasMaxLength(50);
            entity.Property(d => d.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(d => d.Email).HasMaxLength(200);
            entity.Property(d => d.ClinicAddress).HasMaxLength(300);

            entity.HasIndex(d => d.FullName);
        });

        modelBuilder.Entity<ClinicSettings>(entity =>
        {
            entity.Property(c => c.Id).ValueGeneratedNever();
            entity.Property(c => c.ClinicName).IsRequired().HasMaxLength(200);
            entity.Property(c => c.DoctorName).IsRequired().HasMaxLength(200);
            entity.Property(c => c.RegistrationNumber).HasMaxLength(50);
            entity.Property(c => c.Qualification).HasMaxLength(150);
            entity.Property(c => c.Phone).HasMaxLength(20);
            entity.Property(c => c.Address).HasMaxLength(300);
            entity.Property(c => c.FooterNote).HasMaxLength(300);
        });

        modelBuilder.Entity<DoctorAccount>(entity =>
        {
            entity.Property(a => a.Id).ValueGeneratedNever();
            entity.Property(a => a.Username).IsRequired().HasMaxLength(100);
            entity.Property(a => a.PasswordHash).IsRequired().HasMaxLength(200);
            entity.Property(a => a.DisplayName).IsRequired().HasMaxLength(200);

            entity.HasIndex(a => a.Username).IsUnique();
        });
    }
}
