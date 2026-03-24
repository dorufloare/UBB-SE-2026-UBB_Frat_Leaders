namespace matchmaking.Domain.Entities;

public class Job
{
    public int JobId { get; set; }
    public string JobDescription { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string EmploymentType { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public int PromotionLevel { get; set; }
}
