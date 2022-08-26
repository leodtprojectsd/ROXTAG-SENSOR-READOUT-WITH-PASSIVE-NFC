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

using Foundation;
using Monitor.iOS;

[assembly: Xamarin.Forms.Dependency(typeof(BaseUrl))]
namespace Monitor.iOS
{
    public class BaseUrl : Interfaces.IBaseUrl
    {
        public string Get()
        {
            return NSBundle.MainBundle.BundlePath;
        }

        public byte[] ReadAllBytesFromJsFolderFile(string fileName)
        {
            var filePath = NSBundle.MainBundle.PathForResource(fileName, null);
            return File.ReadAllBytes(filePath);

            /****
                        var len = new FileInfo(filePath).Length;
                        var data = File.ReadAllBytes(filePath);
                        return data;
            ****/
            /****
                        var data = NSData.FromFile(filePath);
                        var stream = new MemoryStream();
                        data.AsStream().CopyTo(stream);
                        var ret = stream.ToArray();
                        return ret;
            ****/
        }
    }
}
