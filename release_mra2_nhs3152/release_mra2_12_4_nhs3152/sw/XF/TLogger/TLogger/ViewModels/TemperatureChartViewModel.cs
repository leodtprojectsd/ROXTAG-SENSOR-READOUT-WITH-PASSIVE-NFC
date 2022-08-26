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

using MvvmHelpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TLogger.ViewModels
{
    class TemperatureChartViewModel : ObservableObject
    {
        Msg.Lib _msgLib;

        public TemperatureChartViewModel()
        {
            _msgLib = App.MsgLib;

            TitleText = "ACTIVE CONFIGURATION";

            // Msg lib bindings.
            _msgLib.MeasurementStatusModel.PropertyChanged += MeasurementStatusModel_PropertyChanged;
            _msgLib.TemperatureStatusModel.PropertyChanged += TemperatureStatusModel_PropertyChanged;
            _msgLib.NfcIdModel.PropertyChanged += NfcIdModel_PropertyChanged;
            _msgLib.DataProcessEvent += OnDataProcessEvent;

            // App setting change settings.
            App.AppSettingsService.PropertyChanged += AppSettingsService_PropertyChanged;

            // Refresh GUI on a separate task to offload UI thread.
            Task.Run(() => RefreshGui());
        }

        void AppSettingsService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Services.AppSettingsService.TemperatureUnit))
            {
                RefreshGui();
            }
        }

        void RefreshGui()
        {
            NfcIdModel_PropertyChanged(
                this,
                new PropertyChangedEventArgs(nameof(Msg.Models.NfcIdModel.NfcId)));

            // Measurement status text.
            MeasurementStatusModel_PropertyChanged(
                this,
                new PropertyChangedEventArgs(nameof(Msg.Models.MeasurementStatusModel.Status)));
            TemperatureStatusModel_PropertyChanged(
                this,
                new PropertyChangedEventArgs(nameof(Msg.Models.TemperatureStatusModel.Status)));

        }

        string _titleText;
        public string TitleText
        {
            get => _titleText;
            set => SetProperty(ref _titleText, value);
        }

        string _nfcIdText;
        public string NfcIdText
        {
            get => _nfcIdText;
            set => SetProperty(ref _nfcIdText, value);
        }


        string _numRecordedText;
        public string NumRecordedText
        {
            get => _numRecordedText;
            set => SetProperty(ref _numRecordedText, value);
        }

        string _configurationTimeText;
        public string ConfigurationTimeText
        {
            get => _configurationTimeText;
            set => SetProperty(ref _configurationTimeText, value);
        }

        string _loggingForText;
        public string LoggingForText
        {
            get => _loggingForText;
            set => SetProperty(ref _loggingForText, value);
        }

        string _measurementIntervalText;
        public string MeasurementIntervalText
        {
            get => _measurementIntervalText;
            set => SetProperty(ref _measurementIntervalText, value);
        }

        string _numRetrievedText;
        public string NumRetrievedText
        {
            get => _numRetrievedText;
            set => SetProperty(ref _numRetrievedText, value);
        }


        string _statusText;
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        string _minLimitText;
        public string MinLimitText
        {
            get => _minLimitText;
            set => SetProperty(ref _minLimitText, value);
        }

        string _maxLimitText;
        public string MaxLimitText
        {
            get => _maxLimitText;
            set => SetProperty(ref _maxLimitText, value);
        }

        string _textColor;
        public string TextColor
        {
            get => _textColor;
            set => SetProperty(ref _textColor, value);
        }

        HtmlWebViewSource _htmlContentSourceTemperature = new HtmlWebViewSource { Html = string.Empty };
        public HtmlWebViewSource HtmlContentSourceTemperature
        {
            get => _htmlContentSourceTemperature;
            set => SetProperty(ref _htmlContentSourceTemperature, value);
        }

        void NfcIdModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Msg.Models.NfcIdModel.NfcId))
            {
                NfcIdText = BitConverter.ToString(_msgLib.NfcIdModel.NfcId).Replace("-", ":");
            }
        }

        void OnDataProcessEvent(object sender, Msg.Lib.DataProcessEventArgs e)
        {
            switch (e.Op)
            {
                case Msg.Lib.DataProcessOp.Begin:
                    NumRetrievedText = string.Empty;
                    break;
                case Msg.Lib.DataProcessOp.End:
                    NumRetrievedText = _msgLib.TemperatureDataModel.TemperatureData.NumData.ToString();
                    break;
            }
        }


        void MeasurementStatusModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Msg.Models.MeasurementStatusModel.Status))
            {
                if (_msgLib.MeasurementConfigModel.Config.IsReset ||
                    _msgLib.MeasurementConfigModel.Config.IsUnknown)
                {
                    NumRecordedText = string.Empty;
                    ConfigurationTimeText = string.Empty;
                    LoggingForText = "n.a.";
                    NumRetrievedText = string.Empty;
                    MeasurementIntervalText = string.Empty;
                    StatusText = string.Empty;
                    MinLimitText = "n.a.";
                    MaxLimitText = "n.a.";
                    TextColor = "#E8B410";
                }
                else
                {
                    NumRecordedText = $"{_msgLib.MeasurementStatusModel.Status.NumberOfMeasurements}";
                    ConfigurationTimeText = Helpers.RtcAccuracyHelper.CorrectedLocalTimestamp(
                        _msgLib.MeasurementStatusModel.Status.ConfigTime).ToString(App.AppSettingsService.DateTimeFormat);
                    if (_msgLib.MeasurementStatusModel.Status.RunningDuration == TimeSpan.Zero)
                        LoggingForText = "n.a.";
                    else
                        LoggingForText = Helpers.TimeSpanHelper.ShortReadableTimeSpan(
                            Helpers.RtcAccuracyHelper.CorrectedTimeSpan(_msgLib.MeasurementStatusModel.Status.RunningDuration), 2);
                    MeasurementIntervalText = Helpers.TimeSpanHelper.ReadableTimeSpan(_msgLib.MeasurementConfigModel.Config.Interval, 2);
                    StatusText = GetSimplifiedStatus();

                    bool isCelsius = App.AppSettingsService.TemperatureUnit == Services.AppSettingsService.ETemperatureUnit.Celsius;
                    string temperatureUnit = isCelsius ? "°C" : "°F";
                    int configMin = _msgLib.TemperatureConfigModel.Config.ConfiguredMin;
                    int configMax = _msgLib.TemperatureConfigModel.Config.ConfiguredMax;
                    if (!isCelsius)
                    {
                        configMin = configMin * 9 / 5 + 32;
                        configMax = configMax * 9 / 5 + 32;
                    }
                    MinLimitText = $"{configMin} {temperatureUnit}";
                    MaxLimitText = $"{configMax} {temperatureUnit}";
                    TextColor = (StatusText == "OK") ? "#E8B410" : "#AD4749";
                }
            }
        }

        string GetSimplifiedStatus()
        {
            if (_msgLib.TemperatureStatusModel.Status.Temperature == 
                Msg.Models.TemperatureStatusModel.Temperature.High) return "NOT OK";
            else if (_msgLib.TemperatureStatusModel.Status.Temperature ==
                Msg.Models.TemperatureStatusModel.Temperature.Low) return "NOT OK";
            else if (_msgLib.MeasurementStatusModel.Status.Failure == Msg.Models.MeasurementStatusModel.Failure.Bod)
                return "OK";
            else if (_msgLib.MeasurementStatusModel.Status.Failure == Msg.Models.MeasurementStatusModel.Failure.Full)
                return "OK";
            else if (_msgLib.MeasurementStatusModel.Status.Failure == 
                Msg.Models.MeasurementStatusModel.Failure.Expired) return "OK";

            if (_msgLib.MeasurementStatusModel.Status.Measurement == 
                Msg.Models.MeasurementStatusModel.Measurement.Stopped) return "OK";
            else if (_msgLib.MeasurementStatusModel.Status.Measurement ==
                Msg.Models.MeasurementStatusModel.Measurement.Logging) return "OK";
            else if (_msgLib.MeasurementStatusModel.Status.Measurement ==
                Msg.Models.MeasurementStatusModel.Measurement.Starting) return "OK";
            else if (_msgLib.MeasurementStatusModel.Status.Measurement ==
                Msg.Models.MeasurementStatusModel.Measurement.Configured) return "NOT OK";

            return "NOT OK";
        }

    void TemperatureStatusModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Msg.Models.TemperatureStatusModel.Status))
            {
                (float minLimit, float maxLimit, string colorRanges, string pointers, string trendPoints) =
                    GetTemperatureParams();

                var minLimitStr = minLimit.ToString("0.#", Helpers.GlobalHelper.FormatProvider);
                var maxLimitStr = maxLimit.ToString("0.#", Helpers.GlobalHelper.FormatProvider);

                string htmlContent =
                    $@"
                    <html>
                        <head>
                            <script type='text/javascript' src='fusioncharts.js'></script>
                            <meta name='viewport' content='width=device-width,initial-scale=1,maximum-scale=1'/>
                        </head>
                        <body>
                            <div id='container'>Preparing graph...</div>
                            <script type='text/javascript'>
                            new FusionCharts({{
                                type: 'hlineargauge',
                                renderAt: 'container',
                                width: '100%',
                                height: '100%',
                                dataFormat: 'json',
                                dataSource: {{
                                    chart: {{
                                        theme: 'fint',
                                        bgColor: '#ffffff',
                                        showBorder: '0',
                                        lowerLimit: {minLimitStr},
                                        upperLimit: {maxLimitStr},
                                        chartBottomMargin: 0,
                                        chartTopMargin: 0,
                                        valueFontSize: 12,
                                        valueFontBold: '0',
                                        gaugeFillMix: '{{light-10}},{{light-70}},{{dark-10}}',
                                        gaugeFillRatio: '40,20,40',
                                        showTickMarks: '1',
                                        showTickValues: '1',
                                        pointerOnTop: '1',
                                        valueAbovePointer: '0',
                                        majorTMNumber: 3,
                                        placeValuesInside: '1',
                                    }},
                                    colorRange: {{
                                        color: {colorRanges},
                                    }},
                                    pointers: {{
                                        pointer: {pointers},
                                    }},
                                    trendPoints: {{
                                        point: {trendPoints},
                                    }},
                                }}
                            }}).render();
                            </script>
                        </body>
                    </html>
                    ";

                var baseUrl = DependencyService.Get<Interfaces.IBaseUrl>().Get();
                if (Device.RuntimePlatform == Device.UWP)
                {
                    baseUrl += "tlogger_chart.html";
                }

                // On UI thread.
                Device.BeginInvokeOnMainThread(() =>
                {
                    HtmlContentSourceTemperature = new HtmlWebViewSource
                    {
                        Html = htmlContent,
                        BaseUrl = baseUrl,
                    };
                });
            }
        }

        class ColorRange
        {
            public float minValue { get; set; }
            public float maxValue { get; set; }
            public string label { get; set; }
            public string code { get; set; }
        }

        class Pointer
        {
            public float value { get; set; }
            public string displayValue { get; set; }
        }

        class TrendPoint
        {
            public float startValue { get; set; }
            public float endValue { get; set; }
            public string displayValue { get; set; }
            public int alpha { get; set; }
            public string color { get; set; }
            public string dashed { get; set; }  // bool in fusion charts
            public string showOnTop { get; set; }   // bool in fusion charts
            public string useMarker { get; set; }   // bool in fusion charts
            public string markerColor { get; set; }
            public int markerRadius { get; set; }
        }

        const int MarkerRadius = 15;

        (float minLimit, float maxLimit, string colorRanges, string pointers, string trendPoints)
            GetTemperatureParams()
        {
            const float MinLimit = -40.0f;
            const float MaxLimit = 85.0f;

            float minLimit = MinLimit;
            float maxLimit = MaxLimit;
            string colorRanges = string.Empty;
            string pointers = string.Empty;
            string trendPoints = string.Empty;

            var colorRangeList = new List<ColorRange>();
            var pointerList = new List<Pointer>();
            var trendPointList = new List<TrendPoint>();

            bool isCelsius =
                App.AppSettingsService.TemperatureUnit == Services.AppSettingsService.ETemperatureUnit.Celsius;

            if (_msgLib.TemperatureStatusModel.Status.Temperature ==
                    Msg.Models.TemperatureStatusModel.Temperature.Reset ||
                _msgLib.TemperatureStatusModel.Status.Temperature ==
                    Msg.Models.TemperatureStatusModel.Temperature.Unknown)
            {
                if (!isCelsius)
                {
                    minLimit = minLimit * 9 / 5 + 32;
                    maxLimit = maxLimit * 9 / 5 + 32;
                }

                // Not configured.
                colorRangeList.Add(new ColorRange
                {
                    minValue = minLimit,
                    maxValue = maxLimit,
                    label = "Not configured",
                    code = "#babec4",
                });
            }
            else
            {
                // Configured.
                var configMin = (float)_msgLib.TemperatureConfigModel.Config.ConfiguredMin;
                var configMax = (float)_msgLib.TemperatureConfigModel.Config.ConfiguredMax;
                var recMin = (float)_msgLib.TemperatureStatusModel.Status.RecordedMin;
                var recMax = (float)_msgLib.TemperatureStatusModel.Status.RecordedMax;
                minLimit = configMin;
                maxLimit = configMax;
                if (_msgLib.TemperatureStatusModel.Status.IsRecorded)
                {
                    if (recMin < configMin) minLimit = recMin;
                    if (recMax > configMax) maxLimit = recMax;
                }
                minLimit -= 5.0f;
                maxLimit += 5.0f;
                if (minLimit < MinLimit) minLimit = MinLimit;
                if (maxLimit > MaxLimit) maxLimit = MaxLimit;

                if (!isCelsius)
                {
                    minLimit = minLimit * 9 / 5 + 32;
                    maxLimit = maxLimit * 9 / 5 + 32;
                    recMin = recMin * 9 / 5 + 32;
                    recMax = recMax * 9 / 5 + 32;
                    configMin = configMin * 9 / 5 + 32;
                    configMax = configMax * 9 / 5 + 32;
                }

                colorRangeList.AddRange(new List<ColorRange>
                {
                    new ColorRange
                    {
                        minValue = minLimit,
                        maxValue = configMin,
                        label = "Low",
                        ////code = "#D9E2F1",
                        code =  "#ff6666",
                    },
                    new ColorRange
                    {
                        minValue = configMin,
                        maxValue = configMax,
                        label = "Acceptable",
                        code = "#70db70",
                    },
                    new ColorRange
                    {
                        minValue = configMax,
                        maxValue = maxLimit,
                        label =  "High",
                        code = "#ff6666",
                    },
                });

                pointerList.AddRange(new List<Pointer>
                {
                    new Pointer
                    {
                        value = configMin,
                        displayValue = configMin.ToString("0.#", Helpers.GlobalHelper.FormatProvider),
                    },
                    new Pointer
                    {
                        value = configMax,
                        displayValue = configMax.ToString("0.#", Helpers.GlobalHelper.FormatProvider),
                    },
                });

                if (_msgLib.TemperatureStatusModel.Status.IsRecorded)
                {
                    trendPointList.AddRange(new List<TrendPoint>
                    {
                        new TrendPoint
                        {
                            startValue = recMin,
                            endValue = recMin,
                            displayValue = recMin.ToString("0.#", Helpers.GlobalHelper.FormatProvider),
                            alpha = 100,
                            color = "#264d00",
                            dashed = "1",
                            showOnTop = "0",
                            useMarker = "1",
                            markerColor = "#ffffff",
                            markerRadius = MarkerRadius,
                        },
                        new TrendPoint
                        {
                            startValue = recMax,
                            endValue = recMax,
                            displayValue = recMax.ToString("0.#", Helpers.GlobalHelper.FormatProvider),
                            alpha = 100,
                            color = "#264d00",
                            dashed = "1",
                            showOnTop = "0",
                            useMarker = "1",
                            markerColor = "#ffffff",
                            markerRadius = MarkerRadius,
                        },
                        new TrendPoint
                        {
                            startValue = recMin,
                            endValue = recMax,
                            displayValue = " ",
                            alpha = 50,
                            color = "#264d00",
                            dashed = "0",
                            showOnTop = "0",
                            useMarker = "1",
                            markerColor = "#ffffff",
                            markerRadius = MarkerRadius,
                        },
                    });
                }

            }

            colorRanges = JsonConvert.SerializeObject(colorRangeList);
            pointers = JsonConvert.SerializeObject(pointerList);
            trendPoints = JsonConvert.SerializeObject(trendPointList);

            return (minLimit, maxLimit, colorRanges, pointers, trendPoints);
        }


    }
}
