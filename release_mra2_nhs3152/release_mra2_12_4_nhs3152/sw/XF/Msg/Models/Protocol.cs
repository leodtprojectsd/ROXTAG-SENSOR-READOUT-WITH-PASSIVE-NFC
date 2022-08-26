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

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Msg.Models
{
    // THIS FILE IS TAG FW DEPENDENT

    public class Protocol
    {
        /// <summary>
        /// Takes place in MIME type NDEF record payload.
        ///     |--------|-----------|----------------------|
        ///     | msg id | direction |         data         |
        ///     |--------|-----------|----------------------|
        /// </summary>

        private static readonly Dictionary<string, byte> defaultIds = new Dictionary<string, byte>
        {
            { "GETRESPONSE", 0x01 },
            { "GETVERSION", 0x02 },
            { "RESET", 0x03 },
            { "READREGISTER", 0x04 },
            { "WRITEREGISTER", 0x05 },
            { "READMEMORY", 0x06 },
            { "WRITEMEMORY", 0x07 },
            { "PREPAREDEBUG", 0x08 },
            { "GETUID", 0x09 },
            { "GETNFCUID", 0x0A },
            { "GETDIAGDATA", 0x3E }
        };

        private static readonly Dictionary<string, byte> tloggerIds = new Dictionary<string, byte>
        {
            { "GETMEASUREMENTS", 0x46 },
            { "GETCONFIG", 0x48 },
            { "SETCONFIG", 0x49 },
            { "MEASURETEMPERATURE", 0x50 },
            { "START", 0x5A },
            { "GETEVENTS", 0x5B },
            { "GETPERIODICDATA", 0x5E }
        };

        private static readonly Dictionary<string, byte> accelerationIds = new Dictionary<string, byte>
        {
            { "GETACCELCONFIG" , 0x5C },
            { "SETACCELCONFIG", 0x5D }
        };
        private static readonly Dictionary<string, byte> humidityIds = new Dictionary<string, byte>
        {
            { "GETHUMIDITYCONFIG" , 0x5F },
            { "SETHUMIDITYCONFIG", 0x60 }
        };

        public static byte MSG_ID(string msg)
        {
            if (!defaultIds.TryGetValue(msg, out byte id))
            {
                if (!tloggerIds.TryGetValue(msg, out id))
                {
                    if (Board.BoardType != Board.EBoardType.TLogger)
                    {
                        if (!accelerationIds.TryGetValue(msg, out id))
                        {
                            if (Board.BoardType == Board.EBoardType.SensorBoard)
                            {
                                humidityIds.TryGetValue(msg, out id);
                            }
                        }
                    }
                }
            }
            return id;
        }

        public static byte[] MimeType = Encoding.UTF8.GetBytes("n/p");

        public enum Direction : byte
        {
            Outgoing = 0,
            Incoming = 1,
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MimePayloadHeader
        {
            public byte msgId;
            public Direction direction;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MimePayload
        {
            public MimePayloadHeader header;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public byte[] data;
        }

        public enum MSG_ERR : uint
        {
            MSG_OK = 0,
            MSG_ERR_UNKNOWN_COMMAND = 0x10007,
            MSG_ERR_NO_RESPONSE = 0x1000B,
            MSG_ERR_INVALID_COMMAND_SIZE = 0x1000D,
            MSG_ERR_INVALID_PARAMETER = 0x1000E,
            MSG_ERR_INVALID_PRECONDITION = 0x1000F,
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MSG_RESPONSE_RESULTONLY
        {
            public MSG_ERR result;
        }

        public enum DEVICE_ID : uint
        {
            NHS3100_DEVICE = 0x4E310020,
            NHS3152_DEVICE = 0x4E315220,
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MSG_RESPONSE_GETVERSION
        {
            public ushort reserved;
            public ushort swMajorVersion;
            public ushort swMinorVersion;
            public ushort apiMajorVersion;
            public ushort apiMinorVersion;
            public DEVICE_ID deviceId;
        }

        public enum APP_MSG_EVENT : uint
        {
            APP_MSG_EVENT_PRISTINE = 1 << 0,
            APP_MSG_EVENT_CONFIGURED = 1 << 1,
            APP_MSG_EVENT_STARTING = 1 << 2,
            APP_MSG_EVENT_LOGGING = 1 << 3,
            APP_MSG_EVENT_STOPPED = 1 << 4,
            APP_MSG_EVENT_TEMPERATURE_TOO_HIGH = 1 << 5,
            APP_MSG_EVENT_TEMPERATURE_TOO_LOW = 1 << 6,
            APP_MSG_EVENT_BOD = 1 << 7,
            APP_MSG_EVENT_FULL = 1 << 8,
            APP_MSG_EVENT_EXPIRED = 1 << 9,
            APP_MSG_EVENT_I2C_ERROR = 1 << 10,
            APP_MSG_EVENT_SPI_ERROR = 1 << 11,
            APP_MSG_EVENT_SHOCK = 1 << 12,
            APP_MSG_EVENT_SHAKE = 1 << 13,
            APP_MSG_EVENT_VIBRATION = 1 << 14,
            APP_MSG_EVENT_TILT = 1 << 15,
            APP_MSG_EVENT_SHOCK_CONFIGURED = 1 << 16,
            APP_MSG_EVENT_SHAKE_CONFIGURED = 1 << 17,
            APP_MSG_EVENT_VIBRATION_CONFIGURED = 1 << 18,
            APP_MSG_EVENT_TILT_CONFIGURED = 1 << 19,
            APP_MSG_EVENT_HUMIDITY_CONFIGURED = 1 << 20,
            APP_MSG_EVENT_HUMIDITY_TOO_HIGH = 1 << 21,
            APP_MSG_EVENT_HUMIDITY_TOO_LOW = 1 << 22,

            APP_MSG_EVENT_COUNT = 23,

            APP_MSG_EVENT_ALL = (1 << (int)APP_MSG_EVENT_COUNT) - 1
        }

        public enum EVENT_INFO : byte
        {
            EVENT_INFO_INDEX = 1 << 0,
            EVENT_INFO_TIMESTAMP = 1 << 1,
            EVENT_INFO_ENUM = 1 << 2,
            EVENT_INFO_DATA = 1 << 3,
            EVENT_INFO_COUNT = 4,
            EVENT_INFO_NONE = 0,
            EVENT_INFO_ALL = EVENT_INFO_INDEX | EVENT_INFO_TIMESTAMP | EVENT_INFO_ENUM | EVENT_INFO_DATA,
            EVENT_INFO_MORE = 1 << 7
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TLOGGER_MSG_RESPONSE_GETCONFIG
        {
            public uint result;
            public uint configTime;
            public ushort interval;
            public uint startDelay;
            public uint runningTime;
            public short validMinimum;
            public short validMaximum;
            public short attainedMinimum;
            public short attainedMaximum;
            public ushort count;
            public uint status;
            public uint startTime;
            public uint currentTime;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TLOGGER_MSG_CMD_GETMEASUREMENTS
        {
            public ushort offset;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TLOGGER_MSG_CMD_SETCONFIG
        {
            public uint currentTime;
            public ushort interval;
            public uint startDelay;
            public uint runningTime;
            public short validMinimum;
            public short validMaximum;
        }

        public enum APP_MSG_TSEN_RESOLUTION : byte
        {
            APP_MSG_TSEN_RESOLUTION_7BITS = 2,
            APP_MSG_TSEN_RESOLUTION_8BITS = 3,
            APP_MSG_TSEN_RESOLUTION_9BITS = 4,
            APP_MSG_TSEN_RESOLUTION_10BITS = 5,
            APP_MSG_TSEN_RESOLUTION_11BITS = 6,
            APP_MSG_TSEN_RESOLUTION_12BITS = 7
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TLOGGER_MSG_CMD_MEASURETEMPERATURE
        {
            public APP_MSG_TSEN_RESOLUTION resolution;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct APP_MSG_CMD_GETEVENTS
        {
            public ushort index;
            public uint eventMask;
            public byte info;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct APP_MSG_RESPONSE_GETEVENTS
        {
            public ushort index;
            public uint eventMask;
            public byte info;
            public ushort count;

            // byte data[...];
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TLOGGER_MSG_RESPONSE_GETMEASUREMENTS
        {
            public uint result;
            public ushort offset;
            public byte count;
            public byte zero1;
            public byte zero2;
            public byte zero3;
            // ushort data[...];
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MSG_RESPONSE_GETUID
        {
            public uint uid1;
            public uint uid2;
            public uint uid3;
            public uint uid4;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MSG_RESPONSE_GETNFCID
        {
            public byte nfcuid1;
            public byte nfcuid2;
            public byte nfcuid3;
            public byte nfcuid4;
            public byte nfcuid5;
            public byte nfcuid6;
            public byte nfcuid7;
            public byte nfcuid8;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ACCEL_SHOCK_PARAMS
        {
            public ushort amplitude;
            public ushort waitTime;
            public ushort ringingAmplitude;
            public byte ringingCount;
            public ushort ringingDuration;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ACCEL_SHAKE_PARAMS
        {
            public ushort amplitude;
            public byte count;
            public ushort duration;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ACCEL_VIBRATION_PARAMS
        {
            public ushort amplitude;
            public byte frequency;
            public ushort duration;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ACCEL_TILT_PARAMS
        {
            public ushort waitTime;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct APP_MSG_CMD_SETACCELCONFIG
        {
            public ACCEL_SHOCK_PARAMS shock;
            public ACCEL_SHAKE_PARAMS shake;
            public ACCEL_VIBRATION_PARAMS vibration;
            public ACCEL_TILT_PARAMS tilt;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct APP_MSG_RESPONSE_GETACCELCONFIG
        {
            public uint result;
            public ACCEL_SHOCK_PARAMS shock;
            public ACCEL_SHAKE_PARAMS shake;
            public ACCEL_VIBRATION_PARAMS vibration;
            public ACCEL_TILT_PARAMS tilt;
            public ushort shockEventCount;
            public ushort vibrationEventCount;
            public ushort shakeEventCount;
            public ushort tiltEventCount;
        }

        public enum APP_MSG_PERIODICDATA_TYPE : byte
        {
            APP_MSG_PERIODICDATA_TYPE_TEMPERATURE = 0x01,
            APP_MSG_PERIODICDATA_TYPE_HUMIDITY = 0x02,
            APP_MSG_PERIODICDATA_TYPE_ALL = 0x03,
        }

        public enum APP_MSG_PERIODICDATA_FORMAT : byte
        {
            APP_MSG_PERIODICDATA_FORMAT_FULL = 0,
            APP_MSG_PERIODICDATA_FORMAT_RAW = 1,
        }

        public struct APP_MSG_CMD_GETPERIODICDATA
        {
            public APP_MSG_PERIODICDATA_TYPE which;
            public APP_MSG_PERIODICDATA_FORMAT format;
            public ushort offset;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct APP_MSG_RESPONSE_GETPERIODICDATA
        {
            public uint result;
            public APP_MSG_PERIODICDATA_TYPE which;
            public APP_MSG_PERIODICDATA_FORMAT format;
            public ushort offset;
            //byte data[...];
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct APP_MSG_CMD_SETHUMIDITYCONFIG
        {
            public ushort validMinimum;
            public ushort validMaximum;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct APP_MSG_RESPONSE_GETHUMIDITYCONFIG
        {
            public uint result;
            public ushort validMinimum;
            public ushort validMaximum;
            public ushort attainedMinimum;
            public ushort attainedMaximum;
        }
    }
}
