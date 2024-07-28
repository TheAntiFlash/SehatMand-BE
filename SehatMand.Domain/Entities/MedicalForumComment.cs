namespace SehatMand.Domain.Entities;

public partial class MedicalForumComment
{
    public string id { get; set; } = null!;

    public string? post_id { get; set; }

    public string? author_id { get; set; }

    public string? content { get; set; }

    public DateTime? created_at { get; set; }

    public virtual Doctor? author { get; set; }

    public virtual MedicalForumPost? post { get; set; }
}
