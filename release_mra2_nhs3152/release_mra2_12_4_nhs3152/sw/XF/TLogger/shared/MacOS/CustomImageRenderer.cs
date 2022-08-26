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

using System.ComponentModel;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;
using AppKit;
using System;

[assembly: ExportRenderer(typeof(Image), typeof(Monitor.MacOS.Renderers.CustomImageRenderer))]
namespace Monitor.MacOS.Renderers
{
    public class CustomImageRenderer : ImageRenderer
    {
        // To solve MacOS issue:
        // https://github.com/xamarin/Xamarin.Forms/issues/5434
        // But this sorts out the issue partially.
        // Navigation has been disabled on MacOS till this issue is resolved.
        protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
        {
            //try
            //{
                base.OnElementChanged(e);
            //}
            //catch(Exception ex)
            //{
              //  var x = ex.Message;
            //}
        }

        protected override void Dispose(bool disposing)
        {
            //try
            //{
                base.Dispose(disposing);
            //}
            //catch (Exception ex)
            //{
              //  var x = ex.Message;
            //}
        }

    }
}
