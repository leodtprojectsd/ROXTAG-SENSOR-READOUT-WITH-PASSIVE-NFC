/*
 * Copyright 2019 NXP
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
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace TLogger
{
    public partial class App : Application
    {
        public static Msg.Lib MsgLib { get; } = new Msg.Lib();
        public static Services.AppSettingsService AppSettingsService { get; } = new Services.AppSettingsService();
        public static Services.DatabaseService DatabaseService { get; } = new Services.DatabaseService();
        public static Views.MainPageView MainPageView { get; set; }
        public static Views.HistoryPageView HistoryPageView { get; set; }

        public App()
        {
            InitializeComponent();

            var appVersion = AppInfo.VersionString;

            if (Device.RuntimePlatform != Device.iOS ||
                (AppSettingsService.IsLicenseAccepted && AppSettingsService.AppVersion == appVersion))
                Task.Run(async () => await MsgLib.InitAsync());

            if (AppSettingsService.IsLicenseAccepted && AppSettingsService.AppVersion == appVersion)
            {
                MainPage = new NavigationPage(new Views.MainPageView())
                {
                    BarBackgroundColor = Device.RuntimePlatform == Device.iOS ?
                        Color.FromHex("#F9F9F9") : Color.FromHex("#F0F0F0"),
                    ////BarTextColor = Color.Black,
                };
            }
            else
            {
                MainPage = new Views.AcceptPageView();
            }
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
