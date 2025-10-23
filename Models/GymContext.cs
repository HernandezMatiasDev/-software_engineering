using Microsoft.EntityFrameworkCore;
namespace SuMejorPeso.Models;


public class GymContext : DbContext
{
    public GymContext(DbContextOptions<GymContext> options) : base(options){}

    // Propiedades DbSet para cada clase que quieres mapear a una tabla
    public DbSet<Classroom> Classrooms { get; set; }
    public DbSet<Member> Member { get; set; }
    public DbSet<Action> Action { get; set; }
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


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // -------------------------------------------------------------
        // IDs automáticos en jerarquías TPC
        // ----------------------------------------------------------

        modelBuilder.Entity<BaseRecord>()
            .Property(br => br.id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Schedule>()
            .Property(s => s.id)
            .ValueGeneratedOnAdd();


        // añadirndo autoincremento en las id hijas

        modelBuilder.Entity<Coach>()
            .Property(c => c.id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<ScheduleClassroom>()
            .Property(sc => sc.id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<ScheduleCoach>()
            .Property(sc => sc.id)
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
        // 1. JERARQUÍA DE PERSONAS (Person -> Member, Coach)
        // No se creará la tabla 'Person'. Cada derivada tendrá sus propias columnas.
        // -------------------------------------------------------------

        // Mapeamos las clases concretas a sus nombres de tabla deseados
        modelBuilder.Entity<Coach>().ToTable("Coaches");
        
        // -------------------------------------------------------------
        // 2. JERARQUÍA DE REGISTROS (BaseRecord -> License, Membership)
        // No se creará la tabla 'BaseRecord'.
        // -------------------------------------------------------------
        modelBuilder.Entity<BaseRecord>()
            .HasKey(br => br.id); // Esencial para TPC

        modelBuilder.Entity<BaseRecord>()
            .UseTpcMappingStrategy();

        // Mapeamos las clases concretas a sus nombres de tabla
        modelBuilder.Entity<License>().ToTable("Licenses");
        modelBuilder.Entity<Membership>().ToTable("Memberships");

        // -------------------------------------------------------------
        // 3. JERARQUÍA DE HORARIOS (Schedule -> ScheduleClassroom, ScheduleCoach)
        // No se creará la tabla 'Schedule'.
        // -------------------------------------------------------------
        modelBuilder.Entity<Schedule>()
            .HasKey(s => s.id); // Esencial para TPC

        modelBuilder.Entity<Schedule>()
            .UseTpcMappingStrategy();

        // Mapeamos las clases concretas a sus nombres de tabla
        modelBuilder.Entity<ScheduleClassroom>().ToTable("ScheduleClassrooms");
        modelBuilder.Entity<ScheduleCoach>().ToTable("ScheduleCoaches");

        // -------------------------------------------------------------
        // 4. CONFIGURACIÓN DE RELACIONES N:M (Si fuera necesario ajustar)
        // EF Core mapea automáticamente las relaciones simples de N:M.
        // Ejemplo: Classroom tiene ICollection<Coach> coaches, y Coach tiene ICollection<Classroom> classrooms.
        // Para cambiar el nombre de la tabla de unión (ej: de CoachClassroom a ClassroomInstructors):
        // modelBuilder.Entity<Classroom>()
        //     .HasMany(c => c.coaches)
        //     .WithMany(c => c.classrooms) // Si Classroom tiene la propiedad classrooms
        //     .UsingEntity(j => j.ToTable("ClassroomInstructors")); 

        // Como tus modelos Classroom y Coach no tienen la colección cruzada (el modelo Classroom sí la tiene pero el modelo Coach no la tiene), 
        // EF Core puede necesitar ayuda si quieres una relación N:M bidireccional, o puedes dejarlo como 1:N (uno-a-muchos). 
        // Si quieres N:M (Múltiples coaches en Múltiples clases):
        
        // Relación N:M entre Classroom y Coach (asumiendo que Coach tiene ICollection<Classroom> classrooms):
        // Nota: Asegúrate que la propiedad esté en ambos lados para que EF Core sepa que es una relación N:M
        /*
        modelBuilder.Entity<Classroom>()
            .HasMany(c => c.coaches)
            .WithMany(ch => ch.classrooms) // Asumiendo que Coach.cs tiene: public ICollection<Classroom> classrooms
            .UsingEntity(j => j.ToTable("ClassroomCoach"));
        */
        
        // Relación N:M entre Classroom y Member:
        modelBuilder.Entity<Classroom>()
            .HasMany(c => c.members)
            .WithMany(m => m.classrooms) // Asumiendo que Member.cs tiene: public ICollection<Classroom> classrooms
            .UsingEntity(j => j.ToTable("ClassroomMembers"));

    }
}