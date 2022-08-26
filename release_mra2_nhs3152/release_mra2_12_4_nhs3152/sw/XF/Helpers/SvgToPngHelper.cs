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

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using SkiaSharp;

namespace Helpers
{
    public class SvgToPngHelper
    {
        /// <summary>
        /// This method converts the svgData (string) obtained from FusionCharts into png raw data using SkiaSharp.
        /// Points to consider:
        ///     - svgData contains escape characters for each quotation mark as \" and escape characters
        ///         should be removed.
        ///     - On Android less than character is represented in unicode \u003C and should be converted into
        ///         less than character.
        ///     - <text></text> tags with no or empty content causes exception in SkiaSharp on Load function.
        ///         These tags should be removed.
        ///     - SkiaSharp does not include the image files on conversion. Each image file should be rendered
        ///         manually.
        /// </summary>
        /// <param name="svgData"></param>
        /// <returns>pngData, null if SVG data cannot be converted</returns>
        /// <returns>filteredSvgData</returns>
        /// 
        /// IMPORTANT!!!
        // On iOS the png files are compressed by using pngcrush:
        //      http://osxdaily.com/2013/08/15/pngcrush-mac-os-x/
        // Compressed pngs cannot be handled by SkiaSharp:
        //      https://github.com/mono/SkiaSharp/issues/140
        // It seems the build option on iOS Optimize PNG images has no effect.
        // Make sure the PNG optimization is turned off in the project file by adding
        //      <OptimizePNGs>false</OptimizePNGs> to each configuration.
        //
        // If an exception occcurs for whatever reason we return null pngData. The
        // filtered SVG data is always returned.

        public (byte[] pngData, string filteredSvgData) Convert(string svgData, Func<string, byte[]> readFile)
        {
            // Remove all newline character.
            svgData = svgData.Replace(Environment.NewLine, "");

            // Remove all escape characters - \".
            svgData = svgData.Replace("\\\"", "\"");

            // Convert unicode <.
            svgData = svgData.Replace("\\u003C", "<");

            // Remove empty text tags.
            svgData = Regex.Replace(svgData, @"<text\s*(.+?)\s*</text>", m =>
            {
                if (Regex.Match(m.Value, "> *?<").Success)
                {
                    return "";
                }
                else
                {
                    return m.Value;
                }
            });

            var filteredSvgData = svgData;

            ////var text = Regex.Matches(svgData, @"<text\s*(.+?)\s*</text>");

            // Draw svg. 
            var svg = new SkiaSharp.Extended.Svg.SKSvg();
            var picture = svg.Load(new MemoryStream(Encoding.ASCII.GetBytes(svgData)));
            var dimension = new SKSizeI((int)Math.Ceiling(picture.CullRect.Width),
                (int)Math.Ceiling(picture.CullRect.Height));
            var bitmap = new SKBitmap(dimension.Width, dimension.Height);
            var canvas = new SKCanvas(bitmap);
            canvas.DrawPicture(svg.Picture);

            // Draw all icons.
            float x, y, width, height;
            string icon = string.Empty;
            var icons = Regex.Matches(svgData, @"<image\s*(.+?)\s*/>");
            for (int i = 0; i < icons.Count; i++)
            {
                try
                {
                    var xStr = Regex.Match(icons[i].Value, @"x\s*=\s*(.+?)""");
                    if (!xStr.Success)
                    {
                        throw new Exception("Couldn't get x");
                    }

                    x = System.Convert.ToSingle(xStr.Value.Replace(" ", "").Split('=')[1].Replace("\"", ""),
                        GlobalHelper.FormatProvider);

                    var yStr = Regex.Match(icons[i].Value, @"y\s*=\s*(.+?)""");
                    if (!yStr.Success)
                    {
                        throw new Exception("Couldn't get y");
                    }

                    y = System.Convert.ToSingle(yStr.Value.Replace(" ", "").Split('=')[1].Replace("\"", ""),
                         GlobalHelper.FormatProvider);

                    var widthStr = Regex.Match(icons[i].Value, @"width\s*=\s*(.+?)""");
                    if (!widthStr.Success)
                    {
                        throw new Exception("Couldn't get width");
                    }

                    width = System.Convert.ToSingle(widthStr.Value.Replace(" ", "").Split('=')[1].Replace("\"", ""),
                        GlobalHelper.FormatProvider);

                    var heightStr = Regex.Match(icons[i].Value, @"height\s*=\s*(.+?)""");
                    if (!heightStr.Success)
                    {
                        throw new Exception("Couldn't get height");
                    }

                    height = System.Convert.ToSingle(heightStr.Value.Replace(" ", "").Split('=')[1].Replace("\"", ""),
                        GlobalHelper.FormatProvider);

                    var iconStr = Regex.Match(icons[i].Value, @"xlink:href\s*=\s*(.+?)""");
                    if (!iconStr.Success)
                    {
                        throw new Exception("Couldn't get icon");
                    }

                    icon = iconStr.Value.Trim().Split('=')[1].Replace("\"", "");
                    var iconImg = SKImage.FromBitmap(SKBitmap.Decode(readFile(icon)));
                    canvas.DrawImage(iconImg, new SKRect(x, y, x + width, y + height));
                }
                catch (Exception ex)
                {
                    Helpers.ExceptionLogHelper.Log(string.Empty, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                    System.Diagnostics.Debug.WriteLine("Caught exception:" + Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                    continue;
                }
            }

            canvas.Flush();
            canvas.Save();

            byte[] pngData = null;
            using (var image = SKImage.FromBitmap(bitmap))
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            {
                pngData = data.ToArray();
            }

            return (pngData, filteredSvgData);
        }
    }
}
