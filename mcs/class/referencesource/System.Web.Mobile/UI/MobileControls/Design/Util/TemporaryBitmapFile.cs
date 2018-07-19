//------------------------------------------------------------------------------
// <copyright file="TemporaryBitmapFile.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls.Util
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;

    /// <summary>
    ///    This class encapsulates a bitmap and a file that represents
    ///    the bitmap on disk.  It would have been cleaner to subclass
    ///    bitmap, but the bitmap class is sealed.
    /// </summary>
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class TemporaryBitmapFile : IDisposable
    {
        private String _path;
        private Bitmap _bitmap;

        internal TemporaryBitmapFile(Bitmap bitmap)
        {
            Debug.Assert(bitmap != null,
                "You must provide a valid bitmap object."
            );
            _bitmap = bitmap;
            _path = Path.GetTempPath() + Guid.NewGuid().ToString() + ".bmp";
            Sync();
        }

        public void Dispose()
        {
            if(_bitmap != null)
            {
                _bitmap.Dispose();
                _bitmap = null;
            }
            if(_path != null)
            {
                FileAttributes fa = File.GetAttributes(_path);
                File.SetAttributes(_path, fa & ~FileAttributes.ReadOnly);
                File.Delete(_path);
                _path = null;
            }
        }

        private void Sync()
        {
            FileAttributes fa;

            if(File.Exists(_path))
            {
                fa = File.GetAttributes(_path);
                File.SetAttributes(_path, fa & ~FileAttributes.ReadOnly);
            }

            _bitmap.Save(_path, ImageFormat.Bmp);

            // If the file did not exist previously, fa will not be valid.
            fa = File.GetAttributes(_path);
            File.SetAttributes(_path,  fa | FileAttributes.ReadOnly);
        }

        internal String Url
        {
            get
            {
                return "file:///" + _path;
            }
        }

        internal Bitmap UnderlyingBitmap
        {
            get
            {
                return _bitmap;
            }
            set
            {
                Debug.Assert(value != null,
                    "Do not set UnderlyingBitmap to null.  Instead, "+
                    "dispose of this object and create a new one later if " +
                    "neccessary.  (A zero sized bmp can not be written to disk)"
                );
                if(_bitmap != null)
                {
                    _bitmap.Dispose();
                }
                _bitmap = value;
                Sync();
            }
        }
    }
}
