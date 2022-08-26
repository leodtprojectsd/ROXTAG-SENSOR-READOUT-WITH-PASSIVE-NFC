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

using MvvmHelpers;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using Xamarin.Essentials;

namespace TLogger.Services
{
    public class AppSettingsService : ObservableObject
    {
        // Persist application settings using Xamarin.Essiential.Preferances.        
        public AppSettingsService()
        {
////            Preferences.Clear();

            _isLicenseAccepted = Preferences.Get(nameof(IsLicenseAccepted), false);
            _areDebugLogs = Preferences.Get(nameof(AreDebugLogs), true);
            _temperatureUnit = (ETemperatureUnit)Preferences.Get(nameof(TemperatureUnit), 0);
            _dateFormat = Preferences.Get(nameof(DateFormat), "yyyy-M-d");
            _timeFormat = Preferences.Get(nameof(TimeFormat), "H:mm:ss");
            _isHistory = Preferences.Get(nameof(IsHistory), true);
            _appVersion = Preferences.Get(nameof(AppVersion), string.Empty);
        }

        // License.
        bool _isLicenseAccepted;
        public bool IsLicenseAccepted
        {
            get => _isLicenseAccepted;
            set
            {
                Preferences.Set(nameof(IsLicenseAccepted), value);
                SetProperty(ref _isLicenseAccepted, value);
            }
        }

        // Debug logs.
        bool _areDebugLogs;
        public bool AreDebugLogs
        {
            get => _areDebugLogs;
            set
            {
                Preferences.Set(nameof(AreDebugLogs), value);
                SetProperty(ref _areDebugLogs, value);
            }
        }

        // History
        bool _isHistory;
        public bool IsHistory
        {
            get => _isHistory;
            set
            {
                Preferences.Set(nameof(IsHistory), value);
                SetProperty(ref _isHistory, value);
            }
        }

        // Celsius/Fahrenheit selection.
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public enum ETemperatureUnit
        {
            [EnumMember(Value = "°C")]
            Celsius,
            [EnumMember(Value = "°F")]
            Fahrenheit,
        }
        ETemperatureUnit _temperatureUnit;
        public ETemperatureUnit TemperatureUnit
        {
            get => _temperatureUnit;
            set
            {
                Preferences.Set(nameof(TemperatureUnit), (int)value);
                SetProperty(ref _temperatureUnit, value);
            }
        }

        // Date.
        string _dateFormat;
        public string DateFormat
        {
            get => _dateFormat;
            set
            {
                Preferences.Set(nameof(DateFormat), value);
                SetProperty(ref _dateFormat, value);
            }
        }

        // Time.
        string _timeFormat;
        public string TimeFormat
        {
            get => _timeFormat;
            set
            {
                Preferences.Set(nameof(TimeFormat), value);
                SetProperty(ref _timeFormat, value);
            }
        }

        // DateTime getter.
        public string DateTimeFormat { get => _dateFormat + " " + _timeFormat;  }

        // App version info.
        string _appVersion;
        public string AppVersion
        {
            get => _appVersion;
            set
            {
                Preferences.Set(nameof(AppVersion), value);
                SetProperty(ref _appVersion, value);
            }

        }
    }
}
