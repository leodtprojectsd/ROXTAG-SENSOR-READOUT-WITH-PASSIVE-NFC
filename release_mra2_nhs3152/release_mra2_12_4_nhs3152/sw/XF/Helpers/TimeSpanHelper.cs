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
using System.Linq;

namespace Helpers
{
    public class TimeSpanHelper
    {
        static public string ReadableTimeSpan(TimeSpan timeSpan, int componentLimit = 4)
        {
            Func<Tuple<int, string>, string> tupleFormatter = t =>
                $"{t.Item1} {t.Item2}{(t.Item1 == 1 ? string.Empty : "s")}";

            var components = new List<Tuple<int, string>>
            {
                Tuple.Create((int)timeSpan.TotalDays, "day"),
                Tuple.Create(timeSpan.Hours, "hour"),
                Tuple.Create(timeSpan.Minutes, "minute"),
                Tuple.Create(timeSpan.Seconds, "second"),
            };

            components.RemoveAll(i => i.Item1 == 0);

            while (components.Count > componentLimit)
            {
                components.RemoveAt(components.Count - 1);
            }

            string extra = "";

            if (components.Count > 1)
            {
                var finalComponent = components[components.Count - 1];
                components.RemoveAt(components.Count - 1);
                extra = $" and {tupleFormatter(finalComponent)}";
            }

            return $"{string.Join(", ", components.Select(tupleFormatter))}{extra}";
        }

        static public string ShortReadableTimeSpan(TimeSpan timeSpan, int componentLimit = 4)
        {
            Func<Tuple<int, string>, string> tupleFormatter = t =>
                $"{t.Item1}{t.Item2}{(t.Item1 == 1 ? string.Empty : "")}";

            var components = new List<Tuple<int, string>>
            {
                Tuple.Create((int)timeSpan.TotalDays, "d"),
                Tuple.Create(timeSpan.Hours, "h"),
                Tuple.Create(timeSpan.Minutes, "m"),
                Tuple.Create(timeSpan.Seconds, "s"),
            };

            components.RemoveAll(i => i.Item1 == 0);

            while (components.Count > componentLimit)
            {
                components.RemoveAt(components.Count - 1);
            }

            string extra = "";

            if (components.Count > 1)
            {
                var finalComponent = components[components.Count - 1];
                components.RemoveAt(components.Count - 1);
                extra = $" {tupleFormatter(finalComponent)}";
            }

            var ret = $"{string.Join(", ", components.Select(tupleFormatter))}{extra}";
            return ret;
        }

    }
}
