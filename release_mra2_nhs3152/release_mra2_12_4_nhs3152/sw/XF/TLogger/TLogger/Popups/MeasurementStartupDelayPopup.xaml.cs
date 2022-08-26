/*
 * Copyright 2018-2020 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

using Rg.Plugins.Popup.Pages;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TLogger.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MeasurementStartupDelayPopup : PopupPage
    {
        public struct SMeasurementStartupDelay
        {
            public bool isOk; // true if OK, false if Cancel button is tapped
            public TimeSpan timeSpan;  // measurement startup delay time    
        }

        SMeasurementStartupDelay _measurementStartupDelay = new SMeasurementStartupDelay();
        TaskCompletionSource<SMeasurementStartupDelay> _taskCompletion;

        public MeasurementStartupDelayPopup(TimeSpan timeSpan, TaskCompletionSource<SMeasurementStartupDelay> taskCompletion) 
        {
            InitializeComponent();
            _taskCompletion = taskCompletion;

            _measurementStartupDelay.timeSpan = timeSpan;
            if (timeSpan == TimeSpan.Zero)
            {
                Unit.SelectedItem = "seconds";
                Value.Text = "0";
            }
            else if (timeSpan.Seconds != 0)
            {
                Unit.SelectedItem = "seconds";
                Value.Text = timeSpan.Seconds.ToString();
            }
            else if (timeSpan.Minutes != 0)
            {
                Unit.SelectedItem = "minutes";
                Value.Text = timeSpan.Minutes.ToString();
            }
            else if (timeSpan.Hours != 0)
            {
                Unit.SelectedItem = "hours";
                Value.Text = timeSpan.Hours.ToString();
            }

            icon.Source = ImageSource.FromResource("TLogger.Images.Delay Measure.png");
        }

        protected override bool OnBackgroundClicked()
        {
            return false;
        }

        async void OnOkButton(object sender, EventArgs e)
        {
            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAsync(true);

            _measurementStartupDelay.isOk = int.TryParse(Value.Text, out int value);
            string unit = Unit.SelectedItem as string;
            if (unit == "hours")
            {
                _measurementStartupDelay.timeSpan = TimeSpan.FromHours(value);
            }
            else if (unit == "minutes")
            {
                _measurementStartupDelay.timeSpan = TimeSpan.FromMinutes(value);
            }
            else
            {
                _measurementStartupDelay.timeSpan = TimeSpan.FromSeconds(value);
            }
            _taskCompletion?.SetResult(_measurementStartupDelay);
        }

        async void OnCancelButton(object sender, EventArgs e)
        {
            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAsync(true);

            _measurementStartupDelay.isOk = false;
            _measurementStartupDelay.timeSpan = TimeSpan.Zero;
            _taskCompletion?.SetResult(_measurementStartupDelay);
        }

        async void OnImmediateButton(object sender, EventArgs e)
        {
            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAsync(true);

            _measurementStartupDelay.isOk = true;
            _measurementStartupDelay.timeSpan = TimeSpan.Zero;
            _taskCompletion?.SetResult(_measurementStartupDelay);
        }
    }
}
