using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace SuMejorPeso.Models;

public class Action
{
    public int id { set; get; }
    public required string name { set; get; }
    public required string description { set; get; }

    public required int durationMinutes { set; get; }
    public required string difficulty { set; get; }

}   

