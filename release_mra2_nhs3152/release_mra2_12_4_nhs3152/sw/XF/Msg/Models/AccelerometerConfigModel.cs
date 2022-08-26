/*
 * Copyright 2018-2019 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

using System;
using MvvmHelpers;
using Newtonsoft.Json;

namespace Msg.Models
{
    public class AccelerometerConfigModel
    {
        public class Shock
        {
            public bool IsEnabled { get; set; }
            public int Amplitude { get; set; }
            public TimeSpan WaitTime { get; set; }
            public int RingingAmplitude { get; set; }
            public int RingingCount { get; set; }
            public TimeSpan RingingDuration { get; set; }
        }

        public class Vibration
        {
            public bool IsEnabled { get; set; }
            public int Amplitude { get; set; }
            public int Frequency { get; set; }
            public TimeSpan Duration { get; set; }
        }

        public class Shake
        {
            public bool IsEnabled { get; set; }
            public int Amplitude { get; set; }
            public int Count { get; set; }
            public TimeSpan Duration { get; set; }
        }

        public class Tilt
        {
            public bool IsEnabled { get; set; }
            public int WaitTimeMs { get; set; }
        }

        public class CConfig
        {
            // For GUI updates.
            [JsonIgnore]
            public bool IsReset { get; set; }
            [JsonIgnore]
            public bool IsUnknown { get; set; }

            public Shock Shock { get; set; }
            public Shake Shake { get; set; }
            public Vibration Vibration { get; set; }
            public Tilt Tilt { get; set; }
        }

        public CConfig Config { get; set; } = new CConfig
        {
            IsReset = true,
            Shock = new Shock(),
            Shake = new Shake(),
            Vibration = new Vibration(),
            Tilt = new Tilt(),
        };

        public void Reset(CConfig config = null, bool isInvokePropertyChange = false)
        {
            Config = config?? new CConfig
            {
                IsReset = true,
                Shock = new Shock(),
                Shake = new Shake(),
                Vibration = new Vibration(),
                Tilt = new Tilt(),
            };
        }
    }
}
