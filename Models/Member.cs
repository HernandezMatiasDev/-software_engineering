namespace SuMejorPeso.Models;
    public class Member : Person
{
    public required DateOnly birthdate { set; get; }
    public string direction { set; get; } = string.Empty;
    public required string gender { set; get; }
    public required bool state { set; get; }
    public License? license { set; get; }
    public int? licenseId { set; get; }
    public Membership? membership { set; get; }
    public int? membershipId { set; get; }
    public ICollection<Attendance> attendance { get; set; } = new List<Attendance>();

    public string note { set; get; } = string.Empty;
}
