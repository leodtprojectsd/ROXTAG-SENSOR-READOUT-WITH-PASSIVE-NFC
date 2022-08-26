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
using Foundation;
using Monitor.iOS;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(ShareFile))]
namespace Monitor.iOS
{
    public class ShareFile : Interfaces.IShareFile
    {
        async public Task ShareAsync(string[] filePaths, string title, string message)
        {
            var rootController = UIApplication.SharedApplication.KeyWindow.RootViewController;
            if (rootController.PresentedViewController != null)
            {
                if (rootController.PresentedViewController is UINavigationController)
                {
                    rootController = ((UINavigationController)rootController.PresentedViewController).TopViewController;
                }
                else if (rootController.PresentedViewController is UITabBarController)
                {
                    rootController = ((UITabBarController)rootController.PresentedViewController).SelectedViewController;
                }
                else
                {
                    rootController = rootController.PresentedViewController;
                }
            }


            var urlList = new List<NSObject>();
            foreach (string f in filePaths)
            {
                var url = NSUrl.FromFilename(f);
                urlList.Add(url);
            }

            var activityController = new UIActivityViewController(urlList.ToArray(), null);
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                if (activityController.PopoverPresentationController != null)
                {
                    activityController.PopoverPresentationController.SourceView = rootController.View;
                }
            }

            if (!string.IsNullOrWhiteSpace(title))
            {
                activityController.SetValueForKey(NSObject.FromObject(title), new NSString("subject"));
            }

            await rootController.PresentViewControllerAsync(activityController, true);
        }
    }
}
