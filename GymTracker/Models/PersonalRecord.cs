using SQLite;

namespace GymTracker.Models;

public class PersonalRecord
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int ExerciseId { get; set; }

    public double BestWeight { get; set; }

    public int BestReps { get; set; }

    public DateTime DateAchieved { get; set; }
}