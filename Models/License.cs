namespace SuMejorPeso.Models;
    public class License : BaseRecord
{
    public required int barcode { set; get; }
    public required bool state { set; get; } //true = activo / false = inactivo
}
