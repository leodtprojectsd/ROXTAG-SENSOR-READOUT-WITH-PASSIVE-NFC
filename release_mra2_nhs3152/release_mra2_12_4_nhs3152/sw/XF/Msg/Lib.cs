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

using NdefLibrary.Ndef;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Msg
{
    public class Lib
    {
        private readonly Plugin.Ndef.INdef _ndef;
        public static CommandHandler CommandHandler { get; private set; }
        public static bool IsConfiguringTag { get; set; }

        private bool _isDataProcess;

        public enum DataProcessOp
        {
            Begin,
            Retrieval,
            End,
            RefreshDataFromCache,
            NonNhsTag,
            TextRecords,
            UrlRecords,
        }
        public class DataProcessEventArgs : EventArgs
        {
            public DataProcessOp Op { get; set; }
            public int Current { get; set; }
            public int Total { get; set; }
            public string Text { get; set; }
            public string Url { get; set; }
        }
        public EventHandler<DataProcessEventArgs> DataProcessEvent;

        public enum ENdefAccess
        {
            Read,
            Write,
        }
        public class NdefAccessEventArgs : EventArgs
        {
            public byte[] NfcId { get; set; }
            public DateTime Timestamp { get; set; }
            public ENdefAccess NdefAccess { get; set; }
            public byte[] Data { get; set; }
        }
        public EventHandler<NdefAccessEventArgs> NdefAccessEvent;

        public class SetConfigEventArgs : EventArgs
        {
            public Models.MeasurementConfigModel.CConfig MeasurementConfig { get; set; }
            public Models.TemperatureConfigModel.CConfig TemperatureConfig { get; set; }
            public Models.HumidityConfigModel.CConfig HumidityConfig { get; set; }
            public Models.AccelerometerConfigModel.CConfig AccelerometerConfig { get; set; }
        }
        public EventHandler<SetConfigEventArgs> SetConfigEvent;

        public Models.AccelerometerConfigModel AccelerometerConfigModel { get; }
        public Models.AccelerometerStatusModel AccelerometerStatusModel { get; }
        public Models.EventDataModel EventDataModel { get; }
        public Models.HumidityConfigModel HumidityConfigModel { get; }
        public Models.HumidityDataModel HumidityDataModel { get; }
        public Models.HumidityStatusModel HumidityStatusModel { get; }
        public Models.MeasurementConfigModel MeasurementConfigModel { get; }
        public Models.MeasurementStatusModel MeasurementStatusModel { get; }
        public Models.NfcIdModel NfcIdModel { get; }
        public Models.TagModel TagModel { get; }
        public Models.TemperatureConfigModel TemperatureConfigModel { get; }
        public Models.TemperatureDataModel TemperatureDataModel { get; }
        public Models.TemperatureStatusModel TemperatureStatusModel { get; }
        public Models.UidModel UidModel { get; }
        public Models.VersionModel VersionModel { get; }
        public Models.NdefTextModel NdefText { get; }
        public Models.NdefUrlModel NdefUrl { get; }
        public Models.NdefMimeTypeModel NdefMimeType { get; }

        public Lib()
        {
            _ndef = Plugin.Ndef.CrossNdef.Current;
            CommandHandler = new CommandHandler(this);

            // Model container.
            AccelerometerConfigModel = new Models.AccelerometerConfigModel();
            AccelerometerStatusModel = new Models.AccelerometerStatusModel();
            EventDataModel = new Models.EventDataModel();
            HumidityConfigModel = new Models.HumidityConfigModel();
            HumidityDataModel = new Models.HumidityDataModel();
            HumidityStatusModel = new Models.HumidityStatusModel();
            MeasurementConfigModel = new Models.MeasurementConfigModel();
            MeasurementStatusModel = new Models.MeasurementStatusModel();
            NfcIdModel = new Models.NfcIdModel();
            TagModel = new Models.TagModel();
            TemperatureConfigModel = new Models.TemperatureConfigModel();
            TemperatureDataModel = new Models.TemperatureDataModel();
            TemperatureStatusModel = new Models.TemperatureStatusModel();
            UidModel = new Models.UidModel();
            VersionModel = new Models.VersionModel();
            NdefText = new Models.NdefTextModel();
            NdefUrl = new Models.NdefUrlModel();
            NdefMimeType = new Models.NdefMimeTypeModel();

            TemperatureDataModel.PropertyChanged += TemperatureDataModel_PropertyChanged;
        }

        public async Task InitAsync()
        {
            await _ndef.InitTagReaderAsync(OnTagReaderStatus);
            _ndef.TagConnected += OnTagConnectedAsync;
            _ndef.TagDisconnected += OnTagDisconnectedAsync;
        }

        public async Task DeInitAsync()
        {
            _ndef.TagConnected -= OnTagConnectedAsync;
            _ndef.TagDisconnected -= OnTagDisconnectedAsync;
            await _ndef.DeInitTagReaderAsync();
        }

        private void OnTagReaderStatus(object sender, Plugin.Ndef.TagReaderStatusChangedEventArgs e)
        {
            Models.TagModel.CStatus status = new Models.TagModel.CStatus
            {
                Reader = e.Status.Reader,
                IsWriteSupported = e.Status.IsWriteSupported,
                IsAutoReadSupported = e.Status.IsAutoReadSupported,
            };
            TagModel.Status = status;
        }

        private async void OnTagConnectedAsync(object sender, Plugin.Ndef.TagConnectedEventArgs e)
        {
            // Check validity.
            if (e.NdefRecords == null || e.NdefRecords.Count == 0)
            {
                return;
            }

            // Check if NHS tag.
            if (!e.IsNhsTag)
            {
                DataProcessEvent?.Invoke(this, new DataProcessEventArgs { Op = DataProcessOp.NonNhsTag });
                return;
            }

            bool isTagChanged = e.TagId != TagModel.TagId;
            TagModel.IsTagConnected = true;
            TagModel.TagId = e.TagId;
            // Get config time and check if changed.
            var prevTagConfigTimestamp = TagModel.TagConfigTimestamp;
            bool isTagConfigChanged = false;
            foreach (var ndefRecord in e.NdefRecords)
            {
                ResponseHandler response = new ResponseHandler(this, ndefRecord, true); // parse only
                if (response.ConfigTimestamp != new DateTime() && response.ConfigTimestamp !=
                    TagModel.TagConfigTimestamp)
                {
                    TagModel.TagConfigTimestamp = response.ConfigTimestamp;
                    Debug.WriteLine($"&&&&& Setting config timestamp {TagModel.TagConfigTimestamp}");
                    isTagConfigChanged = true;
                    break;
                }
            }

            // We need to check if this is the same tag previously used or a new one.
            // Or the config has changed.
            // For a new one or config, reset of all Models are required.
            // Also an event is invoked so that upper layer can init data models from cached data (dBase).
            if (isTagChanged || isTagConfigChanged)
            {
                // Reset all data models.
                ResetModels();
                DataProcessEvent?.Invoke(this, new DataProcessEventArgs { Op = DataProcessOp.RefreshDataFromCache });
            }
            else
            {
                // Need to reset NDEF models.
                NdefText.Reset(null, true);
                NdefUrl.Reset(null, true);
                NdefMimeType.Reset(null, true);
            }

            Debug.WriteLine($"&&&&&&&&&&&&&&&&&& First read");

            // Handle the response obtained on first tap (measurement status).
            foreach (var ndefRecord in e.NdefRecords)
            {
                new ResponseHandler(this, ndefRecord);
            }

            // Tell that we are starting with tag reading.
            _isDataProcess = true;
            int configLen = (int)MeasurementStatusModel.Status.NumberOfMeasurements;
            int temperatureLen = TemperatureDataModel.TemperatureData.Data.Length;
            int temperatureRetrieved = TemperatureDataModel.TemperatureData.NumData;

            int total = Math.Max(configLen, temperatureLen);
            int current = temperatureRetrieved;

            DataProcessEvent?.Invoke(this, new DataProcessEventArgs
            {
                Op = DataProcessOp.Begin,
                Current = current,
                Total = total,
            });

            var ndefMessage = new NdefMessage();
            ndefMessage.AddRange(e.NdefRecords);
            NdefAccessEvent?.Invoke(this, new NdefAccessEventArgs
            {
                NfcId = NfcIdModel.NfcId,
                Timestamp = DateTime.UtcNow,
                NdefAccess = ENdefAccess.Read,
                Data = ndefMessage.ToByteArray(),
            });

            // Invoke Text and URL model events.
            NdefText.Invoke();
            NdefUrl.Invoke();

            // Handle rest of the data.
            if (TagModel.Status.IsWriteSupported)
            {
                // Read all data by Command mechanism.
                if (!IsConfiguringTag)
                {
                    await CommandHandler.GetConfigAsync();
                    await CommandHandler.GetDataAsync();
                    // In iOS >= 13, end the seesion here.
                    if (Device.RuntimePlatform == Device.iOS)
                    {
                        await _ndef.DeInitTagReaderAsync();
                        OnTagDisconnectedAsync(this, new Plugin.Ndef.TagDisconnectedEventArgs());
                    }
                }
            }
            else
            {
                // Read all data by Autoread mechanism.
                await ReadAllTagDataByAutoread();
            }

            // In iOS we cannot detect tag removal. We generate disconnect event here.
            if (Device.RuntimePlatform == Device.iOS && !IsConfiguringTag)
            {
                OnTagDisconnectedAsync(this, new Plugin.Ndef.TagDisconnectedEventArgs());
            }

            if (_isDataProcess)
            {
                _isDataProcess = false;
                configLen = (int)MeasurementStatusModel.Status.NumberOfMeasurements;
                temperatureLen = TemperatureDataModel.TemperatureData.Data.Length;
                temperatureRetrieved = TemperatureDataModel.TemperatureData.NumData;

                total = Math.Max(configLen, temperatureLen);
                current = temperatureRetrieved;

                DataProcessEvent?.Invoke(this, new DataProcessEventArgs
                {
                    Op = DataProcessOp.End,
                    Current = current,
                    Total = total,
                });
            }
        }

        public event EventHandler IosNfcDisabled;

        private void OnTagDisconnectedAsync(object sender, Plugin.Ndef.TagDisconnectedEventArgs e)
        {
            TagModel.IsTagConnected = false;
            if (Device.RuntimePlatform == Device.iOS)
            {
                IosNfcDisabled?.Invoke(this, null);
            }
            if (_isDataProcess)
            {
                _isDataProcess = false;
                int configLen = (int)MeasurementStatusModel.Status.NumberOfMeasurements;
                int temperatureLen = TemperatureDataModel.TemperatureData.Data.Length;
                int temperatureRetrieved = TemperatureDataModel.TemperatureData.NumData;

                int total = Math.Max(configLen, temperatureLen);
                int current = temperatureRetrieved;

                DataProcessEvent?.Invoke(this, new DataProcessEventArgs
                {
                    Op = DataProcessOp.End,
                    Current = current,
                    Total = total,
                });
            }
        }

        public void ResetModels(bool isInvokePropertyChange = false)
        {
            AccelerometerConfigModel.Reset(null, isInvokePropertyChange);
            AccelerometerStatusModel.Reset(null, isInvokePropertyChange);
            HumidityConfigModel.Reset(null, isInvokePropertyChange);
            HumidityDataModel.Reset(null, null, isInvokePropertyChange);
            HumidityStatusModel.Reset(null, isInvokePropertyChange);
            MeasurementConfigModel.Reset(null, isInvokePropertyChange);
            MeasurementStatusModel.Reset(null, isInvokePropertyChange);
            NfcIdModel.Reset(null, isInvokePropertyChange);
            TemperatureConfigModel.Reset(null, isInvokePropertyChange);
            TemperatureDataModel.Reset(null, null, isInvokePropertyChange);
            TemperatureStatusModel.Reset(null, isInvokePropertyChange);
            UidModel.Reset(isInvokePropertyChange);
            VersionModel.Reset(null, isInvokePropertyChange);
            EventDataModel.Reset(null, null, isInvokePropertyChange);
            NdefText.Reset(null, isInvokePropertyChange);
            NdefUrl.Reset(null, isInvokePropertyChange);
            NdefMimeType.Reset(null, isInvokePropertyChange);
        }

        private void TemperatureDataModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Models.TemperatureDataModel.TemperatureData))
            {
                int configLen = (int)MeasurementStatusModel.Status.NumberOfMeasurements;
                int temperatureLen = TemperatureDataModel.TemperatureData.Data.Length;
                int temperatureRetrieved = TemperatureDataModel.TemperatureData.NumData;

                int total = Math.Max(configLen, temperatureLen);
                int current = temperatureRetrieved;

                DataProcessEvent?.Invoke(this, new DataProcessEventArgs
                {
                    Op = DataProcessOp.Retrieval,
                    Current = current,
                    Total = total,
                });
            }
        }

        public async Task<bool> IosRefreshCmdAsync()
        {
            await _ndef.DeInitTagReaderAsync();
            await _ndef.InitTagReaderAsync(OnTagReaderStatus);
            return true;
        }

        public async Task IosInitTagReaderAsync()
        {
            await _ndef.InitTagReaderAsync(OnTagReaderStatus);
        }
        public async Task IosDeInitTagReaderAsync()
        {
            await _ndef.DeInitTagReaderAsync();
        }

        private async Task ReadAllTagDataByAutoread()
        {
            int count = 0;
            int errCount = 0;
            while (TagModel.IsTagConnected)
            {
                var rspStatus = await GetAutoreadRspAsync();
                switch (rspStatus)
                {
                    case RspResult.Ok:
                        count++;
                        errCount = 0;
                        Debug.WriteLine($"---- read {count}");
                        break;
                    case RspResult.NoMoreResponse:
                        Debug.WriteLine($"---- ALL MESSAGES READ");
                        return;
                    case RspResult.Error:
                        Debug.WriteLine($"---- error {count}");
                        if (++errCount > 10)
                        {
                            return;
                        }
                        else
                        {
                            break;
                        }
                }
            }
        }

        private enum RspResult
        {
            Ok,
            NoMoreResponse,
            Error,
        }

        private async Task<RspResult> GetAutoreadRspAsync()
        {
            bool isError = false;
            ResponseHandler rspHandler = null;
            try
            {
                var (status, rspList) = await _ndef.ReadAsync();
                if (status == Plugin.Ndef.Status.OK)
                {
                    var ndefMessage = new NdefMessage();
                    ndefMessage.AddRange(rspList);
                    NdefAccessEvent?.Invoke(this, new NdefAccessEventArgs
                    {
                        NfcId = NfcIdModel.NfcId,
                        Timestamp = DateTime.UtcNow,
                        NdefAccess = ENdefAccess.Read,
                        Data = ndefMessage.ToByteArray(),
                    });

                    // To check if we had full readout before tap and only topup is needed.
                    int configLen = (int)MeasurementStatusModel.Status.NumberOfMeasurements;
                    int measurementBufferLen = TemperatureDataModel.TemperatureData.Data.Length;
                    int len = Math.Max(configLen, measurementBufferLen);
                    int measurementRetrieved = TemperatureDataModel.TemperatureData.NumData;
                    ////System.Diagnostics.Debug.WriteLine($"====BEFORE-configLen,measurementBufferLen,measurementRetrieved({configLen}, {measurementBufferLen}, {measurementRetrieved})");
                    bool isTopup = false;
                    if (len > measurementRetrieved)
                    {
                        isTopup = true;
                    }

                    foreach (var rsp in rspList)
                    {
                        rspHandler = new ResponseHandler(this, rsp);

                        if (rspHandler.IsError)
                        {
                            isError |= rspHandler.IsError;
                            continue;
                        }
                        else if (rspHandler.IsNoMoreResponse)
                        {
                            return RspResult.NoMoreResponse;
                        }
                        else
                        {
                            if (isTopup)
                            {
                                configLen = (int)MeasurementStatusModel.Status.NumberOfMeasurements;
                                measurementBufferLen = TemperatureDataModel.TemperatureData.Data.Length;
                                len = Math.Max(configLen, measurementBufferLen);
                                measurementRetrieved = TemperatureDataModel.TemperatureData.NumData;
                                ////       System.Diagnostics.Debug.WriteLine($"====AFTER-configLen,measurementBufferLen,measurementRetrieved({configLen}, {measurementBufferLen}, {measurementRetrieved})");
                                if (measurementRetrieved >= len)
                                {
                                    Debug.WriteLine($"====TOPUP COMPLETED({measurementRetrieved}, {len})");
                                    return RspResult.NoMoreResponse;
                                }
                            }
                            else
                            {
                                // No topup, continue till temperature data so that we can get extra accel and event data.
                                if (rspHandler.IsTemperatureData)
                                {
                                    Debug.WriteLine($"====NON-TOPUP COMPLETED({measurementRetrieved}, {len})");
                                    return RspResult.NoMoreResponse;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                isError = true;
                Helpers.ExceptionLogHelper.Log(TagModel.TagId, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
            }

            return isError == false ? RspResult.Ok : RspResult.Error;
        }


    }
}
