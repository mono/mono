//------------------------------------------------------------------------------
// <copyright file="Constants.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Web;
using System.Collections;
using System.Collections.Specialized;
using System.Web.UI;
using System.IO;
using System.Web.Mobile;
using System.Web.UI.MobileControls;
using System.Web.UI.MobileControls.Adapters;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource.XhtmlAdapters
#else
namespace System.Web.UI.MobileControls.Adapters.XhtmlAdapters
#endif
{
    /// <include file='doc\Constants.uex' path='docs/doc[@for="Doctype"]/*' />
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public enum Doctype {
        /// <include file='doc\Constants.uex' path='docs/doc[@for="Doctype.NotSet"]/*' />
        NotSet,
        /// <include file='doc\Constants.uex' path='docs/doc[@for="Doctype.XhtmlBasic"]/*' />
        XhtmlBasic,
        /// <include file='doc\Constants.uex' path='docs/doc[@for="Doctype.XhtmlMobileProfile"]/*' />
        XhtmlMobileProfile,
        /// <include file='doc\Constants.uex' path='docs/doc[@for="Doctype.Wml20"]/*' />
        Wml20
    }
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal static class XhtmlConstants {
        internal static readonly string CssQueryStringName = "_css";
        internal static readonly string CacheStyleSheetValue = "application";
        internal static readonly string CssSessionKey = CssQueryStringName;
        internal static readonly string CssStateLocationAppSettingKey = "XhtmlCssState";
        internal static readonly string CssMappedFileName = "xhtmlCss.axd";
        internal static readonly StyleFilter Format = StyleFilter.None | StyleFilter.Italic | StyleFilter.Bold | StyleFilter.FontName | StyleFilter.FontSize | StyleFilter.BackgroundColor | StyleFilter.ForegroundColor;
        internal static readonly StyleFilter Layout= StyleFilter.Wrapping | StyleFilter.Alignment;
        internal static readonly StyleFilter All= XhtmlConstants.Format | XhtmlConstants.Layout;
        internal static readonly string SessionKeyPrefix = "_s";
        internal static readonly string PostedFromOtherFile = ".";
        internal static Style DefaultStyle = new Style ();
        internal static XhtmlStyleClass DefaultStyleClass = new XhtmlStyleClass(DefaultStyle, XhtmlConstants.All);

        // Capabilities
        internal static readonly string RequiresXhtmlCssSuppression = "requiresXhtmlCssSuppression";
        internal static readonly string RequiresNewLineSuppression = "requiresNewLineSuppression";
        internal static readonly string BreaksOnBlockElements = "breaksOnBlockElements";
        internal static readonly string BreaksOnInlineElements = "breaksOnInlineElements";
        internal static readonly string RequiresOnEnterForward = "requiresOnEnterForwardForCheckboxLists";
        internal static readonly string SupportsBodyClassAttribute = "supportsBodyClassAttribute";
        internal static readonly string SupportsUrlAttributeEncoding = "supportsUrlAttributeEncoding";
        internal static readonly string InternalStyleConfigSetting = "supportsStyleElement";
        internal static readonly string SupportsNoWrapStyle = "supportsNoWrapStyle";

        // Custom attributes
        internal static readonly string CssClassCustomAttribute = "cssclass";
        internal static readonly string StyleSheetLocationCustomAttribute = "CssLocation";
        internal static readonly string CssPagerClassCustomAttribute = "cssPagerClass";
        internal static readonly string CssLabelClassCustomAttribute = "cssLabelClass";
        internal static readonly string CssCommandClassCustomAttribute = "cssCommandClass";
        internal static readonly string AccessKeyCustomAttribute = "accesskey";
        internal static readonly string PagerNextAccessKeyCustomAttribute = "PagerNextAccessKey";
        internal static readonly string PagerPreviousAccessKeyCustomAttribute = "PagerPrevAccessKey";
        internal static readonly string TitleCustomAttribute = "title";
    }

    /// <include file='doc\Constants.uex' path='docs/doc[@for="StyleSheetLocation"]/*' />
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public enum StyleSheetLocation {
        /// <include file='doc\Constants.uex' path='docs/doc[@for="StyleSheetLocation.NotSet"]/*' />
        NotSet,
        /// <include file='doc\Constants.uex' path='docs/doc[@for="StyleSheetLocation.ApplicationCache"]/*' />
        ApplicationCache,
        /// <include file='doc\Constants.uex' path='docs/doc[@for="StyleSheetLocation.SessionState"]/*' />
        SessionState,
        /// <include file='doc\Constants.uex' path='docs/doc[@for="StyleSheetLocation.PhysicalFile"]/*' />
        PhysicalFile,
        /// <include file='doc\Constants.uex' path='docs/doc[@for="StyleSheetLocation.Internal"]/*' />
        Internal,
        /// <include file='doc\Constants.uex' path='docs/doc[@for="StyleSheetLocation.None // currently not used"]/*' />
        None // currently not used
    }

    [Flags]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal enum StyleFilter : int /* 32 bits */ {
        None = 0x00000000,
        Italic = 0x00000001,
        Bold = 0x00000002,
        FontName = 0x00000004,
        FontSize = 0x00000008,
        BackgroundColor = 0x00000010,
        ForegroundColor = 0x00000020,
        
        Wrapping = 0x00000040,
        Alignment = 0x00000080
    }
}
