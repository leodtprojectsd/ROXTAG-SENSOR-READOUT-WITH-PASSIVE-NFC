/*
 * Copyright 2019-2020 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

using AppKit;
using Foundation;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

namespace TLogger.MacOS
{
    [Register("AppDelegate")]
    public class AppDelegate : FormsApplicationDelegate
    {
        NSWindow window;
        public AppDelegate()
        {
            var style = NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Titled;
            var rect = new CoreGraphics.CGRect(200, 1000, 768, 1000);
            window = new NSWindow(rect, style, NSBackingStore.Buffered, false);
            window.Title = "TLogger";
            window.TitleVisibility = NSWindowTitleVisibility.Visible;
        }

        public override NSWindow MainWindow
        {
            get { return window; }
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            Msg.Board.BoardType = Msg.Board.EBoardType.TLogger;
            Rg.Plugins.Popup.Popup.Init();

            Forms.Init();
            LoadApplication(new TLogger.App());

            base.DidFinishLaunching(notification);
        }
    }
}
