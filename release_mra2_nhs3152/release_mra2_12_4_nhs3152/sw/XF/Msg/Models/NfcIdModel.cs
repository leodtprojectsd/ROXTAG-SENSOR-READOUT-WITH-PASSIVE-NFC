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
    // Obtained from NDEF message.
    public class NfcIdModel : ObservableObject
    {
        byte[] _nfcId = { };

        public byte[] NfcId
        {
            get => _nfcId;
            set => SetProperty(ref _nfcId, value);
        }

        public void Reset(byte[] nfcId = null, bool isInvokePropertyChange = false)
        {
            var backup = _nfcId;

            _nfcId = nfcId?? new byte[]{ };

            if (isInvokePropertyChange)
                SetProperty(ref backup, _nfcId, nameof(NfcId));
        }
    }
}
