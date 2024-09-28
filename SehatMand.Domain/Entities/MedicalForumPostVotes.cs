using System.ComponentModel.DataAnnotations.Schema;

namespace SehatMand.Domain.Entities;

public partial class MedicalForumPostVotes
{
    public string post_id { get; set; }

    public string voted_by { get; set; }
    
    public bool vote_type { get; set; }

    public DateTime? created_at { get; set; }

    public virtual MedicalForumPost? post { get; set; }

    public virtual User? voted_byNavigation { get; set; }
    
    [NotMapped]
    public bool IsUpVote => vote_type;
    [NotMapped]
    public bool IsDownVote => !vote_type;
}
