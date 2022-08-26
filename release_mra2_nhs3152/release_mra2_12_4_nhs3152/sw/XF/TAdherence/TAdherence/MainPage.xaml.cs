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
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace TAdherence
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        void OnActionButtonClicked(object sender, EventArgs e)
        {
            _ = Task.Run(async () => await App.CheckLicense());
            switch ((sender as Button).CommandParameter as string)
            {
                case "start":
                    Popups.WritePopup startPopup = new Popups.WritePopup(new byte[10] { 87, 0, 0, 0, 0, 0, 10, 0, 0, 0}, new byte[6] { 87, 1, 0, 0, 0, 0 } );
                    _ = Rg.Plugins.Popup.Services.PopupNavigation.Instance.PushAsync(startPopup);
                    break;

                case "stop":
                    Popups.WritePopup stopPopup = new Popups.WritePopup(new byte[2] { 90, 0 }, new byte[6] { 90, 1, 0, 0, 0, 0 });
                    _ = Rg.Plugins.Popup.Services.PopupNavigation.Instance.PushAsync(stopPopup);
                    break;

                case "read":
                default:
                    Popups.ReadPopup readPopup = new Popups.ReadPopup();
                    _ = Rg.Plugins.Popup.Services.PopupNavigation.Instance.PushAsync(readPopup);
                    break;
            }
        }

        void OnCollateralClicked(object sender, EventArgs e)
        {
            switch ((sender as Button).CommandParameter as string)
            {
                case "about":
                    Popups.HtmlPopup aboutPopup = new Popups.HtmlPopup("about.html");
                    _ = Rg.Plugins.Popup.Services.PopupNavigation.Instance.PushAsync(aboutPopup);
                    break;

                case "contact":
                    Popups.HtmlPopup supportPopup = new Popups.HtmlPopup("contact.html");
                    _ = Rg.Plugins.Popup.Services.PopupNavigation.Instance.PushAsync(supportPopup);
                    break;

                case "license":
                default:
                    Popups.HtmlPopup licensePopup = new Popups.HtmlPopup("license.html");
                    _ = Rg.Plugins.Popup.Services.PopupNavigation.Instance.PushAsync(licensePopup);
                    break;
            }
        }
    }
}
