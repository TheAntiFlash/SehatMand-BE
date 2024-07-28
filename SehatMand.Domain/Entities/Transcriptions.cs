namespace SehatMand.Domain.Entities;

public partial class Transcriptions
{
    public string transcription_id { get; set; } = null!;

    public string? conference_id { get; set; }

    public string? transcription_text { get; set; }

    public float? sentiment_score { get; set; }

    public string? sentiment_classification { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? modified_at { get; set; }

    public virtual RecordedSessions? conference { get; set; }
}
