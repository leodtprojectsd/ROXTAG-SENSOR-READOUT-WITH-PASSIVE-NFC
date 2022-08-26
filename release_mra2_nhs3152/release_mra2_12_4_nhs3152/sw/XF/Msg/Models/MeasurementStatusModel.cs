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
    public class MeasurementStatusModel : ObservableObject
    {
        public enum Measurement
        {
            Reset,
            Unknown,
            NotConfigured,
            Starting,
            Configured,
            Logging,
            Stopped,
        }

        public enum Failure
        {
            Reset,
            Unknown,
            NoFailure,
            Bod,
            Full,
            Expired,
        }


        public class CStatus : ICloneable, IEquatable<CStatus>
        {
            [JsonIgnore]
            public bool IsReset { get; set; }
            [JsonIgnore]
            public bool IsUnknown { get; set; }
            public Measurement Measurement { get; set; }
            public Failure Failure { get; set; }
            public DateTime ConfigTime { get; set; }
            public DateTime StartTime { get; set; }
            public TimeSpan RunningDuration { get; set; }
            public uint NumberOfMeasurements { get; set; }

            public object Clone()
            {
                var status = (CStatus)this.MemberwiseClone();
                return status;
            }

            public bool Equals(CStatus other)
            {
                var isEqual = false;

                if (other == this)
                    isEqual = true;

                return isEqual;
            }

        }

        CStatus _status = new CStatus
        {
            IsReset = true,
            ConfigTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            StartTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            RunningDuration = TimeSpan.Zero,
        };

        public CStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public void Reset(CStatus status = null, bool isInvokePropertyChange = false)
        {
            var backup = _status.Clone();

            _status = status?? new CStatus
            {
                IsReset = true,
                ConfigTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                StartTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                RunningDuration = TimeSpan.Zero,
            };

            if (isInvokePropertyChange)
                SetProperty(ref backup, _status, nameof(Status));
        }
    }
}
