using SQLite;

namespace GymTracker.Models;

public class WorkoutDay
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int WorkoutProgramId { get; set; }

    public int DayNumber { get; set; }

    [NotNull]
    public string Name { get; set; } = string.Empty;
}