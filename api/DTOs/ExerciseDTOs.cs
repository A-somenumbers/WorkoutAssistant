namespace api.DTOs;

public record ExerciseDTO(int Id, string Name, string? MuscleGroup);

public record CreateExerciseRequest(string Name, string? MuscleGroup);