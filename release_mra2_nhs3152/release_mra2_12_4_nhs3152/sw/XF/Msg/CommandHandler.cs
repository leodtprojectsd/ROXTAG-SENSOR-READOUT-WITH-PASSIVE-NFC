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

using Msg.Models;
using NdefLibrary.Ndef;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Msg
{
    // THIS FILE IS TAG FW DEPENDENT

    public class CommandHandler
    {
        private Plugin.Ndef.INdef _ndef;
        private Lib _lib;

        public CommandHandler(Lib lib)
        {
            _ndef = Plugin.Ndef.CrossNdef.Current;
            _lib = lib;
        }

        public async Task<bool> SetConfigCmdAsync(
            MeasurementConfigModel.CConfig measurementConfig,
            TemperatureConfigModel.CConfig temperatureConfig)
        {
            bool ok = false;
            try
            {
                if (Board.CanMeasureTemperature) {
                    ok = await SetMeasurementAndTemperatureConfigCmdAsync(measurementConfig, temperatureConfig);
                    _lib.MeasurementConfigModel.Config = measurementConfig;
                    _lib.TemperatureConfigModel.Config = temperatureConfig;
                }
            }
            catch (Exception ex)
            {
                Helpers.ExceptionLogHelper.Log(_lib.TagModel.TagId, ex.Message);
            }

            return ok;
        }

        public async Task<bool> SetConfigCmdAsync(
            MeasurementConfigModel.CConfig measurementConfig,
            TemperatureConfigModel.CConfig temperatureConfig,
            HumidityConfigModel.CConfig humidityConfig,
            AccelerometerConfigModel.CConfig accelerometerConfig)
        {
            List<bool> ok = new List<bool>();
            try
            {
                // Order is important. Measurement and temperature should be the last.
                // In current device app, measurement and temperature are handled together.
                if (Board.CanMeasureAcceleration)
                {
                    ok.Add(await SetAccelerometerConfigCmdAsync(accelerometerConfig));
                    _lib.AccelerometerConfigModel.Config = accelerometerConfig;
                }

                if (Board.CanMeasureHumidity)
                {
                    ok.Add(await SetHumidityConfigCmdAsync(humidityConfig));
                    _lib.HumidityConfigModel.Config = humidityConfig;
                }

                if (Board.CanMeasureTemperature)
                {
                    ok.Add(await SetMeasurementAndTemperatureConfigCmdAsync(measurementConfig, temperatureConfig));
                    _lib.MeasurementConfigModel.Config = measurementConfig;
                    _lib.TemperatureConfigModel.Config = temperatureConfig;
                }
            }
            catch (Exception ex)
            {
                Helpers.ExceptionLogHelper.Log(_lib.TagModel.TagId, Helpers.ExceptionLogHelper.GetCurrentMethod() + 
                    " - " + ex.Message);
            }
            return (ok.Count > 0) && (!ok.Contains(false));
        }

        public async Task GetConfigAsync()
        {
            ///* Board state and temperature config are packed together */
            //if (Board.CanMeasureTemperature)
            //{
            //    await NoParameterCmdAsync(Models.Protocol.MSG_ID("GETCONFIG"));
            //}
            //Commented out: already retrieved in initial NDEF message

            if (Board.CanMeasureAcceleration)
            {
                await NoParameterCmdAsync(Protocol.MSG_ID("GETACCELCONFIG"));
            }

            if (Board.CanMeasureHumidity)
            {
                await NoParameterCmdAsync(Protocol.MSG_ID("GETHUMIDITYCONFIG"));
            }
        }

        public async Task GetDataAsync()
        {
            await GetEventDataAsync();

            // Periodic data.
            // Get latest data first.
            // Then get all missing chunks.
            int configLen = (int)_lib.MeasurementStatusModel.Status.NumberOfMeasurements;
            int temperatureLen = _lib.TemperatureDataModel.TemperatureData.Data.Length;
            int numMeasurements = Math.Max(configLen, temperatureLen);
            int lastDataBegin = _lib.TemperatureDataModel.TemperatureData.RangeList.Count == 0 ?
                0 : _lib.TemperatureDataModel.TemperatureData.RangeList[
                    _lib.TemperatureDataModel.TemperatureData.RangeList.Count - 1].End + 1;

            if (numMeasurements > lastDataBegin)
            {
                Debug.WriteLine($"====NEW({lastDataBegin}, 0)");
                if (Board.BoardType == Board.EBoardType.TLogger)
                {
                    await GetMeasurementCmdAsync(lastDataBegin);
                }
                else if ((Board.BoardType == Board.EBoardType.SensorBoard) || 
                    (Board.BoardType == Board.EBoardType.SensorButton))
                {
                    await GetPeriodicDataCmdAsync(lastDataBegin);
                }
            }

            int count = _lib.TemperatureDataModel.TemperatureData.RangeList.Count;
            var rangeList = new List<Range>(count);
            for (int i = 0; i < count; i++)
            {
                rangeList.Add(new Range
                {
                    Begin = _lib.TemperatureDataModel.TemperatureData.RangeList[i].Begin,
                    End = _lib.TemperatureDataModel.TemperatureData.RangeList[i].End,
                });
            }
            int begin = 0;
            int end = 0;
            for (int i = 0; i < rangeList.Count; i++)
            {
                if (begin < rangeList[i].Begin)
                {
                    Debug.WriteLine($"----RANGE({rangeList[i].Begin}, {rangeList[i].End})");
                    end = rangeList[i].Begin - 1;
                    Debug.WriteLine($"----NEW({begin}, {end})");
                    if (Board.BoardType == Board.EBoardType.TLogger)
                    {
                        await GetMeasurementCmdAsync(begin, end);
                    }
                    else if ((Board.BoardType == Board.EBoardType.SensorBoard) || 
                        (Board.BoardType == Board.EBoardType.SensorButton))
                    {
                        await GetPeriodicDataCmdAsync(begin, end);
                    }
                }
                begin = rangeList[i].End + 1;
            }
        }

        private async Task<bool> SetMeasurementAndTemperatureConfigCmdAsync(
            MeasurementConfigModel.CConfig measurementConfig,
            TemperatureConfigModel.CConfig temperatureConfig)
        {
            var mimeHdr = new Protocol.MimePayloadHeader()
            {
                msgId = Protocol.MSG_ID("SETCONFIG"),
                direction = Protocol.Direction.Outgoing,
            };

            var payload = new Protocol.TLOGGER_MSG_CMD_SETCONFIG
            {
                currentTime = Convert.ToUInt32(DateTime.UtcNow.Subtract(
                    new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds),
                interval = Convert.ToUInt16(measurementConfig.Interval.TotalSeconds),
                startDelay = (uint)measurementConfig.StartupDelay.TotalSeconds,
                runningTime = (uint)measurementConfig.Duration.TotalSeconds,
                validMinimum = Convert.ToInt16(temperatureConfig.ConfiguredMin * 10),
                validMaximum = Convert.ToInt16(temperatureConfig.ConfiguredMax * 10),
            };

            var mimeData = new NdefRecord(NdefRecord.TypeNameFormatType.Mime, Protocol.MimeType)
            {
                Payload = Combine(DataTransform.Serialize(mimeHdr), DataTransform.Serialize(payload)),
            };

            var cmdList = new List<NdefRecord>
            {
                mimeData
            };

            (bool isOk, int numData, bool ignoredBool, int ignoredInt) = await SendCmdGetRspAsync(cmdList);
            return isOk;
        }

        private async Task<bool> SetHumidityConfigCmdAsync(HumidityConfigModel.CConfig humidityConfig)
        {
            var mimeHdr = new Protocol.MimePayloadHeader()
            {
                msgId = Protocol.MSG_ID("SETHUMIDITYCONFIG"),
                direction = Protocol.Direction.Outgoing,
            };

            var payload = new Protocol.APP_MSG_CMD_SETHUMIDITYCONFIG
            {
                validMinimum = Convert.ToUInt16(humidityConfig.ConfiguredMin * 10),
                validMaximum = Convert.ToUInt16(humidityConfig.ConfiguredMax * 10),
            };

            var mimeData = new NdefRecord(NdefRecord.TypeNameFormatType.Mime, Protocol.MimeType)
            {
                Payload = Combine(DataTransform.Serialize(mimeHdr), DataTransform.Serialize(payload)),
            };

            var cmdList = new List<NdefRecord>
            {
                mimeData
            };

            (bool isOk, int numData, bool ignoredBool, int ignoredInt) = await SendCmdGetRspAsync(cmdList);
            return isOk;
        }

        private async Task<bool> SetAccelerometerConfigCmdAsync(
            AccelerometerConfigModel.CConfig accelerometerConfig)
        {
            var mimeHdr = new Protocol.MimePayloadHeader()
            {
                msgId = Protocol.MSG_ID("SETACCELCONFIG"),
                direction = Protocol.Direction.Outgoing,
            };

            var shock = accelerometerConfig.Shock.IsEnabled ?
                new Protocol.ACCEL_SHOCK_PARAMS
                {
                    amplitude = Convert.ToUInt16(accelerometerConfig.Shock.Amplitude),
                    waitTime = (ushort)accelerometerConfig.Shock.WaitTime.Milliseconds,
                    ringingAmplitude = (ushort)accelerometerConfig.Shock.RingingAmplitude,
                    ringingCount = (byte)accelerometerConfig.Shock.RingingCount,
                    ringingDuration = (ushort)accelerometerConfig.Shock.RingingDuration.Milliseconds,
                }
                : new Protocol.ACCEL_SHOCK_PARAMS();

            var shake = accelerometerConfig.Shake.IsEnabled ?
                new Protocol.ACCEL_SHAKE_PARAMS
                {
                    amplitude = Convert.ToUInt16(accelerometerConfig.Shake.Amplitude),
                    count = (byte)accelerometerConfig.Shake.Count,
                    duration = (ushort)accelerometerConfig.Shake.Duration.Milliseconds,
                }
                : new Protocol.ACCEL_SHAKE_PARAMS();

            var vibration = accelerometerConfig.Vibration.IsEnabled ?
                new Protocol.ACCEL_VIBRATION_PARAMS
                {
                    amplitude = Convert.ToUInt16(accelerometerConfig.Vibration.Amplitude),
                    frequency = (byte)accelerometerConfig.Vibration.Frequency,
                    duration = (ushort)accelerometerConfig.Vibration.Duration.TotalSeconds,
                }
                : new Protocol.ACCEL_VIBRATION_PARAMS();

            var tilt = accelerometerConfig.Tilt.IsEnabled ?
                new Protocol.ACCEL_TILT_PARAMS
                {
                    waitTime = Convert.ToUInt16(accelerometerConfig.Tilt.WaitTimeMs)
                }
                : new Protocol.ACCEL_TILT_PARAMS();

            var setAccelConfigProtocol = new Protocol.APP_MSG_CMD_SETACCELCONFIG
            {
                shock = shock,
                shake = shake,
                vibration = vibration,
                tilt = tilt
            };

            Debug.WriteLine($"+++++++++ SHOCK: {setAccelConfigProtocol.shock.amplitude}, " +
                $"{setAccelConfigProtocol.shock.waitTime}, {setAccelConfigProtocol.shock.ringingAmplitude}, " +
                $"{setAccelConfigProtocol.shock.ringingCount}, {setAccelConfigProtocol.shock.ringingDuration} ");
            Debug.WriteLine($"+++++++++ TILT: {setAccelConfigProtocol.tilt.waitTime}");
            Debug.WriteLine($"+++++++++ VIBRATION: {setAccelConfigProtocol.vibration.amplitude}, " +
                $"{setAccelConfigProtocol.vibration.frequency}, {setAccelConfigProtocol.vibration.duration}");

            var mimeData = new NdefRecord(NdefRecord.TypeNameFormatType.Mime, Protocol.MimeType)
            {
                Payload = Combine(DataTransform.Serialize(mimeHdr), DataTransform.Serialize(setAccelConfigProtocol)),
            };

            var cmdList = new List<NdefRecord>
            {
                mimeData
            };

            (bool isOk, int numData, bool ignoredBool, int ignoredInt) = await SendCmdGetRspAsync(cmdList);
            return isOk;
        }

        private async Task<bool> GetEventDataAsync()
        {
            if (Board.BoardType == Board.EBoardType.TLogger)
            {
                uint eventMask = (uint)(
                    Protocol.APP_MSG_EVENT.APP_MSG_EVENT_PRISTINE |
                    Protocol.APP_MSG_EVENT.APP_MSG_EVENT_CONFIGURED |
                    Protocol.APP_MSG_EVENT.APP_MSG_EVENT_STARTING |
                    Protocol.APP_MSG_EVENT.APP_MSG_EVENT_LOGGING |
                    Protocol.APP_MSG_EVENT.APP_MSG_EVENT_STOPPED |
                    Protocol.APP_MSG_EVENT.APP_MSG_EVENT_TEMPERATURE_TOO_HIGH |
                    Protocol.APP_MSG_EVENT.APP_MSG_EVENT_TEMPERATURE_TOO_LOW |
                    Protocol.APP_MSG_EVENT.APP_MSG_EVENT_BOD |
                    Protocol.APP_MSG_EVENT.APP_MSG_EVENT_FULL |
                    Protocol.APP_MSG_EVENT.APP_MSG_EVENT_EXPIRED |
                    Protocol.APP_MSG_EVENT.APP_MSG_EVENT_I2C_ERROR |
                    Protocol.APP_MSG_EVENT.APP_MSG_EVENT_SPI_ERROR);

                byte info = (byte)(
                    Protocol.EVENT_INFO.EVENT_INFO_TIMESTAMP |
                    Protocol.EVENT_INFO.EVENT_INFO_ENUM);
                if (!await GetEventDataCmdAsync(eventMask, info, 0))
                {
                    return false;
                }
            }
            else if ((Board.BoardType == Board.EBoardType.SensorBoard) || (Board.BoardType == Board.EBoardType.SensorButton))
            {
                // First get all data except temperature and humidity events.
                // Then temperature and humidity events which have extra data.
                uint eventMask = (uint)(Protocol.APP_MSG_EVENT.APP_MSG_EVENT_ALL & ~(
                    Protocol.APP_MSG_EVENT.APP_MSG_EVENT_TEMPERATURE_TOO_HIGH |
                    Protocol.APP_MSG_EVENT.APP_MSG_EVENT_TEMPERATURE_TOO_LOW |
                    Protocol.APP_MSG_EVENT.APP_MSG_EVENT_HUMIDITY_TOO_HIGH |
                    Protocol.APP_MSG_EVENT.APP_MSG_EVENT_HUMIDITY_TOO_LOW));
                byte info = (byte)(
                    Protocol.EVENT_INFO.EVENT_INFO_INDEX |
                    Protocol.EVENT_INFO.EVENT_INFO_TIMESTAMP |
                    Protocol.EVENT_INFO.EVENT_INFO_ENUM);
                if (!await GetEventDataCmdAsync(eventMask, info, 0))
                {
                    return false;
                }

                eventMask = (uint)(
                    Protocol.APP_MSG_EVENT.APP_MSG_EVENT_TEMPERATURE_TOO_HIGH |
                    Protocol.APP_MSG_EVENT.APP_MSG_EVENT_TEMPERATURE_TOO_LOW |
                    Protocol.APP_MSG_EVENT.APP_MSG_EVENT_HUMIDITY_TOO_HIGH |
                    Protocol.APP_MSG_EVENT.APP_MSG_EVENT_HUMIDITY_TOO_LOW);
                info = (byte)(
                    Protocol.EVENT_INFO.EVENT_INFO_INDEX |
                    Protocol.EVENT_INFO.EVENT_INFO_TIMESTAMP |
                    Protocol.EVENT_INFO.EVENT_INFO_ENUM |
                    Protocol.EVENT_INFO.EVENT_INFO_DATA);
                if (!await GetEventDataCmdAsync(eventMask, info, 0))
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }


        // endOffset 0 means get all the rest of the events
        private async Task<bool> GetEventDataCmdAsync(uint eventMask, byte info, int beginOffset, int endOffset = 0)
        {
            var mimeHdr = new Protocol.MimePayloadHeader()
            {
                msgId = Protocol.MSG_ID("GETEVENTS"),
                direction = Protocol.Direction.Outgoing,
            };

            var payload = new Protocol.APP_MSG_CMD_GETEVENTS
            {
                index = (ushort)beginOffset,
                eventMask = eventMask,
                info = info,
            };

            var mimeData = new NdefRecord(NdefRecord.TypeNameFormatType.Mime, Protocol.MimeType)
            {
                Payload = Combine(DataTransform.Serialize(mimeHdr), DataTransform.Serialize(payload)),
            };

            var cmdList = new List<NdefRecord>
            {
                mimeData
            };

            int offset = beginOffset;
            int count = 0;
            while (_lib.TagModel.IsTagConnected)
            {
                payload.index = (ushort)offset;
                mimeData.Payload = Combine(DataTransform.Serialize(mimeHdr), DataTransform.Serialize(payload));
                (bool isOk, int numData, bool isMoreEventData, int lastEventIndex) = await SendCmdGetRspAsync(cmdList);
                if (!isOk)
                {
                    Debug.WriteLine($"---- error {count}");
                    return false;
                }

                offset += numData;
                if (numData == 0 || !isMoreEventData || (endOffset != 0 && offset >= endOffset))
                {
                    Debug.WriteLine($"---- ALL MESSAGES READ");
                    return true;
                }
                if (isMoreEventData)
                {
                    offset = lastEventIndex;
                }

                count++;
                Debug.WriteLine($"---- read {count}");
            }

            return false;
        }

        // endOffset 0 means get all the rest of the data
        private async Task<bool> GetPeriodicDataCmdAsync(int beginOffset, int endOffset = 0)
        {
            var mimeHdr = new Protocol.MimePayloadHeader()
            {
                msgId = Protocol.MSG_ID("GETPERIODICDATA"),
                direction = Protocol.Direction.Outgoing,
            };

            var payload = new Protocol.APP_MSG_CMD_GETPERIODICDATA
            {
                which = Board.CanMeasureHumidity ?
                    Protocol.APP_MSG_PERIODICDATA_TYPE.APP_MSG_PERIODICDATA_TYPE_ALL :
                    Protocol.APP_MSG_PERIODICDATA_TYPE.APP_MSG_PERIODICDATA_TYPE_TEMPERATURE,
                format = Protocol.APP_MSG_PERIODICDATA_FORMAT.APP_MSG_PERIODICDATA_FORMAT_FULL,
                offset = 0,
            };

            var mimeData = new NdefRecord(NdefRecord.TypeNameFormatType.Mime, Protocol.MimeType)
            {
                Payload = Combine(DataTransform.Serialize(mimeHdr), DataTransform.Serialize(payload)),
            };

            var cmdList = new List<NdefRecord>
            {
                mimeData
            };

            int offset = beginOffset;
            int count = 0;
            while (_lib.TagModel.IsTagConnected)
            {
                payload.offset = (ushort)offset;
                mimeData.Payload = Combine(DataTransform.Serialize(mimeHdr), DataTransform.Serialize(payload));

                (bool isOk, int numData, bool ignoredBool, int ignoredInt) = await SendCmdGetRspAsync(cmdList);
                if (!isOk)
                {
                    Debug.WriteLine($"---- error {count}");
                    return false;
                }

                offset += numData;
                if (numData == 0 || (endOffset != 0 && offset >= endOffset))
                {
                    Debug.WriteLine($"---- ALL MESSAGES READ");
                    return true;
                }

                count++;
                Debug.WriteLine($"---- read {count}");
            }

            return false;
        }

        // endOffset 0 means get all the rest of the data
        private async Task<bool> GetMeasurementCmdAsync(int beginOffset, int endOffset = 0)
        {
            var mimeHdr = new Protocol.MimePayloadHeader()
            {
                msgId = Protocol.MSG_ID("GETMEASUREMENTS"),
                direction = Protocol.Direction.Outgoing,
            };

            var payload = new Protocol.TLOGGER_MSG_CMD_GETMEASUREMENTS
            {
                offset = 0,
            };

            var mimeData = new NdefRecord(NdefRecord.TypeNameFormatType.Mime, Protocol.MimeType)
            {
                Payload = Combine(DataTransform.Serialize(mimeHdr), DataTransform.Serialize(payload)),
            };

            var cmdList = new List<NdefRecord>
            {
                mimeData
            };

            int offset = beginOffset;
            int count = 0;
            while (_lib.TagModel.IsTagConnected)
            {
                payload.offset = (ushort)offset;
                mimeData.Payload = Combine(DataTransform.Serialize(mimeHdr), DataTransform.Serialize(payload));

                (bool isOk, int numData, bool ignoredBool, int ignoredInt) = await SendCmdGetRspAsync(cmdList);
                if (!isOk)
                {
                    Debug.WriteLine($"---- error {count}");
                    return false;
                }

                offset += numData;
                if (numData == 0 || (endOffset != 0 && offset >= endOffset))
                {
                    Debug.WriteLine($"---- ALL MESSAGES READ");
                    return true;
                }

                count++;
                Debug.WriteLine($"---- read {count}");
            }

            return false;
        }

        private async Task<bool> NoParameterCmdAsync(byte msgId)
        {
            var mimeHdr = new Protocol.MimePayloadHeader()
            {
                msgId = msgId,
                direction = Protocol.Direction.Outgoing,
            };

            var mimeData = new NdefRecord(NdefRecord.TypeNameFormatType.Mime, Protocol.MimeType)
            {
                Payload = DataTransform.Serialize(mimeHdr),
            };

            var cmdList = new List<NdefRecord>
            {
                mimeData
            };

            (bool isOk, int numData, bool ignoredBool, int ignoredInt) = await SendCmdGetRspAsync(cmdList);
            return isOk;
        }

        private async Task<(bool isOk, int numData, bool isMoreEventData, int lastEventIndex)> SendCmdGetRspAsync(List<NdefRecord> cmdList)
        {
            ResponseHandler rspHandler = null;
            var cts = new CancellationTokenSource();

            try
            {
                var (status, rspList) = await _ndef.WriteReadAsync(cmdList);
                var wrNdefMessage = new NdefMessage();
                var rdNdefMessage = new NdefMessage();
                wrNdefMessage.AddRange(cmdList);
                rdNdefMessage.AddRange(rspList);
                if (status == Plugin.Ndef.Status.OK)
                {
                    _lib.NdefAccessEvent?.Invoke(this, new Lib.NdefAccessEventArgs
                    {
                        NfcId = _lib.NfcIdModel.NfcId,
                        Timestamp = DateTime.UtcNow,
                        NdefAccess = Lib.ENdefAccess.Write,
                        Data = wrNdefMessage.ToByteArray(),
                    });
                    _lib.NdefAccessEvent?.Invoke(this, new Lib.NdefAccessEventArgs
                    {
                        NfcId = _lib.NfcIdModel.NfcId,
                        Timestamp = DateTime.UtcNow,
                        NdefAccess = Lib.ENdefAccess.Read,
                        Data = rdNdefMessage.ToByteArray(),
                    });

                    bool isError = false;
                    int numData = 0;
                    bool isMoreEventData = false;
                    int lastEventIndex = 0;
                    foreach (var rsp in rspList)
                    {
                        rspHandler = new ResponseHandler(_lib, rsp);
                        isError |= rspHandler.IsError;
                        numData += rspHandler.NumData;
                        isMoreEventData |= rspHandler.IsMoreEventData;
                        lastEventIndex = rspHandler.LastEventIndex;
                    }
                    if (isError)
                    {
                        return (false, 0, false, 0);
                    }
                    else
                    {
                        return (true, numData, isMoreEventData, lastEventIndex);
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.ExceptionLogHelper.Log(_lib.TagModel.TagId, Helpers.ExceptionLogHelper.GetCurrentMethod() + 
                    " - " + ex.Message);
                Debug.WriteLine("Caught exception:" + _lib.TagModel.TagId + " - " +
                    Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
            }

            return (false, 0, false, 0);
        }

        private byte[] Combine(byte[] a, byte[] b)
        {
            byte[] c = new byte[a.Length + b.Length];
            Buffer.BlockCopy(a, 0, c, 0, a.Length);
            Buffer.BlockCopy(b, 0, c, a.Length, b.Length);
            return c;
        }
    }
}
