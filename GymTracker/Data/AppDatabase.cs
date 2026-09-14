using SQLite;
using GymTracker.Models;

namespace GymTracker.Data;

public class AppDatabase
{
    private SQLiteAsyncConnection? _database;

    public async Task InitAsync()
    {
        if (_database != null)
            return;

        _database = new SQLiteAsyncConnection(DatabaseConstants.DatabasePath);

        await _database.CreateTableAsync<WorkoutProgram>();
        await _database.CreateTableAsync<WorkoutDay>();
        await _database.CreateTableAsync<Exercise>();
        await MigrateExerciseSideTrackingAsync();
        await _database.CreateTableAsync<ProgramExercise>();
        await MigrateProgramExerciseRestMinutesAsync();
        await _database.CreateTableAsync<WorkoutSession>();
        await _database.CreateTableAsync<WorkoutSet>();
        await MigrateWorkoutSetSideAsync();
        await _database.CreateTableAsync<PersonalRecord>();
    }
    private async Task MigrateExerciseSideTrackingAsync()
    {
        var columns =
            await _database!.GetTableInfoAsync(
                nameof(Exercise));

        if (!columns.Any(column =>
                column.Name.Equals(
                    nameof(Exercise.IsUnilateral),
                    StringComparison.OrdinalIgnoreCase)))
        {
            await _database.ExecuteAsync(
                """
                ALTER TABLE Exercise
                ADD COLUMN IsUnilateral INTEGER NOT NULL DEFAULT 0
                """);
        }
    }

    private async Task MigrateWorkoutSetSideAsync()
    {
        var columns =
            await _database!.GetTableInfoAsync(
                nameof(WorkoutSet));

        if (!columns.Any(column =>
                column.Name.Equals(
                    nameof(WorkoutSet.Side),
                    StringComparison.OrdinalIgnoreCase)))
        {
            await _database.ExecuteAsync(
                """
                ALTER TABLE WorkoutSet
                ADD COLUMN Side TEXT NOT NULL DEFAULT ''
                """);
        }

        await _database.ExecuteAsync(
            """
            UPDATE WorkoutSet
            SET Side = ''
            WHERE Side IS NULL
            """);
    }
    private async Task MigrateProgramExerciseRestMinutesAsync()
    {
        var columns =
            await _database!.GetTableInfoAsync(
                nameof(ProgramExercise));

        if (!columns.Any(column =>
                column.Name.Equals(
                    nameof(ProgramExercise.RestMinutes),
                    StringComparison.OrdinalIgnoreCase)))
        {
            await _database.ExecuteAsync(
                """
                ALTER TABLE ProgramExercise
                ADD COLUMN RestMinutes REAL NOT NULL DEFAULT 3
                """);
        }

        // sqlite-net may add a new numeric column with zero on an
        // existing table. Zero was never a valid configured value,
        // so legacy rows receive the three-minute default.
        await _database.ExecuteAsync(
            """
            UPDATE ProgramExercise
            SET RestMinutes = 3
            WHERE RestMinutes IS NULL
               OR RestMinutes <= 0
            """);
    }
    public async Task TerminateActiveWorkoutAsync(int sessionId)
    {
        await DeleteWorkoutSessionAsync(sessionId);
    }
    public SQLiteAsyncConnection Database
    {
        get
        {
            if (_database == null)
                throw new InvalidOperationException("Database has not been initialized.");

            return _database;
        }
    }
    public async Task<List<WorkoutProgram>> GetWorkoutProgramsAsync()
    {
        await InitAsync();

        return await Database.Table<WorkoutProgram>()
                             .OrderBy(x => x.Name)
                             .ToListAsync();
    }

    public async Task<int> SaveWorkoutProgramAsync(WorkoutProgram program)
    {
        await InitAsync();

        if (program.Id == 0)
            return await Database.InsertAsync(program);

        return await Database.UpdateAsync(program);
    }
    public async Task<List<Exercise>> GetExercisesAsync()
    {
        await InitAsync();

        return await Database.Table<Exercise>()
                             .OrderBy(x => x.Name)
                             .ToListAsync();
    }

    public async Task<int> SaveExerciseAsync(Exercise exercise)
    {
        await InitAsync();

        if (exercise.Id == 0)
            return await Database.InsertAsync(exercise);

        return await Database.UpdateAsync(exercise);
    }

    public async Task<int> DeleteExerciseAsync(Exercise exercise)
    {
        await InitAsync();

        return await Database.DeleteAsync(exercise);
    }
    public async Task<List<WorkoutDay>> GetWorkoutDaysAsync(int programId)
    {
        await InitAsync();

        return await Database.Table<WorkoutDay>()
            .Where(day => day.WorkoutProgramId == programId)
            .OrderBy(day => day.DayNumber)
            .ToListAsync();
    }

