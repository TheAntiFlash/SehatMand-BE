using System.ComponentModel.DataAnnotations.Schema;

namespace SehatMand.Domain.Entities;

public partial class MedicalForumCommentVotes
{
    public string doctor_id { get; set; }

    public string comment_id { get; set; }

    public bool vote_type { get; set; }

    public DateTime created_at { get; set; }

    public virtual MedicalForumComment? comment { get; set; }

    public virtual Doctor? doctor { get; set; }
    
    [NotMapped]
    public bool IsUpVote => vote_type;
    [NotMapped]
    public bool IsDownVote => !vote_type;
}
