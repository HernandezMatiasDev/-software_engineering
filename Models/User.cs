namespace SuMejorPeso.Models;

public enum UserRole
{

    SuperUser,
    Manager,
    member,
    Administrator,
    coach,
    defaults
}
public class User
{
    public int id { set; get; }
    public required string userName { set; get; }
    public required string passwordHash { set; get; }
    public required string salt { set; get; }
    public UserRole Role { get; set; } = UserRole.defaults;
    public required string name { set; get; }
    public required string lastName { set; get; }
    public required string email { set; get; }

    public required bool active { set; get; } = true;
    public required DateTime lastAccess { set; get; }

    public int? branchId { get; set; } 
    public Branch? branch { get; set; }

}   

