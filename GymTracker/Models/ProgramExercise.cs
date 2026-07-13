using SQLite;

namespace GymTracker.Models;

public class ProgramExercise
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int WorkoutDayId { get; set; }

    public int ExerciseId { get; set; }

    public int Order { get; set; }

    public int PlannedSets { get; set; }
}