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
    public partial class MeasurementIntervalPopup : PopupPage
    {
        public struct SMeasurementInterval
        {
            public bool isOk; // true if OK, false if Cancel button is tapped
            public TimeSpan timeSpan; // measurement startup delay time    
        }

        SMeasurementInterval _measurementInterval = new SMeasurementInterval();
        TaskCompletionSource<SMeasurementInterval> _taskCompletion;

        public MeasurementIntervalPopup(TimeSpan timeSpan, TaskCompletionSource<SMeasurementInterval> taskCompletion)
        {
            InitializeComponent();
            _taskCompletion = taskCompletion;

            _measurementInterval.timeSpan = timeSpan;
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

            icon.Source = ImageSource.FromResource("TLogger.Images.Interval Measure.png");
        }

        protected override bool OnBackgroundClicked()
        {
            return false;
        }

        async void OnOkButton(object sender, EventArgs e)
        {
            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAsync(true);

            _measurementInterval.isOk = int.TryParse(Value.Text, out int value);
            value = Math.Max(1, value);
            string unit = Unit.SelectedItem as string;
            if (unit == "hours")
            {
                _measurementInterval.timeSpan = TimeSpan.FromHours(value);
            }
            else if (unit == "minutes")
            {
                _measurementInterval.timeSpan = TimeSpan.FromMinutes(value);
            }
            else
            {
                _measurementInterval.timeSpan = TimeSpan.FromSeconds(value);
            }
            _taskCompletion?.SetResult(_measurementInterval);
        }

        async void OnCancelButton(object sender, EventArgs e)
        {
            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAsync(true);

            _measurementInterval.isOk = false;
            _measurementInterval.timeSpan = TimeSpan.Zero;
            _taskCompletion?.SetResult(_measurementInterval);
        }
    }
}
