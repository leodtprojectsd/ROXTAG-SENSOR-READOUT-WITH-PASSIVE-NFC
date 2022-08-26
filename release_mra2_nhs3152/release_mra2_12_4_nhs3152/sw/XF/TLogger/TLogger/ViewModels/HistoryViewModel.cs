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
using SQLite;
using SQLiteNetExtensionsAsync.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TLogger.ViewModels
{
    class HistoryViewModel : ObservableObject
    {
        SQLiteAsyncConnection _db = App.DatabaseService.DBase;
        Msg.Lib _msgLib;

        public HistoryViewModel()
        {
            _msgLib = App.MsgLib;
            TitleText = "HISTORY";
            App.DatabaseService.DBaseUpdateEvent += OnDBaseUpdate;
            Task.Run(async () => await ShowListAsync());
        }

        History _selectedItem;
        public History SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                Task.Run(async () => await HandleSelectedItemAsync(value));
            }
        }

        async Task HandleSelectedItemAsync(History item)
        {
            var queryList = await _db.GetAllWithChildrenAsync<Services.DatabaseService.TagsStatusTable>(
                i => i.Id == item.Id, true);
            if (queryList.Count == 1)
            {
                var status = queryList[0];

                var temperatureHistoryPage = new Views.TemperatureHistoryPageViewCS(status);
                Device.BeginInvokeOnMainThread(async () =>
                    await App.HistoryPageView.Navigation.PushAsync(temperatureHistoryPage));
              
            }
            else
            {
                //// TODO: HANDLE ERROR
            }
        }

        async void OnDBaseUpdate(object sender, EventArgs e)
        {
            await ShowListAsync(true);
        }

        async Task ShowListAsync(bool isDBaseUpdate = false)
        {
            Device.BeginInvokeOnMainThread(() => _groupHistoryList.Clear());

            var status = await _db.GetAllWithChildrenAsync<Services.DatabaseService.TagsStatusTable>();
            if (status.Count == 0)
                return;

            // Exclude current reading if any. 
            int excludeStatusId = -1;
            if (isDBaseUpdate)
            {
                var tagId = App.MsgLib.TagModel.TagId;
                if (tagId != string.Empty)
                {
                    // SQLite saves DateTime in ISO-8601 (Kind=Unspecify) format.
                    // So keep all members the same except Kind.
                    var tagConfigTime = DateTime.SpecifyKind(
                        _msgLib.TagModel.TagConfigTimestamp,
                        DateTimeKind.Unspecified);

                    var queryList = await _db.GetAllWithChildrenAsync<Services.DatabaseService.TagsConfigTable>(
                        i => i.TagId == tagId && i.TagConfigTimestamp == tagConfigTime, true);
                    if (queryList.Count == 1)
                    {
                        var config = queryList[0];
                        if (config.StatusTable.Count != 0)
                            excludeStatusId = config.StatusTable[config.StatusTable.Count - 1].Id;
                    }
                }
            }
            if (excludeStatusId != -1)
            {
                status.Remove(status.Single(x => x.Id == excludeStatusId));
            }

            if (status.Count == 0)
                return;

            // Group items using MIME type.
            var mimeList = status.GroupBy(x => x.NdefMimeType).Select(g => g.ToList()).ToList();
            foreach(var mime in mimeList)
            {
                var group = new GroupHistory
                {
                    GroupTitle = $"MIME : {mime[0].NdefMimeType}"
                };
                foreach(Services.DatabaseService.TagsStatusTable item in mime)
                {
                    if (item.NfcId != null)
                    {
                        group.Add(new History
                        {
                            Id = item.Id,
                            Image = GetStatusIcon(item),
                            Nfcid = $"NFC ID: {BitConverter.ToString(item.NfcId).Replace("-", ":")}",
                            Timestamp = item.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                            Status = $"Status: {GetStatusText(JsonConvert.DeserializeObject<string[]>(item.JsonNdefText))}",
                            Version = $"SW: {item.Version.FwVersion} API: {item.Version.ApiVersion}",
                        });
                    }
                }

                Device.BeginInvokeOnMainThread(() => _groupHistoryList.Add(group));
            }
        }

        ImageSource GetStatusIcon(Services.DatabaseService.TagsStatusTable item)
        {
            ImageSource icon = null;
            switch (item.MeasurementStatus.Measurement)
            {
                case Msg.Models.MeasurementStatusModel.Measurement.Reset:
                case Msg.Models.MeasurementStatusModel.Measurement.Unknown:
                    icon = ImageSource.FromResource("TLogger.Images.UNKNOWN_STATE.png");
                    break;
                case Msg.Models.MeasurementStatusModel.Measurement.NotConfigured:
                    icon = ImageSource.FromResource("TLogger.Images.PRISTINE_STATE.png");
                    break;
                case Msg.Models.MeasurementStatusModel.Measurement.Starting:
                    icon = ImageSource.FromResource("TLogger.Images.STARTING_STATE.png");
                    break;
                case Msg.Models.MeasurementStatusModel.Measurement.Configured:
                    icon = ImageSource.FromResource("TLogger.Images.CONFIGURED_STATE.png");
                    break;
                case Msg.Models.MeasurementStatusModel.Measurement.Logging:
                    if (item.TemperatureStatus.Temperature == Msg.Models.TemperatureStatusModel.Temperature.Low)
                        icon = ImageSource.FromResource("TLogger.Images.TEMP_TOO_LOW_STATE.png");
                    else if (item.TemperatureStatus.Temperature == Msg.Models.TemperatureStatusModel.Temperature.High)
                        icon = ImageSource.FromResource("TLogger.Images.TEMP_TOO_LOW_STATE.png");
                    else
                        icon = ImageSource.FromResource("TLogger.Images.LOGGING_STATE.png");
                    break;
                case Msg.Models.MeasurementStatusModel.Measurement.Stopped:
                    // Check failure states.
                    switch (item.MeasurementStatus.Failure)
                    {
                        case Msg.Models.MeasurementStatusModel.Failure.NoFailure:
                            icon = ImageSource.FromResource("TLogger.Images.STOPPED_STATE.png");
                            break;
                        case Msg.Models.MeasurementStatusModel.Failure.Bod:
                            icon = ImageSource.FromResource("TLogger.Images.STOPPED_BROWN_OUT.png");
                            break;
                        case Msg.Models.MeasurementStatusModel.Failure.Full:
                            icon = ImageSource.FromResource("TLogger.Images.STOPPED_DISK_FULL.png");
                            break;
                        case Msg.Models.MeasurementStatusModel.Failure.Expired:
                            icon = ImageSource.FromResource("TLogger.Images.STOPPED_EXPIRED.png");
                            break;
                    }
                    break;
            }

            return icon;
        }
      
        string GetStatusText(string[] jsonNdefText)
        {
            var text = string.Empty;

            foreach (var ndef in jsonNdefText)
            {
                var t = ndef;
                if (App.AppSettingsService.TemperatureUnit == Services.AppSettingsService.ETemperatureUnit.Fahrenheit)
                {
                    // Use regex to replace C into F.
                    t = Regex.Replace(t, @"[+-]*\d{1,3}\.\dC", m =>
                    {
                        var c = Convert.ToSingle(m.Value.Replace("C", ""), Helpers.GlobalHelper.FormatProvider);
                        var f = c * 9 / 5 + 32;
                        var s = f.ToString("0.0", Helpers.GlobalHelper.FormatProvider) + "F";
                        return s;
                    });
                }

                text += t;
            }
            return text;
        }


        string _titleText;
        public string TitleText
        {
            get => _titleText;
            set => SetProperty(ref _titleText, value);
        }

        public class History
        {
            public int Id { get; set; }
            public ImageSource Image { get; set; }
            public string Nfcid { get; set; }
            public string Timestamp { get; set; }
            public string Status { get; set; }
            public string Version { get; set; }
        }

        public class GroupHistory : ObservableCollection<History>
        {
            public string GroupTitle { get; set; }
        }

        ObservableCollection<GroupHistory> _groupHistoryList = new ObservableCollection<GroupHistory>();
        public ObservableCollection<GroupHistory> GroupHistoryList
        {
            get => _groupHistoryList;
        }
    }
}
