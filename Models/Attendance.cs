using System.ComponentModel.DataAnnotations.Schema;
namespace SuMejorPeso.Models;

public class Attendance
{
    public int id { set; get; }
    public required DateTime date { set; get; }

    public required int classId { set; get; }
    [ForeignKey("classId")]
    public Classroom? classroom { get; set; } // Propiedad de navegación

    public required int memberId { set; get; }
    public Member? member { get; set; }
}