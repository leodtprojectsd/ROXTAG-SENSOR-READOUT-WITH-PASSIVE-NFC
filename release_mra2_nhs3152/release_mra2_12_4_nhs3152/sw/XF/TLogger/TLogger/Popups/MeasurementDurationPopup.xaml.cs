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
    public partial class MeasurementDurationPopup : PopupPage
    {
        public struct SMeasurementDuration
        {
            public bool isOk; // true if OK, false if Cancel button is tapped
            public TimeSpan timeSpan;  // measurement startup delay time    
        }

        SMeasurementDuration _measurementDuration = new SMeasurementDuration();
        TaskCompletionSource<SMeasurementDuration> _taskCompletion;

        public MeasurementDurationPopup(TimeSpan timeSpan, TaskCompletionSource<SMeasurementDuration> taskCompletion)
        {
            InitializeComponent();
            _taskCompletion = taskCompletion;

            _measurementDuration.timeSpan = timeSpan;
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

            icon.Source = ImageSource.FromResource("TLogger.Images.Stop Measure.png");
        }

        protected override bool OnBackgroundClicked()
        {
            return false;
        }

        async void OnOkButton(object sender, EventArgs e)
        {
            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAsync(true);

            _measurementDuration.isOk = int.TryParse(Value.Text, out int value);
            string unit = Unit.SelectedItem as string;
            if (unit == "hours")
            {
                _measurementDuration.timeSpan = TimeSpan.FromHours(value);
            }
            else if (unit == "minutes")
            {
                _measurementDuration.timeSpan = TimeSpan.FromMinutes(value);
            }
            else
            {
                _measurementDuration.timeSpan = TimeSpan.FromSeconds(value);
            }
            _taskCompletion?.SetResult(_measurementDuration);
        }

        async void OnCancelButton(object sender, EventArgs e)
        {
            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAsync(true);

            _measurementDuration.isOk = false;
            _measurementDuration.timeSpan = TimeSpan.Zero;
            _taskCompletion?.SetResult(_measurementDuration);
        }

        async void OnFullButton(object sender, EventArgs e)
        {
            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAsync(true);

            _measurementDuration.isOk = true;
            _measurementDuration.timeSpan = TimeSpan.Zero;
            _taskCompletion?.SetResult(_measurementDuration);
        }
    }
}
