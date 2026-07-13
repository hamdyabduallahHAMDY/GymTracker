using GymTracker.Services;

namespace GymTracker;

public partial class App : Application
{
    public App(DatabaseService databaseService)
    {
        InitializeComponent();

        _ = databaseService.InitializeAsync();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new MainPage())
        {
            Title = "GymTracker"
        };
    }
}