    public async Task SaveProgramWithDaysAsync(
        WorkoutProgram program,
        List<WorkoutDay> days)
    {
        await InitAsync();

        await Database.InsertAsync(program);

        foreach (var day in days)
        {
            day.WorkoutProgramId = program.Id;
            await Database.InsertAsync(day);
        }
    }

    public async Task DeleteWorkoutProgramAsync(WorkoutProgram program)
    {
        await InitAsync();

        var days = await GetWorkoutDaysAsync(program.Id);

        foreach (var day in days)
        {
            await Database.DeleteAsync(day);
        }

        await Database.DeleteAsync(program);
    }
    private async Task SeedDay1Async(int workoutDayId)
    {
        var items = new[]
        {
        ("Smith High Incline Press", 1),
        ("T Bar Row", 1),
        ("Machine Lateral Raises", 1),
        ("Machine Wide Grip Lat Pulldown", 1),
        ("DB Preacher Curl", 1),
        ("SA Tricep Pushdown", 1),
        ("Seated Leg Curl", 1),
        ("Leg Extenstion", 1),
        ("Leg Press Calf Raises", 2),
        ("SA Rear Delt Flies", 1),
        ("Wrist Curls", 1)
    };

        var order = 1;

        foreach (var item in items)
        {
            var exerciseId =
                await GetOrCreateExerciseIdAsync(item.Item1);

            await Database.InsertAsync(new ProgramExercise
            {
                WorkoutDayId = workoutDayId,
                ExerciseId = exerciseId,
                PlannedSets = item.Item2,
                RestMinutes = 3,
                Order = order++
            });
        }
    }
    private async Task SeedDay2Async(int workoutDayId)
    {
        var items = new[]
        {
        ("Hack Squat", 1),
        ("Seated Leg Curl", 1),
        ("Hip Adduction", 1),
        ("T Bar Row", 1),
        ("Chest Press Machine", 1),
        ("SA Lat Row", 1),
        ("Machine Shoulder Press", 1),
        ("DB Preacher Curl", 1),
        ("SA Tricep Pushdown", 1),
        ("Reverse Pec Dec", 1),
        ("Lat Pulldown Crunches", 1)
    };

        var order = 1;

        foreach (var item in items)
        {
            var exerciseId =
                await GetOrCreateExerciseIdAsync(item.Item1);

            await Database.InsertAsync(new ProgramExercise
            {
                WorkoutDayId = workoutDayId,
                ExerciseId = exerciseId,
                PlannedSets = item.Item2,
                RestMinutes = 3,
                Order = order++
            });
        }
    }
    public async Task<WorkoutSession?> GetActiveWorkoutSessionAsync()
    {
        await InitAsync();

        return await Database.Table<WorkoutSession>()
            .Where(x => !x.IsCompleted)
            .OrderByDescending(x => x.Date)
            .FirstOrDefaultAsync();
    }
    public async Task DeleteWorkoutSessionAsync(int sessionId)
    {
        await InitAsync();

        var sets = await Database.Table<WorkoutSet>()
            .Where(x => x.WorkoutSessionId == sessionId)
            .ToListAsync();

        foreach (var set in sets)
        {
            await Database.DeleteAsync(set);
        }

        var session = await GetWorkoutSessionAsync(sessionId);

        if (session is not null)
        {
            await Database.DeleteAsync(session);
        }
    }
    public async Task<List<WorkoutSet>>
    GetWorkoutSetsForSessionAsync(int sessionId)
    {
        await InitAsync();

        return await Database
            .Table<WorkoutSet>()
            .Where(set =>
                set.WorkoutSessionId == sessionId)
            .OrderBy(set => set.ExerciseId)
            .ThenBy(set => set.SetNumber)
            .ThenBy(set => set.Side)
            .ToListAsync();
    }

    public async Task<WorkoutSet?> GetPreviousWorkoutSetAsync(
        int exerciseId,
        int setNumber,
        int currentSessionId,
        string side = "")
    {
        await InitAsync();

        var previousSets = await Database.QueryAsync<WorkoutSet>(
            """
            SELECT workoutSet.Id,
                   workoutSet.WorkoutSessionId,
                   workoutSet.ExerciseId,
                   workoutSet.SetNumber,
                   workoutSet.Side,
                   workoutSet.Weight,
                   workoutSet.Reps
            FROM WorkoutSet AS workoutSet
            INNER JOIN WorkoutSession AS session
                ON session.Id = workoutSet.WorkoutSessionId
            WHERE session.IsCompleted = 1
              AND session.Id <> ?
              AND workoutSet.ExerciseId = ?
              AND workoutSet.SetNumber = ?
              AND workoutSet.Side = ?
            ORDER BY session.Date DESC, session.Id DESC
            LIMIT 1
            """,
            currentSessionId,
            exerciseId,
            setNumber,
            side);

        return previousSets.FirstOrDefault();
    }

