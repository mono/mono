//------------------------------------------------------------------------------
// <copyright file="ImageCreator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls.Util
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;

    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal sealed class ImageCreator
    {
        const String _fontFamily = "Tahoma"; // default font used for the 
                                             // title and error message

        private static int GetHeight(
            String text,
            Font font,
            int width
        ) {
            // THIS----S: I need a bitmap to get a graphics object to measure
            // the string, but I can not create the bitmap I intend to return
            // until I know how big it needs to be...

            using(Bitmap bmp = new Bitmap(1,1))
            {
            using(Graphics g = Graphics.FromImage(bmp))
            {
                SizeF size = new SizeF(width, 0);
                size = g.MeasureString(text, font, size);
                return (int) (size.Height + 1);
            }} // using bmp, g
        }
        
        internal static void CreateBackgroundImage(
            ref TemporaryBitmapFile bmpFile,
            String controlID,
            String title,
            String message,
            bool infoMode,
            int controlWidth
        ) {
            // Really, anything this small is not practically going to
            // show readable text.  Truncate instead of trying to display
            // the string vertically.
            if(controlWidth < 75)
            {
                controlWidth = 75;
            }

            Bitmap errorIcon = infoMode? GenericUI.InfoIcon : GenericUI.ErrorIcon;
            
            bool showMessage = message != null && message.Length != 0;
            bool showTitle = (title != null && title.Length != 0)
                || (controlID != null && controlID.Length != 0);

            Debug.Assert(showMessage || showTitle);

            // 


            using(
                Font normalFont = new Font(_fontFamily, 8, FontStyle.Regular),
                boldFont = new Font(normalFont.FontFamily, 8, FontStyle.Bold)
            ) {
            using(
                Brush controlTextBrush = new SolidBrush(SystemColors.ControlText),
                controlDarkBrush = new SolidBrush(SystemColors.ControlDark),
                controlBrush = new SolidBrush(SystemColors.Control),
                windowBrush = new SolidBrush(SystemColors.Window)
            ) {
            using(
                Pen controlDarkPen = new Pen(SystemColors.ControlDark),
                windowPen = new Pen(SystemColors.Window)
            ) {
                int barHeight = 0;
                if(showTitle)
                {
                    // We do not measure the height of the real title because
                    // we inted to truncate rather than wrap.
                    barHeight = GetHeight(
                        "'",
                        normalFont,
                        (controlWidth - 30)
                    ) + 6;
                }
                int messageHeight = 0;
                if(showMessage)
                {
                    int textHeight = GetHeight(
                        message,
                        normalFont,
                        (controlWidth - 30)
                    );
                    messageHeight = (textHeight < (errorIcon.Height + 6)) ?
                        (errorIcon.Height + 6) : textHeight + 3;
                }

                int width = 500; // normally only 300px visible.
                int height = barHeight + messageHeight;

                Bitmap bitmap = new Bitmap(width, height);
                using(Graphics g = Graphics.FromImage(bitmap))
                {
                    if (showTitle)
                    {
                        // The rectangle area
                        g.FillRectangle(controlBrush, 0, 0, width, barHeight);
                        // The gray line below the controlID
                        g.DrawLine(controlDarkPen, 0, barHeight - 1, width, barHeight - 1);
                        // Draw the text "controlTypeName - controlID"
                        g.DrawString(controlID, boldFont, controlTextBrush, 2, 2);
                        if(title != null && title.Length > 0)
                        {
                            int strPelLen = (int) g.MeasureString(controlID, boldFont).Width;
                            g.DrawString(" - " + title, normalFont, controlTextBrush, 4 + strPelLen, 2);
                        }
                    }

                    if (showMessage)
                    {
                        // The transparent line between controlID and errormessage.
                        g.DrawLine(windowPen, 0, barHeight, width, barHeight);
                        // The message rectangle area
                        g.FillRectangle(controlDarkBrush, 0, barHeight + 1, width, messageHeight - 1);
                        // Draw the message text
                        g.DrawString(message, normalFont, windowBrush,
                            new RectangleF(20, barHeight + 1, controlWidth - 30, messageHeight - 1));
                        // Draw the icon
                        g.DrawImage(errorIcon, 2, barHeight + 3);
                    }

                    if(bmpFile == null)
                    {
                        bmpFile = new TemporaryBitmapFile(bitmap);
                    }
                    else
                    {
                        bmpFile.UnderlyingBitmap = bitmap;
                    }
                } // using g
            }}} // using Fonts, Brushes, and Pens
        }
    }
}
