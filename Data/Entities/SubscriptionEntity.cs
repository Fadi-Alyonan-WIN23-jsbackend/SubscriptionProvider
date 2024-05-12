using System.ComponentModel.DataAnnotations;

namespace Data.Entities;

public class SubscriptionEntity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [Required]
    public string Email { get; set; } = null!;
    public bool DailyNewsletter { get; set; }
    public bool AdvertisingUpdates { get; set; }
    public bool WeekinReview { get; set; }
    public bool EventUpdates { get; set; }
    public bool StartupsWeekly { get; set; }
    public bool Podcasts { get; set; }
}
