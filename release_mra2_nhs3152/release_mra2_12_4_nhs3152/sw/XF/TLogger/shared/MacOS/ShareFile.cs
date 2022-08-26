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

using System.Collections.Generic;
using System.Threading.Tasks;
using AppKit;
using Foundation;
using Monitor.MacOS;

[assembly: Xamarin.Forms.Dependency(typeof(ShareFile))]
namespace Monitor.MacOS
{
    public class ShareFile : Interfaces.IShareFile
    {
        async public Task ShareAsync(string[] filePaths, string title, string message)
        {
            var urlList = new List<NSObject>();
            foreach (string f in filePaths)
            {
                var url = NSUrl.FromFilename(f);
                urlList.Add(url);
            }

            NSSharingServicePicker picker = new NSSharingServicePicker(urlList.ToArray());
            var windowSize = NSApplication.SharedApplication.KeyWindow.ContentView.Frame;
            picker.ShowRelativeToRect(new CoreGraphics.CGRect(windowSize.Left, windowSize.Top, 0, 0),
                NSApplication.SharedApplication.KeyWindow.ContentView, NSRectEdge.MinYEdge);

            await Task.CompletedTask;
        }
    }
}
