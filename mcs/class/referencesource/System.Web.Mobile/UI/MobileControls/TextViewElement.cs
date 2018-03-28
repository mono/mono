//------------------------------------------------------------------------------
// <copyright file="TextViewElement.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Web;
using System.Web.UI;
using System.Web.UI.Design.WebControls;
using System.Web.UI.HtmlControls;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{

    /*
     * Mobile TextView Element class.
     * The TextView control stores its contents as a series of elements. This
     * class encapsulates an element.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\TextViewElement.uex' path='docs/doc[@for="TextViewElement"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class TextViewElement
    {
        private String _text;
        private String _url;
        private bool _isBold;
        private bool _isItalic;
        private bool _breakAfter;

        /// <include file='doc\TextViewElement.uex' path='docs/doc[@for="TextViewElement.Text"]/*' />
        public String Text
        {
            get
            {
                return _text;
            }
        }

        /// <include file='doc\TextViewElement.uex' path='docs/doc[@for="TextViewElement.Url"]/*' />
        public String Url
        {
            get
            {
                return _url;
            }
        }

        /// <include file='doc\TextViewElement.uex' path='docs/doc[@for="TextViewElement.IsBold"]/*' />
        public bool IsBold
        {
            get
            {
                return _isBold;
            }
        }

        /// <include file='doc\TextViewElement.uex' path='docs/doc[@for="TextViewElement.IsItalic"]/*' />
        public bool IsItalic
        {
            get
            {
                return _isItalic;
            }
        }

        /// <include file='doc\TextViewElement.uex' path='docs/doc[@for="TextViewElement.BreakAfter"]/*' />
        public bool BreakAfter
        {
            get
            {
                return _breakAfter;
            }
        }

        internal TextViewElement(String text, String url, bool isBold, bool isItalic, bool breakAfter)
        {
            _text       = text;
            _url        = url;
            _isBold     = isBold;
            _isItalic   = isItalic;
            _breakAfter = breakAfter;
        }
    }
}

