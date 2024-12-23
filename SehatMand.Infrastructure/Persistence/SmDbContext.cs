using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SehatMand.Domain.Entities;

namespace SehatMand.Infrastructure.Persistence;

public partial class SmDbContext(
    DbContextOptions<SmDbContext> options
    ) : DbContext(options)
{
    
    /*
    private readonly string _connString = "";
    SmDbContext(IConfiguration config): this()
    {
        _connString = config.GetConnectionString("SmDb")!;
    }*/
    
    
    /*
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySQL(_connString);
        base.OnConfiguring(optionsBuilder);
    }
    */

    public virtual DbSet<Appointment> Appointment { get; set; }

    public virtual DbSet<Billing> Billing { get; set; }

    public virtual DbSet<Clinic> Clinic { get; set; }

    public virtual DbSet<Coupon> Coupon { get; set; }

    public virtual DbSet<Doctor> Doctor { get; set; }
    public virtual DbSet<Qualification> Qualification { get; set; }

    public virtual DbSet<DoctorDailyAvailability> DoctorDailyAvailability { get; set; }

    public virtual DbSet<MedicalForumComment> MedicalForumComment { get; set; }

    public virtual DbSet<MedicalForumCommentVotes> MedicalForumCommentVotes { get; set; }

    public virtual DbSet<MedicalForumPost> MedicalForumPost { get; set; }

    public virtual DbSet<MedicalForumPostVotes> MedicalForumPostVotes { get; set; }

    //public virtual DbSet<MedicalHistory> MedicalHistory { get; set; }

    public virtual DbSet<MedicalHistoryDocument> MedicalHistoryDocument { get; set; }

    public virtual DbSet<Patient> Patient { get; set; }
    public virtual DbSet<Speciality> Speciality { get; set; }

    public virtual DbSet<RecordedSessions> RecordedSessions { get; set; }

    public virtual DbSet<Review> Review { get; set; }

    public virtual DbSet<Transcriptions> Transcriptions { get; set; }

    public virtual DbSet<User> User { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.ToTable("Appointment", "sm-db-ver3");

            entity.HasIndex(e => e.doctor_id, "doctor_id");

            entity.HasIndex(e => e.patient_id, "patient_id");

            entity.Property(e => e.id).HasMaxLength(36);
            entity.Property(e => e.appointment_date).HasColumnType("datetime");
            entity.Property(e => e.created_at).HasColumnType("datetime");
            entity.Property(e => e.doctor_id).HasMaxLength(36);
            entity.Property(e => e.paymentIntentId).HasMaxLength(255).HasColumnName("payment_intent_id");
            entity.Property(e => e.latitude).HasPrecision(9, 6);
            entity.Property(e => e.longitude).HasPrecision(9, 6);
            entity.Property(e => e.modified_at).HasColumnType("datetime");
            entity.Property(e => e.patient_id).HasMaxLength(36);
            entity.Property(e => e.status).HasColumnType("enum('pending','scheduled','rejected','completed','cancelled')");
            entity.Property(e => e.DidDoctorJoin).HasColumnName("did_doctor_join").HasColumnType("tinyint(1)");
            entity.Property(e => e.DidPatientJoin).HasColumnName("did_patient_join").HasColumnType("tinyint(1)");
            
            entity.HasOne(d => d.doctor).WithMany(p => p.Appointment)
                .HasForeignKey(d => d.doctor_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("appointment_ibfk_2");

            entity.HasOne(d => d.patient).WithMany(p => p.Appointment)
                .HasForeignKey(d => d.patient_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("appointment_ibfk_1");
        });

        modelBuilder.Entity<Billing>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.ToTable("Billing", "sm-db-ver3");

            entity.HasIndex(e => e.appointment_id, "appointment_id");

            entity.Property(e => e.id).HasMaxLength(36);
            entity.Property(e => e.amount).HasPrecision(10);
            entity.Property(e => e.appointment_id).HasMaxLength(36);
            entity.Property(e => e.created_at).HasColumnType("datetime");
            entity.Property(e => e.modified_at).HasColumnType("datetime");
            entity.Property(e => e.status).HasColumnType("enum('paid','unpaid','pending')");
            entity.Property(e => e.transaction_date).HasColumnType("datetime");

            entity.HasOne(d => d.appointment).WithMany(p => p.Billing)
                .HasForeignKey(d => d.appointment_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("billing_ibfk_1");
        });

        modelBuilder.Entity<Clinic>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.ToTable("Clinic", "sm-db-ver3");

            entity.HasIndex(e => e.created_by, "created_by");

            entity.HasIndex(e => e.modified_by, "modified_by");

            entity.Property(e => e.id).HasMaxLength(36);
            entity.Property(e => e.city).HasMaxLength(255);
            entity.Property(e => e.created_at).HasColumnType("datetime");
            entity.Property(e => e.created_by).HasMaxLength(36);
            entity.Property(e => e.latitude).HasPrecision(9, 6);
            entity.Property(e => e.longitude).HasPrecision(9, 6);
            entity.Property(e => e.modified_at).HasColumnType("datetime");
            entity.Property(e => e.modified_by).HasMaxLength(36);
            entity.Property(e => e.name).HasMaxLength(255);

            entity.HasOne(d => d.created_byNavigation).WithMany(p => p.CliniccreatedByNavigation)
                .HasForeignKey(d => d.created_by)
                .HasConstraintName("clinic_ibfk_1");

            entity.HasOne(d => d.modified_byNavigation).WithMany(p => p.ClinicmodifiedByNavigation)
                .HasForeignKey(d => d.modified_by)
                .HasConstraintName("clinic_ibfk_2");
        });

        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.HasKey(e => new { e.id, e.used_by }).HasName("PRIMARY");

            entity.ToTable("Coupon", "sm-db-ver3");

            entity.HasIndex(e => e.created_by, "created_by");

            entity.HasIndex(e => e.used_by, "used_by");

            entity.Property(e => e.id).HasMaxLength(36);
            entity.Property(e => e.used_by).HasMaxLength(36);
            entity.Property(e => e.created_at).HasColumnType("datetime");
            entity.Property(e => e.created_by).HasMaxLength(36);
            entity.Property(e => e.expiry).HasColumnType("datetime");
            entity.Property(e => e.percentage_off).HasColumnType("float(2,2)");
            entity.Property(e => e.value).HasMaxLength(8);

            entity.HasOne(d => d.created_byNavigation).WithMany(p => p.CouponcreatedByNavigation)
                .HasForeignKey(d => d.created_by)
                .HasConstraintName("coupon_ibfk_2");

            entity.HasOne(d => d.used_byNavigation).WithMany(p => p.CouponusedByNavigation)
                .HasForeignKey(d => d.used_by)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("coupon_ibfk_1");
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Doctor", "sm-db-ver3");

            entity.HasIndex(e => e.ClinicId, "clinic_id");

            entity.HasIndex(e => e.UserId, "userid");

            entity.Property(e => e.Id).HasMaxLength(36);
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.FatherName).HasMaxLength(50).HasColumnName("father_name");
            entity.Property(e => e.RegistrationType).HasMaxLength(50).HasColumnName("registration_type");
            entity.Property(e => e.RegistrationDate).HasColumnType("date").HasColumnName("registration_date");
            entity.Property(e => e.LicenseExpiry).HasColumnType("date").HasColumnName("license_expiry");
            entity.Property(e => e.ApprovalStatus).HasMaxLength(50).HasColumnName("approval_status");
            entity.Property(e => e.ClinicId).HasMaxLength(36).HasColumnName("clinic_id");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasColumnName("created_at");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime").HasColumnName("modified_at");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.ProfilePictureUrl).HasColumnName("profile_picture_url").HasMaxLength(100);
            entity.Property(e => e.DoctorPaymentId).HasMaxLength(255).HasColumnName("doctor_payment_id");
            entity.Property(e => e.Phone).HasMaxLength(15);
            entity.Property(e => e.ProfileInfo).HasMaxLength(255).HasColumnName("profile_info");
            entity.Property(e => e.RegistrationId).HasMaxLength(50).HasColumnName("registration_id");
            entity.Property(e => e.SpecialityId).HasMaxLength(100).HasColumnName("speciality_id");
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.UserId).HasMaxLength(36).HasColumnName("userid");

            entity.HasOne(d => d.Clinic).WithMany(p => p.Doctor)
                .HasForeignKey(d => d.ClinicId)
                .HasConstraintName("doctor_ibfk_2");
            
            entity.HasOne(d => d.Speciality).WithMany(p => p.Doctors)
                .HasForeignKey(d => d.SpecialityId)
                .HasConstraintName("doctor_ibfk_3");

            entity.HasOne(d => d.User).WithMany(p => p.Doctor)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("doctor_ibfk_1");
            entity.HasMany(e => e.Qualifications)
                .WithMany(e => e.Doctors)
                .UsingEntity(
                    "doctorqualification",
                    l => l.HasOne(typeof(Qualification)).WithMany().HasForeignKey("qualification_id").HasPrincipalKey(nameof(Domain.Entities.Qualification.Id)),
                    r => r.HasOne(typeof(Doctor)).WithMany().HasForeignKey("doctor_id").HasPrincipalKey(nameof(Domain.Entities.Doctor.Id)),
                    j => j.HasKey("doctor_id", "qualification_id"));
        });
        
        modelBuilder.Entity<Qualification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Qualification", "sm-db-ver3");

            entity.Property(e => e.Id).HasMaxLength(36);
            entity.Property(e => e.Speciality).HasMaxLength(50);
            entity.Property(e => e.Degree).HasMaxLength(50);
            entity.Property(e => e.University).HasMaxLength(50);
            entity.Property(e => e.PassingYear).HasColumnType("date").HasColumnName("passing_year");
        });

        modelBuilder.Entity<DoctorDailyAvailability>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.ToTable("DoctorDailyAvailability", "sm-db-ver3");

            entity.HasIndex(e => e.created_by, "created_by");

            entity.HasIndex(e => e.doctor_id, "doctor_id");

            entity.Property(e => e.id).HasMaxLength(36);
            entity.Property(e => e.availability_end).HasColumnType("time");
            entity.Property(e => e.availability_start).HasColumnType("time");
            entity.Property(e => e.created_at).HasColumnType("datetime");
            entity.Property(e => e.created_by).HasMaxLength(36);
            entity.Property(e => e.doctor_id).HasMaxLength(36);
            entity.Property(e => e.modified_at).HasColumnType("datetime");

            entity.HasOne(d => d.created_byNavigation).WithMany(p => p.DoctorDailyAvailability)
                .HasForeignKey(d => d.created_by)
                .HasConstraintName("doctordailyavailability_ibfk_2");

            entity.HasOne(d => d.doctor).WithMany(p => p.DoctorDailyAvailability)
                .HasForeignKey(d => d.doctor_id)
                .HasConstraintName("doctordailyavailability_ibfk_1");
        });

        modelBuilder.Entity<MedicalForumComment>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.ToTable("MedicalForumComment", "sm-db-ver3");

            entity.HasIndex(e => e.author_id, "author_id");

            entity.HasIndex(e => e.post_id, "post_id");

            entity.Property(e => e.id).HasMaxLength(36);
            entity.Property(e => e.author_id).HasMaxLength(36);
            entity.Property(e => e.content).HasMaxLength(500);
            entity.Property(e => e.created_at).HasColumnType("datetime");
            entity.Property(e => e.post_id).HasMaxLength(36);

            entity.HasOne(d => d.author).WithMany(p => p.MedicalForumComment)
                .HasForeignKey(d => d.author_id)
                .HasConstraintName("medicalforumcomment_ibfk_2");

            entity.HasOne(d => d.post).WithMany(p => p.MedicalForumComment)
                .HasForeignKey(d => d.post_id)
                .HasConstraintName("medicalforumcomment_ibfk_1");
        });

        modelBuilder.Entity<MedicalForumCommentVotes>(entity =>
        {
            entity.HasKey(e => new { e.comment_id, e.doctor_id }).HasName("PRIMARY");

            entity.ToTable("MedicalForumCommentVotes", "sm-db-ver3");

            entity.HasIndex(e => e.comment_id, "comment_id");

            entity.HasIndex(e => e.doctor_id, "doctor_id");

            entity.Property(e => e.comment_id).HasMaxLength(36);
            entity.Property(e => e.created_at).HasColumnType("datetime");
            entity.Property(e => e.doctor_id).HasMaxLength(36);

            entity.HasOne(d => d.comment).WithMany(p => p.Votes)
                .HasForeignKey(d => d.comment_id)
                .HasConstraintName("medicalforumcommentvotes_ibfk_2");

            entity.HasOne(d => d.doctor).WithMany()
                .HasForeignKey(d => d.doctor_id)
                .HasConstraintName("medicalforumcommentvotes_ibfk_1");
        });

        modelBuilder.Entity<MedicalForumPost>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.ToTable("MedicalForumPost", "sm-db-ver3");

            entity.HasIndex(e => e.author_id, "author_id");

            entity.Property(e => e.id).HasMaxLength(36);
            entity.Property(e => e.author_id).HasMaxLength(36);
            entity.Property(e => e.heading).HasMaxLength(255);
            entity.Property(e => e.content).HasMaxLength(500);
            entity.Property(e => e.created_at).HasColumnType("datetime");

            entity.HasOne(d => d.author).WithMany(p => p.MedicalForumPost)
                .HasForeignKey(d => d.author_id)
                .HasConstraintName("medicalforumpost_ibfk_1");
        });

        modelBuilder.Entity<MedicalForumPostVotes>(entity =>
        {
            entity.HasKey(e => new { e.post_id, e.voted_by }).HasName("PRIMARY");
            entity.ToTable("MedicalForumPostVotes", "sm-db-ver3");

            entity.HasIndex(e => e.post_id, "post_id");

            entity.HasIndex(e => e.voted_by, "upvoted_by");

            entity.Property(e => e.created_at).HasColumnType("datetime");
            entity.Property(e => e.post_id).HasMaxLength(36);
            entity.Property(e => e.voted_by).HasMaxLength(36);

            entity.HasOne(d => d.post).WithMany(p => p.Votes)
                .HasForeignKey(d => d.post_id)
                .HasConstraintName("medicalforumpostvotes_ibfk_1");

            entity.HasOne(d => d.voted_byNavigation).WithMany()
                .HasForeignKey(d => d.voted_by)
                .HasConstraintName("medicalforumpostvotes_ibfk_2");
        });

        /*modelBuilder.Entity<MedicalHistory>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.ToTable("MedicalHistory", "sm-db-ver3");

            entity.HasIndex(e => e.patient_id, "patient_id");

            entity.Property(e => e.id).HasMaxLength(36);
            entity.Property(e => e.patient_id).HasMaxLength(36);

            /*entity.HasOne(d => d.patient).WithMany(/*p => p.MedicalHistory#2#)
                .HasForeignKey(d => d.patient_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("medicalhistory_ibfk_1");#1#
        });*/

        modelBuilder.Entity<MedicalHistoryDocument>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");
            entity
                .ToTable("MedicalHistoryDocument", "sm-db-ver3");

            entity.HasIndex(e => e.appointment_id, "appointment_id");

            entity.HasIndex(e => e.created_by, "created_by");

            entity.HasIndex(e => e.patient_id, "patient_id");

            entity.HasIndex(e => e.modified_by, "modified_by");

            entity.Property(e => e.appointment_id).HasMaxLength(36);
            entity.Property(e => e.created_at).HasColumnType("datetime");
            entity.Property(e => e.created_by).HasMaxLength(36);
            entity.Property(e => e.diagnosed_disease).HasMaxLength(255);
            entity.Property(e => e.doctors_comments).HasMaxLength(1000);
            entity.Property(e => e.document_path).HasMaxLength(500);
            entity.Property(e => e.document_name).HasMaxLength(255);
            entity.Property(e => e.document_description).HasMaxLength(500);
            entity.Property(e => e.id).HasMaxLength(36);
            entity.Property(e => e.patient_id).HasMaxLength(36);
            entity.Property(e => e.modified_at).HasColumnType("datetime");
            entity.Property(e => e.modified_by).HasMaxLength(36);
            entity.Property(e => e.record_date).HasColumnType("date");
            entity.Property(e => e.symptoms).HasMaxLength(255);

            entity.HasOne(d => d.appointment).WithMany(a => a.Documents)
                .HasForeignKey(d => d.appointment_id)
                .HasConstraintName("medicalhistorydocument_ibfk_1");

            entity.HasOne(d => d.created_byNavigation).WithMany()
                .HasForeignKey(d => d.created_by)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("medicalhistorydocument_ibfk_3");

            entity.HasOne(d => d.patient).WithMany(p => p.MedicalHistory)
                .HasForeignKey(d => d.patient_id )
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("medicalhistorydocument_ibfk_2");

            entity.HasOne(d => d.modified_byNavigation).WithMany()
                .HasForeignKey(d => d.modified_by)
                .HasConstraintName("medicalhistorydocument_ibfk_4");
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Patient", "sm-db-ver3");

            entity.HasIndex(e => e.UserId, "userid");

            entity.Property(e => e.Id).HasMaxLength(36);
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.City).HasMaxLength(255);
            entity.Property(e => e.Gender).HasMaxLength(255);
            entity.Property(e => e.BloodGroup).HasMaxLength(3)
                .HasColumnName("blood_group");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DateOfBirth).HasColumnType("date")
                .HasColumnName("date_of_birth");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime")
                .HasColumnName("modified_at");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(15);
            entity.Property(e => e.ProfileInfo).HasMaxLength(255)
                .HasColumnName("profile_info");
            entity.Property(e => e.UserId).HasMaxLength(36)
                .HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.Patient)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("patient_ibfk_1");
        });

        modelBuilder.Entity<RecordedSessions>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.ToTable("RecordedSessions", "sm-db-ver3");

            entity.HasIndex(e => e.appointment_id, "appointment_id");

            entity.Property(e => e.id).HasMaxLength(36);
            entity.Property(e => e.appointment_id).HasMaxLength(36);
            entity.Property(e => e.created_at).HasColumnType("datetime");
            entity.Property(e => e.session_link).HasMaxLength(255);

            entity.HasOne(d => d.appointment).WithMany(p => p.RecordedSessions)
                .HasForeignKey(d => d.appointment_id)
                .HasConstraintName("recordedsessions_ibfk_1");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.ToTable("Review", "sm-db-ver3");

            entity.HasIndex(e => e.appointment_id, "appointment_id");

            entity.Property(e => e.id).HasMaxLength(36);
            entity.Property(e => e.appointment_id).HasMaxLength(36);
            entity.Property(e => e.created_at).HasColumnType("datetime");
            entity.Property(e => e.feedback).HasMaxLength(500);

            entity.HasOne(d => d.appointment).WithMany(p => p.Review)
                .HasForeignKey(d => d.appointment_id)
                .HasConstraintName("review_ibfk_1");
        });

        modelBuilder.Entity<Transcriptions>(entity =>
        {
            entity.HasKey(e => e.transcription_id).HasName("PRIMARY");

            entity.ToTable("Transcriptions", "sm-db-ver3");

            entity.HasIndex(e => e.conference_id, "conference_id");

            entity.Property(e => e.transcription_id).HasMaxLength(36);
            entity.Property(e => e.conference_id).HasMaxLength(36);
            entity.Property(e => e.created_at).HasColumnType("datetime");
            entity.Property(e => e.modified_at).HasColumnType("datetime");
            entity.Property(e => e.sentiment_classification).HasMaxLength(50);
            entity.Property(e => e.transcription_text).HasMaxLength(500);

            entity.HasOne(d => d.conference).WithMany(p => p.Transcriptions)
                .HasForeignKey(d => d.conference_id)
                .HasConstraintName("transcriptions_ibfk_1");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("User", "sm-db-ver3");

            entity.Property(e => e.Id).HasMaxLength(36)
                .HasColumnName("id");
            entity.Property(e => e.Email).HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.PasswordHash).HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Role).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.Otp).HasMaxLength(20);
            entity.Property(e => e.OtpExpiry).HasColumnType("datetime")
                .HasColumnName("otp_expiry");
        });

        modelBuilder.Entity<Speciality>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.ToTable("speciality", "sm-db-ver3");
            entity.Property(e => e.Id).HasMaxLength(36).HasColumnName("id");
            entity.Property(e => e.Value).HasMaxLength(255).HasColumnName("value");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
