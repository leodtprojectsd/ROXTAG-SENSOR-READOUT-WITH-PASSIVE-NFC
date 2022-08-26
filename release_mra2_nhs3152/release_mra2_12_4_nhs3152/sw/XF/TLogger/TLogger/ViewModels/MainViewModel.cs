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

using TLogger.Popups;
using Msg;
using Rg.Plugins.Popup.Services;
using SkiaSharp;
using System;
using System.IO;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace TLogger.ViewModels
{
    class MainViewModel
    {
        Msg.Lib _msgLib;
        Popups.DataProcessPopup _dataProcessPopup;

        public MainViewModel()
        {
            _msgLib = App.MsgLib;
            _msgLib.DataProcessEvent += OnDataProcessEvent;
        }

        async void OnDataProcessEvent(object sender, Lib.DataProcessEventArgs e)
        {
            switch(e.Op)
            {
                case Lib.DataProcessOp.Begin:
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        // Allow only one popup.
                        if(_dataProcessPopup != null)
                        {
                            await _dataProcessPopup.TerminateAsync();
                            _dataProcessPopup = null;
                        }

                        if (e.Total != 0 && e.Total >= e.Current && /*App.IsConfiguringTag*/Lib.IsConfiguringTag == false)
                        {
                            _dataProcessPopup = new Popups.DataProcessPopup();
                            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PushAsync(_dataProcessPopup);
                            _dataProcessPopup.SetDataRetrieval(e.Current, e.Total);
                        }
                    });
                    break;

                case Lib.DataProcessOp.Retrieval:
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        if (_dataProcessPopup != null)
                        {
                            if (/*App.IsConfiguringTag*/Msg.Lib.IsConfiguringTag)
                            {
                                await _dataProcessPopup.TerminateAsync();
                                _dataProcessPopup = null;
                            }
                            else
                                _dataProcessPopup.SetDataRetrieval(e.Current, e.Total);
                        }
                    });
                    break;

                case Lib.DataProcessOp.End:
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        if (_dataProcessPopup != null)
                        {
                            await _dataProcessPopup.TerminateAsync();
                            _dataProcessPopup = null;
                        }
                    });
                    break;

                case Lib.DataProcessOp.NonNhsTag:
                    await PopupNavigation.Instance.PushAsync(
                        new OkPopup(ImageSource.FromResource("TLogger.Images.error.png"),
                        string.Empty, "Non-NHS tag, tap NHS tag to proceed"));
                    break;

                case Lib.DataProcessOp.TextRecords:
                    await PopupNavigation.Instance.PushAsync(
                        new OkPopup(ImageSource.FromResource("TLogger.Images.ok.png"),
                        "Text Records", e.Text));
                    break;

                case Lib.DataProcessOp.UrlRecords:
                    await PopupNavigation.Instance.PushAsync(
                        new OkPopup(ImageSource.FromResource("TLogger.Images.ok.png"),
                        "URL records", e.Url));
                    break;
            }
        }
    }
}
