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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreNFC;
using Foundation;
using NdefLibrary.Ndef;

namespace Plugin.Ndef
{
    /// <summary>
    /// Interface for $safeprojectgroupname$
    /// </summary>
    internal class Ios12Minus : NSObject, INdef, INFCNdefReaderSessionDelegate
    {
        private NFCNdefReaderSession _session;
        private TagReaderStatusChanged _onTagReaderStatus;
        private TagReaderStatusChangedEventArgs _tagReaderStatusEventArgs = new TagReaderStatusChangedEventArgs();
        private SemaphoreSlim _sem = new SemaphoreSlim(1);
        private byte[] _lastNdefMsgByteArray;
        private byte[] _tagId;
        private string _tagIdStr;
        private TaskCompletionSource<(Status status, NdefMessage mgs)> _tcsReadData;
        private TaskCompletionSource<bool> _tcsInvalidateSession;
        private bool _isNewRead;

        internal Ios12Minus()
        {
        }

        /// <summary>
        /// Init API implementation.
        /// </summary>
        /// <param name="onTagReaderStatusChanged"></param>
        /// <returns></returns>
        public async Task InitTagReaderAsync(TagReaderStatusChanged onTagReaderStatusChanged)
        {
            _isNewRead = true;
            _onTagReaderStatus = onTagReaderStatusChanged;
            _tagReaderStatusEventArgs.Status.IsWriteSupported = false;
            _tagReaderStatusEventArgs.Status.IsAutoReadSupported = true;

            _lastNdefMsgByteArray = new byte[] { };

            _session = new NFCNdefReaderSession(this, null, true);
            if (_session != null)
            {
                _tagReaderStatusEventArgs.Status.Reader = TagReaderStatusChangedEventArgs.EReader.Available;
                _session.AlertMessage = "Hold your iPhone near the NTAG SmartSensor to read out the status";
                _session.BeginSession();
            }
            else
            {
                _tagReaderStatusEventArgs.Status.Reader = TagReaderStatusChangedEventArgs.EReader.NotAvailable;
            }

            _onTagReaderStatus?.Invoke(this, _tagReaderStatusEventArgs);
            await Task.CompletedTask;
        }

