namespace SehatMand.Domain.Entities;

public partial class MedicalForumPost
{
    public string id { get; set; } = Guid.NewGuid().ToString();

    public string author_id { get; set; }
    
    public string heading { get; set; }

    public string content { get; set; }

    public DateTime created_at { get; set; }

    public virtual ICollection<MedicalForumComment> MedicalForumComment { get; set; } = new List<MedicalForumComment>();
    public virtual ICollection<MedicalForumPostVotes> Votes { get; set; } = new List<MedicalForumPostVotes>();

    public virtual Patient? author { get; set; }
}
