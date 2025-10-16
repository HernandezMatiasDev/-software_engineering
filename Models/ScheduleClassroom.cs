namespace SuMejorPeso.Models;

    public class ScheduleClassroom: Schedule
    {

        public int classroomId { get; set; } 
        public required Classroom classroom { get; set; }
    }
