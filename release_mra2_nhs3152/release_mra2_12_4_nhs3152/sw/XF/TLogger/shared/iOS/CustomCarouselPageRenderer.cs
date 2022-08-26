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

using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CarouselPage), typeof(Monitor.iOS.Renderers.CustomCarouselPageRenderer))]
namespace Monitor.iOS.Renderers
{
    public class CustomCarouselPageRenderer : CarouselPageRenderer
    {
        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            UIView view = this.NativeView;
            UIScrollView scrollView = (UIKit.UIScrollView)view.Subviews[0];
            scrollView.ShowsVerticalScrollIndicator = false;
            scrollView.ContentSize = new CoreGraphics.CGSize(scrollView.ContentSize.Width, scrollView.Frame.Size.Height);
            AutomaticallyAdjustsScrollViewInsets = false;
        }
    }
}
