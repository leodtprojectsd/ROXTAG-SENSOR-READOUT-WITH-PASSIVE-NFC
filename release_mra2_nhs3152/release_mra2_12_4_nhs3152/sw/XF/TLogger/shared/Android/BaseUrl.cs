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

using System.IO;

using Android.App;
using Monitor.Droid;


[assembly: Xamarin.Forms.Dependency(typeof(BaseUrl))]
namespace Monitor.Droid
{
    public class BaseUrl : Interfaces.IBaseUrl
    {
        public string Get()
        {
            return "file:///android_asset/";
        }

        public byte[] ReadAllBytesFromJsFolderFile(string fileName)
        {
            var data = new byte[0];
            var assetFd = Application.Context.Assets.OpenFd(fileName);
            var asset = Application.Context.Assets.Open(fileName);
            using (BinaryReader stream = new BinaryReader(asset))
            {
                data = stream.ReadBytes((int)assetFd.Length);
            }
            return data;
        }
    }
}
