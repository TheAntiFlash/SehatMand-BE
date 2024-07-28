namespace SehatMand.Domain.Entities;

public partial class Coupon
{
    public string id { get; set; } = null!;

    public string? value { get; set; }

    public bool? is_active { get; set; }

    public string used_by { get; set; } = null!;

    public float? percentage_off { get; set; }

    public DateTime? expiry { get; set; }

    public DateTime? created_at { get; set; }

    public string? created_by { get; set; }

    public virtual User? created_byNavigation { get; set; }

    public virtual User used_byNavigation { get; set; } = null!;
}
