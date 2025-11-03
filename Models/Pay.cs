namespace SuMejorPeso.Models;

public class Pay
{
    public int id { set; get; }
    public required int memberId { set; get; }
    public Member? member { get; set; }
    public required int membershipId { set; get; }
    public Membership? membership { get; set; }
    public required DateTime date { set; get; }
    public required int amount { set; get; }
    public required string paymentMethod { set; get; }


}   

