/*
 * Copyright 2019 NXP
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
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Msg.Models
{
    public class NdefTextModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        List<string> _ndefText = new List<string>();

        public string NdefTextIn
        {
            set => _ndefText.Add(value.Replace("\0", ""));
        }

        public void Invoke()
        {
            OnPropertyChanged(nameof(NdefText));
        }

        public string[] NdefText
        {
            get => _ndefText.ToArray();
        }

        public void Reset(string[] ndefText = null, bool isInvokePropertyChange = false)
        {
            _ndefText = ndefText != null ? ndefText.ToList<string>() : new List<string>();
            if (isInvokePropertyChange)
                OnPropertyChanged(nameof(NdefText));
        }

    }
}
