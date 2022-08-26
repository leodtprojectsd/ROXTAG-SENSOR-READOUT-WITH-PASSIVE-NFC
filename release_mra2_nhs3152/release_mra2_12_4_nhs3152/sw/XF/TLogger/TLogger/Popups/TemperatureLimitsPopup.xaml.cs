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
    public partial class TemperatureLimitsPopup : PopupPage
    {
        public struct STemperatureLimits
        {
            public bool isOk; // true if OK, false if Cancel button is tapped
            public int min;         
            public int max;
        }

        STemperatureLimits _temperatureLimits = new STemperatureLimits();
        TaskCompletionSource<STemperatureLimits> _taskCompletion;

        public TemperatureLimitsPopup(int min, int max, string unit, TaskCompletionSource<STemperatureLimits> taskCompletion)
        {
            InitializeComponent();
            _taskCompletion = taskCompletion;

            Unit.SelectedItem = unit;
            Min.Text = min.ToString();
            Max.Text = max.ToString();
            _temperatureLimits.min = min;
            _temperatureLimits.max = max;

            if (Device.RuntimePlatform == Device.iOS)
            {
                /* iOS keyboards don't have a minus sign. */
                Min.Keyboard = Keyboard.Plain;
                Max.Keyboard = Keyboard.Plain;
            }

            temperatureIcon.Source = ImageSource.FromResource("TLogger.Images.temperature.png");
        }

        protected override bool OnBackgroundClicked()
        {
            return false;
        }

        async void OnOkButton(object sender, EventArgs e)
        {
            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAsync(true);

            _temperatureLimits.isOk = int.TryParse(Min.Text, out _temperatureLimits.min);
            _temperatureLimits.isOk &= int.TryParse(Max.Text, out _temperatureLimits.max);
            if (_temperatureLimits.min > _temperatureLimits.max)
            {
                int swap = _temperatureLimits.min;
                _temperatureLimits.min = _temperatureLimits.max;
                _temperatureLimits.max = swap;
            }
            _taskCompletion?.SetResult(_temperatureLimits);
        }

        async void OnCancelButton(object sender, EventArgs e)
        {
            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAsync(true);

            _temperatureLimits.isOk = false;
            _temperatureLimits.min = 0;
            _temperatureLimits.max = 0;
            _taskCompletion?.SetResult(_temperatureLimits);
        }
    }
}
