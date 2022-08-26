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
    /// Inter module message codes.
    /// </summary>
    public class MessagingCenterMessages
    {
        /// <summary>
        /// Android messages.
        /// </summary>
        public class Android
        {
            /// <summary>
            /// Android app is created.
            /// </summary>
            public const string OnCreate = "OnCreate";
            /// <summary>
            /// Android app is resumed.
            /// </summary>
            public const string OnResume = "OnResume";
            /// <summary>
            /// Android app is paused.
            /// </summary>
            public const string OnPause = "OnPause";
            /// <summary>
            /// New intent for the android app.
            /// </summary>
            public const string OnNewIntent = "OnNewIntent";
            /// <summary>
            /// NFC adapter state.
            /// </summary>
            public const string OnNewAdapterState = "OnNewAdapterState";
            /// <summary>
            /// To disable android drawing cache.
            /// </summary>
            public const string DisableDrawingCache = "DisableDrawingCache";
        }
    }
}
