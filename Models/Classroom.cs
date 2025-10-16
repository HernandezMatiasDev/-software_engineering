namespace SuMejorPeso.Models;

public class Classroom
{
    public required int id { init; get; }
    public required Activity activity { set; get; }
    public required string name { set; get; }
    public required string description { set; get; }

    public ICollection<ScheduleClassroom> schedule { get; set; } = new List<ScheduleClassroom>();
    public required int limitedPlace { set; get; }
    public ICollection<Member> members { get; set; } = new List<Member>();
    public ICollection<Coach> coaches { get; set; } = new List<Coach>();


}   

