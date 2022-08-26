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

using Android.Content;
using NdefLibrary.Ndef;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace Plugin.Ndef
{
    /// <summary>
    /// Interface for $safeprojectgroupname$
    /// </summary>
    internal class NdefImplementation : INdef
    {
        private Android.Nfc.Tag _tag;
        private TagReaderStatusChanged _onTagReaderStatus;
        private TagReaderStatusChangedEventArgs _tagReaderStatusEventArgs = new TagReaderStatusChangedEventArgs();

        // Provides exclusive access to NFC. Either Read or WriteRead is active at a time. 
        private SemaphoreSlim _sem = new SemaphoreSlim(1);

        private enum NfcEventIndex
        {
            Connect,
            Timer,
            WriteReq,
            ReadReq,
            Terminate,
        }

        private AutoResetEvent[] _nfcEvents = new AutoResetEvent[Enum.GetNames(typeof(NfcEventIndex)).Length];
        private NdefMessage _txNdefMsg = null;
        private TaskCompletionSource<Status> _txTcs = null;
        private TaskCompletionSource<(Status rxStat, List<NdefRecord> rxMsgDataList)> _rxTcs = null;

        // Holds previously read or written NDEF message to detect a new message in auto read.
        private byte[] _lastNdefMsgByteArray;
        private string _tagIdStr = "";

        internal NdefImplementation() => StartNfcTask();

        /// <summary>
        /// Init API implementation.
        /// </summary>
        /// <param name="onTagReaderStatus"></param>
        /// <returns></returns>
        async public Task InitTagReaderAsync(TagReaderStatusChanged onTagReaderStatus)
        {
            _onTagReaderStatus = onTagReaderStatus;
            _tagReaderStatusEventArgs.Status.IsWriteSupported = true;
            _tagReaderStatusEventArgs.Status.IsAutoReadSupported = true;
            _lastNdefMsgByteArray = new byte[] { };

            MessagingCenter.Subscribe<FormsAppCompatActivity>(this, MessagingCenterMessages.Android.OnCreate, (sender) =>
            {
                try
                {
                    var adapter = Android.Nfc.NfcAdapter.GetDefaultAdapter(sender);
                    if (adapter == null)
                    {
                        _tagReaderStatusEventArgs.Status.Reader = TagReaderStatusChangedEventArgs.EReader.NotAvailable;
                    }
                    else
                    {
                        _tagReaderStatusEventArgs.Status.Reader = TagReaderStatusChangedEventArgs.EReader.Available;
                    }

                    _onTagReaderStatus?.Invoke(this, _tagReaderStatusEventArgs);
                }
                catch (Exception ex)
                {
                    Helpers.ExceptionLogHelper.Log(_tagIdStr, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                    Debug.WriteLine("Caught exception:" + _tagIdStr + " - " + Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                }
            });

            MessagingCenter.Subscribe<FormsAppCompatActivity>(this, MessagingCenterMessages.Android.OnResume, (sender) =>
            {
                try
                {
                    var mainActivity = sender;
                    Android.Nfc.NfcAdapter adapter = Android.Nfc.NfcAdapter.GetDefaultAdapter(sender);
                    if (adapter == null || !adapter.IsEnabled)
                    {
                        return;
                    }

                    Android.App.PendingIntent pendingIntent = Android.App.PendingIntent.GetActivity(
                        mainActivity,
                        0,
                        new Intent(mainActivity, mainActivity.GetType()).AddFlags(ActivityFlags.SingleTop), 0);

                    IntentFilter ndefFilter = new IntentFilter(Android.Nfc.NfcAdapter.ActionNdefDiscovered);
                    ndefFilter.AddDataType("*/*");

                    IntentFilter tagFilter = new IntentFilter(Android.Nfc.NfcAdapter.ActionTagDiscovered);
                    tagFilter.AddCategory(Intent.CategoryDefault);

                    adapter.EnableForegroundDispatch(mainActivity, pendingIntent, new IntentFilter[] { ndefFilter, tagFilter }, null);
                }
                catch (Exception ex)
                {
                    Helpers.ExceptionLogHelper.Log(_tagIdStr, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                    Debug.WriteLine("Caught exception:" + _tagIdStr + " - " + Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                }
            });

            MessagingCenter.Subscribe<FormsAppCompatActivity>(this, MessagingCenterMessages.Android.OnPause, (sender) =>
            {
                try
                {
                    var mainActivity = sender;
                    var adapter = Android.Nfc.NfcAdapter.GetDefaultAdapter(sender);
                    if (adapter == null || !adapter.IsEnabled)
                    {
                        return;
                    }

                    adapter.DisableForegroundDispatch(mainActivity);
                }
                catch (Exception ex)
                {
                    Helpers.ExceptionLogHelper.Log(_tagIdStr, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                    Debug.WriteLine("Caught exception:" + _tagIdStr + " - " + Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                }
            });

            MessagingCenter.Subscribe<FormsAppCompatActivity, int>(this, MessagingCenterMessages.Android.OnNewAdapterState, (sender, arg) =>
            {
                try
                {
                    var newState = arg;
                    switch (newState)
                    {
                        case Android.Nfc.NfcAdapter.StateTurningOn:
                            break;

                        case Android.Nfc.NfcAdapter.StateOn:
                            _tagReaderStatusEventArgs.Status.Reader = TagReaderStatusChangedEventArgs.EReader.Available;
                            _onTagReaderStatus?.Invoke(this, _tagReaderStatusEventArgs);
                            break;

                        case Android.Nfc.NfcAdapter.StateTurningOff:
                            break;

                        case Android.Nfc.NfcAdapter.StateOff:
                            _tagReaderStatusEventArgs.Status.Reader = TagReaderStatusChangedEventArgs.EReader.NotAvailable;
                            _onTagReaderStatus?.Invoke(this, _tagReaderStatusEventArgs);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Helpers.ExceptionLogHelper.Log(_tagIdStr, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                    Debug.WriteLine("Caught exception:" + _tagIdStr + " - " + Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                }
            });

            MessagingCenter.Subscribe<FormsAppCompatActivity, Intent>(this, MessagingCenterMessages.Android.OnNewIntent, (sender, arg) =>
            {
                try
                {
                    var intent = arg;

                    if (_tagReaderStatusEventArgs.Status.Reader != TagReaderStatusChangedEventArgs.EReader.Available)
                    {
                        _tagReaderStatusEventArgs.Status.Reader = TagReaderStatusChangedEventArgs.EReader.Available;
                        _onTagReaderStatus?.Invoke(this, _tagReaderStatusEventArgs);
                    }                   

                    if ((intent.Action == Android.Nfc.NfcAdapter.ActionTagDiscovered) || (intent.Action == Android.Nfc.NfcAdapter.ActionNdefDiscovered))
                    {
                        _tag = intent.GetParcelableExtra(Android.Nfc.NfcAdapter.ExtraTag) as Android.Nfc.Tag;
                        _nfcEvents[(int)NfcEventIndex.Connect].Set();
                    }
                }
                catch (Exception ex)
                {
                    Helpers.ExceptionLogHelper.Log(_tagIdStr, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                    Debug.WriteLine("Caught exception:" + _tagIdStr + " - " + Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                }
            });
            await Task.CompletedTask;
        }

        /// <summary>
        /// DeInit API implementation.
        /// </summary>
        /// <returns></returns>
        async public Task DeInitTagReaderAsync()
        {
            try
            {
                MessagingCenter.Unsubscribe<FormsAppCompatActivity, Android.Nfc.NfcAdapter>(this, MessagingCenterMessages.Android.OnCreate);
                MessagingCenter.Unsubscribe<FormsAppCompatActivity, FormsAppCompatActivity>(this, MessagingCenterMessages.Android.OnResume);
                MessagingCenter.Unsubscribe<FormsAppCompatActivity, FormsAppCompatActivity>(this, MessagingCenterMessages.Android.OnPause);
                MessagingCenter.Unsubscribe<FormsAppCompatActivity, Intent>(this, MessagingCenterMessages.Android.OnNewIntent);
                _tagReaderStatusEventArgs.Status.Reader = TagReaderStatusChangedEventArgs.EReader.NotAvailable;
                _onTagReaderStatus?.Invoke(this, _tagReaderStatusEventArgs);
            }
            catch (Exception ex)
            {
                Helpers.ExceptionLogHelper.Log(_tagIdStr, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                Debug.WriteLine("Caught exception:" + _tagIdStr + " - " + Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
            }

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
        /// Write and Read API implementation.
        /// </summary>
        /// <returns></returns>
        public async Task<(Status status, List<NdefRecord> rdNdefRecords)> WriteReadAsync(List<NdefRecord> wrNdefRecords)
        {
            Status rxStat = Status.OK;
            List<NdefRecord> rdNdefRecords = null;
            bool isSemAcquired = false;

            try
            {
                if (_tagReaderStatusEventArgs.Status.Reader == TagReaderStatusChangedEventArgs.EReader.NotAvailable)
                {
                    rxStat = Status.TagReaderNotAvailable;
                    throw new Exception("Tag reader is not available");
                }

                var wrNdefMessage = new NdefMessage();
                wrNdefMessage.AddRange(wrNdefRecords);

                // Make sure only one NFC operation is taking place.
                await _sem.WaitAsync();
                isSemAcquired = true;

                // First invoke write.
                _txNdefMsg = wrNdefMessage;
                _txTcs = new TaskCompletionSource<Status>();
                _nfcEvents[(int)NfcEventIndex.WriteReq].Set();
                var txStat = Status.OK;

                await _txTcs.Task.ContinueWith(antecedent =>
                {
                    if (antecedent.IsCompleted)
                    {
                        txStat = antecedent.Result;
                    }
                    else
                    {
                        txStat = Status.TagWriteFailed;
                        throw new Exception("Tx TCS failure");
                    }
                });
                if (txStat != Status.OK)
                {
                    rxStat = txStat;
                    throw new Exception("Write NDEF message failed");
                }
                _lastNdefMsgByteArray = wrNdefMessage.ToByteArray();

                // Then invoke reading.
                _rxTcs = new TaskCompletionSource<(Status, List<NdefRecord>)>();
                _nfcEvents[(int)NfcEventIndex.ReadReq].Set();

                await _rxTcs.Task.ContinueWith(antecedent =>
                {
                    if (antecedent.IsCompleted)
                    {
                        rxStat = antecedent.Result.rxStat;
                        rdNdefRecords = antecedent.Result.rxMsgDataList;
                    }
                    else
                    {
                        rxStat = Status.TagReadFailed;
                        throw new Exception("Rx TCS failure");
                    }
                });
            }
            catch (Exception ex)
            {
                Helpers.ExceptionLogHelper.Log(_tagIdStr, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                Debug.WriteLine("Caught exception:" + _tagIdStr + " - " + Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
            }
            finally
            {
                if (isSemAcquired)
                {
                    _sem.Release();
                }
            }

            if (rxStat != Status.OK)
            {
                rdNdefRecords = null;
            }
            return (rxStat, rdNdefRecords);
        }

        /// <summary>
        /// Read API implemenetation.
        /// </summary>
        /// <returns></returns>
        public async Task<(Status status, List<NdefRecord> rdNdefRecords)> ReadAsync()
        {
            Status rxStat = Status.OK;
            List<NdefRecord> ndefRecords = null;
            bool isSemAcquired = false;

            try
            {
                if (_tagReaderStatusEventArgs.Status.Reader == TagReaderStatusChangedEventArgs.EReader.NotAvailable)
                {
                    rxStat = Status.TagReaderNotAvailable;
                    throw new Exception("Tag reader is not available");
                }

                // Make sure only one NFC operation is taking place.
                await _sem.WaitAsync();
                isSemAcquired = true;

                _rxTcs = new TaskCompletionSource<(Status, List<NdefRecord>)>();
                _nfcEvents[(int)NfcEventIndex.ReadReq].Set();

                await _rxTcs.Task.ContinueWith(antecedent =>
                {
                    if (antecedent.IsCompleted)
                    {
                        rxStat = antecedent.Result.rxStat;
                        ndefRecords = antecedent.Result.rxMsgDataList;
                    }
                    else
                    {
                        rxStat = Status.TagReadFailed;
                        throw new Exception("Rx TCS failure");
                    }
                });
            }
            catch (Exception ex)
            {
                Helpers.ExceptionLogHelper.Log(_tagIdStr, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                Debug.WriteLine("Caught exception:" + _tagIdStr + " - " + Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
            }
            finally
            {
                if (isSemAcquired)
                {
                    _sem.Release();
                }
            }

            if (rxStat != Status.OK)
            {
                ndefRecords = null;
            }
            return (rxStat, ndefRecords);
        }

        private void StartNfcTask()
        {
            Task.Run(async () => { await NfcTaskAsync(); });
        }

        private async Task NfcTaskAsync()
        {
            for (int i = 0; i < _nfcEvents.Length; i++)
            {
                _nfcEvents[i] = new AutoResetEvent(false);
            }

            {
                bool isTerminate = false;
                while (!isTerminate)
                {
                    int index = WaitHandle.WaitAny(_nfcEvents);
                    switch (index)
                    {
                        case (int)NfcEventIndex.Connect:
                            try
                            {
                                (Status status, NdefMessage ndefMsg, byte[] uid, byte[] tagVer) = await ReadNdefMessageAsync(true);
                                if (status == Status.OK && ndefMsg != null && uid != null && tagVer != null)
                                {
                                    _tagIdStr = BitConverter.ToString(uid).Replace("-", string.Empty);
                                    var args = new TagConnectedEventArgs
                                    {
                                        Status = Status.OK,
                                        IsNhsTag = (tagVer != null) && (tagVer.Length == 8) &&
                                            (tagVer[0] == 0x00) && (tagVer[1] == 0x04) && (tagVer[2] == 0x04) && (tagVer[3] == 0x06) && (tagVer[7] == 0x03),
                                        TagId = _tagIdStr,
                                        TagVersion = new byte[] { tagVer[4], tagVer[5] },
                                        NdefRecords = ndefMsg.ToList(),
                                    };

                                    Device.StartTimer(TimeSpan.FromMilliseconds(222), () =>
                                    {
                                        _nfcEvents[(int)NfcEventIndex.Timer].Set();
                                        return false;
                                    });
                                    TagConnected?.Invoke(this, args);
                                }
                            }
                            catch (Exception ex)
                            {
                                Helpers.ExceptionLogHelper.Log(_tagIdStr, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                                Debug.WriteLine("Caught exception:" + _tagIdStr + " - " + Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                            }
                            break;

                        case (int)NfcEventIndex.Timer:
                            Android.Nfc.Tech.MifareUltralight mifareUltralight = null;
                            try
                            {
                                mifareUltralight = Android.Nfc.Tech.MifareUltralight.Get(_tag);
                                mifareUltralight.Connect();
                                mifareUltralight.Close();
                                mifareUltralight = null;

                                Device.StartTimer(TimeSpan.FromMilliseconds(111), () =>
                                {
                                    _nfcEvents[(int)NfcEventIndex.Timer].Set();
                                    return false;
                                });
                            }
                            catch (Exception ex)
                            {
                                if (mifareUltralight != null && mifareUltralight.IsConnected)
                                {
                                    mifareUltralight.Close();
                                }

                                Debug.WriteLine($"+++ DISCONNECT " + ex.Message);
                                TagDisconnected?.Invoke(this, new TagDisconnectedEventArgs { });
                            }
                            break;

                        case (int)NfcEventIndex.WriteReq:
                            try
                            {
                                Status status = await WriteNdefMessageAsync(_txNdefMsg);
                                _txTcs?.SetResult(status);
                            }
                            catch (Exception ex)
                            {
                                Helpers.ExceptionLogHelper.Log(_tagIdStr, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                                Debug.WriteLine("Caught exception:" + _tagIdStr + " - " + Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                            }
                            break;

                        case (int)NfcEventIndex.ReadReq:
                            try
                            {
                                List<NdefRecord> ndefRecords = null;
                                (Status status, NdefMessage ndefMsg, byte[] uid, byte[] tagVer) = await ReadNdefMessageAsync();
                                if (status == Status.OK)
                                {
                                    ndefRecords = ndefMsg.ToList();
                                }
                                _rxTcs?.SetResult((status, ndefRecords));
                            }
                            catch (Exception ex)
                            {
                                Helpers.ExceptionLogHelper.Log(_tagIdStr, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                                Debug.WriteLine("Caught exception:" + _tagIdStr + " - " + Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                            }
                            break;

                        case (int)NfcEventIndex.Terminate:
                            isTerminate = true;
                            break;
                    }
                }
            }
        }

        private async Task<Status> WriteNdefMessageAsync(NdefMessage msg)
        {
            Status status = Status.OK;
            Android.Nfc.Tech.Ndef ndef = null;

            try
            {
                Debug.WriteLine($"{DateTime.Now.TimeOfDay} - WriteNdefMessageAsync DATA:" + BitConverter.ToString(msg.ToByteArray()));

                List<Android.Nfc.NdefRecord> records = new List<Android.Nfc.NdefRecord>();
                for (int i = 0; i < msg.Count; i++)
                {
                    if (msg[i].CheckIfValid())
                    {
                        records.Add(new Android.Nfc.NdefRecord(
                            Android.Nfc.NdefRecord.TnfMimeMedia,
                            msg[i].Type,
                            msg[i].Id,
                            msg[i].Payload));
                    }
                    else
                    {
                        status = Status.InvalidNdefData;
                        throw new Exception("Invalid NDEF data");
                    };
                }

                Android.Nfc.NdefMessage m = new Android.Nfc.NdefMessage(records.ToArray());
                ndef = Android.Nfc.Tech.Ndef.Get(_tag);
                ndef.Connect();
                await ndef.WriteNdefMessageAsync(m);
                ndef.Close();
                ndef = null;
            }
            catch (Exception ex)
            {
                Helpers.ExceptionLogHelper.Log(_tagIdStr, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                Debug.WriteLine("Caught exception:" + _tagIdStr + " - " + Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                status = Status.TagWriteFailed;
            }
            finally
            {
                if (ndef != null && ndef.IsConnected)
                {
                    ndef.Close();
                }
            }

            return status;
        }

        private async Task<(Status status, NdefMessage ndefMsg, byte[] uid, byte[] tagVer)> ReadNdefMessageAsync(bool isNewTag = false)
        {
            if (isNewTag)
            {
                Android.Nfc.Tech.MifareUltralight mifareUltralight = null;
                try
                {
                    // Read low level info.
                    // From the HW documentation:
                    // The GET_VERSION command [60h] is used to retrieve information on the MIFARE family, product
                    // version, storage size and other product data required to identify the product. This command is
                    // available on other MIFARE products to have a common way of identifying products across platforms
                    // and evolution steps. The GET_VERSION command has no arguments and replies the version
                    // information for the specific type.
                    // GET_VERSION response for NHS devices:
                    // 0	Fixed header	00h
                    // 1	Vendor ID	04h	NXP Semiconductors
                    // 2	Product type	04h	NTAG
                    // 3	Product subtype	06h	NHS
                    // 4	Major product version	00h
                    // 5	Minor product version	00h
                    // 6	Size	13h
                    // 7	Protocol type	03h	ISO/IEC 14443-3 compliant
                    var uid = _tag.GetId();
                    mifareUltralight = Android.Nfc.Tech.MifareUltralight.Get(_tag);
                    mifareUltralight.Connect();
                    var tagVer = await mifareUltralight.TransceiveAsync(new byte[] { 0x60 });
                    mifareUltralight.Close();
                    mifareUltralight = null;

                    var ndef = Android.Nfc.Tech.Ndef.Get(_tag);
                    var ndefMsg = NdefMessage.FromByteArray(ndef.CachedNdefMessage.ToByteArray());
                    Debug.WriteLine($"{DateTime.Now.TimeOfDay} - ReadNdefMessageAsync DATA:" + BitConverter.ToString(ndefMsg.ToByteArray()));
                    _lastNdefMsgByteArray = ndefMsg.ToByteArray();
                    return (Status.OK, ndefMsg, uid, tagVer);
                }
                catch (Exception ex)
                {
                    Helpers.ExceptionLogHelper.Log(_tagIdStr, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                    Debug.WriteLine("Caught exception:" + _tagIdStr + " - " + Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                    if (mifareUltralight != null && mifareUltralight.IsConnected)
                    {
                        mifareUltralight.Close();
                    }

                    return (Status.TagReadFailed, null, null, null);
                }
            }
            else
            {
                Android.Nfc.Tech.Ndef ndef = null;
                for (int i = 0; i < 8; i++)
                {
                    try
                    {
                        await Task.Delay(70);

                        ndef = Android.Nfc.Tech.Ndef.Get(_tag);
                        ndef.Connect();
                        var ndefMsg = NdefMessage.FromByteArray(ndef.NdefMessage.ToByteArray());
                        ndef.Close();
                        ndef = null;
                        Debug.WriteLine($"{DateTime.Now.TimeOfDay} - ReadNdefMessageAsync DATA:" + BitConverter.ToString(ndefMsg.ToByteArray()));

                        // Check if we get a new message by comparing to the previous content.
                        // The content must be different.
                        if (!_lastNdefMsgByteArray.SequenceEqual(ndefMsg.ToByteArray()))
                        {
                            _lastNdefMsgByteArray = ndefMsg.ToByteArray();
                            return (Status.OK, ndefMsg, null, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        Helpers.ExceptionLogHelper.Log(_tagIdStr, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                        Debug.WriteLine("Caught exception:" + _tagIdStr + " - " + Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                        if (ndef != null && ndef.IsConnected)
                        {
                            ndef.Close();
                            ndef = null;
                        }
                    }
                }
                return (Status.TagReadFailed, null, null, null);
            }
        }

        public Task<Status> WriteAsync(List<NdefRecord> wrNdefRecords)
        {
            throw new NotImplementedException();
        }
    }
}
