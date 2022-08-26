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
    public class MeasurementConfigModel
    {
        public class CConfig
        {
            // For GUI updates.
            [JsonIgnore]
            public bool IsReset { get; set; }
            [JsonIgnore]
            public bool IsUnknown { get; set; }

            public DateTime CurrentTime { get; set; }
            public TimeSpan StartupDelay { get; set; }
            public TimeSpan Interval { get; set; }
            public TimeSpan Duration { get; set; }
        }

        public CConfig Config { get; set; } = new CConfig
        {
            IsReset = true,
            CurrentTime = new DateTime(),
            StartupDelay = TimeSpan.Zero,
            Duration = TimeSpan.Zero,
        };

        public void Reset(CConfig config = null, bool isInvokePropertyChange = false)
        {
            Config = config?? new CConfig
            {
                IsReset = true,
                CurrentTime = new DateTime(),
                StartupDelay = TimeSpan.Zero,
                Duration = TimeSpan.Zero,
            };
        }
    }
}
