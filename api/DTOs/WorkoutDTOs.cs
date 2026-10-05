using System.ComponentModel.DataAnnotations;

namespace Api.Dtos;

// ---------- Requests (what the client sends) ----------

public record CreateSetRequest(
    int ExerciseId,
    [property: Range(1, 1000)] int SetOrder,
    [property: Range(1, 100)] int Reps,
    [property: Range(0, 2000)] double Weight);

public record CreateWorkoutRequest(
    DateOnly Date,
    string? Notes,
    List<CreateSetRequest> Sets);

public record UpdateWorkoutRequest(
    DateOnly Date,
    string? Notes,
    List<CreateSetRequest> Sets);

// ---------- Responses (what the API returns) ----------

public record SetDto(
    int Id,
    int ExerciseId,
    string ExerciseName,
    int SetNumber,
    int Reps,
    double Weight);

public record WorkoutSummaryDto(
    int Id,
    DateOnly Date,
    string? Notes,
    int SetCount);

public record WorkoutDetailDto(
    int Id,
    DateOnly Date,
    string? Notes,
    List<SetDto> Sets);

// One point per workout for a given exercise (for progress charts)
public record ProgressPointDto(
    DateOnly Date,
    double TopWeight,
    double EstimatedOneRepMax,
    int TotalReps);