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
using System.Runtime.CompilerServices;

namespace Helpers
{
    public class ExceptionLogEventArgs : EventArgs
    {
        public DateTime Timestamp { get; set; }
        public string TagId { get; set; }
        public string Description { get; set; }
    }

    static public class ExceptionLogHelper
    {
        static public event EventHandler<ExceptionLogEventArgs> ExceptionLogEvent;

        static public void Log(string tagId, string description)
        {
            //var caller = new StackTrace().GetFrame(1);
            //var str = $" - Caller: {caller.GetMethod()}";

            ExceptionLogEvent?.Invoke(null, new ExceptionLogEventArgs
            {
                Timestamp = DateTime.UtcNow,
                TagId = tagId,
                Description = description,
                //Description = description + str,
            });
        }

        static public string GetCurrentMethod([CallerMemberName] string callerName = "")
        {
            return callerName;
        }
    }
}
