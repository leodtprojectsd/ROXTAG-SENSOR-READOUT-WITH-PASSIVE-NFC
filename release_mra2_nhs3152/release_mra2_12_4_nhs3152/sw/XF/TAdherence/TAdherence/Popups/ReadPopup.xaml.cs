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
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;

namespace TAdherence.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReadPopup : PopupPage
    {
        public ReadPopup()
        {
            InitializeComponent();

            title.Text = "Tap the tag to read";
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
            string text = "";
            foreach (NdefLibrary.Ndef.NdefRecord ndefRecord in e.NdefRecords)
            {
                if (ndefRecord.TypeNameFormat == NdefLibrary.Ndef.NdefRecord.TypeNameFormatType.Mime)
                {
                    //string mime = System.Text.Encoding.UTF8.GetString(ndefRecord.Type);
                    //string bytes = System.BitConverter.ToString(ndefRecord.Payload);
                    //text += $"{mime} : {bytes}\n";
                }
                else /* Assume NfcRtd */
                {
                    if (System.Text.Encoding.UTF8.GetString(ndefRecord.Type, 0, ndefRecord.Type.Length) == "T")
                    {
                        int languageLen = ndefRecord.Payload[0];
                        string plaintext = System.Text.Encoding.UTF8.GetString(ndefRecord.Payload, languageLen + 1, ndefRecord.Payload.Length - languageLen - 1).Replace("\0", "\n");
                        text += $"{plaintext}\n";
                    }
                    else /* Assume "U" */
                    {
                        string prefix = System.BitConverter.ToString(ndefRecord.Payload, 0, 1);
                        _ = new Dictionary<byte, string>
                        {
                            { 0x00, ""},
                            { 0x01, "http://www."},
                            { 0x02, "https://www."},
                            { 0x03, "http://"},
                            { 0x04, "https://"},
                            { 0x06, "mailto:"},
                            { 0x07, "ftp://anonymous:anonymous@"},
                            { 0x08, "ftp://ftp."},
                            { 0x09, "ftps://"},
                            { 0x0A, "sftp://"}
                        }.TryGetValue(ndefRecord.Payload[0], out prefix);
                        string url = System.Text.Encoding.UTF8.GetString(ndefRecord.Payload, 1, ndefRecord.Payload.Length - 1);
                        text += $"{prefix}{url}\n";
                    }
                }
            }
            Device.BeginInvokeOnMainThread(() => (Application.Current.MainPage.FindByName("label") as Label).Text = text);

            Task.Run(async () => await ExitAsync());
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
    }
}
