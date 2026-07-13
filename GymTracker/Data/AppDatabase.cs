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
        await _database.CreateTableAsync<ProgramExercise>();
        await _database.CreateTableAsync<WorkoutSession>();
        await _database.CreateTableAsync<WorkoutSet>();
        await _database.CreateTableAsync<PersonalRecord>();
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
    public async Task<WorkoutProgram?> GetWorkoutProgramAsync(int programId)
    {
        await InitAsync();

        return await Database.Table<WorkoutProgram>()
            .FirstOrDefaultAsync(program => program.Id == programId);
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

    public async Task<List<WorkoutSet>> GetWorkoutSetsAsync(int sessionId)
    {
        await InitAsync();

        return await Database.Table<WorkoutSet>()
            .Where(set => set.WorkoutSessionId == sessionId)
            .OrderBy(set => set.ExerciseId)
            .ThenBy(set => set.SetNumber)
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