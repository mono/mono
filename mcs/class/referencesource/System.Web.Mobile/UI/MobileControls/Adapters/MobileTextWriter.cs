//------------------------------------------------------------------------------
// <copyright file="MobileTextWriter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Web.Mobile;
using System.Web.UI;
using System.Web.UI.MobileControls.Adapters;
using System.Security.Permissions;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource
#else
namespace System.Web.UI.MobileControls.Adapters
#endif

{

    /*
     * MobileTextWriter class. All device-specific mobile text writers
     * inherit from this class.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\MobileTextWriter.uex' path='docs/doc[@for="MobileTextWriter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class MobileTextWriter : MultiPartWriter
    {
        private MobileCapabilities _device;
        private MultiPartWriter _multiPartWriter;
        private bool _partStarted = false;

        /// <include file='doc\MobileTextWriter.uex' path='docs/doc[@for="MobileTextWriter.Device"]/*' />
        public MobileCapabilities Device
        {
            get
            {
                return _device;
            }
        }

        /// <include file='doc\MobileTextWriter.uex' path='docs/doc[@for="MobileTextWriter.MobileTextWriter"]/*' />
        public MobileTextWriter(TextWriter writer, MobileCapabilities device) : base(writer)
        {
            _multiPartWriter = writer as MultiPartWriter;
            _device = device;
        }

        /// <include file='doc\MobileTextWriter.uex' path='docs/doc[@for="MobileTextWriter.EnterLayout"]/*' />
        public virtual void EnterLayout(Style style)
        {
        }

        /// <include file='doc\MobileTextWriter.uex' path='docs/doc[@for="MobileTextWriter.ExitLayout"]/*' />
        public virtual void ExitLayout(Style style, bool breakAfter)
        {
        }

        /// <include file='doc\MobileTextWriter.uex' path='docs/doc[@for="MobileTextWriter.ExitLayout1"]/*' />
        public virtual void ExitLayout(Style style)
        {
            ExitLayout(style, false);
        }

        /// <include file='doc\MobileTextWriter.uex' path='docs/doc[@for="MobileTextWriter.EnterFormat"]/*' />
        public virtual void EnterFormat(Style style)
        {
        }

        /// <include file='doc\MobileTextWriter.uex' path='docs/doc[@for="MobileTextWriter.ExitFormat"]/*' />
        public virtual void ExitFormat(Style style)
        {
        }

        /// <include file='doc\MobileTextWriter.uex' path='docs/doc[@for="MobileTextWriter.ExitFormat1"]/*' />
        public virtual void ExitFormat(Style style, bool breakAfter)
        {
        }

        /// <include file='doc\MobileTextWriter.uex' path='docs/doc[@for="MobileTextWriter.EnterStyle"]/*' />
        public void EnterStyle(Style style)
        {
            EnterLayout(style);
            EnterFormat(style);
        }

        /// <include file='doc\MobileTextWriter.uex' path='docs/doc[@for="MobileTextWriter.ExitStyle"]/*' />
        public void ExitStyle(Style style)
        {
            ExitFormat(style);
            ExitLayout(style);
        }


        /////////////////////////////////////////////////////////////////////////
        //  MultiPartWriter implementation. The MobileTextWriter class itself
        //  does not support multipart writing, unless it is wrapped on top
        //  of another writer that does.
        /////////////////////////////////////////////////////////////////////////

        /// <include file='doc\MobileTextWriter.uex' path='docs/doc[@for="MobileTextWriter.SupportsMultiPart"]/*' />
        public override bool SupportsMultiPart
        {
            get
            {
                return _multiPartWriter != null && _multiPartWriter.SupportsMultiPart;
            }
        }

        /// <include file='doc\MobileTextWriter.uex' path='docs/doc[@for="MobileTextWriter.BeginResponse"]/*' />
        public override void BeginResponse()
        {
            if (_multiPartWriter != null)
            {
                _multiPartWriter.BeginResponse();
            }
        }

        /// <include file='doc\MobileTextWriter.uex' path='docs/doc[@for="MobileTextWriter.EndResponse"]/*' />
        public override void EndResponse()
        {
            if (_multiPartWriter != null)
            {
                _multiPartWriter.EndResponse();
            }
        }

        /// <include file='doc\MobileTextWriter.uex' path='docs/doc[@for="MobileTextWriter.BeginFile"]/*' />
        public override void BeginFile(String url, String contentType, String charset)
        {
            if (_multiPartWriter != null)
            {
                _multiPartWriter.BeginFile(url, contentType, charset);
            }
            else if (_partStarted)
            {
                throw new Exception(SR.GetString(SR.MobileTextWriterNotMultiPart));
            }
            else
            {
                if (contentType != null && contentType.Length > 0)
                {
                    HttpContext.Current.Response.ContentType = contentType;
                }
                _partStarted = true;
            }
        }

        /// <include file='doc\MobileTextWriter.uex' path='docs/doc[@for="MobileTextWriter.EndFile"]/*' />
        public override void EndFile()
        {
            if (_multiPartWriter != null)
            {
                _multiPartWriter.EndFile();
            }
        }

        /// <include file='doc\MobileTextWriter.uex' path='docs/doc[@for="MobileTextWriter.AddResource"]/*' />
        public override void AddResource(String url, String contentType)
        {
            if (_multiPartWriter != null)
            {
                _multiPartWriter.AddResource(url, contentType);
            }
        }
    }

}


