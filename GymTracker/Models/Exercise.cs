using SQLite;

namespace GymTracker.Models;

public class Exercise
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]

    public string Name { get; set; } = string.Empty;
}