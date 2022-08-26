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
    public class RtcAccuracyHelper
    {
        private static DateTime _initTimestamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static double _correctionFactor = 1;

        static public void CalculateCorrectionFactor(DateTime initTimestamp, DateTime finalTimestamp)
        {
            _initTimestamp = initTimestamp;

            if (finalTimestamp.CompareTo(initTimestamp) > 0 && DateTime.UtcNow.CompareTo(initTimestamp) > 0)
            {
                var worldSec = DateTime.UtcNow.Subtract(initTimestamp).TotalSeconds;
                var islandSec = (ulong)finalTimestamp.Subtract(initTimestamp).TotalSeconds;

                _correctionFactor = worldSec / islandSec;
            }
            else
            {
                _correctionFactor = 1;
            }
        }

        static public DateTime CorrectedTimestamp(DateTime timestamp)
        {
            var correctedTimestamp = timestamp;

            if (timestamp.CompareTo(_initTimestamp) > 0)
            {
                var delta = timestamp.Subtract(_initTimestamp).TotalSeconds * _correctionFactor;
                correctedTimestamp = _initTimestamp.AddSeconds(delta);
            }

            return correctedTimestamp;
        }

        static public DateTime CorrectedLocalTimestamp(DateTime timestamp)
        {
            var correctedLocalTimestamp = CorrectedTimestamp(timestamp);
            return correctedLocalTimestamp.ToLocalTime();
        }

        static public DateTime CorrectedTimestamp(DateTime initTimestamp, DateTime timestamp, double correctionFactor)
        {
            var correctedTimestamp = timestamp;

            if (timestamp.CompareTo(initTimestamp) > 0)
            {
                var delta = timestamp.Subtract(initTimestamp).TotalSeconds * _correctionFactor;
                correctedTimestamp = initTimestamp.AddSeconds(delta);
            }

            return correctedTimestamp;
        }

        static public DateTime CorrectedLocalTimestamp(DateTime initTimestamp, DateTime timestamp,
            double correctionFactor)
        {
            var correctedLocalTimestamp = CorrectedTimestamp(initTimestamp, timestamp, correctionFactor);
            return correctedLocalTimestamp.ToLocalTime();
        }


        static public TimeSpan CorrectedTimeSpan(TimeSpan timeSpan)
        {
            var correctedTimeSpan = timeSpan;

            var delta = correctedTimeSpan.TotalSeconds * _correctionFactor;
            correctedTimeSpan.Add(TimeSpan.FromSeconds(delta));


            return correctedTimeSpan;
        }

        static public double GetDrift()
        {
            return (1 - _correctionFactor) * 100;
        }
    }
}
