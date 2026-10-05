using api.Data;
using Api.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace api.Endpoints;

public static class ExerciseEndpoints
{
    public static WebApplication MapExerciseEndpoints(this WebApplication app)
    {
        var exercises = app.MapGroup("/api/exercises")
            .WithTags("Exercises");

        exercises.MapGet("/", async (
                AppDbContext dbContext,
                CancellationToken cancellationToken) =>
                TypedResults.Ok(await dbContext.Exercises
                    .AsNoTracking()
                    .OrderBy(exercise => exercise.Name)
                    .Select(exercise => new api.DTOs.ExerciseDTO(
                        exercise.Id,
                        exercise.Name,
                        exercise.MuscleGroup))
                    .ToListAsync(cancellationToken)))
            .WithName("GetExercises")
            .WithSummary("Get all exercises")
            .WithDescription("Returns every exercise sorted by name.")
            .Produces<IReadOnlyList<api.DTOs.ExerciseDTO>>(StatusCodes.Status200OK);

        exercises.MapGet("/{exerciseId:int}/progress", async Task<Results<
                Ok<IReadOnlyList<ProgressPointDto>>,
                NotFound>> (
                int exerciseId,
                AppDbContext dbContext,
                CancellationToken cancellationToken) =>
            {
                var exerciseExists = await dbContext.Exercises
                    .AnyAsync(exercise => exercise.Id == exerciseId, cancellationToken);

                if (!exerciseExists)
                {
                    return TypedResults.NotFound();
                }

                var sets = await dbContext.WorkoutSets
                    .AsNoTracking()
                    .Where(set => set.ExerciseId == exerciseId)
                    .Select(set => new
                    {
                        set.Workout.Date,
                        set.Weight,
                        set.Reps
                    })
                    .ToListAsync(cancellationToken);

                var progress = sets
                    .GroupBy(set => set.Date)
                    .OrderBy(group => group.Key)
                    .Select(group => new ProgressPointDto(
                        group.Key,
                        group.Max(set => set.Weight),
                        group.Max(set => set.Weight * (1 + set.Reps / 30.0)),
                        group.Sum(set => set.Reps)))
                    .ToList();

                return TypedResults.Ok<IReadOnlyList<ProgressPointDto>>(progress);
            })
            .WithName("GetExerciseProgress")
            .WithSummary("Get exercise progress points")
            .WithDescription("Returns one chart point per workout date for the specified exercise.")
            .Produces<IReadOnlyList<ProgressPointDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
