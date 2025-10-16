namespace SuMejorPeso.Models;
    public class Person
{
    public required int id { init; get; }
    public required string name { set; get; }
    public required string llastName { set; get; }
    public required int dni { set; get; }
    public required string phone { set; get; }
    public required string email { set; get; }
    public required User user { set; get; }
    public required int userId{ set; get; }
}
