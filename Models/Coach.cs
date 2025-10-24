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
    public required bool active { set; get; } = true;
    public ICollection<Classroom> classrooms { get; set; } = new List<Classroom>();
    public required int branchId { get; set; }
    public required Branch branch { get; set; }

}
