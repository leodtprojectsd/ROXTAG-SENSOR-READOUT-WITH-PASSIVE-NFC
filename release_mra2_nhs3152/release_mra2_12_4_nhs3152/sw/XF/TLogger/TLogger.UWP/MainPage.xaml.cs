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

using Windows.Foundation;
using Windows.UI.ViewManagement;

namespace TLogger.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
            ApplicationView.PreferredLaunchViewSize = new Size(800, 1024);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            LoadApplication(new TLogger.App());
        }
    }
}
