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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Msg.Models;
using NdefLibrary.Ndef;

namespace Msg
{
    // THIS FILE IS TAG FW DEPENDENT

    internal class ResponseHandler
    {
        private readonly bool _isParseOnly;
        public bool IsError { get; private set; }
        public bool IsNoMoreResponse { get; private set; }
        public bool IsEventData { get; private set; }
        public bool IsTemperatureData { get; private set; }
        public bool IsHumidityData { get; private set; }
        public bool IsMoreEventData { get; private set; }
        public int LastEventIndex { get; private set; }
        public int NumData { get; private set; }
        public DateTime ConfigTimestamp { get; private set; }

        public struct H
        {
            public Type type;
            public Action<Lib, Object, byte[]> func;
        }
        public ResponseHandler(Lib lib, NdefRecord ndefRecord, bool isParseOnly = false)
        {
            Dictionary<byte, H> kvp = new Dictionary<byte, H>
            {
                { Protocol.MSG_ID("GETRESPONSE"), new H { type = typeof(Protocol.MSG_RESPONSE_RESULTONLY), func = HandleGetResponseResponse} },
                { Protocol.MSG_ID("GETVERSION"), new H { type = typeof(Protocol.MSG_RESPONSE_GETVERSION), func = HandleGetVersionResponse} },
                { Protocol.MSG_ID("GETNFCUID"), new H { type = typeof(Protocol.MSG_RESPONSE_GETNFCID), func = HandleGetNfcIdResponse} },
            };
            if ((Board.BoardType == Board.EBoardType.TLogger) || 
                (Board.BoardType == Board.EBoardType.SensorBoard) || 
                (Board.BoardType == Board.EBoardType.SensorButton))
            {
                kvp.Add(Protocol.MSG_ID("GETCONFIG"), 
                    new H { type = typeof(Protocol.TLOGGER_MSG_RESPONSE_GETCONFIG), func = HandleGetConfigResponse });
                kvp.Add(Protocol.MSG_ID("GETEVENTS"), new H { type = typeof(Protocol.APP_MSG_RESPONSE_GETEVENTS), func = HandleGetEventsResponse });
                kvp.Add(Protocol.MSG_ID("GETMEASUREMENTS"), new H { type = typeof(Protocol.TLOGGER_MSG_RESPONSE_GETMEASUREMENTS), func = HandleGetMeasurementsResponse });
                kvp.Add(Protocol.MSG_ID("GETPERIODICDATA"), new H { type = typeof(Protocol.APP_MSG_RESPONSE_GETPERIODICDATA), func = HandleGetPeriodicDataResponse });
                kvp.Add(Protocol.MSG_ID("SETCONFIG"), new H { type = typeof(Protocol.MSG_RESPONSE_RESULTONLY), func = HandleSetConfigResponse });
            }
            if ((Board.BoardType == Board.EBoardType.SensorBoard) || 
                (Board.BoardType == Board.EBoardType.SensorButton))
            {
                kvp.Add(Protocol.MSG_ID("GETACCELCONFIG"), new H { type = typeof(Protocol.APP_MSG_RESPONSE_GETACCELCONFIG), func = HandleGetAccelConfigResponse });
                kvp.Add(Protocol.MSG_ID("SETACCELCONFIG"), new H { type = typeof(Protocol.MSG_RESPONSE_RESULTONLY), func = HandleSetAccelConfigResponse });
            }
            if (Board.BoardType == Board.EBoardType.SensorBoard)
            {
                kvp.Add(Protocol.MSG_ID("GETHUMIDITYCONFIG"), new H { type = typeof(Protocol.APP_MSG_RESPONSE_GETHUMIDITYCONFIG), func = HandleGetHumidityConfigResponse });
                kvp.Add(Protocol.MSG_ID("SETHUMIDITYCONFIG"), new H { type = typeof(Protocol.MSG_RESPONSE_RESULTONLY), func = HandleSetHumidityConfigResponse });
            }
            if (Board.BoardType == Board.EBoardType.TAdherence)
            {
                kvp.Add(Protocol.MSG_ID("SETPRISTINE"), new H { type = typeof(Protocol.MSG_RESPONSE_RESULTONLY), func = HandleTAdherenceSetPristineResponse });
                kvp.Add(Protocol.MSG_ID("START"), new H { type = typeof(Protocol.MSG_RESPONSE_RESULTONLY), func = HandleTAdherenceStartResponse });
            }

            _isParseOnly = isParseOnly;

            if (ndefRecord.TypeNameFormat == NdefRecord.TypeNameFormatType.Mime)
            {
                if (ndefRecord.Payload is null)
                {
                    Helpers.ExceptionLogHelper.Log(lib.TagModel.TagId, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - Empty MIME data payload");
                    Debug.WriteLine("Caught error: Empty MIME data payload");
                    IsError = true;
                    return;
                }
                object _mimeHdr = DataTransform.Deserialize(ndefRecord.Payload, typeof(Protocol.MimePayloadHeader));
                if (_mimeHdr == null)
                {
                    Helpers.ExceptionLogHelper.Log(lib.TagModel.TagId, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - Wrong MIME data payload");
                    Debug.WriteLine("Caught error: Wrong MIME data payload");
                    IsError = true;
                    return;
                }
                Protocol.MimePayloadHeader mimeHdr = (Protocol.MimePayloadHeader)_mimeHdr;

                // Make sure it is incoming.
                if (mimeHdr.direction != Protocol.Direction.Incoming)
                {
                    Helpers.ExceptionLogHelper.Log(lib.TagModel.TagId, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - NDEF message incoming is not set");
                    Debug.WriteLine("Caught error:" + lib.TagModel.TagId + " - " + Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + "NDEF message incoming is not set");
                    IsError = true;
                    return;
                }

                byte[] rawStruct = null;
                byte[] rawData = null;
                int mimePayloadHeaderLen = Marshal.SizeOf(typeof(Protocol.MimePayloadHeader));

                kvp.TryGetValue(mimeHdr.msgId, out H h);
                if (h.type != null)
                {
                    int srcOffset = mimePayloadHeaderLen;
                    rawStruct = new byte[Marshal.SizeOf(h.type)];
                    if (srcOffset + rawStruct.Length <= ndefRecord.Payload.Length)
                    {
                        Buffer.BlockCopy(ndefRecord.Payload, srcOffset, rawStruct, 0, rawStruct.Length);
                        //dynamic response = Convert.ChangeType(DataTransform.Deserialize(rawStruct, type), type);
                        object response = DataTransform.Deserialize(rawStruct, h.type);

                        srcOffset += rawStruct.Length;
                        if (srcOffset < ndefRecord.Payload.Length)
                        {
                            rawData = new byte[ndefRecord.Payload.Length - srcOffset];
                            Buffer.BlockCopy(ndefRecord.Payload, srcOffset, rawData, 0, rawData.Length);
                        }
                        h.func(lib, response, rawData);
                    }
                    else
                    {
                        Helpers.ExceptionLogHelper.Log(lib.TagModel.TagId, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - NDEF record payload too short");
                        Debug.WriteLine("Caught error:" + lib.TagModel.TagId + " - " + Helpers.ExceptionLogHelper.GetCurrentMethod() + " - NDEF record payload too short");
                        IsError = true;
                        return;
                    }
                }
                else
                {
                    Helpers.ExceptionLogHelper.Log(lib.TagModel.TagId, Helpers.ExceptionLogHelper.GetCurrentMethod() + $" - Unknown NDEF record payload {mimeHdr.msgId}");
                    Debug.WriteLine("Caught error:" + lib.TagModel.TagId + " - " + Helpers.ExceptionLogHelper.GetCurrentMethod() + $" - Unknown NDEF record payload {mimeHdr.msgId}");
                    IsError = true;
                    return;
                }

                // Update MIME type.
                if (!ndefRecord.Type.SequenceEqual(Protocol.MimeType))
                {
                    lib.NdefMimeType.NdefMimeType = Encoding.UTF8.GetString(ndefRecord.Type);
                }
            }
            else if (ndefRecord.TypeNameFormat == NdefRecord.TypeNameFormatType.NfcRtd)
            {
                string text = string.Empty;
                switch (Encoding.UTF8.GetString(ndefRecord.Type, 0, ndefRecord.Type.Length))
                {
                    case "T":
                        int lanLen = ndefRecord.Payload[0];
                        int textLen = ndefRecord.Payload.Length - lanLen - 1;
                        if (textLen != 0)
                        {
                            text = Encoding.UTF8.GetString(
                                ndefRecord.Payload, lanLen + 1, ndefRecord.Payload.Length - lanLen - 1).Replace("\0","");
                        }
                        lib.NdefText.NdefTextIn = text;
                        break;

                    case "U":
                        text = Encoding.UTF8.GetString(ndefRecord.Payload);
                        lib.NdefUrl.NdefUrlIn = text;
                        break;
                }
            }
        }

        private void HandleGetConfigResponse(Lib lib, Object response, byte[] ignored)
        {
            Protocol.TLOGGER_MSG_RESPONSE_GETCONFIG r = (Protocol.TLOGGER_MSG_RESPONSE_GETCONFIG)response;

            // The order of updating Models is important. Measurement status model should be first as
            // icons can be disabled based on it.
            if (r.result != (uint)Protocol.MSG_ERR.MSG_OK)
            {
                IsError = true;
                return;
            }

            // Measurement.
            MeasurementStatusModel.Measurement measurement;
            if ((r.status & (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_STOPPED) != 0)
            {
                measurement = MeasurementStatusModel.Measurement.Stopped;
            }
            else if ((r.status & (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_LOGGING) != 0)
            {
                measurement = MeasurementStatusModel.Measurement.Logging;
            }
            else if ((r.status & (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_STARTING) != 0)
            {
                measurement = MeasurementStatusModel.Measurement.Starting;
            }
            else if ((r.status & (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_CONFIGURED) != 0)
            {
                measurement = MeasurementStatusModel.Measurement.Configured;
            }
            else if ((r.status & (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_PRISTINE) != 0)
            {
                measurement = MeasurementStatusModel.Measurement.NotConfigured;
            }
            else
            {
                measurement = MeasurementStatusModel.Measurement.Unknown;
            }

            // Failure.
            MeasurementStatusModel.Failure failure = MeasurementStatusModel.Failure.NoFailure;
            if ((r.status & (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_BOD) != 0)
            {
                failure = MeasurementStatusModel.Failure.Bod;
            }
            else if ((r.status & (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_EXPIRED) != 0)
            {
                failure = MeasurementStatusModel.Failure.Expired;
            }
            else if ((r.status & (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_FULL) != 0)
            {
                failure = MeasurementStatusModel.Failure.Full;
            }
            else
            {
                failure = MeasurementStatusModel.Failure.NoFailure;
            }

            // Timestamps.
            ConfigTimestamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(r.configTime);
            DateTime startTimestamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(r.startTime);
            DateTime currentTimestamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(r.currentTime);

            // Calculate RTC accuracy factor.
            if (r.startTime > 0)
            {
                Helpers.RtcAccuracyHelper.CalculateCorrectionFactor(startTimestamp, currentTimestamp);
            }

            // Measurement status and config.
            MeasurementStatusModel.CStatus updatedMeasurementStatus = null;
            if (((r.status & (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_LOGGING) != 0) ||
                ((r.status & (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_STARTING) != 0) ||
                ((r.status & (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_CONFIGURED) != 0) ||
                ((r.status & (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_STOPPED) != 0))
            {
                lib.MeasurementConfigModel.Config = new MeasurementConfigModel.CConfig
                {
                    CurrentTime = ConfigTimestamp,
                    StartupDelay = TimeSpan.FromSeconds(r.startDelay),
                    Interval = TimeSpan.FromSeconds(r.interval),
                    Duration = TimeSpan.FromSeconds(r.runningTime),
                };
                updatedMeasurementStatus = new MeasurementStatusModel.CStatus
                {
                    Measurement = measurement,
                    Failure = failure,
                    StartTime = startTimestamp,
                    ConfigTime = ConfigTimestamp,
                    RunningDuration = currentTimestamp.CompareTo(startTimestamp) > 0
                        &&
                        ((r.status & (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_LOGGING) != 0
                        || (r.status & (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_STOPPED) != 0)
                        ?
                        currentTimestamp.Subtract(startTimestamp) : TimeSpan.Zero,
                    NumberOfMeasurements = r.count,
                };
                if (currentTimestamp.CompareTo(startTimestamp) > 0)
                {
                    updatedMeasurementStatus.RunningDuration = currentTimestamp.Subtract(startTimestamp);
                }
                else
                {
                    updatedMeasurementStatus.RunningDuration = TimeSpan.FromSeconds(r.count * r.interval);
                }

                // We need to keep temperature and humidity data sizes same as measurement count.
                // Enlarge the data buffers as required.
                if (r.count > lib.TemperatureDataModel.TemperatureData.Data.Length)
                {
                    float[] data = Enumerable.Repeat(Helpers.GlobalHelper.NotInitializedData, r.count).ToArray();
                    lib.TemperatureDataModel.TemperatureData.Data.CopyTo(data, 0);
                    lib.TemperatureDataModel.TemperatureData.Data = data;
                }
                if (r.count > lib.HumidityDataModel.HumidityData.Data.Length)
                {
                    float[] data = Enumerable.Repeat(Helpers.GlobalHelper.NotInitializedData, r.count).ToArray();
                    lib.HumidityDataModel.HumidityData.Data.CopyTo(data, 0);
                    lib.HumidityDataModel.HumidityData.Data = data;
                }

                Debug.WriteLine($"======> NumMeasurements from config: {updatedMeasurementStatus.NumberOfMeasurements}");
            }
            else
            {
                updatedMeasurementStatus = new MeasurementStatusModel.CStatus
                {
                    Measurement = measurement,
                    Failure = failure,
                };

                lib.MeasurementConfigModel.Config = new MeasurementConfigModel.CConfig
                {
                    IsUnknown = true,
                };
            }

            // Temperature alarm.
            TemperatureStatusModel.CStatus updatedTemperatureStatus = new TemperatureStatusModel.CStatus();
            if ((r.status & (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_TEMPERATURE_TOO_LOW) == 0)
            {
                if ((r.status & (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_TEMPERATURE_TOO_HIGH) == 0)
                {
                    updatedTemperatureStatus.Temperature = TemperatureStatusModel.Temperature.Normal;
                }
                else
                {
                    updatedTemperatureStatus.Temperature = TemperatureStatusModel.Temperature.High;
                }
            }
            else
            {
                if ((r.status & (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_TEMPERATURE_TOO_HIGH) == 0)
                {
                    updatedTemperatureStatus.Temperature = TemperatureStatusModel.Temperature.Low;
                }
                else
                {
                    updatedTemperatureStatus.Temperature = TemperatureStatusModel.Temperature.Both;
                }
            }
            if ((r.status & (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_CONFIGURED) != 0)
            {
                lib.TemperatureConfigModel.Config.IsConfigured = true;
                lib.TemperatureConfigModel.Config.ConfiguredMin = r.validMinimum / 10;
                lib.TemperatureConfigModel.Config.ConfiguredMax = r.validMaximum / 10;
                updatedTemperatureStatus.IsRecorded = r.count != 0;
                updatedTemperatureStatus.RecordedMin = r.attainedMinimum / 10.0f;
                updatedTemperatureStatus.RecordedMax = r.attainedMaximum / 10.0f;
            }
            else
            {
                updatedTemperatureStatus.Temperature = TemperatureStatusModel.Temperature.Unknown;
                updatedTemperatureStatus.IsUnknown = true;
            }

            // Humidity status.
            HumidityStatusModel.CStatus updatedHumidityStatus = new HumidityStatusModel.CStatus();
            if ((r.status & (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_HUMIDITY_TOO_LOW) == 0)
            {
                if ((r.status & (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_HUMIDITY_TOO_HIGH) == 0)
                {
                    updatedHumidityStatus.Humidity = HumidityStatusModel.Humidity.Normal;
                }
                else
                {
                    updatedHumidityStatus.Humidity = HumidityStatusModel.Humidity.High;
                }
            }
            else
            {
                if ((r.status & (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_HUMIDITY_TOO_HIGH) == 0)
                {
                    updatedHumidityStatus.Humidity = HumidityStatusModel.Humidity.Low;
                }
                else
                {
                    updatedHumidityStatus.Humidity = HumidityStatusModel.Humidity.Both;
                }
            }
            if ((r.status & (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_LOGGING) != 0)
            {
                updatedHumidityStatus.IsRecorded = lib.HumidityStatusModel.Status.IsRecorded;
                updatedHumidityStatus.RecordedMin = lib.HumidityStatusModel.Status.RecordedMin;
                updatedHumidityStatus.RecordedMax = lib.HumidityStatusModel.Status.RecordedMax;
            }
            else
            {
                updatedHumidityStatus.Humidity = HumidityStatusModel.Humidity.Unknown;
                updatedHumidityStatus.IsUnknown = true;
            }

            // Accelerometer alarm.
            AccelerometerStatusModel.CStatus updatedAccelerometerStatus = new AccelerometerStatusModel.CStatus
            {
                Counts = new AccelerometerStatusModel.Counts()
            };
            updatedAccelerometerStatus.Flags = new AccelerometerStatusModel.Flags
            {
                IsShock = ((r.status & (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_SHOCK) != 0),
                IsShake = ((r.status & (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_SHAKE) != 0),
                IsVibration = ((r.status & (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_VIBRATION) != 0),
                IsTilt = ((r.status & (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_TILT) != 0),
            };
            if ((r.status & (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_LOGGING) != 0)
            {
                updatedAccelerometerStatus.IsUnknown = false;
            }
            else
            {
                updatedAccelerometerStatus.IsUnknown = true;
                updatedAccelerometerStatus.Flags = new AccelerometerStatusModel.Flags();
            }
            
            if (!_isParseOnly)
            {
                // Some status's are observable. Make sure to update in this order, to maximize availibility of data.
                lib.TemperatureStatusModel.Status = updatedTemperatureStatus; 
                lib.MeasurementStatusModel.Status = updatedMeasurementStatus;
                lib.AccelerometerStatusModel.Status = updatedAccelerometerStatus;
                lib.HumidityStatusModel.Status = updatedHumidityStatus;
            }
        }

        private void HandleSetConfigResponse(Lib lib, Object response, byte[] ignored)
        {
            Protocol.MSG_RESPONSE_RESULTONLY r = (Protocol.MSG_RESPONSE_RESULTONLY)response;
            IsError = r.result != Protocol.MSG_ERR.MSG_OK;
        }

        private void HandleGetResponseResponse(Lib lib, Object response, byte[] ignored)
        {
            Protocol.MSG_RESPONSE_RESULTONLY r = (Protocol.MSG_RESPONSE_RESULTONLY)response;
            IsNoMoreResponse = r.result == Protocol.MSG_ERR.MSG_ERR_NO_RESPONSE;
        }

        private void HandleGetVersionResponse(Lib lib, Object response, byte[] ignored)
        {
            Protocol.MSG_RESPONSE_GETVERSION r = (Protocol.MSG_RESPONSE_GETVERSION)response;
            lib.VersionModel.Version = new VersionModel.CVersion
            {
                FwVersion = $"{r.swMajorVersion}.{r.swMinorVersion}",
                ApiVersion = $"{r.apiMajorVersion}.{r.apiMinorVersion}"
            }; ;
        }

        private void HandleGetNfcIdResponse(Lib lib, Object response, byte[] ignored)
        {
            Protocol.MSG_RESPONSE_GETNFCID r = (Protocol.MSG_RESPONSE_GETNFCID)response;
            if (!_isParseOnly)
            {
                lib.NfcIdModel.NfcId = new byte[] { r.nfcuid1, r.nfcuid2, r.nfcuid3, r.nfcuid5, r.nfcuid6, r.nfcuid7, r.nfcuid8 };
            }
        }

        private void HandleGetAccelConfigResponse(Lib lib, Object response, byte[] ignored)
        {
            Protocol.APP_MSG_RESPONSE_GETACCELCONFIG r = (Protocol.APP_MSG_RESPONSE_GETACCELCONFIG)response;
            if (r.result != (uint)Protocol.MSG_ERR.MSG_OK)
            {
                IsError = true;
                return;
            }

            AccelerometerConfigModel.CConfig accelerometer = new AccelerometerConfigModel.CConfig
            {
                Shock = new AccelerometerConfigModel.Shock(),
                Shake = new AccelerometerConfigModel.Shake(),
                Vibration = new AccelerometerConfigModel.Vibration(),
                Tilt = new AccelerometerConfigModel.Tilt(),
            };

            accelerometer.Shock.Amplitude = r.shock.amplitude;
            accelerometer.Shock.WaitTime = TimeSpan.FromMilliseconds(r.shock.waitTime);
            accelerometer.Shock.RingingAmplitude = r.shock.ringingAmplitude;
            accelerometer.Shock.RingingCount = r.shock.ringingCount;
            accelerometer.Shock.RingingDuration = TimeSpan.FromMilliseconds(r.shock.ringingDuration);
            if (r.shock.amplitude == 0 &&
                r.shock.waitTime == 0 &&
                r.shock.ringingAmplitude == 0 &&
                r.shock.ringingCount == 0 &&
                r.shock.ringingDuration == 0)
            {
                accelerometer.Shock.IsEnabled = false;
            }
            else
            {
                accelerometer.Shock.IsEnabled = true;
            }

            accelerometer.Shake.IsEnabled = false;

            accelerometer.Vibration.Amplitude = r.vibration.amplitude;
            accelerometer.Vibration.Frequency = r.vibration.frequency;
            accelerometer.Vibration.Duration = TimeSpan.FromSeconds(r.vibration.duration);
            if (r.vibration.amplitude == 0 &&
               r.vibration.frequency == 0 &&
               r.vibration.duration == 0)
            {
                accelerometer.Vibration.IsEnabled = false;
            }
            else
            {
                accelerometer.Vibration.IsEnabled = true;
            }

            accelerometer.Tilt.IsEnabled = true;

            lib.AccelerometerConfigModel.Config = accelerometer;

            AccelerometerStatusModel.CStatus status = new AccelerometerStatusModel.CStatus
            {
                Flags = new AccelerometerStatusModel.Flags(),
                IsReset = lib.AccelerometerStatusModel.Status.IsReset,
                IsUnknown = lib.AccelerometerStatusModel.Status.IsUnknown
            };
            status.Flags = lib.AccelerometerStatusModel.Status.Flags;
            status.Counts = new AccelerometerStatusModel.Counts
            {
                NumShocks = r.shockEventCount,
                NumShakes = r.shakeEventCount,
                NumVibrations = r.vibrationEventCount,
                NumTilts = r.tiltEventCount,
            };
            if (!_isParseOnly)
            {
                lib.AccelerometerStatusModel.Status = status;
            }
        }

        private void HandleSetAccelConfigResponse(Lib lib, Object response, byte[] data)
        {
            Protocol.MSG_RESPONSE_RESULTONLY r = (Protocol.MSG_RESPONSE_RESULTONLY)response;
            IsError = r.result != Protocol.MSG_ERR.MSG_OK;
        }

        private void HandleGetEventsResponse(Lib lib, Object response, byte[] data)
        {
            Protocol.APP_MSG_RESPONSE_GETEVENTS r = (Protocol.APP_MSG_RESPONSE_GETEVENTS)response;

            if ((r.info & (byte)Protocol.EVENT_INFO.EVENT_INFO_MORE) ==
                (byte)Protocol.EVENT_INFO.EVENT_INFO_MORE)
            {
                IsMoreEventData = true;
            }

            List<EventDataModel.Event> eventData = new List<EventDataModel.Event>();
            int idx = 0;
            for (int i = 0; i < r.count; i++)
            {
                EventDataModel.Event entry = new EventDataModel.Event();
                EventDataModel.Event entryOk = null;

                if ((r.info & (byte)Protocol.EVENT_INFO.EVENT_INFO_INDEX) ==
                    (byte)Protocol.EVENT_INFO.EVENT_INFO_INDEX)
                {
                    LastEventIndex = BitConverter.ToUInt16(data, idx);
                    idx += 2;
                }
                if ((r.info & (byte)Protocol.EVENT_INFO.EVENT_INFO_TIMESTAMP) ==
                    (byte)Protocol.EVENT_INFO.EVENT_INFO_TIMESTAMP)
                {
                    entry.Timestamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(BitConverter.ToUInt32(data, idx));
                    idx += 4;
                }
                if ((r.info & (byte)Protocol.EVENT_INFO.EVENT_INFO_ENUM) ==
                    (byte)Protocol.EVENT_INFO.EVENT_INFO_ENUM)
                {
                    switch ((uint)(1 << data[idx]))
                    {
                        case (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_PRISTINE:
                            entry.Id = EventDataModel.EventId.MeasurementPristine;
                            break;
                        case (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_CONFIGURED:
                            entry.Id = EventDataModel.EventId.MeasurementConfigured;
                            break;
                        case (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_STARTING:
                            entry.Id = EventDataModel.EventId.MeasurementStarting;
                            break;
                        case (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_LOGGING:
                            entry.Id = EventDataModel.EventId.MeasurementLogging;
                            break;
                        case (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_STOPPED:
                            entry.Id = EventDataModel.EventId.MeasurementStopped;
                            break;
                        case (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_TEMPERATURE_TOO_HIGH:
                            entry.Id = EventDataModel.EventId.TemperatureTooHigh;
                            break;
                        case (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_TEMPERATURE_TOO_LOW:
                            entry.Id = EventDataModel.EventId.TemperatureTooLow;
                            break;
                        case (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_BOD:
                            entry.Id = EventDataModel.EventId.BatteryBod;
                            break;
                        case (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_FULL:
                            entry.Id = EventDataModel.EventId.StorageFull;
                            break;
                        case (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_EXPIRED:
                            entry.Id = EventDataModel.EventId.DemoExpired;
                            break;
                        case (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_I2C_ERROR:
                            entry.Id = EventDataModel.EventId.I2cError;
                            break;
                        case (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_SPI_ERROR:
                            entry.Id = EventDataModel.EventId.SpiError;
                            break;
                        case (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_SHOCK:
                            entry.Id = EventDataModel.EventId.AccelerometerShock;
                            break;
                        case (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_SHAKE:
                            entry.Id = EventDataModel.EventId.AccelerometerShake;
                            break;
                        case (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_VIBRATION:
                            entry.Id = EventDataModel.EventId.AccelerometerVibration;
                            break;
                        case (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_TILT:
                            entry.Id = EventDataModel.EventId.AccelerometerTilt;
                            break;
                        case (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_SHOCK_CONFIGURED:
                            entry.Id = EventDataModel.EventId.AccelerometerShockConfigured;
                            break;
                        case (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_SHAKE_CONFIGURED:
                            entry.Id = EventDataModel.EventId.AccelerometerShakeConfigured;
                            break;
                        case (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_VIBRATION_CONFIGURED:
                            entry.Id = EventDataModel.EventId.AccelerometerVibrationConfigured;
                            break;
                        case (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_TILT_CONFIGURED:
                            entry.Id = EventDataModel.EventId.AccelerometerTiltConfigured;
                            break;
                        case (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_HUMIDITY_CONFIGURED:
                            entry.Id = EventDataModel.EventId.HumidityConfigured;
                            break;
                        case (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_HUMIDITY_TOO_HIGH:
                            entry.Id = EventDataModel.EventId.HumidityTooHigh;
                            break;
                        case (uint)Protocol.APP_MSG_EVENT.APP_MSG_EVENT_HUMIDITY_TOO_LOW:
                            entry.Id = EventDataModel.EventId.HumidityTooLow;
                            break;
                    }
                    idx += 1;
                    entry.Description = lib.EventDataModel.EventParamsArray[(int)entry.Id].Description;
                }
                if ((r.info & (byte)Protocol.EVENT_INFO.EVENT_INFO_DATA) ==
                    (byte)Protocol.EVENT_INFO.EVENT_INFO_DATA)
                {
                    // Timestamp of opposition event such as temp low --> temp OK.
                    DateTime timestamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(BitConverter.ToUInt32(data, idx));
                    idx += 4;
                    if (timestamp != new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                    {
                        entryOk = new EventDataModel.Event();
                        switch (entry.Id)
                        {
                            case EventDataModel.EventId.TemperatureTooHigh:
                            case EventDataModel.EventId.TemperatureTooLow:
                                entryOk.Id = EventDataModel.EventId.TemperatureOk;
                                break;
                            case EventDataModel.EventId.HumidityTooHigh:
                            case EventDataModel.EventId.HumidityTooLow:
                                entryOk.Id = EventDataModel.EventId.HumidityOk;
                                break;
                        }
                        entryOk.Timestamp = timestamp;
                        entryOk.Description = lib.EventDataModel.EventParamsArray[(int)entryOk.Id].Description;
                    }
                }

                eventData.Add(entry);
                if (entryOk != null)
                {
                    eventData.Add(entryOk);
                }
            }

            if (!_isParseOnly)
            {
                lib.EventDataModel.EventDataIn = new EventDataModel.DataIn
                {
                    ////Offset = offset,
                    Data = eventData.ToArray(),
                };
            }

            IsEventData = true;
            NumData = r.count;
        }

        private void HandleGetMeasurementsResponse(Lib lib, Object response, byte[] data)
        {
            Protocol.TLOGGER_MSG_RESPONSE_GETMEASUREMENTS r = (Protocol.TLOGGER_MSG_RESPONSE_GETMEASUREMENTS)response;

            if (r.result != (uint)Protocol.MSG_ERR.MSG_OK)
            {
                IsError = true;
                return;
            }

            if (data != null)
            {
                if (r.count != data.Length / 2)
                {
                    IsError = true;
                    return;
                }

                short[] sdata = new short[data.Length / 2];
                Buffer.BlockCopy(data, 0, sdata, 0, data.Length);
                float[] fdata = new float[sdata.Length];
                for (int i = 0; i < sdata.Length; i++)
                {
                    fdata[i] = sdata[i] / 10.0f;
                }

                if (!_isParseOnly)
                {
                    lib.TemperatureDataModel.TemperatureDataIn = new DataIn
                    {
                        Offset = r.offset,
                        Data = fdata,
                    };
                }
            }
            IsTemperatureData = true;
            NumData = r.count;

            Debug.WriteLine($"=======> Current Offset: {r.offset}");
            Debug.WriteLine($"=======> Current NumData: {r.count}");
        }

        private void HandleGetPeriodicDataResponse(Lib lib, Object response, byte[] data)
        {
            Protocol.APP_MSG_RESPONSE_GETPERIODICDATA r = (Protocol.APP_MSG_RESPONSE_GETPERIODICDATA)response;
            if (r.result != (uint)Protocol.MSG_ERR.MSG_OK)
            {
                IsError = true;
                return;
            }

            int interleave = 0;

            if ((uint)r.which ==
                    (uint)(Protocol.APP_MSG_PERIODICDATA_TYPE.APP_MSG_PERIODICDATA_TYPE_TEMPERATURE |
                    Protocol.APP_MSG_PERIODICDATA_TYPE.APP_MSG_PERIODICDATA_TYPE_HUMIDITY))
            {
                if (data != null)
                {

                    interleave = data.Length / 3;
                    float[] temperatureData = new float[interleave];
                    float[] humidityData = new float[interleave];
                    for (int i = 0; i < interleave; i++)
                    {
                        temperatureData[i] = BitConverter.ToInt16(data, i * 3) / 10.0f;
                        humidityData[i] = data[(i * 3) + 2] / 2.0f;
                    }

                    if (!_isParseOnly)
                    {
                        lib.TemperatureDataModel.TemperatureDataIn = new /*TemperatureDataModel.*/DataIn
                        {
                            Offset = r.offset,
                            Data = temperatureData,
                        };
                    }

                    if (!_isParseOnly)
                    {
                        lib.HumidityDataModel.HumidityDataIn = new /*HumidityDataModel.*/DataIn
                        {
                            Offset = r.offset,
                            Data = humidityData,
                        };
                    }
                }

                IsTemperatureData = true;
                IsHumidityData = true;
            }
            else if (r.which == Protocol.APP_MSG_PERIODICDATA_TYPE.APP_MSG_PERIODICDATA_TYPE_TEMPERATURE)
            {
                if (data != null)
                {
                    interleave = data.Length / 2;
                    float[] temperatureData = new float[interleave];
                    for (int i = 0; i < interleave; i++)
                    {
                        temperatureData[i] = BitConverter.ToInt16(data, i * 2) / 10.0f;
                    }

                    if (!_isParseOnly)
                    {
                        lib.TemperatureDataModel.TemperatureDataIn = new /*TemperatureDataModel.*/DataIn
                        {
                            Offset = r.offset,
                            Data = temperatureData,
                        };
                    }
                }
                IsTemperatureData = true;
            }
            else if (r.which == Protocol.APP_MSG_PERIODICDATA_TYPE.APP_MSG_PERIODICDATA_TYPE_HUMIDITY)
            {
                if (data != null)
                {
                    interleave = data.Length;
                    float[] humidityData = new float[interleave];
                    for (int i = 0; i < interleave; i++)
                    {
                        humidityData[i] = data[i] / 2.0f;
                    }

                    if (!_isParseOnly)
                    {
                        lib.HumidityDataModel.HumidityDataIn = new /*HumidityDataModel.*/DataIn
                        {
                            Offset = r.offset,
                            Data = humidityData,
                        };
                    }
                }
                IsHumidityData = true;
            }

            NumData = interleave;
        }

        private void HandleGetHumidityConfigResponse(Lib lib, Object response, byte[] ignored)
        {
            Protocol.APP_MSG_RESPONSE_GETHUMIDITYCONFIG r = (Protocol.APP_MSG_RESPONSE_GETHUMIDITYCONFIG)response;
            if (r.result != (uint)Protocol.MSG_ERR.MSG_OK)
            {
                IsError = true;
                return;
            }

            HumidityConfigModel.CConfig config = new HumidityConfigModel.CConfig
            {
                IsConfigured = r.validMinimum != 1000 || r.validMaximum != 0,
                ConfiguredMin = r.validMinimum / 10,
                ConfiguredMax = r.validMaximum / 10,
            };
            if (!_isParseOnly)
            {
                lib.HumidityConfigModel.Config = config;
            }

            HumidityStatusModel.CStatus status = new HumidityStatusModel.CStatus()
            {
                IsReset = lib.HumidityStatusModel.Status.IsReset,
                IsUnknown = lib.HumidityStatusModel.Status.IsUnknown,
                Humidity = lib.HumidityStatusModel.Status.Humidity,
                IsRecorded = (r.attainedMinimum == 1000 && r.attainedMaximum == 0) ? false : true,
                RecordedMin = r.attainedMinimum / 10.0f,
                RecordedMax = r.attainedMaximum / 10.0f,
            };
            if (!_isParseOnly)
            {
                lib.HumidityStatusModel.Status = status;
            }
        }

        private void HandleSetHumidityConfigResponse(Lib lib, Object response, byte[] ignored)
        {
            Protocol.MSG_RESPONSE_RESULTONLY r = (Protocol.MSG_RESPONSE_RESULTONLY)response;
            IsError = r.result != Protocol.MSG_ERR.MSG_OK;
        }

        private void HandleTAdherenceSetPristineResponse(Lib lib, Object response, byte[] ignored)
        {
            Protocol.MSG_RESPONSE_RESULTONLY r = (Protocol.MSG_RESPONSE_RESULTONLY)response;
            IsError = r.result != Protocol.MSG_ERR.MSG_OK;
        }

        private void HandleTAdherenceStartResponse(Lib lib, Object response, byte[] ignored)
        {
            Protocol.MSG_RESPONSE_RESULTONLY r = (Protocol.MSG_RESPONSE_RESULTONLY)response;
            IsError = r.result != Protocol.MSG_ERR.MSG_OK;
        }
    }
}
