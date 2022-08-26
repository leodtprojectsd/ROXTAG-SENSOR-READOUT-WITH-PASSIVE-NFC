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

using Renderers;
using System;
using System.IO;
using Windows.Storage;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(CustomWebView), typeof(Monitor.UWP.Renderers.CustomWebViewRenderer))]
namespace Monitor.UWP.Renderers
{
    public class CustomWebViewRenderer : Xamarin.Forms.Platform.UWP.WebViewRenderer, IWebViewDelegate
    {
        // To solve refreshing problem.
        void IWebViewDelegate.LoadHtml(string html, string baseUrl)
        {
            if (!string.IsNullOrEmpty(baseUrl))
            {
                var file = Path.GetFileName(baseUrl);

                // UWP HtmlWebViewSource data binding works only the first time for an unknown reason!!!
                // Solution is to create an html file in custom renderer and use navigate.
                // This requires copying of all js used files (html, png, js etc.) to 
                // ms-appdata:///local/html/ as html file is created there.
                string path = Path.Combine(ApplicationData.Current.LocalFolder.Path, "html", file);
                File.WriteAllText(path, html);
                var uri = new Uri($"ms-appdata:///local/html/{file}");

                Control.Navigate(uri);
            }
            else
            {
                LoadHtml(html, baseUrl);
            }
        }
    }
}
