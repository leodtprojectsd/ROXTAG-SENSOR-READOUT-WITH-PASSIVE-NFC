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
using System.Threading.Tasks;

namespace Plugin.Ndef
{
    /// <summary>
    /// Interface for $safeprojectgroupname$
    /// </summary>
    internal class NdefImplementation : INdef
    {
        private readonly INdef _pcscDevice;
        private INdef _device;

        internal NdefImplementation()
        {
            _pcscDevice = new PcscDevice();
        }

        /// <summary>
        /// Init API implementation.
        /// </summary>
        /// <param name="onTagReaderStatusChanged"></param>
        /// <returns></returns>
        public async Task InitTagReaderAsync(TagReaderStatusChanged onTagReaderStatusChanged)
        {
            _device = _pcscDevice;
            await _device.InitTagReaderAsync(onTagReaderStatusChanged);
        }

        /// <summary>
        /// DeInit API implementation.
        /// </summary>
        /// <returns></returns>
        public async Task DeInitTagReaderAsync()
        {
            await _device.DeInitTagReaderAsync();
        }

        /// <summary>
        /// Tag connected API event.
        /// </summary>
        public event EventHandler<TagConnectedEventArgs> TagConnected
        {
            add => _device.TagConnected += value;
            remove => _device.TagConnected -= value;
        }

        /// <summary>
        /// Tag disconnected API event.
        /// </summary>
        public event EventHandler<TagDisconnectedEventArgs> TagDisconnected
        {
            add => _device.TagDisconnected += value;
            remove => _device.TagDisconnected -= value;
        }

        /// <summary>
        /// Write and Read API implmenetation.
        /// </summary>
        /// <param name="wrNdefRecords"></param>
        /// <returns></returns>
        public async Task<(Status status, List<NdefRecord> rdNdefRecords)> WriteReadAsync(
            List<NdefRecord> wrNdefRecords)
        {
            return await _device.WriteReadAsync(wrNdefRecords);
        }

        /// <summary>
        /// Read API implementation.
        /// </summary>
        /// <returns></returns>
        public async Task<(Status status, List<NdefRecord> rdNdefRecords)> ReadAsync()
        {
            return await _device.ReadAsync();
        }

        public Task<Status> WriteAsync(List<NdefRecord> wrNdefRecords)
        {
            throw new NotImplementedException();
        }
    }
}
