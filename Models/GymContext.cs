using Microsoft.EntityFrameworkCore;
namespace SuMejorPeso.Models;


public class GymContext : DbContext
{
    public GymContext(DbContextOptions<GymContext> options) : base(options){}

    // Propiedades DbSet para cada clase que quieres mapear a una tabla
    public DbSet<Classroom> Classrooms { get; set; }
    public DbSet<Member> Member { get; set; }
    public DbSet<Assignment> Assignment { get; set; }
    public DbSet<ActivityRecord> ActivityRecord { get; set; }
    public DbSet<Attendance> Attendance { get; set; }
    public DbSet<Coach> Coach { get; set; }
    public DbSet<License> License { get; set; }
    public DbSet<Membership> Membership { get; set; }
    public DbSet<Pay> Pay { get; set; }
    public DbSet<ScheduleClassroom> ScheduleClassroom { get; set; }
    public DbSet<ScheduleCoach> ScheduleCoach { get; set; }
    public DbSet<Specialty> Specialty { get; set; }
    public DbSet<TypeMembreship> TypeMembreship { get; set; }
    public DbSet<User> User { get; set; }
    public DbSet<Branch> Branch { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // -------------------------------------------------------------
        // 1. JERARQUÍA DE PERSONAS (Person -> Member, Coach)
        // (Tu configuración para Member y Coach ya seguía este patrón)
        // -------------------------------------------------------------

        modelBuilder.Entity<Coach>().ToTable("Coaches");
        modelBuilder.Entity<Coach>()
            .Property(c => c.id)
            .ValueGeneratedOnAdd();
        
        modelBuilder.Entity<Member>(entity =>
        {
            entity.ToTable("Members"); // nombre de la tabla
            entity.HasKey(m => m.id);  // clave primaria
            entity.Property(m => m.id)
                .ValueGeneratedOnAdd()
                .UseMySqlIdentityColumn(); // autoincremento MySQL
        });

        // -------------------------------------------------------------
        // 2. JERARQUÍA DE REGISTROS (BaseRecord -> License, Membership)
        // (¡SE ELIMINÓ LA CONFIGURACIÓN TPC DE 'BaseRecord'!)
        // Ahora configuramos cada hija por separado:
        // -------------------------------------------------------------

        modelBuilder.Entity<License>(entity =>
        {
            entity.ToTable("Licenses");
            entity.HasKey(l => l.id); // Define la clave primaria en la hija
            entity.Property(l => l.id)
                .ValueGeneratedOnAdd()
                .UseMySqlIdentityColumn();
        });

        modelBuilder.Entity<Membership>(entity =>
        {
            entity.ToTable("Memberships");
            entity.HasKey(m => m.id); // Define la clave primaria en la hija
            entity.Property(m => m.id)
                .ValueGeneratedOnAdd()
                .UseMySqlIdentityColumn();
        });

        // -------------------------------------------------------------
        // 3. JERARQUÍA DE HORARIOS (Schedule -> ScheduleClassroom, ScheduleCoach)
        // (¡SE ELIMINÓ LA CONFIGURACIÓN TPC DE 'Schedule'!)
        // Ahora configuramos cada hija por separado:
        // -------------------------------------------------------------
        
        modelBuilder.Entity<ScheduleClassroom>(entity =>
        {
            entity.ToTable("ScheduleClassrooms");
            entity.HasKey(s => s.id); // Define la clave primaria en la hija
            entity.Property(s => s.id)
                .ValueGeneratedOnAdd()
                .UseMySqlIdentityColumn();
        });

        modelBuilder.Entity<ScheduleCoach>(entity =>
        {
            entity.ToTable("ScheduleCoaches");
            entity.HasKey(s => s.id); // Define la clave primaria en la hija
            entity.Property(s => s.id)
                .ValueGeneratedOnAdd()
                .UseMySqlIdentityColumn();
        });
        
        // -------------------------------------------------------------
        // (El resto de tu código de relaciones estaba perfecto y no cambia)
        // -------------------------------------------------------------

        // 4. CONFIGURACIÓN DE RELACIONES N:M (Existentes)
        modelBuilder.Entity<Classroom>()
            .HasMany(c => c.members)
            .WithMany(m => m.classrooms) 
            .UsingEntity(j => j.ToTable("ClassroomMembers"));

        
        // 5. NUEVA CONFIGURACIÓN DE SUCURSALES (Branch)
        modelBuilder.Entity<Coach>()
            .HasOne(coach => coach.branch)
            .WithMany(branch => branch.coaches)
            .HasForeignKey(coach => coach.branchId)
            .IsRequired(); 

        modelBuilder.Entity<Classroom>()
            .HasOne(classroom => classroom.branch)
            .WithMany(branch => branch.classrooms)
            .HasForeignKey(classroom => classroom.branchId)
            .IsRequired();

        modelBuilder.Entity<User>()
            .HasOne(user => user.branch)
            .WithMany(branch => branch.staff)
            .HasForeignKey(user => user.branchId)
            .IsRequired(false);


    // Configura la relación Classroom -> Assignment
         modelBuilder.Entity<Classroom>()
            .HasOne(c => c.assignment)
            .WithMany() // Assignment no necesita una lista de Classrooms
            .HasForeignKey(c => c.assignmentId)
            .IsRequired(); // Una clase debe tener una asignación/actividad
    }
}