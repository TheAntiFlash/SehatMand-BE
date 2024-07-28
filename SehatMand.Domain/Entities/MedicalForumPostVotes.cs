namespace SehatMand.Domain.Entities;

public partial class MedicalForumPostVotes
{
    public string? post_id { get; set; }

    public string? upvoted_by { get; set; }

    public DateTime? created_at { get; set; }

    public virtual MedicalForumPost? post { get; set; }

    public virtual User? upvoted_byNavigation { get; set; }
}
