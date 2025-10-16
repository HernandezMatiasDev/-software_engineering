namespace SuMejorPeso.Models;

public class BaseRecord
{
    public required int id { init; get; }
    public required Member member { set; get; }
    public required int memberId { set; get; }

    public required DateOnly startDate { set; get; }
    public required DateOnly endDate { set; get; }
            
}
