using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AppKit;
using CoreGraphics;
using Foundation;
using ImageIO;
using MobileCoreServices;
using Plugin.Screenshot.Abstractions;



namespace Plugin.Screenshot
{
    public class ScreenshotImplementation : IScreenshot
    {
        [DllImport("/System/Library/Frameworks/ApplicationServices.framework/Versions/A/Frameworks/CoreGraphics.framework/CoreGraphics")]
        static extern IntPtr CGWindowListCreateImage(CGRect screenBounds, CGWindowListOption windowOption, 
            uint windowID, CGWindowImageOption imageOption);

        public async Task<string> CaptureAndSaveAsync()
        {
            await Task.Delay(1000);
            string filePath = string.Empty;

            using (var pool = new NSAutoreleasePool())
            {
                CGRect windowSize = NSScreen.MainScreen.Frame;
                IntPtr imageRef = CGWindowListCreateImage(windowSize, CGWindowListOption.All, 0, 
                    CGWindowImageOption.Default);
                var cgImage = new CGImage(imageRef);
                string date = DateTime.Now.ToString().Replace("/", "-").Replace(":", "-");
                filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                    "Screnshot-" + date + ".png");
                var fileURL = new NSUrl(filePath, false);
                var imageDestination = CGImageDestination.Create(fileURL, UTType.PNG, 1);
                imageDestination.AddImage(cgImage);
                imageDestination.Close();
            }

            return filePath;
        }

        public async Task<byte[]> CaptureAsync()
        {
            await Task.Delay(1000);
            var image = new byte[0];
            using (var pool = new NSAutoreleasePool())
            {
                // TODO: Investigate how to get the only app screen.
                CGRect windowSize = NSApplication.SharedApplication.KeyWindow.Frame;
                IntPtr imageRef = CGWindowListCreateImage(windowSize, CGWindowListOption.All, 0,
                    CGWindowImageOption.Default);
                var cgImage = new CGImage(imageRef);

#if false

                //// TODO: Investigate how to get raw data from image. CUrrently saved into a temp file and raw data is obtained from there.
                var rawData = new byte[cgImage.Width * cgImage.Height * 4];
                var handle = GCHandle.Alloc(rawData);
                var content = new CGBitmapContext(rawData, cgImage.Width, cgImage.Height, 8, 4 * cgImage.Width,
                    cgImage.ColorSpace, CGImageAlphaInfo.PremultipliedLast);
                content.DrawImage(new CGRect(0, 0, cgImage.Width, cgImage.Height),cgImage);
                handle.Free();





                ///image = cgImage.DataProvider .CopyData().ToArray();
                image = rawData; 
#endif

                var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                    "temporary.png");
                var fileURL = new NSUrl(filePath, false);
                var imageDestination = CGImageDestination.Create(fileURL, UTType.PNG, 1);
                imageDestination.AddImage(cgImage);
                imageDestination.Close();

                image = File.ReadAllBytes(filePath);
                File.Delete(filePath);
                              
            }

            return image;
        }

    }
}
