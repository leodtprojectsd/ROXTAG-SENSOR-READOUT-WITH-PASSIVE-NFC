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

using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TAdherence.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HtmlPopup : PopupPage
    {
        public HtmlPopup(string htmlFile)
        {
            InitializeComponent();

            string baseUrl = DependencyService.Get<Interfaces.IBaseUrl>().Get();
            webView.Source = new UrlWebViewSource
            {
                Url = System.IO.Path.Combine(baseUrl, htmlFile)
            };
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync();
        }

        protected override bool OnBackgroundClicked()
        {
            return false;
        }   

    }
}
