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
using Newtonsoft.Json;

namespace Helpers
{
    public class JsonConverterHelper
    {
        public class ByteArrayHexConverter : JsonConverter
        {
            private readonly string _separator;

            public ByteArrayHexConverter() => _separator = ":";
            public ByteArrayHexConverter(string separator = ":") => _separator = separator;

            public override bool CanConvert(Type objectType) => objectType == typeof(byte[]);
            public override bool CanRead => false;
            public override bool CanWrite => true;

            public override object ReadJson(
                JsonReader reader,
                Type objectType,
                object existingValue,
                JsonSerializer serializer) => throw new NotImplementedException();

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                byte[] array = (byte[])value;
                writer.WriteValue(BitConverter.ToString(array).Replace("-", _separator));
            }
        }

        public class DateTimeConverter : JsonConverter
        {
            private readonly string _format;

            public DateTimeConverter() => _format = "s";    // "s" denotes UTC format
            public DateTimeConverter(string format = "s") => _format = format;

            public override bool CanConvert(Type objectType) => objectType == typeof(DateTime);
            public override bool CanRead => false;
            public override bool CanWrite => true;

            public override object ReadJson(
                JsonReader reader,
                Type objectType,
                object existingValue,
                JsonSerializer serializer) => throw new NotImplementedException();

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                DateTime dateTime = (DateTime)value;
                writer.WriteValue(dateTime.ToString(_format));
            }
        }
    }
}
