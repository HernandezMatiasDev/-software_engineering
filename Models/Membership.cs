namespace SuMejorPeso.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

    public class Membership : BaseRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { set; get; }

    public required TypeMembreship type { set; get; } 
    public required string state { set; get; } 
    public required bool active { set; get; } = true;

    public required float pricePaid { set; get; } = 0;
    public required float debt { set; get; }
    public required float discount { set; get; } = 0;
}
