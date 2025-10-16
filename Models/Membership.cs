namespace SuMejorPeso.Models;
    public class Membership : BaseRecord
{
    public required TypeMembreship type { set; get; } 
    public required string state { set; get; } 
    public required float pricePaid { set; get; } = 0;
    public required float debt { set; get; }
    public required float discount { set; get; } = 0;
}
