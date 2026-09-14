using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Media;
using Android.OS;
using AndroidX.Core.App;

namespace GymTracker.Platforms.Android;

[BroadcastReceiver(
    Enabled = true,
    Exported = false)]
public class RestAlarmReceiver : BroadcastReceiver
{
    public override void OnReceive(
        Context? context,
        Intent? intent)
    {
        if (context is null)
            return;

        var pendingResult = GoAsync();

        _ = HandleAlarmAsync(
            context,
            pendingResult);
    }
    private async Task HandleAlarmAsync(
        Context context,
        PendingResult pendingResult)
    {
        MediaPlayer? player = null;
        AssetFileDescriptor? descriptor = null;

        try
        {
            ShowNotification(context);

            Vibrate(context);

            System.Diagnostics.Debug.WriteLine(
                "Trying to play alarm.mp3...");

            descriptor =
                context.Assets?.OpenFd(
                    "alarm.mp3");

            if (descriptor is null)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Could not open alarm.mp3");

                return;
            }

            player = new MediaPlayer();

            player.SetDataSource(
                descriptor.FileDescriptor,
                descriptor.StartOffset,
                descriptor.Length);

            var attributes =
                new AudioAttributes.Builder()
                    .SetUsage(
                        AudioUsageKind.NotificationEvent)
                    .SetContentType(
                        AudioContentType.Sonification)
                    .Build();

            player.SetAudioAttributes(
                attributes);

            player.Prepare();

            player.SetVolume(
                1.0f,
                1.0f);

            player.Start();

            System.Diagnostics.Debug.WriteLine(
                "REST ALARM IS PLAYING");

            await Task.Delay(5000);

            if (player.IsPlaying)
            {
                player.Stop();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"REST ALARM ERROR: {ex}");
        }
        finally
        {
            try
            {
                descriptor?.Dispose();

                player?.Release();

                player?.Dispose();
            }
            catch
            {
            }

            pendingResult.Finish();
        }
    }

    private void Vibrate(
        Context context)
    {
        try
        {
            if (Build.VERSION.SdkInt >=
                BuildVersionCodes.S)
            {
                var manager =
                    (VibratorManager?)
                    context.GetSystemService(
                        Context.VibratorManagerService);

                manager?
                    .DefaultVibrator
                    .Vibrate(
                        VibrationEffect.CreateOneShot(
                            1000,
                            VibrationEffect.DefaultAmplitude));
            }
            else
            {
#pragma warning disable CA1422

                var vibrator =
                    (Vibrator?)
                    context.GetSystemService(
                        Context.VibratorService);

                vibrator?.Vibrate(
                    VibrationEffect.CreateOneShot(
                        1000,
                        VibrationEffect.DefaultAmplitude));

#pragma warning restore CA1422
            }
        }
        catch
        {
        }
    }

    private void ShowNotification(
        Context context)
    {
        const string channelId =
            "rest_timer_channel";

        if (Build.VERSION.SdkInt >=
            BuildVersionCodes.O)
        {
            var channel =
                new NotificationChannel(
                    channelId,
                    "Rest Timer",
                    NotificationImportance.High)
                {
                    Description =
                        "Workout rest timer alerts"
                };

            var manager =
                (NotificationManager?)
                context.GetSystemService(
                    Context.NotificationService);

            manager?.CreateNotificationChannel(
                channel);
        }

        var notification =
            new NotificationCompat
                .Builder(
                    context,
                    channelId)
                .SetContentTitle(
                    "Rest finished")
                .SetContentText(
                    "Time for your next set.")
                .SetSmallIcon(
                    global::Android.Resource
                        .Drawable
                        .IcDialogInfo)
                .SetPriority(
                    NotificationCompat.PriorityHigh)
                .SetAutoCancel(true)
                .Build();

        NotificationManagerCompat
            .From(context)
            .Notify(
                1001,
                notification);
    }
}