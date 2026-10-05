using Microsoft.EntityFrameworkCore;
using api.Models;

namespace api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options){

    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<Workout> Workouts => Set<Workout>();
    public DbSet<WorkoutSet> WorkoutSets => Set<WorkoutSet>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Exercise>().HasData(
            new Exercise { Id = 1, Name = "Bench Press", MuscleGroup = "Chest" },
            new Exercise { Id = 2, Name = "Squat", MuscleGroup = "Legs" },
            new Exercise { Id = 3, Name = "Deadlift", MuscleGroup = "Back" },
            new Exercise { Id = 4, Name = "Overhead Press", MuscleGroup = "Shoulders" },
            new Exercise { Id = 5, Name = "Barbell Row", MuscleGroup = "Back" },
            new Exercise { Id = 6, Name = "Pull-Up", MuscleGroup = "Back" },
            new Exercise { Id = 7, Name = "Dumbbell Curl", MuscleGroup = "Biceps" },
            new Exercise { Id = 8, Name = "Tricep Extension", MuscleGroup  = "Triceps" }, 
            new Exercise { Id = 9, Name = "Lateral Raise", MuscleGroup = "Shoulders" },
            new Exercise { Id = 10, Name = "Leg Press", MuscleGroup = "Legs" },
            new Exercise { Id = 11, Name = "Leg Curl", MuscleGroup = "Legs" },
            new Exercise { Id = 12, Name = "Leg Extension", MuscleGroup = "Legs" },
            new Exercise { Id = 13, Name = "Calf Raise", MuscleGroup = "Calves" }
        );
    }

}
    