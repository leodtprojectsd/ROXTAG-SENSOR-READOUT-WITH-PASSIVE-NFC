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

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MvvmHelpers;

namespace Msg.Models
{
    public class TagModel : ObservableObject
    {
        public class CStatus : Plugin.Ndef.TagReaderStatusChangedEventArgs.TagReaderStatus { }

        CStatus _status = new CStatus();
        public CStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        bool _isTagConnected;
        public bool IsTagConnected
        {
            get => _isTagConnected;
            set => SetProperty(ref _isTagConnected, value);
        }

        string _tagId = string.Empty;
        public string TagId
        {
            get => _tagId;
            set => SetProperty(ref _tagId, value);
        }

        DateTime _tagConfigTimestamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public DateTime TagConfigTimestamp
        {
            get => _tagConfigTimestamp;
            set => SetProperty(ref _tagConfigTimestamp, value);
        }
    }
}