        /// <summary>
        /// DeInit API implementation.
        /// </summary>
        /// <returns></returns>
        public async Task DeInitTagReaderAsync()
        {
            if (_session != null)
            {
                _tcsInvalidateSession = new TaskCompletionSource<bool>();
                _session.InvalidateSession();
                try
                {
                    await _tcsInvalidateSession.Task.ContinueWith(antecedent =>
                    {
                        if (antecedent.IsCompleted)
                        {
                            if (antecedent.Result == true)
                            {
                            }
                        }
                        else
                        {
                        }
                    });
                }
                catch (Exception ex)
                {
                    Helpers.ExceptionLogHelper.Log(_tagIdStr, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + 
                        ex.Message);
                    Debug.WriteLine("Caught exception:" + _tagIdStr + " - " + 
                        Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                }
                finally
                {
                    _tcsInvalidateSession = null;
                }
            }

            _tagReaderStatusEventArgs.Status.Reader = TagReaderStatusChangedEventArgs.EReader.NotAvailable;
            _onTagReaderStatus?.Invoke(this, _tagReaderStatusEventArgs);
        }

        /// <summary>
        /// Tag connected API event.
        /// </summary>
        public event EventHandler<TagConnectedEventArgs> TagConnected;
        /// <summary>
        /// Tag disconnected API event.
        /// </summary>
        public event EventHandler<TagDisconnectedEventArgs> TagDisconnected;

        public async Task<(Status status, List<NdefRecord> rdNdefRecords)> WriteReadAsync(
            List<NdefRecord> wrNdefRecords)
        {
            await Task.CompletedTask;
            return (Status.NotSupported, null);
        }

        /// <summary>
        /// Read API implementation.
        /// </summary>
        /// <returns></returns>
        public async Task<(Status status, List<NdefRecord> rdNdefRecords)> ReadAsync()
        {
            if (_tagReaderStatusEventArgs.Status.Reader == TagReaderStatusChangedEventArgs.EReader.NotAvailable)
            {
                return (Status.TagReaderNotAvailable, null);
            }

            // Make sure that session of the first tap has been completed.
            int loop = 50;
            while (_session != null && loop > 0)
            {
                await Task.Delay(20);
                loop--;
            }
            if (_session != null)
            {
                return (Status.TagReadFailed, null);
            }

            // Make sure only one tag reader operation is taking place.
            await _sem.WaitAsync();

            try
            {
                for (int i = 0; i < 3; i++)
                {
                    await Task.Delay(5);

                    var (rxStat, rdNdefMessage) = await ReadNdefMessageAsync();
                    switch (rxStat)
                    {
                        case Status.TagReadFailed:
                            return (rxStat, null);
                    }

                    Debug.WriteLine(
                        $"{DateTime.Now.TimeOfDay}" +
                        "#### ReadAsync DATA:" +
                        BitConverter.ToString(rdNdefMessage.ToByteArray()).Replace("-", string.Empty));

                    // Check if we get a new message by comparing to the previous read content.
                    // The content must be different.
                    if (!_lastNdefMsgByteArray.SequenceEqual(rdNdefMessage.ToByteArray()))
                    {
                        _lastNdefMsgByteArray = rdNdefMessage.ToByteArray();

                        // Convert rxNdefData into rdMsgData.
                        ////var rdMsgData = Converter.NdefToMsgDataList(rxNdefMsg);

                        return (Status.OK, rdNdefMessage.ToList());
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.ExceptionLogHelper.Log(_tagIdStr, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + 
                    ex.Message);
                Debug.WriteLine("Caught exception:" + _tagIdStr + " - " + 
                    Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
            }
            finally
            {
                _sem.Release();
            }

            Debug.WriteLine(">>>> CANNOT READ");
            return (Status.TagReadFailed, null);
        }

        /// <summary>
        /// iOS callback when NDEF session is started.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="messages"></param>
        public void DidDetect(NFCNdefReaderSession session, NFCNdefMessage[] messages)
        {
            Debug.WriteLine("#### SESSION STARTED");

            var msg = new NdefMessage();

            for (int i = 0; i < messages[0].Records.Length; i++)
            {
                var record = new NdefRecord
                {
                    Id = null,
                    Type = messages[0].Records[i].Type.ToArray(),
                    TypeNameFormat = (NdefRecord.TypeNameFormatType)messages[0].Records[i].TypeNameFormat,
                    Payload = messages[0].Records[i].Payload.ToArray()
                };

                if (_isNewRead)
                {
                    // Since it is not possible to get Tag ID on iOS we do the hack here and get
                    // it from incoming NDEF message, NFCUID MIME record.
                    if (record.TypeNameFormat == NdefRecord.TypeNameFormatType.Mime && record.Payload[0] == 0x0A &&
                    record.Payload[1] == 0x01)
                    {
                        // Mime record payload: ID + Direction + MimePayload[]
                        // 0x0A is NFCUID ID.
                        // 0x01 is Incoming
                        _tagId = new byte[]
                        {
                        record.Payload[2], record.Payload[3], record.Payload[4],
                        record.Payload[6], record.Payload[7], record.Payload[8], record.Payload[9]
                        };
                        _tagIdStr = BitConverter.ToString(_tagId).Replace("-", string.Empty);
                    }
                }
                msg.Add(record);
            }

            if (_isNewRead)
            {
                Debug.WriteLine(
                    $"{DateTime.Now.TimeOfDay}" +
                    "#### DidDetect DATA:" +
                    BitConverter.ToString(msg.ToByteArray()).Replace("-", string.Empty));

                _lastNdefMsgByteArray = msg.ToByteArray();

                TagConnected?.Invoke(this, new TagConnectedEventArgs
                {
                    IsNhsTag = true, // assume true, not possible to check if NHS tag in iOS
                    TagId = _tagIdStr,
                    TagVersion = new byte[] { 0x00, 0x00 }, // not possible to get this in iOS
                    NdefRecords = msg.ToList(),
                });
            }
            else
            {
                _tcsReadData?.SetResult((Status.OK, msg));
            }
            _isNewRead = false;
        }

        /// <summary>
        /// iOS callback when NDEF session is ended.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="error"></param>
        public void DidInvalidate(NFCNdefReaderSession session, NSError error)
        {
            Debug.WriteLine($"#### SESSION ENDED: {error.ToString()}");
            if (error.ToString() == "Session is invalidated by user cancellation")
            {
                TagDisconnected?.Invoke(this, new TagDisconnectedEventArgs { });
            }

            _session = null;
            _tcsInvalidateSession?.SetResult(true);
            ////TagDisconnected?.Invoke(this, new TagDisconnectedEventArgs { });
        }

        private async Task<(Status status, NdefMessage mgs)> ReadNdefMessageAsync()
        {
            Status status = Status.OK;
            Helpers.TimerHelper timer;

            var msg = new NdefMessage();

            _tcsReadData = new TaskCompletionSource<(Status status, NdefMessage mgs)>();
            _tcsInvalidateSession = new TaskCompletionSource<bool>();

            _session = new NFCNdefReaderSession(this, null, true);
            if (_session != null)
            {
                _session.AlertMessage = "Maintain the position for repetitive reading and a full data readout";
                _session.BeginSession();
                timer = new Helpers.TimerHelper(TimeSpan.FromMilliseconds(1000), () =>
                {
                    try
                    {
                        _tcsReadData?.SetResult((Status.TagReadFailed, msg));
                        _session.InvalidateSession();
                    }
                    catch (Exception ex)
                    {
                        Helpers.ExceptionLogHelper.Log(_tagIdStr, Helpers.ExceptionLogHelper.GetCurrentMethod() + 
                            " - " + ex.Message);
                        Debug.WriteLine("Caught exception:" + _tagIdStr + " - " + 
                            Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                    }
                });
            }
            else
            {
                return (Status.TagReadFailed, msg);
            }

            // Wait here till data is available.
            try
            {
                await _tcsReadData.Task.ContinueWith(antecedent =>
                {
                    if (antecedent.IsCompleted)
                    {
                        status = antecedent.Result.status;
                        msg = antecedent.Result.mgs;
                    }
                    else
                    {
                    }
                });
            }
            catch (Exception ex)
            {
                Helpers.ExceptionLogHelper.Log(_tagIdStr, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + 
                    ex.Message);
                Debug.WriteLine("Caught exception:" + _tagIdStr + " - " + 
                    Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                status = Status.TagReadFailed;
            }
            finally
            {
                timer.Cancel();
                _tcsReadData = null;
            }

            // Wait here till session is ended.
            try
            {
                await _tcsInvalidateSession.Task.ContinueWith(antecedent =>
                {
                    if (antecedent.IsCompleted)
                    {
                        if (antecedent.Result == true)
                        {
                        }
                    }
                    else
                    {
                    }
                });
            }
            catch (Exception ex)
            {
                Helpers.ExceptionLogHelper.Log(_tagIdStr, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + 
                    ex.Message);
                Debug.WriteLine("Caught exception:" + _tagIdStr + " - " + 
                    Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                status = Status.TagReadFailed;
            }
            finally
            {
                _tcsInvalidateSession = null;
            }

            return (status, msg);
        }

        private async Task<Status> WriteNdefMessageAsync(NdefMessage msg)
        {
            await Task.CompletedTask;
            return Status.NotSupported;
        }

        public async Task<Status> WriteAsync(List<NdefRecord> wrNdefRecords)
        {
            await Task.CompletedTask;
            return Status.NotSupported;
        }
    }
}
