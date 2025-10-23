namespace SuMejorPeso.Models;
    public class TypeMembreship
{
    public int id { set; get; }
    public required string name { set; get; }
    public required string description { set; get; }
    public required int daysDuration { set; get; }
    public required float price { set; get; }

}
