namespace SuMejorPeso.Models;

public abstract class BaseRecord
{
    public int id { set; get; }
    public required Member member { set; get; }
    public required DateOnly startDate { set; get; }
    public required DateOnly endDate { set; get; }
            
}
