namespace SuMejorPeso.Models;

public class Classroom
{
    public int id { set; get; }
    public required Assignment assignment { set; get; }
    public required int assignmentId { set; get; }
    public required string name { set; get; }
    public required string description { set; get; }
    public required bool active { set; get; } = true;

    public ICollection<ScheduleClassroom> schedule { get; set; } = new List<ScheduleClassroom>();
    public required int limitedPlace { set; get; }
    public ICollection<Member> members { get; set; } = new List<Member>();
    public ICollection<Coach> coaches { get; set; } = new List<Coach>();

    public int branchId { get; set; }
    public required Branch branch { get; set; }
}   

