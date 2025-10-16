namespace SuMejorPeso.Models;

    public class ScheduleCoach : Schedule
    {

        public int coachId { get; set; } 
        public required Coach Entrenador { get; set; }
    }
