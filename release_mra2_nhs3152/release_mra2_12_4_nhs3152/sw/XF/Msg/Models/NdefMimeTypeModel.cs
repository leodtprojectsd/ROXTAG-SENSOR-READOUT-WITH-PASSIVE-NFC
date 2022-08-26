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

using MvvmHelpers;
using System;

namespace Msg.Models
{
    public class NdefMimeTypeModel : ObservableObject
    {
        string _ndefMimeType = string.Empty;
        public string NdefMimeType
        {
            get => _ndefMimeType;
            set => SetProperty(ref _ndefMimeType, _ndefMimeType + value + Environment.NewLine );
        }
        public void Reset(string ndefMimeType = null, bool isInvokePropertyChange = false)
        {
            var backup = _ndefMimeType;

            _ndefMimeType = ndefMimeType ?? string.Empty;

            if (isInvokePropertyChange)
                SetProperty(ref backup, _ndefMimeType, nameof(NdefMimeType));
        }

    }
}
