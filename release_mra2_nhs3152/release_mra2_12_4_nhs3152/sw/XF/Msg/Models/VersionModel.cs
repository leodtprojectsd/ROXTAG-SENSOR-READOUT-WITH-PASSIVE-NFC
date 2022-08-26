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

namespace Msg.Models
{
    public class VersionModel : ObservableObject
    {
        public class CVersion
        {
            public string FwVersion { get; set; }
            public string ApiVersion { get; set; }
        }

        CVersion _version = new CVersion
        {
            FwVersion = string.Empty,
            ApiVersion = string.Empty,
        };

        public CVersion Version
        {
            get => _version;
            set => SetProperty(ref _version, value);
        }


        public void Reset(CVersion version = null, bool isInvokePropertyChange = false)
        {
            var backup = _version;

            _version = version?? new CVersion
            {
                FwVersion = string.Empty,
                ApiVersion = string.Empty,
            };

            if (isInvokePropertyChange)
                SetProperty(ref backup, _version, nameof(Version));
        }
    }
}
