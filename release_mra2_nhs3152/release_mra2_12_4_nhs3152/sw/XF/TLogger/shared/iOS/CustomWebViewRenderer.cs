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
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CustomWebView), typeof(Monitor.iOS.Renderers.CustomWebViewRenderer))]
namespace Monitor.iOS.Renderers
{
    public class CustomWebViewRenderer : WkWebViewRenderer
    {
        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            // Remove the black line at the bottom.
            if (NativeView != null)
            {
                var webView = (UIWebView)NativeView;
                webView.Opaque = false;
                webView.BackgroundColor = UIColor.Clear;
                webView.ScrollView.Scrolled += ScrollView_Scrolled;
            }
        }

        private void ScrollView_Scrolled(object sender, System.EventArgs e)
        {
            var scrollView = (UIScrollView)sender;
            if (scrollView.ContentOffset.Y != 0)
            {
                scrollView.ContentOffset = new CoreGraphics.CGPoint(scrollView.ContentOffset.X, 0);
            }
        }
    }
}
