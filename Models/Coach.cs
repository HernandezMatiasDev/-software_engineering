namespace SuMejorPeso.Models;
    public class Coach : Person
{

    public ICollection<Specialty> specialties { get; set; } = new List<Specialty>();
    
    public ICollection<ScheduleCoach> schedule { get; set; } = new List<ScheduleCoach>();
    public required string state { set; get; }


}
