//------------------------------------------------------------------------------
// <copyright file="WbmpConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls.Util
{
    using System;
    using System.Drawing;
    using System.Diagnostics;
    
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class WbmpConverter
    {
        private static int ExtractMultiByte(Byte[] buffer, ref int cursor)
        {
            int sum = 0;
            do
            {
                sum <<= 7;
                sum += buffer[cursor] & 0x7F;
            }
            while((buffer[cursor++] & 0x80) != 0);
            return sum;
        }
        
        internal static Bitmap Convert(Byte[] buffer)
        {
            try
            {
                int cursor = 0;
                int type = buffer[cursor++];

                if(type != 0)
                {
                    Debug.Fail("Wbmp is not type 0. (Unsupported)");
                    return null;
                }
                
                int header = buffer[cursor++];
                int width = ExtractMultiByte(buffer, ref cursor);
                int height = ExtractMultiByte(buffer, ref cursor);

                Bitmap bitmap = new Bitmap(width, height);
                Byte mask = 0x80;

                for(int y = 0; y < height; y++)
                {
                    for(int x = 0; x < width; x++)
                    {
                        if((buffer[cursor] & mask) == 0)
                        {
                            bitmap.SetPixel(x, y, Color.Black);
                        }
                        else
                        {
                            bitmap.SetPixel(x, y, Color.White);
                        }
                        mask >>= 1;
                        if(mask == 0)
                        {
                           mask = 0x80;
                           cursor++;
                        }
                    }
                    // each row starts at the beginning of an octet
                    if(mask != 0x80)
                    {
                        mask = 0x80;
                        cursor++;
                    }
                }
                return bitmap;
            }
            catch
            {
                Debug.Fail("Wbmp file appears to be corrupt.");
                return null;
            }
        }
    }
}
