using System;
#if __ANDROID_26__
using Android;
using Android.OS;
#endif

namespace Xamarin.Essentials
{
    public static partial class Vibration
    {
        internal static bool IsSupported => true;

        static void PlatformVibrate(TimeSpan duration)
        {
            Permissions.EnsureDeclared(PermissionType.Vibrate);

            var time = (long)duration.TotalMilliseconds;
#if __ANDROID_26__
            if (Platform.HasApiLevelO)
            {
                Platform.Vibrator.Vibrate(VibrationEffect.CreateOneShot(time, VibrationEffect.DefaultAmplitude));
                return;
            }
#endif

#pragma warning disable CS0618 // Type or member is obsolete
            Platform.Vibrator.Vibrate(time);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        static void PlatformCancel()
        {
            Permissions.EnsureDeclared(PermissionType.Vibrate);

            Platform.Vibrator.Cancel();
        }
    }
}
