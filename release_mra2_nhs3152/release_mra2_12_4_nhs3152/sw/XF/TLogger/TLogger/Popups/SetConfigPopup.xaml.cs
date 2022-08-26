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
using Rg.Plugins.Popup.Services;
using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TLogger.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SetConfigPopup : PopupPage
    {
        Msg.Lib _msgLib;

        bool _isStarted;
        bool _isReset;
        bool _isIosTagActive;
        Msg.Models.MeasurementConfigModel.CConfig _measurementConfig = new Msg.Models.MeasurementConfigModel.CConfig();
        Msg.Models.TemperatureConfigModel.CConfig _temperatureConfig = new Msg.Models.TemperatureConfigModel.CConfig();

        public SetConfigPopup(
            bool isReset,
            Msg.Models.MeasurementConfigModel.CConfig measurementConfig,
            Msg.Models.TemperatureConfigModel.CConfig temperatureConfig)
        {
            InitializeComponent();

            _msgLib = App.MsgLib;
            Msg.Lib.IsConfiguringTag = true;
            _isStarted = false;
            _isReset = isReset;
            _measurementConfig = measurementConfig;
            _temperatureConfig = temperatureConfig;

            // Msg lib bindings.
            _msgLib.TagModel.PropertyChanged += TagStatusModel_PropertyChanged;
            if (Device.RuntimePlatform == Device.iOS)
            {
                iosNfc.IsVisible = true;
                _msgLib.IosNfcDisabled += _msgLib_IosNfcDisabled;
            }

            TagStatusModel_PropertyChanged(this, new PropertyChangedEventArgs(nameof(Msg.Models.TagStatusModel.IsTagConnected)));
        }

        private void _msgLib_IosNfcDisabled(object sender, EventArgs e)
        {
            if (_isReset)
                Device.BeginInvokeOnMainThread(() => { Info.Text = "Hit Enable NFC and then tap the tag to reset..."; });
            else
                Device.BeginInvokeOnMainThread(() => { Info.Text = "Hit Enable NFC and then tap the tag to configure..."; });

            _isIosTagActive = false;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (Device.RuntimePlatform == Device.iOS)
            {
                _isIosTagActive = true;
                await App.MsgLib.IosInitTagReaderAsync();
            }
        }

        protected override bool OnBackgroundClicked()
        {
            return false; 
        }

        async void OnIosNfcButton(object sender, EventArgs e)
        {
            if (_isReset)
                Device.BeginInvokeOnMainThread(() => { Info.Text = "Tap the tag to reset..."; });
            else
                Device.BeginInvokeOnMainThread(() => { Info.Text = "Tap the tag to configure..."; });
            _isIosTagActive = true;
            await App.MsgLib.IosInitTagReaderAsync();
        }

        async void OnCancelButton(object sender, EventArgs e)
        {
            Msg.Lib.IsConfiguringTag = false;

            _msgLib.TagModel.PropertyChanged -= TagStatusModel_PropertyChanged;
            _msgLib.IosNfcDisabled -= _msgLib_IosNfcDisabled;
            try
            {
                await PopupNavigation.Instance.PopAsync(true);
                if (Device.RuntimePlatform == Device.iOS && _isIosTagActive)
                {
                    _isIosTagActive = false;
                    await App.MsgLib.IosDeInitTagReaderAsync();
                }
            }
            catch (Exception ex)
            {
                Helpers.ExceptionLogHelper.Log(_msgLib.TagModel.TagId, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
            }

            if (button.Text == "OK")
            { 
                _msgLib.ResetModels(true);
            }
        }

        async void TagStatusModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_msgLib.TagModel.IsTagConnected))
            {
                if (_msgLib.TagModel.IsTagConnected)
                {
                    // Tag connected.
                    Device.BeginInvokeOnMainThread(() => { Info.Text = "Executing..."; });
                    // Status ConfigTime will be constructed using the configTime epoch
                    // from device. The format is different when we construct the DateTime from epoch.
                    // So we do the same here to have same DateTime format for both 
                    // CurrentTime and ConfigTime.
                    var currentTime = Convert.ToUInt32(
                        DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
                    _measurementConfig.CurrentTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(currentTime);

                    if (await Msg.Lib.CommandHandler.SetConfigCmdAsync(
                        _measurementConfig,
                        _temperatureConfig))
                    {
                        if (Device.RuntimePlatform == Device.iOS && _isIosTagActive)
                        {
                            _isIosTagActive = false;
                            await App.MsgLib.IosDeInitTagReaderAsync();
                        }

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            if(Device.RuntimePlatform == Device.iOS)
                            {
                                iosNfc.IsVisible = false;
                            }
                            button.Text = "OK";
                            if(_isReset)
                                Info.Text = "Configuration has been cleared, no measurements will be taken.";
                            else
                                if(_measurementConfig.StartupDelay == TimeSpan.Zero)
                                    Info.Text =  "Configuration has been set, the first measurement has already been taken.";
                                else
                                    Info.Text =  "Configuration has been set, the first measurement will be taken after " +
                                        Helpers.TimeSpanHelper.ReadableTimeSpan(_measurementConfig.StartupDelay, 2) + ".";
                        });
                        _msgLib.TagModel.PropertyChanged -= TagStatusModel_PropertyChanged;
                        _msgLib.IosNfcDisabled -= _msgLib_IosNfcDisabled;

                        _msgLib.SetConfigEvent?.Invoke(this, new Msg.Lib.SetConfigEventArgs
                        {
                            MeasurementConfig = _measurementConfig,
                            TemperatureConfig = _temperatureConfig,
                            HumidityConfig = null,
                            AccelerometerConfig = null,
                        });
                        return;
                    }

                    if (Device.RuntimePlatform == Device.iOS)
                    {
                        if (_isIosTagActive)
                        {
                            await App.MsgLib.IosDeInitTagReaderAsync();
                        }
                        _isIosTagActive = true;
                        await App.MsgLib.IosInitTagReaderAsync();

                    }
                    Device.BeginInvokeOnMainThread(() => { Info.Text = "Write failed, tap again..."; });
                }
                else
                {
                    // Tag disconnected.
                    if (!_isStarted)
                    {
                        _isStarted = true;
                        if (_isReset)
                            Device.BeginInvokeOnMainThread(() => { Info.Text = "Tap the tag to reset..."; });
                        else
                            Device.BeginInvokeOnMainThread(() => { Info.Text = "Tap the tag to configure..."; });
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("tag disconnected");
                    }
                }
            }
        }
    }
}
