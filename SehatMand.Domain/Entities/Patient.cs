﻿namespace SehatMand.Domain.Entities;

public partial class Patient
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string? UserId { get; set; } = null!;

    public string Name { get; set; }

    public DateTime DateOfBirth { get; set; }

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Gender { get; set; }

    public string? ProfileInfo { get; set; }

    public float? Weight { get; set; }

    public string? BloodGroup { get; set; }

    public float? Height { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public virtual ICollection<Appointment> Appointment { get; set; } = new List<Appointment>();

    public virtual ICollection<MedicalForumPost> MedicalForumPost { get; set; } = new List<MedicalForumPost>();

    public virtual ICollection<MedicalHistoryDocument> MedicalHistory { get; set; } = new List<MedicalHistoryDocument>();

    public virtual User User { get; set; } = null!;
}
