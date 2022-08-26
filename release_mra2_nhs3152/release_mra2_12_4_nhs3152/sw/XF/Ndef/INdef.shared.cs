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
using System.Threading;
using System.Threading.Tasks;

namespace Plugin.Ndef
{
    // The host device to access(read and/or write) the tags is called "NFC emitter". But here we name it
    // TagReader as this term is commonly used. 

    /// <summary>
    /// Delegate to be invoked when tag reader status is changed.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void TagReaderStatusChanged(object sender, TagReaderStatusChangedEventArgs e);

    /// <summary>
    /// NDEF plug-in API. 
    /// Each platform should implement this interface.
    /// </summary>
    public interface INdef
    {
        /// <summary>
        /// Initilize tag reader (NFC emitter) device. The status will be reported via the delegate 
        /// TagReaderStatusChanged.
        /// </summary>
        /// <param name="onTagReaderStatusChanged"></param>
        /// <returns></returns>
        Task InitTagReaderAsync(TagReaderStatusChanged onTagReaderStatusChanged);

        /// <summary>
        /// Deinitialize the tag reader HW.
        /// </summary>
        /// <returns></returns>
        Task DeInitTagReaderAsync();

        /// <summary>
        /// This event will be invoked when a new tag is connected.
        /// In the arguments we wil be reporting:
        ///     - If NHS tag
        ///     - If the tag contains NDEF message, if yes the NDEF message will be associated as well
        ///     - If the tag contains compatible version information
        ///     
        /// Higher level can further check:
        ///     - If NDEF message contains interpretable Text, MIME and URL records
        ///     - If the high level version information is compatible
        /// </summary>
        event EventHandler<TagConnectedEventArgs> TagConnected;

        /// <summary>
        /// This event will be invoked when the tag is disconnected.
        /// </summary>
        event EventHandler<TagDisconnectedEventArgs> TagDisconnected;

        /// <summary>
        ///     - Wraps all Text, MIME and URL NDEF records into an NDEF message.
        ///     - Writes it.
        ///     - Read back NDEF message and extracts all NDEF records.
        ///     - Returns all NDEF records with a status.
        /// <param name="wrNdefRecords"></param>
        /// <returns>Status and read NDEF record list. List will be null if Status is not OK.</returns>
        /// </summary>
        Task<(Status status, List<NdefRecord> rdNdefRecords)> WriteReadAsync(List<NdefRecord> wrNdefRecords);

        /// <summary>
        ///     - Read new NDEF message and extracts all NDEF records.
        ///     - Returns the NDEF records with a status.
        /// <returns>Status and read NDEF record list. List will be null if Status is not OK.</returns>
        /// </summary>
        Task<(Status status, List<NdefRecord> rdNdefRecords)> ReadAsync();

        /// <summary>
        ///     - Wraps all Text, MIME and URL NDEF records into an NDEF message.
        ///     - Writes it.
        /// <param name="wrNdefRecords"></param>
        /// <returns>Status</returns>
        /// </summary>
        Task<Status> WriteAsync(List<NdefRecord> wrNdefRecords);
    }
}