    public async Task<List<WorkoutSet>> GetPreviousWorkoutSetsAsync(
        int currentSessionId)
    {
        await InitAsync();

        return await Database.QueryAsync<WorkoutSet>(
            """
            SELECT Id,
                   WorkoutSessionId,
                   ExerciseId,
                   SetNumber,
                   Side,
                   Weight,
                   Reps
            FROM (
                SELECT workoutSet.Id,
                       workoutSet.WorkoutSessionId,
                       workoutSet.ExerciseId,
                       workoutSet.SetNumber,
                       workoutSet.Side,
                       workoutSet.Weight,
                       workoutSet.Reps,
                       ROW_NUMBER() OVER (
                           PARTITION BY workoutSet.ExerciseId,
                                        workoutSet.SetNumber,
                                        workoutSet.Side
                           ORDER BY session.Date DESC, session.Id DESC
                       ) AS performanceOrder
                FROM WorkoutSet AS workoutSet
                INNER JOIN WorkoutSession AS session
                    ON session.Id = workoutSet.WorkoutSessionId
                WHERE session.IsCompleted = 1
                  AND session.Id <> ?
            )
            WHERE performanceOrder = 1
            """,
            currentSessionId);
    }

    public async Task<int> DeleteWorkoutSetAsync(WorkoutSet set)
    {
        await InitAsync();

        return await Database.DeleteAsync(set);
    }
    private async Task SeedDay3Async(int workoutDayId)
    {
        var items = new[]
        {
        ("SLDL", 1),
        ("45D T Bar Row", 2),
        ("Incline Chest Press Machine", 2),
        ("Leg Extenstion", 2),
        ("SA Y Raises", 2),
        ("Face Away Curl", 1),
        ("Overhead Extenstion", 1),
        ("Hip Adduction", 1),
        ("Leg Press Calf Raises", 2),
        ("Wrist Curls", 1)
    };

        var order = 1;

        foreach (var item in items)
        {
            var exerciseId =
                await GetOrCreateExerciseIdAsync(item.Item1);

            await Database.InsertAsync(new ProgramExercise
            {
                WorkoutDayId = workoutDayId,
                ExerciseId = exerciseId,
                PlannedSets = item.Item2,
                RestMinutes = 3,
                Order = order++
            });
        }
    }
    public async Task<WorkoutProgram?> GetWorkoutProgramAsync(int programId)
    {
        await InitAsync();

        return await Database.Table<WorkoutProgram>()
            .FirstOrDefaultAsync(program => program.Id == programId);
    }
    private async Task<int> GetOrCreateExerciseIdAsync(string name)
    {
        var exercise = await Database.Table<Exercise>()
            .FirstOrDefaultAsync(x => x.Name == name);

        if (exercise != null)
            return exercise.Id;

        exercise = new Exercise
        {
            Name = name
        };

        await Database.InsertAsync(exercise);

        return exercise.Id;
    }

    public async Task<List<ProgramExercise>> GetProgramExercisesAsync(int workoutDayId)
    {
        await InitAsync();

        return await Database.Table<ProgramExercise>()
            .Where(item => item.WorkoutDayId == workoutDayId)
            .OrderBy(item => item.Order)
            .ToListAsync();
    }

    public async Task<int> AddExerciseToDayAsync(ProgramExercise programExercise)
    {
        await InitAsync();

        return await Database.InsertAsync(programExercise);
    }

    public async Task<int> UpdateProgramExerciseAsync(ProgramExercise programExercise)
    {
        await InitAsync();

        return await Database.UpdateAsync(programExercise);
    }

    public async Task<int> RemoveExerciseFromDayAsync(ProgramExercise programExercise)
    {
        await InitAsync();

        return await Database.DeleteAsync(programExercise);
    }

