namespace SuMejorPeso.Models;

public class Pay
{
    public int id { set; get; }
    public required int memberId { set; get; }
    public required int membershipId { set; get; }
    public required DateTime date { set; get; }
    public required int amount { set; get; }
    public required string paymentMethod { set; get; }


}   

