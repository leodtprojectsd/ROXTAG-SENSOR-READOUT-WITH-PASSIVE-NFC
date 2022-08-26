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

using Monitor.MacOS;
using System.Globalization;
using System.Linq;

[assembly: Xamarin.Forms.Dependency(typeof(BuildDateTime))]
namespace Monitor.MacOS
{
    public class BuildDateTime : Interfaces.IBuildDateTime
    {
        public System.DateTime Get()
        {
            return RetrieveTimestampAsDateTime(); 
        }

        private string RetrieveTimestamp()
        {
            object attribute = System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(false).First(x => x.GetType().Name == "TimestampAttribute");
            return (string)attribute.GetType().GetProperty("Timestamp").GetValue(attribute);
        }
        private System.DateTime RetrieveTimestampAsDateTime()
        {
            return System.DateTime.ParseExact(RetrieveTimestamp(), "yyyy-MM-ddTHH:mm:ss.fffZ", null, DateTimeStyles.AssumeUniversal).ToUniversalTime();
        }
    }
}
