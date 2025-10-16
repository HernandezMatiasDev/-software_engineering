namespace SuMejorPeso.Models;

public class Attendance
{
    public required int id { init; get; }
    public required int memberId { set; get; }
    public required int classId { set; get; }
    public required DateTime date { set; get; }


}   

