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

namespace TLogger.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AcceptPageView : ContentPage
	{
		public AcceptPageView()
		{
			InitializeComponent();

            var baseUrl = DependencyService.Get<Interfaces.IBaseUrl>().Get();
            acceptWebView.Source = new UrlWebViewSource
            {
                Url = System.IO.Path.Combine(baseUrl, "accept.html")
            };

        }

        void OnAcceptButton(object sender, EventArgs e)
        {
            App.AppSettingsService.IsLicenseAccepted = true;
            App.AppSettingsService.AppVersion = AppInfo.VersionString;

            if (Device.RuntimePlatform == Device.iOS)
                Task.Run(async () => await App.MsgLib.InitAsync());
            Application.Current.MainPage = new NavigationPage(new MainPageView())
            {
                BarBackgroundColor = Device.RuntimePlatform == Device.iOS ?
                    Color.FromHex("#F9F9F9") : Color.FromHex("#F0F0F0"),
                ////BarTextColor = Color.FromHex("#E8B410")
            };
        }

        void OnDeclineButton(object sender, EventArgs e)
        {
            App.AppSettingsService.IsLicenseAccepted = false;
            App.AppSettingsService.AppVersion = string.Empty;
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}
