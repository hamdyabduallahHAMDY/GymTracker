using GymTracker.Data;

namespace GymTracker.Services;

public class DatabaseService
{
    public AppDatabase Database { get; }

    public DatabaseService(AppDatabase database)
    {
        Database = database;
    }

    public async Task InitializeAsync()
    {
        await Database.InitAsync();
    }
}