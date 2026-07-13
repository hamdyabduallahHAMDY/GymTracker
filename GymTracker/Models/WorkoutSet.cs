using SQLite;

namespace GymTracker.Models;

public class WorkoutSet
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int WorkoutSessionId { get; set; }

    public int ExerciseId { get; set; }

    public int SetNumber { get; set; }

    public double Weight { get; set; }

    public int Reps { get; set; }
}