    public async Task<Exercise?> GetExerciseAsync(int exerciseId)
    {
        await InitAsync();

        return await Database.Table<Exercise>()
            .FirstOrDefaultAsync(exercise => exercise.Id == exerciseId);
    }
    public async Task MoveProgramExerciseUpAsync(ProgramExercise item)
    {
        await InitAsync();

        var items = await GetProgramExercisesAsync(item.WorkoutDayId);

        var previousItem = items
            .Where(x => x.Order < item.Order)
            .OrderByDescending(x => x.Order)
            .FirstOrDefault();

        if (previousItem is null)
            return;

        var oldOrder = item.Order;

        item.Order = previousItem.Order;
        previousItem.Order = oldOrder;

        await Database.UpdateAsync(previousItem);
        await Database.UpdateAsync(item);
    }

    public async Task MoveProgramExerciseDownAsync(ProgramExercise item)
    {
        await InitAsync();

        var items = await GetProgramExercisesAsync(item.WorkoutDayId);

        var nextItem = items
            .Where(x => x.Order > item.Order)
            .OrderBy(x => x.Order)
            .FirstOrDefault();

        if (nextItem is null)
            return;

        var oldOrder = item.Order;

        item.Order = nextItem.Order;
        nextItem.Order = oldOrder;

        await Database.UpdateAsync(nextItem);
        await Database.UpdateAsync(item);
    }

    public async Task RemoveExerciseFromDayAndReorderAsync(
        ProgramExercise item)
    {
        await InitAsync();

        await Database.DeleteAsync(item);

        var remainingItems =
            await GetProgramExercisesAsync(item.WorkoutDayId);

        for (var index = 0; index < remainingItems.Count; index++)
        {
            remainingItems[index].Order = index + 1;
            await Database.UpdateAsync(remainingItems[index]);
        }
    }
    public async Task<WorkoutDay?> GetWorkoutDayAsync(int workoutDayId)
    {
        await InitAsync();

        return await Database.Table<WorkoutDay>()
            .FirstOrDefaultAsync(day => day.Id == workoutDayId);
    }

    public async Task<int> SaveWorkoutSessionAsync(WorkoutSession session)
    {
        await InitAsync();

        if (session.Id == 0)
            return await Database.InsertAsync(session);

        return await Database.UpdateAsync(session);
    }

    public async Task<int> SaveWorkoutSetAsync(WorkoutSet workoutSet)
    {
        await InitAsync();

        if (workoutSet.Id == 0)
            return await Database.InsertAsync(workoutSet);

        return await Database.UpdateAsync(workoutSet);
    }
    public async Task<List<WorkoutSession>> GetWorkoutSessionsAsync()
    {
        await InitAsync();

        return await Database.Table<WorkoutSession>()
            .OrderByDescending(session => session.Date)
            .ToListAsync();
    }

    public async Task<WorkoutSession?> GetWorkoutSessionAsync(int sessionId)
    {
        await InitAsync();

        return await Database.Table<WorkoutSession>()
            .FirstOrDefaultAsync(session => session.Id == sessionId);
    }
    public async Task SeedFixedWorkoutPlanAsync()
    {
        await InitAsync();

        var existingDays = await Database.Table<WorkoutDay>().ToListAsync();

        if (existingDays.Any())
            return;

        var fixedProgram = new WorkoutProgram
        {
            Name = "Belghamdi Full Body",
            CreatedAt = DateTime.Now
        };

        await Database.InsertAsync(fixedProgram);

        var day1 = new WorkoutDay
        {
            WorkoutProgramId = fixedProgram.Id,
            DayNumber = 1,
            Name = "Full Body #1"
        };

        var day2 = new WorkoutDay
        {
            WorkoutProgramId = fixedProgram.Id,
            DayNumber = 2,
            Name = "Full Body #2"
        };

        var day3 = new WorkoutDay
        {
            WorkoutProgramId = fixedProgram.Id,
            DayNumber = 3,
            Name = "Full Body #3"
        };

        await Database.InsertAsync(day1);
        await Database.InsertAsync(day2);
        await Database.InsertAsync(day3);

        await SeedDay1Async(day1.Id);
        await SeedDay2Async(day2.Id);
        await SeedDay3Async(day3.Id);
    }
    public async Task<List<WorkoutSet>> GetWorkoutSetsAsync(int sessionId)
    {
        await InitAsync();

        return await Database.Table<WorkoutSet>()
            .Where(set => set.WorkoutSessionId == sessionId)
            .OrderBy(set => set.ExerciseId)
            .ThenBy(set => set.SetNumber)
            .ThenBy(set => set.Side)
            .ToListAsync();
    }
    public async Task<List<WorkoutSet>> GetWorkoutSetsForExerciseAsync(
    int exerciseId)
    {
        await InitAsync();

        return await Database.Table<WorkoutSet>()
            .Where(set => set.ExerciseId == exerciseId)
            .ToListAsync();
    }
}
