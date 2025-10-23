namespace SuMejorPeso.Models;
    public class Specialty
    {
        public int id { get; set; }
        public required string name { get; set; } 
        
        public ICollection<Coach> coaches { get; set; } = new List<Coach>();
    }
