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
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace TLogger.ViewModels
{
    class TemperatureGraphViewModel : ObservableObject
    {
        Msg.Lib _msgLib;

        public TemperatureGraphViewModel()
        {
            _msgLib = App.MsgLib;
            TitleText = "ACTIVE CONFIGURATION";
            _msgLib.DataProcessEvent += OnDataProcessEvent;
            App.AppSettingsService.PropertyChanged += AppSettingsService_PropertyChanged;
            ButtonCommand = new Command(async () => await OnButtonCommandAsync());
        }

        string _titleText;
        public string TitleText
        {
            get => _titleText;
            set => SetProperty(ref _titleText, value);
        }

        HtmlWebViewSource _htmlContentSource = new HtmlWebViewSource
        {
            Html = string.Empty,
        };
        public HtmlWebViewSource HtmlContentSource
        {
            get => _htmlContentSource;
            set => SetProperty(ref _htmlContentSource, value);
        }

        string _infoText;
        public string InfoText
        {
            get => _infoText;
            set => SetProperty(ref _infoText, value);
        }

        void OnDataProcessEvent(object sender, Msg.Lib.DataProcessEventArgs e)
        {
            switch (e.Op)
            {
                case Msg.Lib.DataProcessOp.End:
                    RefreshGui();
                    break;
            }
        }

        void AppSettingsService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshGui();
        }

        void RefreshGui()
        {
            var schema = GetSchema();
            (string data, string temperatureUnit, float temperatureMin, float temperatureMax, float temperatureLimitMin,
                float temperatureLimitMax) = GetData();

            var temperatureMinStr = temperatureMin.ToString("0.#", Helpers.GlobalHelper.FormatProvider);
            var temperatureMaxStr = temperatureMax.ToString("0.#", Helpers.GlobalHelper.FormatProvider);
            var temperatureLimitMinStr = temperatureLimitMin.ToString("0.#", Helpers.GlobalHelper.FormatProvider);
            var temperatureLimitMaxStr = temperatureLimitMax.ToString("0.#", Helpers.GlobalHelper.FormatProvider);

            string htmlContent =
                $@"
                <html>
                    <head>
                        <script type='text/javascript' src='fusioncharts.js'></script>
                        <meta name='viewport' content='width=device-width,initial-scale=1,maximum-scale=1'/>
                    </head>
                    <body>
                        <div id='container'>Preparing graphs...</div>
                        <script type='text/javascript'>
                        var schema = {schema};
                        var data = {data};
                        var events = [{{ }}];
                        var dataTable = new FusionCharts.DataStore().createDataTable(data, schema);
                        new FusionCharts({{
                            type: 'timeseries',
                            width: '100%',
                            height: '100%',
                            renderAt: 'container',
                            dataSource: {{
                                caption: {{text: 'Recorded Temperature Values'}},
                                data: dataTable,
                                xaxis: [{{plot: 'Time', timeMarker: events}}],
                                yAxis: [{{
                                    plot: [{{value: 'Temperature', type: 'area'}}],
                                    format: {{suffix: '{temperatureUnit}'}},
                                    title: '',
                                    min: {temperatureMinStr},
                                    max: {temperatureMaxStr},
                                    referenceLine: [{{value: {temperatureLimitMinStr}}}, {{value: {temperatureLimitMaxStr}}}]
                                }}],
                            }}
                        }}).render();
                        </script>
                    </body>
                </html>
                ";

            string baseUrl = DependencyService.Get<Interfaces.IBaseUrl>().Get();
            if (Device.RuntimePlatform == Device.UWP)
            {
                baseUrl += "tlogger_graph.html";
            }

            // On UI thread.
            Device.BeginInvokeOnMainThread(() =>
            {
                HtmlContentSource = new HtmlWebViewSource
                {
                    Html = htmlContent,
                    BaseUrl = baseUrl,
                };
            });

            InfoText = $"{_msgLib.TemperatureDataModel.TemperatureData.NumData} values in " +
                Helpers.TimeSpanHelper.ShortReadableTimeSpan(
                    Helpers.RtcAccuracyHelper.CorrectedTimeSpan(
                        _msgLib.MeasurementStatusModel.Status.RunningDuration), 2) +
                (_msgLib.TemperatureDataModel.TemperatureData.NumData <
                    _msgLib.MeasurementStatusModel.Status.NumberOfMeasurements ? " - not all data have been read" : "");
        }

        class Schema
        {
            public string name { get; set; }
            public string type { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string format { get; set; }
        };

        string GetSchema()
        {
            string timestampFormatJs = "%Y-%m-%d %H:%M:%S";

            var schemaList = new List<Schema>();
            schemaList.AddRange(new List<Schema> {
                new Schema
                {
                    name = "SeqNo",
                    type = "number",
                },
                new Schema
                {
                    name = "Time",  // Do NOT use Timestamp as name, this causes problems!!!
                    type = "date",
                    format = $"{timestampFormatJs}",
                },
                new Schema
                {
                    name = "Temperature",
                    type = "number",
                }
             });

            var schema = JsonConvert.SerializeObject(schemaList);
            return schema;
        }

        class Data
        {
            public int seqNo { get; set; }
            public DateTime timestamp { get; set; }
            public float temperature { get; set; }
            public float humidity { get; set; }
        }

        (string data, string temperatureUnit, float temperatureMin, float temperatureMax, float temperatureLimitMin,
            float temperatureLimitMax)
            GetData()
        {
            var temperatureData = _msgLib.TemperatureDataModel.TemperatureData.Data;
            var startTime = Helpers.RtcAccuracyHelper.CorrectedLocalTimestamp(
                _msgLib.MeasurementStatusModel.Status.StartTime);
            var interval = _msgLib.MeasurementConfigModel.Config.Interval;
            float temperatureValue;
            string timestampFormatCs = "yyyy-MM-dd HH:mm:ss";

            var dataBuilder = new StringBuilder();

            bool isCelsius =
                App.AppSettingsService.TemperatureUnit == Services.AppSettingsService.ETemperatureUnit.Celsius;
            string temperatureUnit = isCelsius ? "°C" : "°F";

            var temperatureMin = Math.Min(_msgLib.TemperatureStatusModel.Status.RecordedMin,
                _msgLib.TemperatureConfigModel.Config.ConfiguredMin);
            var temperatureMax = Math.Max(_msgLib.TemperatureStatusModel.Status.RecordedMax,
                _msgLib.TemperatureConfigModel.Config.ConfiguredMax);
            var temperatureLimitMin = (float)_msgLib.TemperatureConfigModel.Config.ConfiguredMin;
            var temperatureLimitMax = (float)_msgLib.TemperatureConfigModel.Config.ConfiguredMax;
            if (!isCelsius)
            {
                temperatureMin = temperatureMin * 9 / 5 + 32;
                temperatureMax = temperatureMax * 9 / 5 + 32;
                temperatureLimitMin = temperatureLimitMin * 9 / 5 + 32;
                temperatureLimitMax = temperatureLimitMax * 9 / 5 + 32;
            }

            for (int i = 0; i < temperatureData.Length; i++)
            {
                var entry = new Data
                {
                    seqNo = i + 1,
                    timestamp = startTime + TimeSpan.FromSeconds(interval.TotalSeconds * i),
                };

                temperatureValue = temperatureData[i];
                if (temperatureValue == Helpers.GlobalHelper.NotInitializedData)
                    temperatureValue = 0;
                else
                {
                    if (temperatureValue == Helpers.GlobalHelper.TemperaturePlaceholder)
                    {
                        // Place holder value. It is reported when the measurement is done while NFC field 
                        // is present. Use neighbour value.
                        int range = Math.Max(i, temperatureData.Length - 1 - i);
                        for (int j = 1; j <= range; j++)
                        {
                            if (i + j < temperatureData.Length)
                            {
                                if (temperatureData[i + j] != Helpers.GlobalHelper.TemperaturePlaceholder)
                                {
                                    temperatureValue = temperatureData[i + j];
                                    break;
                                }
                            }
                            if (i - j >= 0)
                            {
                                if (temperatureData[i - j] != Helpers.GlobalHelper.TemperaturePlaceholder)
                                {
                                    temperatureValue = temperatureData[i - j];
                                    break;
                                }
                            }
                        }
                    }
                    if (!isCelsius)
                    {
                        temperatureValue = temperatureValue * 9 / 5 + 32;
                    }
                }
                entry.temperature = temperatureValue;

                var temperatureStr = entry.temperature.ToString("0.#", Helpers.GlobalHelper.FormatProvider);
                dataBuilder.Append($"[{entry.seqNo},'{entry.timestamp.ToString(timestampFormatCs)}'," +
                    $"{temperatureStr}],");
            }

            var data = "[" + dataBuilder.ToString() + "]";
            return (data, temperatureUnit, temperatureMin, temperatureMax, temperatureLimitMin, temperatureLimitMax);
        }

        public Command ButtonCommand { get; }
        async Task OnButtonCommandAsync()
        {
            // Prepare CSV file and share.
            var now = DateTime.Now;
            var sharePath = Path.Combine(FileSystem.CacheDirectory, "share");
            Directory.CreateDirectory(sharePath);
            var filePath = Path.Combine(
                sharePath,
                "NHS3100TemperatureData_" + now.ToString("yyyy.MM.dd_HH.mm.ss") + ".csv");

            var csv = new StringBuilder();

            // NFCID.
            csv.AppendLine("NFC ID: " + BitConverter.ToString(_msgLib.NfcIdModel.NfcId).Replace("-", ":"));

            // Drift.
            csv.AppendLine($"DRIFT: " + Helpers.RtcAccuracyHelper.GetDrift().ToString("0.0", Helpers.GlobalHelper.FormatProvider));

            // Data format.
            csv.AppendLine("#, epoch, corrected, UTC, temperature(C)");

            // Temperature data.
            var temperatureData = _msgLib.TemperatureDataModel.TemperatureData.Data;
            var startTime = _msgLib.MeasurementStatusModel.Status.StartTime;
            var interval = _msgLib.MeasurementConfigModel.Config.Interval;
            float temperatureValue;
            string timestampFormat = "yyyy-MM-dd HH:mm:ss";

            bool isCelsius =
                App.AppSettingsService.TemperatureUnit == Services.AppSettingsService.ETemperatureUnit.Celsius;
            string temperatureUnit = isCelsius ? "°C" : "°F";

            bool isPlaceholder = false;

            for (int i = 0; i < temperatureData.Length; i++)
            {
                var seqNo = i + 1;
                var timestamp = startTime + TimeSpan.FromSeconds(interval.TotalSeconds * i);
                var correctedTimestamp = Helpers.RtcAccuracyHelper.CorrectedTimestamp(timestamp);
                var epoch = (long)(timestamp - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
                var correctedEpoch = (long)(correctedTimestamp - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).
                    TotalSeconds;
                var temperatureString = string.Empty;

                temperatureValue = temperatureData[i];
                if (temperatureValue == Helpers.GlobalHelper.TemperaturePlaceholder)
                {
                    isPlaceholder = true;
                }
                else
                {
                    if (!isCelsius)
                    {
                        temperatureValue = temperatureValue * 9 / 5 + 32;
                    }
                    temperatureString = temperatureValue.ToString("0.0", Helpers.GlobalHelper.FormatProvider);
                    csv.AppendLine(
                        $"{i + 1}, " +
                        $"{epoch}, " +
                        $"{correctedEpoch}, " +
                        $"{correctedTimestamp.ToString(timestampFormat)}, " +
                        temperatureString
                    );
                }
            }

            if (isPlaceholder)
                csv.Append("Temperature measurements were skipped when an NFC field was present");

            File.WriteAllText(filePath, csv.ToString());
            await DependencyService.Get<Interfaces.IShareFile>().ShareAsync(new string[]
            {
                filePath
            });
        }
    }
}
