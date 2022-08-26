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

using NdefLibrary.Ndef;
using PCSC;
using PCSC.Iso7816;
using PCSC.Monitoring;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Plugin.Ndef
{
    internal class PcscDevice : INdef, IDisposable
    {
        bool _isDisposed = false;
        IDeviceMonitor _deviceMonitor;
        TagReaderStatusChanged _onTagReaderStatus;

        class ReaderContext
        {
            internal ISCardMonitor cardMonitor;
            internal ISCardContext cardContext;
            internal IIsoReader isoReader;
        }
        IDictionary<string, ReaderContext> _readers = new Dictionary<string, ReaderContext>();

        // We allow only one reader to be active.
        string _activeReader = string.Empty;

        byte[] _tagId;
        string _tagIdStr = string.Empty;

        const byte CUSTOM_CLA = 0xFF;

        byte _ndefTlvPage;    // NDEF page on NFC memory (page 4 to 0x84)
        const byte NDEF_TLV = 0x03;

        SemaphoreSlim sem = new SemaphoreSlim(1);
        byte[] _lastNdefMsgByteArray;

        internal PcscDevice()
        {
            var deviceFactory = DeviceMonitorFactory.Instance;
            _deviceMonitor = deviceFactory.Create(SCardScope.System);
            _deviceMonitor.StatusChanged += OnStatusChanged;
            _deviceMonitor.MonitorException += OnMonitorException;
            _deviceMonitor.Start();
        }

        /// <summary>
        /// Tag connected API implementation.
        /// </summary>
        public event EventHandler<TagConnectedEventArgs> TagConnected;
        /// <summary>
        /// Tag disconnected API implementation.
        /// </summary>
        public event EventHandler<TagDisconnectedEventArgs> TagDisconnected;

        /// <summary>
        /// Init API implementation.
        /// </summary>
        /// <param name="onTagReaderStatus"></param>
        /// <returns></returns>
        public async Task InitTagReaderAsync(TagReaderStatusChanged onTagReaderStatus)
        {
            _onTagReaderStatus = onTagReaderStatus;

            using (var context = ContextFactory.Instance.Establish(SCardScope.System))
            {
                InitReaders(context.GetReaders());
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// DeInit API implementation.
        /// </summary>
        /// <returns></returns>
        public async Task DeInitTagReaderAsync()
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Write and Read API implementation.
        /// </summary>
        /// <param name="wrNdefRecords"></param>
        /// <returns></returns>
        public async Task<(Status status, List<NdefRecord> rdNdefRecords)>WriteReadAsync(
            List<NdefRecord> wrNdefRecords)
        {
            NdefMessage txNdefMsg = new NdefMessage();       //Converter.MsgDataListToNdef(txMsgDataList);
            txNdefMsg.AddRange(wrNdefRecords);

            await sem.WaitAsync(); // Make sure only one tag reader operation is taking place.
            try
            {
                {
                    var txStat = WriteNdefMessage(txNdefMsg);
                    if (txStat != Status.OK)
                        return (txStat, null);

                    // Write succeeded, now read after a wait. Try multiple times before reporting failure.
                    for (int i = 0; i < 5; i++)
                    {
                        await Task.Delay(60);

                        var (rxStat, rxNdefMsg) = ReadNdefMessage();
                        switch (rxStat)
                        {
                            case Status.TagReadFailed:
                            case Status.InvalidNdefData:
                                // try again.
                                continue;
                        }

                        // Check if we get a new message by comparing to the previous read or write content.
                        // The content must be different.
                        if (!(txNdefMsg.ToByteArray()).SequenceEqual(rxNdefMsg.ToByteArray()))
                        {
                            var rdNdefRecords = rxNdefMsg.ToList();
                            return (Status.OK, rdNdefRecords);
                        }
                    }
                }

                // Read failed.
                return (Status.TagReadFailed, null);
            }
            catch (Exception ex)
            {
                Helpers.ExceptionLogHelper.Log(_tagIdStr, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + 
                    ex.Message);
                System.Diagnostics.Debug.WriteLine("Caught exception:" + _tagIdStr + " - " + 
                    Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
            }
            finally
            {
                sem.Release();
            }
            // Read failed.
            return (Status.TagReadFailed, null);
        }

        /// <summary>
        /// Read API implementation.
        /// </summary>
        /// <returns></returns>
        public async Task<(Status status, List<NdefRecord> rdNdefRecords)>ReadAsync()
        {
            // Make sure only one tag reader operation is taking place.
            await sem.WaitAsync();
            try
            {
                for (int i = 0; i < 5; i++)
                {
                    await Task.Delay(60);

                    var (rxStat, rxNdefMsg) = ReadNdefMessage();
                    switch (rxStat)
                    {
                        case Status.TagReadFailed:
                        case Status.InvalidNdefData:
                            // Device is in the middle of writing to NFC memory, try again.
                            continue;
                    }

                    // Check if we get a new message by comparing to the previous read content.
                    // The content must be different.
                    if (!_lastNdefMsgByteArray.SequenceEqual(rxNdefMsg.ToByteArray()))
                    {
                        _lastNdefMsgByteArray = rxNdefMsg.ToByteArray();

                        // Convert rxNdefData into rdMsgData.
                        ////var rdMsgDataList = Converter.NdefToMsgDataList(rxNdefMsg);
                        return (Status.OK, rxNdefMsg.ToList());
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.ExceptionLogHelper.Log(_tagIdStr, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + 
                    ex.Message);
                System.Diagnostics.Debug.WriteLine("Caught exception:" + _tagIdStr + " - " + 
                    Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
            }
            finally
            {
                sem.Release();
            }

            return (Status.TagReadFailed, null);
        }


        (Status status, NdefMessage mgs) ReadNdefMessage()
        {
            Status stat = Status.OK;
            NdefMessage msg = new NdefMessage();
            byte[] response = new byte[0];

            try
            {
                if (_tagIdStr == string.Empty)
                    throw new Exception("Tag initialization failed");

                // NHS31xx provides TLV datas from page 0x04 to 0x84 (512 bytes). 
                // TLVs start on page boundaries always, hence length field is in the same page
                // reagrdless of short or long length.
                // Page 4 on there are TLVs. Get the NDEF one.
                // When last page of NDEF message read, it will create interrupt on device
                // that we read the NFC message.
                byte ndefTlvPage = 0;
                for (byte i = 4; i < 0x84; i++)
                {
                    byte[] page = ReadBinary(0, i, 4);
                    if (page[0] == NDEF_TLV)
                    {
                        ndefTlvPage = i;
                        response = page;
                        break;
                    }
                }
                // Check if we have good NDEF msg.
                if (ndefTlvPage == 0)
                    throw new Exception();

                _ndefTlvPage = ndefTlvPage;

                // Get length.
                int ndefTlvLen = response[1];
                int numPayloadPages = 0;
                int payloadOffset = 0;

                if (ndefTlvLen == 0)
                {
                    // Len 0 means the device is in the middle of updating the memory.
                    // Try again after a timeout.
                    stat = Status.InvalidNdefData;
                    throw new Exception();
                }
                else if (ndefTlvLen == 0xFF)
                {
                    // Get length.
                    ndefTlvLen = response[2] << 8 | response[3];

                    // Calculate number of payload page still to be read.
                    numPayloadPages = ndefTlvLen / 4 + (ndefTlvLen % 4 == 0 ? 0 : 1);

                    // Payload offset.
                    payloadOffset = 4;
                }
                else
                {
                    // Calculate number of payload page still to be read.
                    numPayloadPages = ndefTlvLen / 4 + (ndefTlvLen % 4 < 3 ? 0 : 1);

                    // Payload offset.
                    payloadOffset = 2;
                }

                // Make sure we have enough room for payload.
                if (ndefTlvPage + numPayloadPages > 0x84)
                    throw new Exception();

                // Read payload pages.
                for (byte i = (byte)(ndefTlvPage + 1); i < ndefTlvPage + numPayloadPages + 1; i++)
                {
                    byte[] page = ReadBinary(0, i, 4);
                    response = Combine(response, page);
                }

                var ndef = new byte[ndefTlvLen];
                Buffer.BlockCopy(response, payloadOffset, ndef, 0, ndefTlvLen);
                Debug.WriteLine($"{DateTime.Now.TimeOfDay}" + $"#### READ:" + BitConverter.ToString(ndef).
                    Replace("-", string.Empty));

                msg = NdefMessage.FromByteArray(ndef);
            }
            catch (Exception ex)
            {
                Helpers.ExceptionLogHelper.Log(_tagIdStr, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + 
                    ex.Message);
                System.Diagnostics.Debug.WriteLine("Caught exception:" + _tagIdStr + " - " + 
                    Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                if (stat == Status.OK)
                {
                    stat = Status.TagReadFailed;
                }
            }


            return (stat, msg);

            // local function
            byte[] Combine(byte[] a, byte[] b)
            {
                byte[] c = new byte[a.Length + b.Length];
                Buffer.BlockCopy(a, 0, c, 0, a.Length);
                Buffer.BlockCopy(b, 0, c, a.Length, b.Length);
                return c;
            }

        }

        Status WriteNdefMessage(NdefMessage msg)
        {
            Status stat = Status.OK;

            try
            {
                if (_tagIdStr == string.Empty)
                    throw new Exception("Tag initialization failed");

                // Start writing NDEF TLV starting from _ndefTlvPage.
                // First reset TLV length.
                byte[] tlvPage = { NDEF_TLV, 0, 0, 0 };
                UpdateBinary(0, _ndefTlvPage, tlvPage);

                // Write NDEF data (TLV payload).
                int ndefTlvLen = msg.ToByteArray().Length;
                int numTotalPages = 0;
                int payloadOffset = 0;

                if (ndefTlvLen >= 0xFF)
                {
                    // Calculate number of total pages to write.
                    numTotalPages = ndefTlvLen / 4 + (ndefTlvLen % 4 == 0 ? 0 : 1) + 1;

                    // Payload offset.
                    payloadOffset = 4;
                }
                else
                {
                    // Calculate number of total pages to write.
                    numTotalPages = ndefTlvLen / 4 + (ndefTlvLen % 4 < 3 ? 0 : 1) + 1;

                    // Payload offset.
                    payloadOffset = 2;
                }

                if (numTotalPages > 0x84)
                    throw new Exception();

                byte[] command = new byte[numTotalPages * 4];
                Buffer.BlockCopy(tlvPage, 0, command, 0, 4);
                Buffer.BlockCopy(msg.ToByteArray(), 0, command, payloadOffset, ndefTlvLen);

                byte[] page = new byte[4];
                int idx = 0;
                for (byte i = _ndefTlvPage; i < _ndefTlvPage + numTotalPages; i++)
                {
                    Buffer.BlockCopy(command, idx, page, 0, 4);
                    idx += 4;
                    UpdateBinary(0, i, page);
                }

                // Calculate TLV total length.
                int ndefTlvTotalLen = 0;
                if (payloadOffset == 4)
                    ndefTlvTotalLen = 4 + ndefTlvLen;
                else
                    ndefTlvTotalLen = 2 + ndefTlvLen;

                Debug.WriteLine($"{DateTime.Now.TimeOfDay}" + $"####WRITE:" + 
                    BitConverter.ToString(msg.ToByteArray()).Replace("-", string.Empty));

                // Add TLV terminator.
                byte tlvTerminatePage = 0;
                byte[] tlvTerminate = new byte[4];
                if (ndefTlvTotalLen % 4 == 0)
                {
                    tlvTerminatePage = (byte)(_ndefTlvPage + numTotalPages);
                    tlvTerminate[0] = 0xFE;
                }
                else
                {
                    tlvTerminatePage = (byte)(_ndefTlvPage + numTotalPages - 1);
                    Buffer.BlockCopy(command, (numTotalPages - 1) * 4, tlvTerminate, 0, 4);
                    tlvTerminate[ndefTlvTotalLen % 4] = 0xFE;
                }
                UpdateBinary(0, tlvTerminatePage, tlvTerminate);

                // Update TLV length.
                // Should be the last write.
                Buffer.BlockCopy(command, 0, tlvPage, 0, 4);
                if (payloadOffset == 4)
                {
                    tlvPage[1] = 0xFF;
                    tlvPage[2] = (byte)(ndefTlvLen >> 8);
                    tlvPage[3] = (byte)(ndefTlvLen);
                }
                else
                {
                    tlvPage[1] = (byte)(ndefTlvLen);
                }
                UpdateBinary(0, _ndefTlvPage, tlvPage);

            }
            catch (Exception ex)
            {
                Helpers.ExceptionLogHelper.Log(_tagIdStr, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + 
                    ex.Message);
                System.Diagnostics.Debug.WriteLine("Caught exception:" + _tagIdStr + " - " + 
                    Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                if (stat == Status.OK)
                {
                    stat = Status.TagWriteFailed;
                }
            }

            return stat;
        }

        void Invoke()
        {
            _onTagReaderStatus?.Invoke(this, new TagReaderStatusChangedEventArgs
            {
                Status = new TagReaderStatusChangedEventArgs.TagReaderStatus
                {
                    Reader = _readers.Count == 0 ?
                        TagReaderStatusChangedEventArgs.EReader.NotAvailable : TagReaderStatusChangedEventArgs.EReader.Available,
                    IsWriteSupported = true,
                    IsAutoReadSupported = true,
                }
            });
        }

        void InitReaders(string[] readers)
        {
            _readers.Clear();
            foreach (string reader in readers)
                AddReader(reader);
            Invoke();
        }

        void AddReader(string reader, bool isInvoke = false)
        {
            var readerContext = new ReaderContext
            {
                cardMonitor = MonitorFactory.Instance.Create(SCardScope.System)
            };
            readerContext.cardMonitor.CardInserted += OnCardInserted;
            readerContext.cardMonitor.CardRemoved += OnCardRemoved;
            readerContext.cardMonitor.Start(reader);

            _readers.Add(reader, readerContext);

            if (isInvoke)
                Invoke();
        }


        void RemoveReader(string reader)
        {
            if (_readers.TryGetValue(reader, out ReaderContext readerContext))
            {
                readerContext.cardMonitor.CardInserted -= OnCardInserted;
                readerContext.cardMonitor.CardRemoved -= OnCardRemoved;
                readerContext.cardMonitor.Cancel();
                readerContext.cardMonitor.Dispose();

                _readers.Remove(reader);
            }
        }

        void OnCardInserted(object sender, CardStatusEventArgs e)
        {
            lock (this)
            {
                // One reader can be active at a time.
                if (_activeReader == string.Empty)
                    _activeReader = e.ReaderName;
                else
                    return;
            }

            try
            {
                if (!_readers.TryGetValue(e.ReaderName, out ReaderContext readerContext))
                    throw new KeyNotFoundException("Tag reader does not exist");

                readerContext.cardContext = ContextFactory.Instance.Establish(SCardScope.System);
                readerContext.isoReader = new IsoReader(
                    readerContext.cardContext,
                    e.ReaderName,
                    SCardShareMode.Shared,
                    SCardProtocol.Any,
                    false);

                // Page 0 and 1 contains UID.
                // Also check if we can read a page (4 bytes).
                var uid = ReadBinary(0, 0, 4);
                if (uid.Length != 4)
                {
                    _tagId = null;
                    _tagIdStr = string.Empty;
                    return;
                }

                var uid0 = ReadBinary(0, 0, 4);
                var uid1 = ReadBinary(0, 1, 4);
                _tagId = new byte[]
                {
                    uid0[0], uid0[1], uid0[2], uid1[0], uid1[1], uid1[2], uid1[3]
                };
                _tagIdStr = BitConverter.ToString(_tagId).Replace("-", string.Empty);

                var (stat, msg) = ReadNdefMessage();
                if (stat == Status.OK)
                {
                    TagConnected?.Invoke(this, new TagConnectedEventArgs
                    {
                        Status = Status.OK,
                        IsNhsTag = true, //////// TODO ADD THIS
                        TagId = _tagIdStr,
                        TagVersion = new byte[] { 0x00, 0x00 }, ////// TODO: ADD THIS
                        NdefRecords = msg.ToList(),
                    });
                }
                else
                {
                    TagConnected?.Invoke(this, new TagConnectedEventArgs
                    {
                        Status = stat,
                    });
                }
            }
            catch (Exception ex)
            {
                Helpers.ExceptionLogHelper.Log(_tagIdStr, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Caught exception:" + _tagIdStr + " - " + Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
            }
        }

        void OnCardRemoved(object sender, CardStatusEventArgs e)
        {
            try
            {
                if (!_readers.TryGetValue(e.ReaderName, out ReaderContext readerContext))
                    throw new KeyNotFoundException("Tag reader does not exist");

                readerContext.isoReader.Dispose();

                readerContext.cardContext.Cancel();
                readerContext.cardContext.Dispose();
            }
            catch (Exception ex)
            {
                Helpers.ExceptionLogHelper.Log(_tagIdStr, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Caught exception:" + _tagIdStr + " - " + Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
            }

            lock (this)
            {
                _activeReader = string.Empty;
            }

            TagDisconnected?.Invoke(this, new TagDisconnectedEventArgs { });
        }

        void OnInitialized(object sender, DeviceChangeEventArgs e)
        {
            Debug.WriteLine("Current connected readers:");
            foreach (var name in e.AllReaders)
            {
                Debug.WriteLine(name);
            }

            InitReaders(e.AllReaders.ToArray());
        }

        void OnMonitorException(object sender, DeviceMonitorExceptionEventArgs e)
        {
            Debug.WriteLine($"Exception: {e.Exception}");
        }

        void OnStatusChanged(object sender, DeviceChangeEventArgs e)
        {
            foreach (var removed in e.DetachedReaders)
            {
                RemoveReader(removed);
                Debug.WriteLine($"Reader detached: {removed}");
            }

            foreach (var added in e.AttachedReaders)
            {
                AddReader(added);
                Debug.WriteLine($"New reader attached: {added}");
            }
        }

        byte[] ReadBinary(byte msb, byte lsb, int size)
        {
            if (!_readers.TryGetValue(_activeReader, out ReaderContext readerContext))
                throw new KeyNotFoundException("Tag reader does not exist");

            unchecked
            {
                var readBinaryCmd = new CommandApdu(IsoCase.Case2Short, SCardProtocol.Any)
                {
                    CLA = CUSTOM_CLA,
                    Instruction = InstructionCode.ReadBinary,
                    P1 = msb,
                    P2 = lsb,
                    Le = size
                };

                //Debug.WriteLine($"Read Binary: {BitConverter.ToString(readBinaryCmd.ToArray())}");
                var response = readerContext.isoReader.Transmit(readBinaryCmd);
                //Debug.WriteLine($"SW1 SW2 = {response.SW1:X2} {response.SW2:X2} Data: {BitConverter.ToString(response.GetData())}");

                if ((response.SW1 == (byte)SW1Code.Normal) && (response.SW2 == 0x00))
                    return response.GetData();
                else
                    throw new InvalidOperationException("Read binary failed");
            }
        }

        public void UpdateBinary(byte msb, byte lsb, byte[] data)
        {
            if (!_readers.TryGetValue(_activeReader, out ReaderContext readerContext))
                throw new KeyNotFoundException("Tag reader does not exist");

            var updateBinaryCmd = new CommandApdu(IsoCase.Case3Short, SCardProtocol.Any)
            {
                CLA = CUSTOM_CLA,
                Instruction = InstructionCode.UpdateBinary,
                P1 = msb,
                P2 = lsb,
                Data = data
            };

            //Debug.WriteLine($"Update Binary: {BitConverter.ToString(updateBinaryCmd.ToArray())}");
            var response = readerContext.isoReader.Transmit(updateBinaryCmd);
            //Debug.WriteLine($"SW1 SW2 = {response.SW1:X2} {response.SW2:X2}");

            if ((response.SW1 == (byte)SW1Code.Normal) && (response.SW2 == 0x00))
                return;
            else
                throw new InvalidOperationException("Update binary failed");
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            if (disposing)
            {
                // Free any managed objects here.
                _deviceMonitor.Initialized -= OnInitialized;
                _deviceMonitor.StatusChanged -= OnStatusChanged;
                _deviceMonitor.MonitorException -= OnMonitorException;
                _deviceMonitor.Cancel();
                _deviceMonitor.Dispose();
            }

            // Free any unmanaged objects here.

            _isDisposed = true;
        }

        public Task<Status> WriteAsync(List<NdefRecord> wrNdefRecords)
        {
            throw new NotImplementedException();
        }
    }
}
