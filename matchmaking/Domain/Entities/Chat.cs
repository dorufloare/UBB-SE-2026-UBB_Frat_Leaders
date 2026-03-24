namespace matchmaking.Domain.Entities;

public class Chat
{
    public int ChatId { get; set; }
    public int UserId { get; set; }
    public int? CompanyId { get; set; }
    public int? SecondUserId { get; set; }
    public int? JobId { get; set; }
    public bool IsBlocked { get; set; }
    public int? BlockedByUserId { get; set; }
    public bool IsDeletedByUser { get; set; }
    public bool IsDeletedBySecondParty { get; set; }
}
