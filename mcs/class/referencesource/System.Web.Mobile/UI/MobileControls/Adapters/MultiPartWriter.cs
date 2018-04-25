//------------------------------------------------------------------------------
// <copyright file="MultiPartWriter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Web.Mobile;
using System.Web.UI;
using System.Security.Permissions;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource
#else
namespace System.Web.UI.MobileControls.Adapters
#endif    

{

    /*
     * MultiPartWriter class. Base class for writers that can 
     * handle multipart documents, like MHTML or Palm clippings.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\MultiPartWriter.uex' path='docs/doc[@for="MultiPartWriter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public abstract class MultiPartWriter : HtmlTextWriter
    {
        /// <include file='doc\MultiPartWriter.uex' path='docs/doc[@for="MultiPartWriter.MultiPartWriter"]/*' />
        protected MultiPartWriter(TextWriter writer) : base(writer)
        {
        }

        /// <include file='doc\MultiPartWriter.uex' path='docs/doc[@for="MultiPartWriter.SupportsMultiPart"]/*' />
        public virtual bool SupportsMultiPart
        {
            get
            {
                return true;
            }
        }

        /// <include file='doc\MultiPartWriter.uex' path='docs/doc[@for="MultiPartWriter.NewUrl"]/*' />
        public virtual String NewUrl(String filetype)
        {
            return Guid.NewGuid().ToString() + filetype;
        }

        /// <include file='doc\MultiPartWriter.uex' path='docs/doc[@for="MultiPartWriter.BeginResponse"]/*' />
        public abstract void BeginResponse();

        /// <include file='doc\MultiPartWriter.uex' path='docs/doc[@for="MultiPartWriter.EndResponse"]/*' />
        public abstract void EndResponse();

        /// <include file='doc\MultiPartWriter.uex' path='docs/doc[@for="MultiPartWriter.BeginFile"]/*' />
        public abstract void BeginFile(String url, String contentType, String charset);

        /// <include file='doc\MultiPartWriter.uex' path='docs/doc[@for="MultiPartWriter.EndFile"]/*' />
        public abstract void EndFile();

        /// <include file='doc\MultiPartWriter.uex' path='docs/doc[@for="MultiPartWriter.AddResource"]/*' />
        public abstract void AddResource(String url, String contentType);

        /// <include file='doc\MultiPartWriter.uex' path='docs/doc[@for="MultiPartWriter.AddResource1"]/*' />
        public void AddResource(String url)
        {
            AddResource (url, null);
        }
    }
}



