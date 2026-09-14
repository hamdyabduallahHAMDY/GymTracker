using Android.App;
using Android.Content;

namespace GymTracker.Platforms.Android;

public class AndroidRestAlarmService
{
    private const int RestAlarmRequestCode =
        5001;

    public void ScheduleAlarm(
        DateTime alarmTime)
    {
        var context =
            global::Android.App
                .Application.Context;

        var alarmManager =
            (AlarmManager?)
            context.GetSystemService(
                Context.AlarmService);

        if (alarmManager is null)
            return;

        var intent =
            new Intent(
                context,
                typeof(RestAlarmReceiver));

        var pendingIntent =
            PendingIntent.GetBroadcast(
                context,
                RestAlarmRequestCode,
                intent,
                PendingIntentFlags.UpdateCurrent
                |
                PendingIntentFlags.Immutable);

        var triggerMilliseconds =
            new DateTimeOffset(alarmTime)
                .ToUnixTimeMilliseconds();

        try
        {
            alarmManager.SetExactAndAllowWhileIdle(
                AlarmType.RtcWakeup,
                triggerMilliseconds,
                pendingIntent);
        }
        catch
        {
            alarmManager.Set(
                AlarmType.RtcWakeup,
                triggerMilliseconds,
                pendingIntent);
        }
    }

    public void CancelAlarm()
    {
        var context =
            global::Android.App
                .Application.Context;

        var alarmManager =
            (AlarmManager?)
            context.GetSystemService(
                Context.AlarmService);

        if (alarmManager is null)
            return;

        var intent =
            new Intent(
                context,
                typeof(RestAlarmReceiver));

        var pendingIntent =
            PendingIntent.GetBroadcast(
                context,
                RestAlarmRequestCode,
                intent,
                PendingIntentFlags.UpdateCurrent
                |
                PendingIntentFlags.Immutable);

        alarmManager.Cancel(
            pendingIntent);

        pendingIntent.Cancel();
    }
}   