/*
 * Copyright 2018-2019 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

namespace Plugin.Ndef
{
    /// <summary>
    /// Returned status from NDEF plug-in.
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// Successful.
        /// </summary>
        OK,
        /// <summary>
        /// Cancelled.
        /// </summary>
        Cancelled,
        /// <summary>
        /// Not supported.
        /// </summary>
        NotSupported,
        /// <summary>
        /// Tag reader is not available.
        /// </summary>
        TagReaderNotAvailable,
        /// <summary>
        /// Reading from tag failed.
        /// </summary>
        TagReadFailed,
        /// <summary>
        /// Writing to tag failed.
        /// </summary>
        TagWriteFailed,
        /// <summary>
        /// Invalid NDEF format.
        /// </summary>
        InvalidNdefData,

////        NfcServiceIsNotAvailable,
////        TagReaderDoesNotExist,

    }
}
