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
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using NdefLibrary.Ndef;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;

namespace TAdherence.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WritePopup : PopupPage
    {
        private readonly byte[] _payload;
        private readonly byte[] _expectedResponse;

        public WritePopup(byte[] payload, byte[] expectedResponse)
        {
            _payload = payload;
            _expectedResponse = expectedResponse;

            InitializeComponent();

            title.Text = "Tap the tag to write";
            description.Text = "";

            Plugin.Ndef.INdef ndef = Plugin.Ndef.CrossNdef.Current;
            if (Device.RuntimePlatform == Device.iOS)
            {
                ndef.InitTagReaderAsync(OnTagReaderStatus);
            }
            ndef.TagConnected += OnTagConnected;
            ndef.TagDisconnected += OnTagDisconnected;
        }

        private void OnTagReaderStatus(object sender, Plugin.Ndef.TagReaderStatusChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"reader: {e.Status.Reader}");
        }

        private void OnTagConnected(object sender, Plugin.Ndef.TagConnectedEventArgs e)
        {
            Task.Run(async () => await NdefWriteRead(_payload, _expectedResponse));
        }

        private void OnTagDisconnected(object sender, Plugin.Ndef.TagDisconnectedEventArgs e)
        {
        }

        private async void CancelButton_Clicked(object sender, EventArgs e)
        {
            await ExitAsync();
        }

        protected override bool OnBackgroundClicked()
        {
            return false;
        }

        private async Task ExitAsync()
        {
            Plugin.Ndef.INdef ndef = Plugin.Ndef.CrossNdef.Current;
            if (Device.RuntimePlatform == Device.iOS)
            {
                await ndef.DeInitTagReaderAsync();
            }
            ndef.TagConnected -= OnTagConnected;
            ndef.TagDisconnected -= OnTagDisconnected;
            await PopupNavigation.Instance.PopAsync();
        }

        private async Task NdefWriteRead(byte[] payload, byte[] expectedResponse)
        {
            List<NdefRecord> ndefRecordsToWrite = new List<NdefRecord> {
                new NdefRecord {
                    TypeNameFormat = NdefRecord.TypeNameFormatType.Mime,
                    Type = System.Text.Encoding.UTF8.GetBytes("n/p"),
                    Payload = payload
                }
            };
            (Plugin.Ndef.Status status, List<NdefRecord> ndefRecordsRead) = await Plugin.Ndef.CrossNdef.Current.WriteReadAsync(ndefRecordsToWrite);
            bool success = (status == Plugin.Ndef.Status.OK)
                && (ndefRecordsRead.Count >= 1)
                && (string.Compare(System.BitConverter.ToString(ndefRecordsRead[0].Payload), System.BitConverter.ToString(expectedResponse)) == 0);
            Device.BeginInvokeOnMainThread(async () => {
                (Application.Current.MainPage.FindByName("label") as Label).Text = success ? "Success" : "Error\nRemove tag and try again";
                await ExitAsync();
            });
        }
    }
}
