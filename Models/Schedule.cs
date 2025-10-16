namespace SuMejorPeso.Models;

    public class Schedule
    {
        public int id { get; init; }
        
        public required string dayWeek { get; set; }
        public TimeSpan startTime { get; set; } 
        public TimeSpan endTime { get; set; }
    
    }
