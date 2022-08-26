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

using Android.Content;
using Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomWebView), typeof(Monitor.Droid.Renderers.CustomWebViewRenderer))]
namespace Monitor.Droid.Renderers
{
    public class CustomWebViewRenderer : WebViewRenderer
    {
        public CustomWebViewRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<WebView> e)
        {
            base.OnElementChanged(e);
        }

        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            // Disable vertical scrolling.
            if (Control != null)
            {
                Control.VerticalScrollBarEnabled = false;
                Control.ScrollbarFadingEnabled = false;
                Control.ScrollChange += Control_ScrollChange;
            }
        }

        private void Control_ScrollChange(object sender, ScrollChangeEventArgs e)
        {
            if (e.ScrollY > 0)
            {
                Control.ScrollY = 0;
            }
        }
    }
}
