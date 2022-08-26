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
using Android.App;
using Android.Content;
using Android.OS;
using Monitor.Droid;

[assembly: Xamarin.Forms.Dependency(typeof(ShareFile))]
namespace Monitor.Droid
{
    public class ShareFile : Interfaces.IShareFile
    {
        async public Task ShareAsync(string[] filePaths, string title, string message)
        {
            var intent = new Intent(Intent.ActionSendMultiple);
            intent.SetFlags(ActivityFlags.GrantReadUriPermission);
            intent.AddFlags(ActivityFlags.NoHistory);
            intent.AddFlags(ActivityFlags.ClearWhenTaskReset | ActivityFlags.NewTask);

            var application = "text/plain";

            var uriList = new List<IParcelable>();
            foreach (string f in filePaths)
            {
                Java.IO.File file = new Java.IO.File(f);
                file.SetReadable(true);
                Android.Net.Uri uri = Android.Support.V4.Content.FileProvider.GetUriForFile(
                    Application.Context,
                    Application.Context.PackageName + ".fileprovider",
                    file);
                uriList.Add(uri);
            }
            intent.SetType(application);

            intent.PutExtra(Intent.ExtraText, message);
            intent.PutExtra(Intent.ExtraSubject, title);

            intent.PutParcelableArrayListExtra(Intent.ExtraStream, uriList);

            var chooserIntent = Intent.CreateChooser(intent, title);
            chooserIntent.SetFlags(ActivityFlags.ClearTop);
            chooserIntent.SetFlags(ActivityFlags.NewTask);
            Application.Context.StartActivity(chooserIntent);

            await Task.CompletedTask;
        }
    }
}
