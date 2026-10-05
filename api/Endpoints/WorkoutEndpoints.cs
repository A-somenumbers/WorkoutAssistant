using api.Data;
using api.Models;
using Api.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace api.Endpoints;

public static class WorkoutEndpoints
{
    public static WebApplication MapWorkoutEndpoints(this WebApplication app)
    {
        var workouts = app.MapGroup("/api/workouts")
            .WithTags("Workouts");

        workouts.MapGet("/", async (
                AppDbContext dbContext,
                CancellationToken cancellationToken) =>
                TypedResults.Ok(await dbContext.Workouts
                    .AsNoTracking()
                    .OrderByDescending(workout => workout.Date)
                    .ThenByDescending(workout => workout.Id)
                    .Select(workout => new WorkoutSummaryDto(
                        workout.Id,
                        workout.Date,
                        workout.Notes,
                        dbContext.WorkoutSets.Count(set => set.WorkoutId == workout.Id)))
                    .ToListAsync(cancellationToken)))
            .WithName("GetWorkouts")
            .WithSummary("Get workout history")
            .WithDescription("Returns workout summaries with the newest workout first.")
            .Produces<IReadOnlyList<WorkoutSummaryDto>>(StatusCodes.Status200OK);

        workouts.MapGet("/{workoutId:int}", async Task<Results<
                Ok<WorkoutDetailDto>,
                NotFound>> (
                int workoutId,
                AppDbContext dbContext,
                CancellationToken cancellationToken) =>
            {
                var workout = await dbContext.Workouts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(item => item.Id == workoutId, cancellationToken);

                return workout is null
                    ? TypedResults.NotFound()
                    : TypedResults.Ok(new WorkoutDetailDto(
                        workout.Id,
                        workout.Date,
                        workout.Notes,
                        await dbContext.WorkoutSets
                            .AsNoTracking()
                            .Where(set => set.WorkoutId == workoutId)
                            .OrderBy(set => set.SetNumber)
                            .ThenBy(set => set.Id)
                            .Select(set => new SetDto(
                                set.Id,
                                set.ExerciseId,
                                set.Exercise.Name,
                                set.SetNumber,
                                set.Reps,
                                set.Weight))
                            .ToListAsync(cancellationToken)));
            })
            .WithName("GetWorkoutById")
            .WithSummary("Get a workout with its sets")
            .WithDescription("Returns the specified workout and its sets, ordered by set number.")
            .Produces<WorkoutDetailDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        workouts.MapPost("/", async Task<Results<
                Created<WorkoutDetailDto>,
                ValidationProblem>> (
                CreateWorkoutRequest request,
                AppDbContext dbContext,
                CancellationToken cancellationToken) =>
            {
                var validationErrors = ValidateCreateWorkoutRequest(request);
                if (validationErrors.Count > 0)
                {
                    return TypedResults.ValidationProblem(validationErrors);
                }

                var exerciseIds = request.Sets
                    .Select(set => set.ExerciseId)
                    .Distinct()
                    .ToArray();
                var exercises = await dbContext.Exercises
                    .AsNoTracking()
                    .Where(exercise => exerciseIds.Contains(exercise.Id))
                    .Select(exercise => new { exercise.Id, exercise.Name })
                    .ToListAsync(cancellationToken);
                var exerciseNames = exercises.ToDictionary(exercise => exercise.Id, exercise => exercise.Name);
                var missingExerciseIds = exerciseIds
                    .Where(exerciseId => !exerciseNames.ContainsKey(exerciseId))
                    .ToArray();

                if (missingExerciseIds.Length > 0)
                {
                    return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                    {
                        ["sets.exerciseId"] =
                        [
                            $"Exercise IDs do not exist: {string.Join(", ", missingExerciseIds)}."
                        ]
                    });
                }

                var workout = new Workout
                {
                    Date = request.Date,
                    Notes = request.Notes
                };
                var sets = request.Sets
                    .Select(set => new WorkoutSet
                    {
                        Workout = workout,
                        ExerciseId = set.ExerciseId,
                        SetNumber = set.SetOrder,
                        Reps = set.Reps,
                        Weight = set.Weight
                    })
                    .ToList();

                dbContext.Workouts.Add(workout);
                dbContext.WorkoutSets.AddRange(sets);
                await dbContext.SaveChangesAsync(cancellationToken);

                var response = new WorkoutDetailDto(
                    workout.Id,
                    workout.Date,
                    workout.Notes,
                    sets.OrderBy(set => set.SetNumber)
                        .ThenBy(set => set.Id)
                        .Select(set => new SetDto(
                            set.Id,
                            set.ExerciseId,
                            exerciseNames[set.ExerciseId],
                            set.SetNumber,
                            set.Reps,
                            set.Weight))
                        .ToList());

                return TypedResults.Created($"/api/workouts/{workout.Id}", response);
            })
            .WithName("CreateWorkout")
            .WithSummary("Create a workout")
            .WithDescription("Saves a workout and all of its sets.")
            .Produces<WorkoutDetailDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        return app;
    }

    private static Dictionary<string, string[]> ValidateCreateWorkoutRequest(
        CreateWorkoutRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.Sets is null)
        {
            errors["sets"] = ["Sets are required."];
            return errors;
        }

        for (var index = 0; index < request.Sets.Count; index++)
        {
            var set = request.Sets[index];
            if (set is null)
            {
                errors[$"sets[{index}]"] = ["Set entries cannot be null."];
                continue;
            }

            if (set.SetOrder is < 1 or > 1000)
            {
                errors[$"sets[{index}].setOrder"] = ["Set order must be between 1 and 1000."];
            }

            if (set.Reps is < 1 or > 100)
            {
                errors[$"sets[{index}].reps"] = ["Reps must be between 1 and 100."];
            }

            if (set.Weight is < 0 or > 2000)
            {
                errors[$"sets[{index}].weight"] = ["Weight must be between 0 and 2000."];
            }
        }

        return errors;
    }
}
