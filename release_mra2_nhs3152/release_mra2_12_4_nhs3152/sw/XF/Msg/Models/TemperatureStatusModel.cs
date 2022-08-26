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

using MvvmHelpers;
using Newtonsoft.Json;
using System;

namespace Msg.Models
{
    public class TemperatureStatusModel : ObservableObject
    {
        public enum Temperature
        {
            Reset,
            Unknown,
            Normal,
            Low,
            High,
            Both,
        }

        public class CStatus : ICloneable, IEquatable<CStatus>
        {
            // For GUI updates.
            [JsonIgnore]
            public bool IsReset { get; set; }
            [JsonIgnore]
            public bool IsUnknown { get; set; }

            public Temperature Temperature { get; set; }
            public bool IsRecorded { get; set; }
            public float RecordedMin { get; set; }
            public float RecordedMax { get; set; }

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
            };

            if (isInvokePropertyChange)
                SetProperty(ref backup, _status, nameof(Status));
        }
    }
}
