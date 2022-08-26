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

namespace Helpers
{
    static public class GlobalHelper
    {
        public const float NotInitializedData = -85.1f;
        public const float TemperaturePlaceholder = 85.1f;

        // Converting float into string and vice versa takes the current culture separator as decimal point.
        // This can be comma instead of dot for some cultures. But comma is JSON separator and
        // we cannot use comma as decimal. The following will force dot in float to string conversions.
        public static IFormatProvider FormatProvider = new System.Globalization.CultureInfo("en-US");

    }
}
