namespace GymTracker.Data;

public static class DatabaseConstants
{
    public const string DatabaseFilename = "GymTracker.db3";

    public static string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
}