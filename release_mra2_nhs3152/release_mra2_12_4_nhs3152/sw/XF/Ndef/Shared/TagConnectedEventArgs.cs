/*
 * Copyright 2018-2020 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

using NdefLibrary.Ndef;
using System;
using System.Collections.Generic;

namespace Plugin.Ndef
{
    /// <summary>
    /// Tag connected event arguments.
    /// </summary>
    public class TagConnectedEventArgs : EventArgs
    {
        /// <summary>
        /// Status of the tag connection operation.
        /// </summary>
        public Status Status { get; set; }
        /// <summary>
        /// If this is NHS tag.
        /// </summary>
        public bool IsNhsTag { get; set; }
        /// <summary>
        /// Tag ID.
        /// </summary>
        public string TagId { get; set; }
        /// <summary>
        /// Tag version.
        /// </summary>
        public byte[] TagVersion { get; set; }
        /// <summary>
        /// On tag connect we read all NDEF messages and convert them into Msg data.
        /// Msg data is presented to the event handler(s).
        /// </summary>
        public List<NdefRecord> NdefRecords { get; set; }
    }
}
