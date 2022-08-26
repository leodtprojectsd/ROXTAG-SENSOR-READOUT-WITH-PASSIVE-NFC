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
using System.Collections.Generic;
using System.Threading.Tasks;
using NdefLibrary.Ndef;
using Xamarin.Essentials;

namespace Plugin.Ndef
{
    /// <summary>
    /// Interface for $safeprojectgroupname$
    /// </summary>
    internal class NdefImplementation : INdef
    {
        private INdef _iosDevice;

        internal NdefImplementation()
        {
            // Check iOS version of the phone.
            var osVersion = Convert.ToInt32(DeviceInfo.VersionString.Split('.')[0]);
            if (osVersion >= 13)
            {
                _iosDevice = new Ios13Plus();
            }
            else
            {
                _iosDevice = new Ios12Minus();
            }
        }

        /// <summary>
        /// Init API implementation.
        /// </summary>
        /// <param name="onTagReaderStatusChanged"></param>
        /// <returns></returns>
        public async Task InitTagReaderAsync(TagReaderStatusChanged onTagReaderStatusChanged)
        {
            await _iosDevice.InitTagReaderAsync(onTagReaderStatusChanged);
        }

        /// <summary>
        /// DeInit API implementation.
        /// </summary>
        /// <returns></returns>
        public async Task DeInitTagReaderAsync()
        {
            await _iosDevice.DeInitTagReaderAsync();
        }

        /// <summary>
        /// Tag connected API event.
        /// </summary>
        public event EventHandler<TagConnectedEventArgs> TagConnected
        {
            add => _iosDevice.TagConnected += value;
            remove => _iosDevice.TagConnected -= value;
        }
        /// <summary>
        /// Tag disconnected API event.
        /// </summary>
        public event EventHandler<TagDisconnectedEventArgs> TagDisconnected
        {
            add => _iosDevice.TagDisconnected += value;
            remove => _iosDevice.TagDisconnected -= value;
        }

        public async Task<(Status status, List<NdefRecord> rdNdefRecords)> WriteReadAsync(
            List<NdefRecord> wrNdefRecords)
        {
            return await _iosDevice.WriteReadAsync(wrNdefRecords);
        }

        /// <summary>
        /// Read API implementation.
        /// </summary>
        /// <returns></returns>
        public async Task<(Status status, List<NdefRecord> rdNdefRecords)> ReadAsync()
        {
            return await _iosDevice.ReadAsync();
        }

        public async Task<Status> WriteAsync(List<NdefRecord> wrNdefRecords)
        {
            return await _iosDevice.WriteAsync(wrNdefRecords);
        }
    }
}
