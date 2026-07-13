using SQLite;

namespace GymTracker.Models;

public class WorkoutSession
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int WorkoutDayId { get; set; }

    public DateTime Date { get; set; } = DateTime.Now;

    public TimeSpan Duration { get; set; }
}