namespace SuMejorPeso.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

    public class Coach : Person
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }
    public ICollection<Specialty> specialties { get; set; } = new List<Specialty>();
    
    public ICollection<ScheduleCoach> schedule { get; set; } = new List<ScheduleCoach>();
    public required string state { set; get; }
    public ICollection<Classroom> classrooms { get; set; } = new List<Classroom>();


}
