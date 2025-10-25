using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace SuMejorPeso.Models;

public class Assignment
{
    public int id { set; get; }
    public required string name { set; get; }
    public required string description { set; get; }
    public bool active { get; set; } = true;
    public required int durationMinutes { set; get; }
    public required string difficulty { set; get; }

}   

