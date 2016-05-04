//------------------------------------------------------------------------------
// <copyright file="FileReader.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls.Util
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.IO;

    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class FileReader
    {
        // Helper class should not be instantiated.
        private FileReader() {
        }

        /// <summary>
        ///    This method reads a file specified by a uri and returns it
        ///    as a byte array.  If the file is located on the local file
        ///    system, a FileStream is used instead of a WebRequest.
        /// </summary>
        internal static Byte[] Read(Uri uri)
        {
            int length;
            Stream stream;

            Byte[] buffer = null;
            try
            {
                WebRequest request = WebRequest.Create(uri);
                WebResponse response = request.GetResponse();
                length = (int) response.ContentLength;
                stream = response.GetResponseStream();
                buffer = new Byte[length];
                stream.Read(buffer, 0, length);
                stream.Close();
            }
            catch(Exception e)
            {
                Debug.Fail("FileReader - Unable to read url '"
                    + uri.ToString() + ":\r\n" + e.ToString());
                return null;
            }
            return buffer;
        }
    }
}
