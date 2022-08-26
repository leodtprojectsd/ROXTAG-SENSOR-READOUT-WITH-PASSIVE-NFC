/*
 * Copyright 2019 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace TLogger.ViewModels
{
    class AboutViewMode : ObservableObject
    {
        string _baseUrl = DependencyService.Get<Interfaces.IBaseUrl>().Get();

        public AboutViewMode()
        {
            TitleText = "ABOUT";

            UrlContentSource.Url = System.IO.Path.Combine(_baseUrl, "about_tlogger.html");
        }

        string _titleText;
        public string TitleText
        {
            get => _titleText;
            set => SetProperty(ref _titleText, value);
        }

        UrlWebViewSource _urlContentSource = new UrlWebViewSource()
        {
            Url = string.Empty
        };
        public UrlWebViewSource UrlContentSource
        {
            get => _urlContentSource;
            set => SetProperty(ref _urlContentSource, value);
        }
    }
}
