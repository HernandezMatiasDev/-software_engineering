namespace SuMejorPeso.Models;

public class User
{
    public int id { set; get; }
    public required string userName { set; get; }
    public required string passwordHash { set; get; }
    public required string salt { set; get; }
    public required string rol { set; get; }
    public required string name { set; get; }
    public required string lastName { set; get; }
    public required string email { set; get; }

    public required bool active { set; get; } = true;
    public required DateTime lastAccess { set; get; }



}   

