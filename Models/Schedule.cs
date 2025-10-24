namespace SuMejorPeso.Models;

    public abstract class Schedule
    {

        public required string dayWeek { get; set; }
        public TimeSpan startTime { get; set; } 
        public TimeSpan endTime { get; set; }
    
    }
