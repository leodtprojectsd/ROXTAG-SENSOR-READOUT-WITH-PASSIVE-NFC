/*
 * Copyright 2019-2020 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Plugin.CurrentActivity;
using Xamarin.Forms;

namespace TLogger.Droid
{
    [Activity(
        Label = "TLogger",
        Icon = "@drawable/app_icon_tlogger",
        Theme = "@style/splashscreen",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        ScreenOrientation = ScreenOrientation.Portrait)]
    [Obsolete("Cause it aint", false)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.SetTheme(Resource.Style.MainTheme);
            base.OnCreate(savedInstanceState);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironment_UnhandledExceptionRaiser;

            CrossCurrentActivity.Current.Init(this, savedInstanceState);
            MessagingCenter.Subscribe<Page>(this, Plugin.Ndef.MessagingCenterMessages.Android.DisableDrawingCache, (page) =>
            {
                // Disable drawing cache. Next call to Screenshot CaptureAsync will enable it again and capture a new screen shot.
                CrossCurrentActivity.Current.Activity.Window.DecorView.DrawingCacheEnabled = false;
            });

            Msg.Board.BoardType = Msg.Board.EBoardType.TLogger;
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Rg.Plugins.Popup.Popup.Init(this, savedInstanceState);

            Forms.Init(this, savedInstanceState);
            LoadApplication(new App());

            // Android requires the following MainActivity actions
            // for NFC operations:
            //      - Adaper detection on OnCreate
            //      - Enable foreground dispatch on OnResume if adapter is available
            //      - Disable foreground dispatch on OnPause if adapter is available
            //      - Detected NDEF message will be delivered to OnNewIntent
            // The following code should be added to provide messages to
            // NFC Android implementation.
            MessagingCenter.Send<Xamarin.Forms.Platform.Android.FormsAppCompatActivity>(this, Plugin.Ndef.MessagingCenterMessages.Android.OnCreate);

            // Register adapter state change listener.
            // This will generate NFC service enable/disable events.
            RegisterReceiver(new AdapterStateActionBroadcastReceiver(), new Android.Content.IntentFilter(Android.Nfc.NfcAdapter.ActionAdapterStateChanged));
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("CurrentDomain Unhandled Exception: " + (e.ExceptionObject as Exception).Message);
            Helpers.ExceptionLogHelper.Log("TaskScheduler UnobservedTask exception", (e.ExceptionObject as Exception).Message);
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("TaskScheduler UnobservedTask Exception: " + e.Exception.Message);
            Helpers.ExceptionLogHelper.Log("TaskScheduler UnobservedTask exception", e.Exception.Message);
        }

        private void AndroidEnvironment_UnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("AndroidEnvironment Unhandled exception: " + e.Exception.Message);
            Helpers.ExceptionLogHelper.Log("AndroidEnvironment Unhandled exception", e.Exception.Message);
        }

        protected override void OnResume()
        {
            base.OnResume();
            MessagingCenter.Send<Xamarin.Forms.Platform.Android.FormsAppCompatActivity>(this, Plugin.Ndef.MessagingCenterMessages.Android.OnResume);
        }

        protected override void OnPause()
        {
            base.OnPause();
            MessagingCenter.Send<Xamarin.Forms.Platform.Android.FormsAppCompatActivity>(this, Plugin.Ndef.MessagingCenterMessages.Android.OnPause);
        }

        protected override void OnNewIntent(Android.Content.Intent intent)
        {
            base.OnNewIntent(intent);
            try
            {
                MessagingCenter.Send<Xamarin.Forms.Platform.Android.FormsAppCompatActivity, Android.Content.Intent>(this, Plugin.Ndef.MessagingCenterMessages.Android.OnNewIntent, intent);
            }
            catch (Exception ex)
            {
                Helpers.ExceptionLogHelper.Log(App.MsgLib.TagModel.TagId, ex.Message);
            }
        }

        public void OnNewAdapterState(int newState)
        {
            try
            {
                MessagingCenter.Send<Xamarin.Forms.Platform.Android.FormsAppCompatActivity, int>(this, Plugin.Ndef.MessagingCenterMessages.Android.OnNewAdapterState, newState);
            }
            catch (Exception ex)
            {
                Helpers.ExceptionLogHelper.Log(App.MsgLib.TagModel.TagId, ex.Message);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }

    [Android.Content.BroadcastReceiver]
    public class AdapterStateActionBroadcastReceiver : Android.Content.BroadcastReceiver
    {
        public override void OnReceive(Android.Content.Context context, Android.Content.Intent intent)
        {
            (context as MainActivity).OnNewAdapterState(intent.Extras.GetInt(Android.Nfc.NfcAdapter.ExtraAdapterState));
        }
    }
}
