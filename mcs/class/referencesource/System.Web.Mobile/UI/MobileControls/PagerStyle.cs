//------------------------------------------------------------------------------
// <copyright file="PageStyle.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{
    /*
     * Pager Style class. Style properties used to render a form pagination UI.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\PagerStyle.uex' path='docs/doc[@for="PagerStyle"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class PagerStyle : Style
    {
        /// <include file='doc\PagerStyle.uex' path='docs/doc[@for="PagerStyle.NextPageTextKey"]/*' />
        public static readonly Object
            NextPageTextKey = RegisterStyle("NextPageText", typeof(String), String.Empty, false),
            PreviousPageTextKey = RegisterStyle("PreviousPageText", typeof(String), String.Empty, false),
            PageLabelKey = RegisterStyle("PageLabel", typeof(String), String.Empty, false);

        /// <include file='doc\PagerStyle.uex' path='docs/doc[@for="PagerStyle.NextPageText"]/*' />
        [
            Bindable(true),
            DefaultValue(""),
            MobileCategory(SR.Category_Appearance),
            MobileSysDescription(SR.PagerStyle_NextPageText),
            NotifyParentProperty(true),
        ]
        public String NextPageText
        {
            get
            {
                return (String)this[NextPageTextKey];
            }
            set
            {
                this[NextPageTextKey] = value;
            }
        }

        /// <include file='doc\PagerStyle.uex' path='docs/doc[@for="PagerStyle.PreviousPageText"]/*' />
        [
            Bindable(true),
            DefaultValue(""),
            MobileCategory(SR.Category_Appearance),
            MobileSysDescription(SR.PagerStyle_PreviousPageText),
            NotifyParentProperty(true),
        ]
        public String PreviousPageText
        {
            get
            {
                return (String)this[PreviousPageTextKey];
            }
            set
            {
                this[PreviousPageTextKey] = value;
            }
        }

        /// <include file='doc\PagerStyle.uex' path='docs/doc[@for="PagerStyle.PageLabel"]/*' />
        [
            Bindable(true),
            DefaultValue(""),
            MobileCategory(SR.Category_Appearance),
            MobileSysDescription(SR.PagerStyle_PageLabel),
            NotifyParentProperty(true),
        ]
        public String PageLabel
        {
            get
            {
                return (String)this[PageLabelKey];
            }
            set
            {
                this[PageLabelKey] = value;
            }
        }

        /// <include file='doc\PagerStyle.uex' path='docs/doc[@for="PagerStyle.GetNextPageText"]/*' />
        public String GetNextPageText(int currentPageIndex)
        {
            String s = (String)this[NextPageTextKey, true];
            if (!String.IsNullOrEmpty(s))
            {
                return String.Format(CultureInfo.CurrentCulture, s, currentPageIndex + 1);
            }
            else
            {
                return SR.GetString(SR.PagerStyle_NextPageText_DefaultValue);
            }
        }

        /// <include file='doc\PagerStyle.uex' path='docs/doc[@for="PagerStyle.GetPreviousPageText"]/*' />
        public String GetPreviousPageText(int currentPageIndex)
        {
            String s = (String)this[PreviousPageTextKey, true];
            if (!String.IsNullOrEmpty(s))
            {
                return String.Format(CultureInfo.CurrentCulture, s, currentPageIndex - 1);
            }
            else
            {
                return SR.GetString(SR.PagerStyle_PreviousPageText_DefaultValue);
            }
        }

        /// <include file='doc\PagerStyle.uex' path='docs/doc[@for="PagerStyle.GetPageLabelText"]/*' />
        public String GetPageLabelText(int currentPageIndex, int pageCount)
        {
            String s = (String)this[PageLabelKey, true];
            if (s == null)
            {
                s = String.Empty;
            }
            if (s.Length > 0)
            {
                s = String.Format(CultureInfo.CurrentCulture, s, currentPageIndex, pageCount);
            }
            return s;
        }
    }
}
