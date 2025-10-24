namespace SuMejorPeso.Models;
    public class Specialty
    {
        public int id { get; set; }
        public required string name { get; set; } 
        public bool active { get; set; } = true;
        public ICollection<Coach> coaches { get; set; } = new List<Coach>();
    }
