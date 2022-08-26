/*
 * Copyright 2019-2020 NXP
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MvvmHelpers;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace TLogger.ViewModels
{
    internal class TemperatureStatusViewModel : ObservableObject
    {
        private readonly Msg.Lib _msgLib;
        private const string STATUS_LABEL_MESSAGE_NOT_SUPPORTED =
            "Firmware not supported. Please upgrade and try again";
        private const string STATUS_LABEL_MESSAGE_DEVICE_DISABLED =
            "Invalid NDEF message";
        private const string STATUS_LABEL_MESSAGE_DEVICE_PRISTINE_STATE =
            "In Pristine State";
        private const string STATUS_LABEL_MESSAGE_DEVICE_RECORDING =
            "The tag is configured and is logging.";
        private const string STATUS_LABEL_MESSAGE_DEVICE_STOPPED_BATTERY_DIED =
            "Stopped as battery died";
        private const string STATUS_LABEL_MESSAGE_DEVICE_STOPPED_STORAGE_FULL =
            "Stopped as storage is full";
        private const string STATUS_LABEL_MESSAGE_DEVICE_STOPPED_DEMO_EXPIRED =
            "Stopped as configured running time has expired";
        private const string STATUS_LABEL_MESSAGE_DEVICE_UNSUPPORTED =
            "Unsupported tag, please tap an NTAG SmartSensor.";
        private const string STATUS_APP_MSG_EVENT_EXPIRED =
            "Logging was stopped after the configured running time.";
        private const string STATUS_APP_MSG_EVENT_FULL =
            "Logging has stopped because no more free space is available to store samples.";
        private const string STATUS_APP_MSG_EVENT_BOD =
            "Battery is (nearly) depleted.";
        private const string STATUS_APP_MSG_EVENT_TEMPERATURE_TOO_LOW =
            "At least one temperature value was lower than the valid minimum value.";
        private const string STATUS_APP_MSG_EVENT_TEMPERATURE_TOO_HIGH =
            "At least one temperature value was higher than the valid maximum value.";
        private const string STATUS_APP_MSG_EVENT_STOPPED =
            "The tag is configured and has been logging. Now it has stopped logging.";
        private const string STATUS_APP_MSG_EVENT_LOGGING =
            "The tag is configured and is logging. At least one sample is available.";
        private const string STATUS_APP_MSG_EVENT_STARTING =
            "The tag is configured and will make a first measurement after the configured delay.";
        private const string STATUS_APP_MSG_EVENT_CONFIGURED =
            "The tag is configured, but requires an external trigger to start measuring.";
        private const string STATUS_APP_MSG_EVENT_PRISTINE =
            "The tag is not yet configured and contains no data.";

        public TemperatureStatusViewModel()
        {
            _msgLib = App.MsgLib;

            TitleText = "ACTIVE CONFIGURATION";

            // Msg lib bindings.
            _msgLib.MeasurementStatusModel.PropertyChanged += MeasurementStatusModel_PropertyChanged;
            _msgLib.NdefText.PropertyChanged += NdefText_PropertyChanged;
            _msgLib.NdefMimeType.PropertyChanged += NdefMimeType_PropertyChanged;
            _msgLib.VersionModel.PropertyChanged += VersionModel_PropertyChanged;
            _msgLib.NfcIdModel.PropertyChanged += NfcIdModel_PropertyChanged;

            // App setting change settings.
            App.AppSettingsService.PropertyChanged += AppSettingsService_PropertyChanged;

            // Refresh GUI on a separate task to offload UI thread.
            Task.Run(() => RefreshGui());
        }

        private void AppSettingsService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Services.AppSettingsService.TemperatureUnit))
            {
                RefreshGui();
            }
        }

        private void RefreshGui()
        {
            MeasurementStatusModel_PropertyChanged(this, new PropertyChangedEventArgs(nameof(Msg.Models.MeasurementStatusModel.Status)));
            NdefText_PropertyChanged(this, new PropertyChangedEventArgs(nameof(Msg.Models.NdefTextModel.NdefText)));
            NdefMimeType_PropertyChanged(this, new PropertyChangedEventArgs(nameof(Msg.Models.NdefMimeTypeModel.NdefMimeType)));
            VersionModel_PropertyChanged(this, new PropertyChangedEventArgs(nameof(Msg.Models.VersionModel.Version)));
        }

        private string _titleText;
        public string TitleText
        {
            get => _titleText;
            set => SetProperty(ref _titleText, value);
        }

        private ImageSource _statusIcon;
        public ImageSource StatusIcon
        {
            get => _statusIcon;
            set => SetProperty(ref _statusIcon, value);
        }

        private string _statusText;
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        private string _ndefText;
        public string NdefText
        {
            get => _ndefText;
            set => SetProperty(ref _ndefText, value);
        }

        private string _ndefMimeTypeText;
        public string NdefMimeTypeText
        {
            get => _ndefMimeTypeText;
            set => SetProperty(ref _ndefMimeTypeText, value);
        }

        private string _versionText;
        public string VersionText
        {
            get => _versionText;
            set => SetProperty(ref _versionText, value);
        }

        public EventHandler<EventArgs> NfcIdValidEvent;
        private bool _isNfcIdValid;
        public bool IsNfcIdValid
        {
            get => _isNfcIdValid;
            set
            {
                SetProperty(ref _isNfcIdValid, value);
                NfcIdValidEvent?.Invoke(this, new EventArgs());
            }
        }

        private void MeasurementStatusModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_msgLib.TagModel.TagId == string.Empty)
            {
                StatusIcon = ImageSource.FromResource("TLogger.Images.UNKNOWN_STATE.png");
                StatusText = STATUS_LABEL_MESSAGE_NOT_SUPPORTED;
            }
            else if (e.PropertyName == nameof(Msg.Models.MeasurementStatusModel.Status))
            {
                var status = _msgLib.MeasurementStatusModel.Status.Measurement;
                switch (status)
                {
                    case Msg.Models.MeasurementStatusModel.Measurement.Reset:
                    case Msg.Models.MeasurementStatusModel.Measurement.Unknown:
                        StatusIcon = ImageSource.FromResource("TLogger.Images.UNKNOWN_STATE.png");
                        StatusText = STATUS_LABEL_MESSAGE_NOT_SUPPORTED;
                        break;
                    case Msg.Models.MeasurementStatusModel.Measurement.NotConfigured:
                        StatusIcon = ImageSource.FromResource("TLogger.Images.PRISTINE_STATE.png");
                        StatusText = STATUS_APP_MSG_EVENT_PRISTINE;
                        break;
                    case Msg.Models.MeasurementStatusModel.Measurement.Starting:
                        StatusIcon = ImageSource.FromResource("TLogger.Images.STARTING_STATE.png");
                        StatusText = STATUS_APP_MSG_EVENT_STARTING;
                        break;
                    case Msg.Models.MeasurementStatusModel.Measurement.Configured:
                        StatusIcon = ImageSource.FromResource("TLogger.Images.CONFIGURED_STATE.png");
                        StatusText = STATUS_APP_MSG_EVENT_CONFIGURED;
                        break;
                    case Msg.Models.MeasurementStatusModel.Measurement.Logging:
                        if (_msgLib.TemperatureStatusModel.Status.Temperature ==
                            Msg.Models.TemperatureStatusModel.Temperature.Low)
                        {
                            StatusIcon = ImageSource.FromResource("TLogger.Images.TEMP_TOO_LOW_STATE.png");
                            StatusText = STATUS_APP_MSG_EVENT_TEMPERATURE_TOO_LOW;
                        }
                        else if (_msgLib.TemperatureStatusModel.Status.Temperature ==
                            Msg.Models.TemperatureStatusModel.Temperature.High)
                        {
                            StatusIcon = ImageSource.FromResource("TLogger.Images.TEMP_TOO_LOW_STATE.png");
                            StatusText = STATUS_APP_MSG_EVENT_TEMPERATURE_TOO_HIGH;
                        }
                        else
                        {
                            StatusIcon = ImageSource.FromResource("TLogger.Images.LOGGING_STATE.png");
                            StatusText = STATUS_APP_MSG_EVENT_LOGGING;
                        }
                        break;
                    case Msg.Models.MeasurementStatusModel.Measurement.Stopped:
                        // Check failure states.
                        switch (_msgLib.MeasurementStatusModel.Status.Failure)
                        {
                            case Msg.Models.MeasurementStatusModel.Failure.NoFailure:
                                StatusIcon = ImageSource.FromResource("TLogger.Images.STOPPED_STATE.png");
                                StatusText = STATUS_APP_MSG_EVENT_STOPPED;
                                break;
                            case Msg.Models.MeasurementStatusModel.Failure.Bod:
                                StatusIcon = ImageSource.FromResource("TLogger.Images.STOPPED_BROWN_OUT.png");
                                StatusText = STATUS_APP_MSG_EVENT_BOD;
                                break;
                            case Msg.Models.MeasurementStatusModel.Failure.Full:
                                StatusIcon = ImageSource.FromResource("TLogger.Images.STOPPED_DISK_FULL.png");
                                StatusText = STATUS_APP_MSG_EVENT_FULL;
                                break;
                            case Msg.Models.MeasurementStatusModel.Failure.Expired:
                                StatusIcon = ImageSource.FromResource("TLogger.Images.STOPPED_EXPIRED.png");
                                StatusText = STATUS_APP_MSG_EVENT_EXPIRED;
                                break;
                        }
                        break;
                }
            }
        }

        private void NdefText_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NdefText = string.Empty;
            string ndefText = string.Empty;

            foreach (string text in _msgLib.NdefText.NdefText)
            {
                string t = text;
                if (App.AppSettingsService.TemperatureUnit == Services.AppSettingsService.ETemperatureUnit.Fahrenheit)
                {
                    // Use regex to replace C into F.
                    t = Regex.Replace(t, @"[+-]*\d{1,3}\.\dC", m =>
                    {
                        float c = Convert.ToSingle(m.Value.Replace("C", ""), Helpers.GlobalHelper.FormatProvider);
                        float f = c * 9 / 5 + 32;
                        string s = f.ToString("0.0", Helpers.GlobalHelper.FormatProvider) + "F";
                        return s;
                    });
                }

                ndefText += t;
                ndefText += Environment.NewLine;
            }

            NdefText = ndefText;
        }

        private void NdefMimeType_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NdefMimeTypeText = "MIME type: n.a.";

            if (_msgLib.NdefMimeType.NdefMimeType != string.Empty)
            {
                NdefMimeTypeText = $"MIME type: {_msgLib.NdefMimeType.NdefMimeType}";
            }

        }

        private void VersionModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            VersionText = "n.a.";

            if (_msgLib.VersionModel.Version.FwVersion != string.Empty &&
                _msgLib.VersionModel.Version.ApiVersion != string.Empty)
            {
                VersionText =
                    $"SW:{_msgLib.VersionModel.Version.FwVersion}  " +
                    $"API:{_msgLib.VersionModel.Version.ApiVersion}  " +
                    $"APP:{AppInfo.VersionString}";
            }
        }

        private void NfcIdModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Msg.Models.NfcIdModel.NfcId))
            {
                if (_msgLib.NfcIdModel.NfcId.Length != 0)
                {
                    IsNfcIdValid = true;
                }
                else
                {
                    IsNfcIdValid = false;
                }
            }
        }

    }
}
