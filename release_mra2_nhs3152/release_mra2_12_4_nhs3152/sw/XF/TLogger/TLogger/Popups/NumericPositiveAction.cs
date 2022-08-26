/*
 * Copyright 2020 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

using Xamarin.Forms;

namespace TLogger.Popups
{
    public class NumericPositiveTriggerAction : TriggerAction<Entry>
    {
        private string _previousValue = string.Empty;

        protected override void Invoke(Entry entry)
        {
            if (entry.Text.Length == 0)
            {
                /* Ok */
            }
            else if ((entry.Text.Length > 3) || !int.TryParse(entry.Text, out _))
            {
                /* Not Ok */
                entry.Text = _previousValue;
            }
            else
            {
                /* Ok */
                _previousValue = entry.Text;
            }
        }
    }
}
