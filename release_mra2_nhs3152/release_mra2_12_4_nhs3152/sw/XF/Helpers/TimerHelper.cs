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
using System.Threading;
using Xamarin.Forms;

namespace Helpers
{
    public class TimerHelper
    {
        private CancellationTokenSource _cts;
        private bool _isActive;

        public TimerHelper(TimeSpan timeout, Action callback, bool isPeriodic = false)
        {
            _cts = new CancellationTokenSource();

            var cts = _cts;
            _isActive = true;
            Device.StartTimer(timeout, () =>
            {
                lock (this)
                {
                    if (cts.IsCancellationRequested)
                    {
                        _isActive = false;
                        return false;
                    }
                    callback.Invoke();
                    if (isPeriodic)
                    {
                        return true;
                    }
                    else
                    {
                        _isActive = false;
                        return false;
                    }
                }
            });
        }

        public void Cancel()
        {
            lock (this)
            {
                if (_isActive)
                {
                    Interlocked.Exchange(ref _cts, new CancellationTokenSource()).Cancel();
                }
            }
        }
    }
}
