//------------------------------------------------------------------------------
// <copyright file="WmlPostFieldType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Drawing;
using System.IO;
using System.Web;
using System.Web.Mobile;
using System.Web.UI;
using System.Web.UI.MobileControls;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource
#else
namespace System.Web.UI.MobileControls.Adapters
#endif    

{
    /*
     * WmlPostFieldType enumeration.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\WmlPostFieldType.uex' path='docs/doc[@for="WmlPostFieldType"]/*' />
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public enum WmlPostFieldType
    {
        /// <include file='doc\WmlPostFieldType.uex' path='docs/doc[@for="WmlPostFieldType.Normal"]/*' />
        Normal,
        /// <include file='doc\WmlPostFieldType.uex' path='docs/doc[@for="WmlPostFieldType.Submit"]/*' />
        Submit,
        /// <include file='doc\WmlPostFieldType.uex' path='docs/doc[@for="WmlPostFieldType.Variable"]/*' />
        Variable,
        /// <include file='doc\WmlPostFieldType.uex' path='docs/doc[@for="WmlPostFieldType.Raw"]/*' />
        Raw
    }
}


