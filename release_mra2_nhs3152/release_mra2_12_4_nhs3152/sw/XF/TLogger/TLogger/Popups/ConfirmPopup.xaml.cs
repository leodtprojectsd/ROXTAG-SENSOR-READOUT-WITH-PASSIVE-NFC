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

using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TLogger.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConfirmPopup : PopupPage
    {
        TaskCompletionSource<bool> _taskCompletion;

        public ConfirmPopup(ImageSource imageSource, string title, string info, 
            TaskCompletionSource<bool> taskCompletion)
        {
            InitializeComponent();

            _taskCompletion = taskCompletion;

            ////            icon.Source = imageSource;
            this.title.Text = title;
            this.info.Text = info;
        }

        protected override bool OnBackgroundClicked()
        {
            return false;
        }

        async void OnOkButton(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync(true);
            _taskCompletion?.SetResult(true);
        }

        async void OnCancelButton(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync(true);
            _taskCompletion?.SetResult(false);
        }

    }
}
