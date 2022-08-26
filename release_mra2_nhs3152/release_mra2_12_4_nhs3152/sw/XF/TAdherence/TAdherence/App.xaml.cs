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

using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace TAdherence
{
    public partial class App : Application
    {
        public static System.IServiceProvider ServiceProvider { get; private set; }

        public App()
        {
            InitializeComponent();

            Plugin.Ndef.INdef _ndef = Plugin.Ndef.CrossNdef.Current;
            if (Device.RuntimePlatform != Device.iOS)
            {
                _ndef.InitTagReaderAsync(OnTagReaderStatus);
                _ndef.TagConnected += OnTagConnected;
                _ndef.TagDisconnected += OnTagDisconnected;
            }
            MainPage = new MainPage();

            if ((Device.RuntimePlatform == Device.Android) || (Device.RuntimePlatform == Device.iOS))
            {
                _ = Task.Run(async () => await CheckLicense());
            }
        }

        public static async Task CheckLicense()
        {
            string acceptedVersion = Preferences.Get("accepted_version", "-");
            if (acceptedVersion != AppInfo.VersionString)
            {
                Popups.AcceptPopup acceptPopup = new Popups.AcceptPopup();
                await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PushAsync(acceptPopup);
                bool accepted = await acceptPopup.GetResultAsync();
                if (accepted)
                {
                    Preferences.Set("accepted_version", AppInfo.VersionString);
                }
                else
                {
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                }
            }
        }

        private void OnTagReaderStatus(object sender, Plugin.Ndef.TagReaderStatusChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"reader: {e.Status.Reader}");
        }

        private void OnTagConnected(object sender, Plugin.Ndef.TagConnectedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"{e.NdefRecords.Count} NDEF records read");
        }

        private void OnTagDisconnected(object sender, Plugin.Ndef.TagDisconnectedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("OnTagDisconnected");
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
