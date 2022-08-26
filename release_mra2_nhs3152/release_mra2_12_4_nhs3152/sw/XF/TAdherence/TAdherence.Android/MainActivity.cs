/*
 * Copyright 2020 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

using System;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;

namespace TAdherence.Droid
{
    [Activity(
        Label = "TAdherence",
        Icon = "@drawable/app_icon_tadherence",
        Theme = "@style/MainTheme", 
        MainLauncher = true, 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Rg.Plugins.Popup.Popup.Init(this, savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());

            Xamarin.Forms.MessagingCenter.Send<Xamarin.Forms.Platform.Android.FormsAppCompatActivity>(this, Plugin.Ndef.MessagingCenterMessages.Android.OnCreate);
            RegisterReceiver(new AdapterStateActionBroadcastReceiver(), new Android.Content.IntentFilter(Android.Nfc.NfcAdapter.ActionAdapterStateChanged));
            AppDomain.CurrentDomain.UnhandledException += AppDomain_CurrentDomain_UnhandledException;
            System.Threading.Tasks.TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironment_UnhandledExceptionRaiser;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnResume()
        {
            base.OnResume();
            Xamarin.Forms.MessagingCenter.Send<Xamarin.Forms.Platform.Android.FormsAppCompatActivity>(this, Plugin.Ndef.MessagingCenterMessages.Android.OnResume);
        }

        protected override void OnPause()
        {
            base.OnPause();
            Xamarin.Forms.MessagingCenter.Send<Xamarin.Forms.Platform.Android.FormsAppCompatActivity>(this, Plugin.Ndef.MessagingCenterMessages.Android.OnPause);
        }

        public override void OnBackPressed()
        {
            _ = Rg.Plugins.Popup.Popup.SendBackPressed(base.OnBackPressed);
        }

        protected override void OnNewIntent(Android.Content.Intent intent)
        {
            base.OnNewIntent(intent);
            Xamarin.Forms.MessagingCenter.Send<Xamarin.Forms.Platform.Android.FormsAppCompatActivity, Android.Content.Intent>(this, Plugin.Ndef.MessagingCenterMessages.Android.OnNewIntent, intent);
        }

        public void OnNewAdapterState(int newState)
        {
            Xamarin.Forms.MessagingCenter.Send<Xamarin.Forms.Platform.Android.FormsAppCompatActivity, int>(this, Plugin.Ndef.MessagingCenterMessages.Android.OnNewAdapterState, newState);
        }

        static private void AppDomain_CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("AppDomain.CurrentDomain.UnhandledException: " + (e.ExceptionObject as Exception).Message);
        }

        static private void TaskScheduler_UnobservedTaskException(object sender, System.Threading.Tasks.UnobservedTaskExceptionEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("TaskScheduler.UnobservedTaskException: " + e.Exception.Message);
        }

        static private void AndroidEnvironment_UnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("AndroidEnvironment.UnhandledExceptionRaiser: " + e.Exception.Message);
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
