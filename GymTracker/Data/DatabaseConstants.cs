namespace GymTracker.Data;

public static class DatabaseConstants
{
    public const string DatabaseFilename = "GymTrackerV2.db3";
    public static string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
}