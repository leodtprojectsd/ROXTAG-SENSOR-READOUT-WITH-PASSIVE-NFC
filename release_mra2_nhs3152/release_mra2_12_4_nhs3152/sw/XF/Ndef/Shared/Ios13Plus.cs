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

using CoreFoundation;
using CoreNFC;
using Foundation;
using NdefLibrary.Ndef;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Plugin.Ndef
{
    /// <summary>
    /// Interface for $safeprojectgroupname$
    /// </summary>
    internal class Ios13Plus : NFCTagReaderSessionDelegate, INdef
    {
        private NFCTagReaderSession _session;
        private INFCMiFareTag _tag;
        private TagReaderStatusChanged _onTagReaderStatusChanged;
        private TagReaderStatusChangedEventArgs _tagReaderStatusChangedEventArgs = new TagReaderStatusChangedEventArgs();
        private SemaphoreSlim _sem = new SemaphoreSlim(1);
        private byte[] _lastNdefMessageBytes;
        private string _tagIdStr = string.Empty; 

        internal Ios13Plus()
        {
        }

        /// <summary>
        /// Init API implementation.
        /// </summary>
        /// <param name="onTagReaderStatusChanged"></param>
        /// <returns></returns>
        public async Task InitTagReaderAsync(TagReaderStatusChanged onTagReaderStatusChanged)
        {
            _onTagReaderStatusChanged = onTagReaderStatusChanged;
            _tagReaderStatusChangedEventArgs.Status.IsWriteSupported = true;
            _tagReaderStatusChangedEventArgs.Status.IsAutoReadSupported = true;
            _lastNdefMessageBytes = new byte[] { };

            _session = new NFCTagReaderSession(NFCPollingOption.Iso14443, this, DispatchQueue.CurrentQueue);
            if (_session != null)
            {
                _tagReaderStatusChangedEventArgs.Status.Reader = TagReaderStatusChangedEventArgs.EReader.Available;
                _session.AlertMessage = "Hold your iPhone near the NTAG SmartSensor tag";
                _session.BeginSession();
            }
            else
            {
                _tagReaderStatusChangedEventArgs.Status.Reader = TagReaderStatusChangedEventArgs.EReader.NotAvailable;
            }

            _onTagReaderStatusChanged?.Invoke(this, _tagReaderStatusChangedEventArgs);
            await Task.CompletedTask;
        }

        /// <summary>
        /// DeInit API implementation.
        /// </summary>
        /// <returns></returns>
        public async Task DeInitTagReaderAsync()
        {
            _session?.InvalidateSession();
            _session = null;
            _tagReaderStatusChangedEventArgs.Status.Reader = TagReaderStatusChangedEventArgs.EReader.NotAvailable;
            _onTagReaderStatusChanged?.Invoke(this, _tagReaderStatusChangedEventArgs);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Tag connected API event.
        /// </summary>
        public event EventHandler<TagConnectedEventArgs> TagConnected;
        /// <summary>
        /// Tag disconnected API event.
        /// </summary>
        public event EventHandler<TagDisconnectedEventArgs> TagDisconnected;

        /// <summary>
        /// iOS callback when NDEF session is started.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="tags"></param>
        public override async void DidDetectTags(NFCTagReaderSession session, INFCTag[] tags)
        {
            Debug.WriteLine("#### SESSION STARTED");
            try
            {
                var tag = tags.First();
                if (tag.Type == (NFCTagType)4) // NFCTag.MiFare
                {
                    _tag = tag.GetNFCMiFareTag();
                    await session.ConnectToAsync(_tag);

                    // Read NDEF message.
                    var (status, ndefRecords) = await ReadAsync();
                    if (status != Status.OK)
                    {
                        throw new Exception("Read NDEF failed");
                    }

                    // If present in a dedicated MIME record, get the NFC ID from the NDEF message.
                    _tagIdStr = string.Empty;
                    foreach (var record in ndefRecords)
                    {
                        if (record.TypeNameFormat == NdefRecord.TypeNameFormatType.Mime
                            && record.Payload.Length >= 10
                            && record.Payload[0] == 0x0A // NFC ID message id
                            && record.Payload[1] == 0x01) // message sent from NHS31xx to tag reader
                        {
                            var tagId = new byte[]
                            {
                                record.Payload[2], record.Payload[3], record.Payload[4],
                                record.Payload[6], record.Payload[7], record.Payload[8], record.Payload[9]
                            };
                            _tagIdStr = BitConverter.ToString(tagId).Replace("-", string.Empty);
                            break;
                        }
                    }

                    TagConnected?.Invoke(this, new TagConnectedEventArgs
                    {
                        IsNhsTag = true, // assume true on iOS
                        TagId = _tagIdStr,
                        TagVersion = new byte[] { 0x00, 0x00 }, // assume 0.0 on iOS
                        NdefRecords = ndefRecords,
                    });
                }
            }
            catch (Exception ex) 
            {
                Helpers.ExceptionLogHelper.Log(_tagIdStr, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                Debug.WriteLine("Caught exception:" + _tagIdStr + " - " + Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                session.InvalidateSession();
            }
        }

        /// <summary>
        /// iOS callback when NDEF session is ended.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="error"></param>
        public override void DidInvalidate(NFCTagReaderSession session, NSError error)
        {
            Debug.WriteLine($"### SESSION ENDED: {error.Code} {error}");
            TagDisconnected?.Invoke(this, new TagDisconnectedEventArgs { });
            _session = null;
            _tag = null;
        }

        public async Task<(Status status, List<NdefRecord> rdNdefRecords)> WriteReadAsync(List<NdefRecord> wrNdefRecords)
        {
            var status = await WriteAsync(wrNdefRecords);
            if (status != Status.OK)
            {
                return (status, null);
            }

            await Task.Delay(40);
            return await ReadAsync();
        }

        /// <summary>
        /// Read API implementation.
        /// </summary>
        /// <returns></returns>
        public async Task<(Status status, List<NdefRecord> rdNdefRecords)> ReadAsync()
        {
            (Status status, List<NdefRecord> rdNdefRecords) ret = (Status.TagReadFailed, null);
            if (_tag == null)
            {
                return ret;
            }

            // Make sure only one tag reader operation is taking place.
            await _sem.WaitAsync();
            for (int i = 0; i < 10; i++)
            {
                ret = await ReadNdefAsync();
                if (ret.status == Status.OK)
                {
                    var rdNdefMessage = new NdefMessage();
                    rdNdefMessage.AddRange(ret.rdNdefRecords);
                    var rdNdefMessageBytes = rdNdefMessage.ToByteArray();
                    Debug.WriteLine($"{DateTime.Now.TimeOfDay} - ReadAsync DATA:" + BitConverter.ToString(rdNdefMessageBytes));

                    if (!rdNdefMessageBytes.SequenceEqual(_lastNdefMessageBytes))
                    {
                        _lastNdefMessageBytes = rdNdefMessageBytes;
                        break;
                    }
                    else
                    {
                        ret.status = Status.TagReadFailed;
                        ret.rdNdefRecords = null;
                    }
                }
                else
                {
                    break;
                }
                
                ////await Task.Delay(10);
            }
            _sem.Release();
            return ret;

            async Task<(Status status, List<NdefRecord> rdNdefRecords)> ReadNdefAsync()
            {
                try
                {
                    var tcs = new TaskCompletionSource<(Status, List<NdefRecord>)>();
                    _tag.ReadNdef((iosNdefMessage, error) =>
                    {
                        try
                        {
                            if (error != null)
                            {
                                throw new Exception(error.Description);
                            }

                            var ndefRecords = new List<NdefRecord>();
                            for (int i = 0; i < iosNdefMessage.Records.Length; i++)
                            {
                                ndefRecords.Add(new NdefRecord
                                {
                                    Id = iosNdefMessage.Records[i].Identifier.ToArray(),
                                    Type = iosNdefMessage.Records[i].Type.ToArray(),
                                    TypeNameFormat = (NdefRecord.TypeNameFormatType)iosNdefMessage.Records[i].TypeNameFormat,
                                    Payload = iosNdefMessage.Records[i].Payload.ToArray()
                                });
                            }
                            tcs.SetResult((Status.OK, ndefRecords));
                        }
                        catch(Exception ex) 
                        {
                            Helpers.ExceptionLogHelper.Log(_tagIdStr, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                            Debug.WriteLine("Caught exception:" + _tagIdStr + " - " + Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                            tcs.SetResult((Status.TagReadFailed, null));
                        }
                    });
                    return await tcs.Task;
                }
                catch(Exception ex)
                {
                    Helpers.ExceptionLogHelper.Log(_tagIdStr, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                    Debug.WriteLine("Caught exception:" + _tagIdStr + " - " + Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                    return (Status.TagReadFailed, null);
                }
            }
        }

        public async Task<Status> WriteAsync(List<NdefRecord> wrNdefRecords)
        {
            // Make sure only one tag reader operation is taking place.
            await _sem.WaitAsync();

            if (_tag != null)
            {
                try
                {
                    var tcs = new TaskCompletionSource<Status>();
                    var iosNdefRecords = new NFCNdefPayload[wrNdefRecords.Count];
                    for (int i = 0; i < wrNdefRecords.Count; i++)
                    {
                        var id = wrNdefRecords[i].Id == null ? new byte[] { } : wrNdefRecords[i].Id;
                        iosNdefRecords[i] = new NFCNdefPayload(
                            (NFCTypeNameFormat)wrNdefRecords[i].TypeNameFormat,
                            NSData.FromArray(wrNdefRecords[i].Type),
                            NSData.FromArray(id),
                            NSData.FromArray(wrNdefRecords[i].Payload));
                    }
                    var iosNdefMessage = new NFCNdefMessage(iosNdefRecords);
                    var wrNdefMessage = new NdefMessage();
                    wrNdefMessage.AddRange(wrNdefRecords);
                    Debug.WriteLine($"{DateTime.Now.TimeOfDay} - WriteAsync DATA:" + BitConverter.ToString(wrNdefMessage.ToByteArray()));
                    _tag.WriteNdef(iosNdefMessage, error =>
                    {
                        try
                        {
                            if (error != null)
                            {
                                throw new Exception("iOS NDEF write error");
                            }

                            _lastNdefMessageBytes = wrNdefMessage.ToByteArray();
                            tcs.SetResult(Status.OK);
                        }
                        catch(Exception ex)
                        {
                            Helpers.ExceptionLogHelper.Log(_tagIdStr, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                            Debug.WriteLine("Caught exception:" + _tagIdStr + " - " + Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                            tcs.SetResult(Status.TagWriteFailed);
                        }
                    });

                    _sem.Release();
                    return await tcs.Task;
                }
                catch(Exception ex) 
                {
                    Helpers.ExceptionLogHelper.Log(_tagIdStr, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                    Debug.WriteLine("Caught exception:" + _tagIdStr + " - " + Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                }
            }

            _sem.Release();
            return Status.TagWriteFailed;
        }
    }
}
