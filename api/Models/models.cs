namespace api.Models
{
    public class Exercise
    {
        public int Id { get; set; }
        public required string Name { get; set; } // name of the exercise
        public string? MuscleGroup { get; set; } // optional muscle group targeted by the exercise
    }

    public class Workout
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; } // date of the workout
        public string? Notes { get; set; } // optional notes about the workout (eg. intensity, ROM, etc.)
        
    }
    public class WorkoutSet
    {
        public int Id { get; set; }
        public int WorkoutId { get; set; }// foreign key to the workout
        public Workout Workout { get; set; } = null!; // navigation property to the workout
        public int ExerciseId { get; set; } // foreign key to the exercise
        public Exercise Exercise { get; set; } = null!; // navigation property to the exercise
        public int SetNumber { get; set; } // the set number (eg. 1, 2, 3, etc.)
        public int Reps { get; set; } // number of repetitions performed in the set
        public double Weight { get; set; } // weight used in the set (in pounds or kg, depending on user preference)
    }
        
}