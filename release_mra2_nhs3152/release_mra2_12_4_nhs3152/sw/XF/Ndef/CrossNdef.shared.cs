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

using System;

namespace Plugin.Ndef
{
    /// <summary>
    /// Cross Ndef
    /// </summary>
    public static class CrossNdef
    {
        private static Lazy<INdef> implementation = new Lazy<INdef>(() =>
            CreateNdef(), System.Threading.LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        /// Gets if the plugin is supported on the current platform.
        /// </summary>
        public static bool IsSupported => implementation.Value == null ? false : true;

        /// <summary>
        /// Current plugin implementation to use
        /// </summary>
        public static INdef Current
        {
            get
            {
                INdef current = implementation.Value;
                if (current == null)
                {
                    string error = "Cannot access the NFC interface. Make sure:\n" +
                        "- the Ndef library project is built for your platform.\n" +
                        "- the referenced OS version is equal to your application's OS version.\n" +
                        "- the Ndef libraray project is referenced both in the platform agnostic and platform specific application project.\n";
                    throw new NotImplementedException(error);
                }
                return current;
            }
        }

        private static INdef CreateNdef()
        {
#if NETSTANDARD2_0
            return null;
#else
            return new NdefImplementation();
#endif
        }
    }
}
