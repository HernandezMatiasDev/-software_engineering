namespace SuMejorPeso.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

    public class License : BaseRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]   
    public int id { set; get; }

    [StringLength(50)] 
    public required string barcode { set; get; }
    public required bool active { set; get; } //true = activo / false = inactivo
}
