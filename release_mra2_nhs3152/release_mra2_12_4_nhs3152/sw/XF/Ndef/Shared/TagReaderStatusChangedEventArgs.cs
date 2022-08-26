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

using System;

namespace Plugin.Ndef
{
    /// <summary>
    /// tag reader sttaus event parameters.
    /// </summary>
    public class TagReaderStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Reader availability.
        /// </summary>
        public enum EReader
        {
            /// <summary>
            /// Not available
            /// </summary>
            NotAvailable,
            /// <summary>
            /// Available.
            /// </summary>
            Available,
        }

        /// <summary>
        /// Tag reader status event parameters.
        /// </summary>
        public class TagReaderStatus
        {
            /// <summary>
            /// Reader availability.
            /// </summary>
            public EReader Reader { get; set; }
            /// <summary>
            /// If write operation is supported.
            /// </summary>
            public bool IsWriteSupported { get; set; }
            /// <summary>
            /// If auto read operation is supported.
            /// </summary>
            public bool IsAutoReadSupported { get; set; }
        }

        TagReaderStatus _status = new TagReaderStatus();
        /// <summary>
        /// Tag reader status property.
        /// </summary>
        public TagReaderStatus Status
        {
            get => _status;
            set { _status = value; }
        }
    }
}
