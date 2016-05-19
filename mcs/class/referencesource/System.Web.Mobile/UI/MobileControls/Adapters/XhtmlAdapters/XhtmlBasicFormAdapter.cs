//------------------------------------------------------------------------------
// <copyright file="XhtmlBasicFormAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;
using System.Web.Mobile;
using System.Web.Security;
using System.Web.UI.MobileControls;
using System.Web.UI.MobileControls.Adapters;
using System.Collections;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource.XhtmlAdapters
#else
namespace System.Web.UI.MobileControls.Adapters.XhtmlAdapters
#endif
{
    /// <include file='doc\XhtmlBasicFormAdapter.uex' path='docs/doc[@for="XhtmlFormAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class XhtmlFormAdapter : XhtmlControlAdapter {
        /// <include file='doc\XhtmlBasicFormAdapter.uex' path='docs/doc[@for="XhtmlFormAdapter.Control"]/*' />
        public new Form Control {
            get {
                return base.Control as Form;
            }
        }

        /// <include file='doc\XhtmlBasicFormAdapter.uex' path='docs/doc[@for="XhtmlFormAdapter.Render"]/*' />
        public override void Render (XhtmlMobileTextWriter writer) {
            // Note: <head>, <body> rendered by page adapter, as in HTML case.
            String formsAuthCookieName = FormsAuthentication.FormsCookieName;
            if(!Device.SupportsRedirectWithCookie)
            {
                if(formsAuthCookieName != null && formsAuthCookieName.Length > 0)
                {
                    HttpContext.Current.Response.Cookies.Remove(formsAuthCookieName);
                }
            }
            writer.WriteBeginTag ("form");
            writer.WriteAttribute ("id", Control.ClientID);
            writer.WriteAttribute ("method", Control.Method.ToString().ToLower(CultureInfo.CurrentCulture));
            writer.Write (" action=\"");
            RenderPostbackUrl(writer);
            if(Control.Action.Length > 0) {
                if(Control.Action.IndexOf("?", StringComparison.Ordinal) != -1) {
                    writer.Write("&");
                }
                else {
                    writer.Write("?");
                }
            }
            else {
                writer.Write("?");
            }
            writer.Write(Page.UniqueFilePathSuffix);

            if (Control.Method != FormMethod.Get &&
                Control.Action.Length == 0) {   // VSWhidbey 411176: We don't include QueryStringText if Action is explicitly set

                String queryStringText = PreprocessQueryString(Page.QueryStringText);
                if (queryStringText != null && queryStringText.Length > 0) {
                    String amp = (String)Device[XhtmlConstants.SupportsUrlAttributeEncoding] != "false"  ? "&amp;" : "&";
                    writer.Write(amp);
                    if((String)Device[XhtmlConstants.SupportsUrlAttributeEncoding] != "false") {
                        writer.WriteEncodedText(queryStringText);
                    }
                    else {
                        writer.Write(queryStringText);
                    }
                }
            }
            writer.WriteLine ("\">");
            bool needDivStyle = (String)Device[XhtmlConstants.RequiresXhtmlCssSuppression] != "true" &&
                (String)Device["supportsBodyClassAttribute"] == "false";
            if (!needDivStyle) {
                if((String)Device["usePOverDiv"] == "true")
                    writer.WriteFullBeginTag("p");
                else
                    writer.WriteFullBeginTag ("div");
            }
            else {
                if((String)Device["usePOverDiv"] == "true")
                    writer.EnterStyle(Style, "p");
                else
                    writer.EnterStyle (Style, "div");
            }
            RenderPostBackHeader (writer);
            // Renders hidden variables for IPostBackDataHandlers which are
            // not displayed due to pagination or secondary UI.
            RenderOffPageVariables(writer, Control, Control.CurrentPage);
            
            RenderChildren (writer);
            if (!needDivStyle) {
                if((String)Device["usePOverDiv"] == "true")
                    writer.WriteEndTag("p");
                else
                    writer.WriteEndTag ("div");
            }
            else {
                if((String)Device["usePOverDiv"] == "true")
                    writer.ExitStyle(Style);
                else
                    writer.ExitStyle (Style);
            }
            writer.WriteEndTag ("form");
        }
    
        private void RenderPostBackHeader(XhtmlMobileTextWriter writer) {
            bool postBack = Page.ActiveForm.Action.Length == 0;

            RenderPageState(writer);
            if (!postBack) {
                writer.WriteHiddenField(PageAdapter.EventSourceKey, XhtmlConstants.PostedFromOtherFile);
            }
            else if (Page.ClientViewState == null) {
                // The empty event source variable is used to identify a
                // postback request.  Value attribute is not needed, and some
                // devices do not allow empty string value attributes.
                if ((String)Device["requiresHiddenFieldValues"] != "true") {
                    writer.WriteHiddenField(PageAdapter.EventSourceKey);
                }
                else {
                    // Placeholder value is never used, just needed for some devices.
                    writer.WriteHiddenField(PageAdapter.EventSourceKey, PageAdapter.EventSourceKey);
                }
            }

            RenderHiddenVariables(writer);
        }

        private void RenderHiddenVariables(XhtmlMobileTextWriter writer) {
            if (Page.HasHiddenVariables()) {
                String hiddenVariablePrefix = MobilePage.HiddenVariablePrefix;
                foreach (DictionaryEntry entry in Page.HiddenVariables) {
                    if (entry.Value != null) {
                        writer.WriteHiddenField(hiddenVariablePrefix + (String)entry.Key, 
                            (String)entry.Value);
                    }
                }
            }
        }

        private void RenderPageState (XhtmlMobileTextWriter writer) {
            String viewState = Page.ClientViewState;
            if (viewState != null) {
                writer.WriteHiddenField (MobilePage.ViewStateID, viewState);
            }
        }

        private void RenderChildren (XhtmlMobileTextWriter writer) {

            if (SecondaryUIControl != null) {
                RenderSecondaryUI (writer);
                return;
            }

            bool pagerRendered = false;
            if(Control.HasControls()) {
                foreach(Control child in Control.Controls) {
                    if(Control.Footer == child) {
                        RenderPager(writer);
                        pagerRendered = true;
                    }
                    child.RenderControl(writer);
                }
            }
            if(!pagerRendered) {
                RenderPager(writer);
            }
        }

        private void RenderSecondaryUI (XhtmlMobileTextWriter writer){
            bool secondaryUIInHeaderOrFooter = IsControlInFormHeader (SecondaryUIControl) 
                || IsControlInFormFooter (SecondaryUIControl);

            SetControlPageRecursive(SecondaryUIControl, -1);
            if (Control.Header != null && !secondaryUIInHeaderOrFooter) {
                Control.Header.RenderControl (writer);
            }
            OpenSecondaryUIDivs(writer, SecondaryUIControl);
            SecondaryUIControl.RenderControl (writer);
            CloseSecondaryUIDivs(writer);
            if (Control.Footer != null && !secondaryUIInHeaderOrFooter) {
                Control.Footer.RenderControl (writer);
            }
        }

        /////////////////////////////////////////////////////////////////////////
        //  SECONDARY UI SUPPORT
        /////////////////////////////////////////////////////////////////////////
        private MobileControl _secondaryUIControl;
        private int _secondaryUIMode;
        private int _secondaryUIDivsOpened = 0;
        
        private void OpenSecondaryUIDivs(XhtmlMobileTextWriter writer, Control control) {
            Control ctl = control.Parent as MobileControl;
            while (ctl != null) {
                String cssClass = ((IAttributeAccessor) ctl).GetAttribute(XhtmlConstants.CssClassCustomAttribute);
                if (cssClass != null && cssClass. Length > 0) {
                    if((String)Device["usePOverDiv"] == "true") {
                        writer.WriteBeginTag("p");
                    }
                    else {
                        writer.WriteBeginTag("div");
                    }
                    writer.WriteAttribute("class", cssClass, true);
                    writer.WriteLine(">");
                    _secondaryUIDivsOpened++;
                }
                ctl = ctl.Parent as MobileControl;
            }
        }

        private void CloseSecondaryUIDivs(XhtmlMobileTextWriter writer) {
            for (int i = 0; i < _secondaryUIDivsOpened; i++) {
                if((String)Device["usePOverDiv"] == "true") {
                    writer.WriteEndTag("p");
                }
                else {
                    writer.WriteEndTag("div");
                }
                writer.WriteLine();
            }
        }
        
        internal int GetSecondaryUIMode(MobileControl control) {
            return (control != null && _secondaryUIControl == control) ? _secondaryUIMode : NotSecondaryUI;
        }

        internal void SetSecondaryUIMode(MobileControl control, int mode) {
            if (mode != NotSecondaryUI) {

                if (_secondaryUIControl != null && _secondaryUIControl != control) {
                    throw new Exception(
                        SR.GetString(SR.FormAdapterMultiControlsAttemptSecondaryUI));
                }
                _secondaryUIControl = control;
                _secondaryUIMode = mode;
                return;
            }

            if (control == _secondaryUIControl) {

                _secondaryUIControl = null;
            }
        }

        internal MobileControl SecondaryUIControl {
            get {
                return _secondaryUIControl;
            }
        }

        // Used for Secondary UI.
        private bool IsControlInFormHeader(MobileControl control) {
            return IsAncestor(Control.Header, control);
        }

        // Used for Secondary UI.
        private bool IsControlInFormFooter(MobileControl control) {
            return IsAncestor(Control.Footer, control);
        }

        private bool IsAncestor(MobileControl ancestor, MobileControl descendant) {
            for (Control i = descendant; i != null; i = i.Parent) {
                if (i == ancestor) {
                    return true;
                }
            }
            return false;
        }

        /////////////////////////////////////////////////////////////////////////
        //  PAGINATION SUPPORT
        /////////////////////////////////////////////////////////////////////////
        /// <include file='doc\XhtmlBasicFormAdapter.uex' path='docs/doc[@for="XhtmlFormAdapter.RenderPager"]/*' />
        protected virtual void RenderPager (XhtmlMobileTextWriter writer) {
            PagerStyle pagerStyle = Control.PagerStyle;

            int pageCount = Control.PageCount;
            if (pageCount <= 1) {
                return;
            }
            int page = Control.CurrentPage;
            String text = pagerStyle.GetPageLabelText(page, pageCount);

            if((page > 1) || (text.Length > 0) || (page < pageCount)) {
                writer.WritePendingBreak();
                ConditionalEnterStyle(writer, pagerStyle);
                ConditionalEnterPagerSpan(writer);
            }

            if (page > 1) {
                RenderPagerTag(writer, page - 1,
                    pagerStyle.GetPreviousPageText(page),
                    XhtmlConstants.PagerPreviousAccessKeyCustomAttribute);
                writer.Write(" ");
            }

            if (text.Length > 0) {
                writer.WriteEncodedText(text);
                writer.Write(" ");
            }

            if (page < pageCount) {
                RenderPagerTag(writer, page + 1,
                    pagerStyle.GetNextPageText(page),
                    XhtmlConstants.PagerNextAccessKeyCustomAttribute);
            }

            if((page > 1) || (text.Length > 0) || (page < pageCount)) {
                ConditionalExitPagerSpan(writer);
                ConditionalExitStyle(writer, pagerStyle);
                writer.SetPendingBreak();
            }
        }

        private bool _pagerCssSpanWritten = false;
        private void ConditionalEnterPagerSpan(XhtmlMobileTextWriter writer) {
            String cssClass = GetCustomAttributeValue(XhtmlConstants.CssPagerClassCustomAttribute);
            if (CssLocation == StyleSheetLocation.PhysicalFile &&
                cssClass != null && 
                cssClass.Length > 0) {
                writer.WriteBeginTag ("span");
                writer.WriteAttribute("class", cssClass, true);
                _pagerCssSpanWritten = true;
                writer.Write(">");
            }
        }
        

        private void ConditionalExitPagerSpan(XhtmlMobileTextWriter writer) {
            if (_pagerCssSpanWritten) {
                writer.WriteEndTag("span");
            }
        }

        /// <include file='doc\XhtmlBasicFormAdapter.uex' path='docs/doc[@for="XhtmlFormAdapter.RenderPagerTag"]/*' />
        protected virtual void RenderPagerTag(
            XhtmlMobileTextWriter writer,
            int pageToNavigate,
            String text,
            String accessKeyCustomAttribute) {
            writer.WriteBeginTag("input");

            // Specially encode the page number with the control id.
            // The corresponding code that handles postback should know how
            // to extract the page number correctly.
            writer.Write(" name=\"");
            writer.Write(Control.UniqueID);
            writer.Write(Constants.PagePrefix);
            writer.Write(pageToNavigate.ToString(CultureInfo.InvariantCulture));
            writer.Write("\"");

            writer.WriteAttribute("type", "submit");
            writer.WriteAttribute("value", text, true);
            ConditionalRenderCustomAttribute(writer, accessKeyCustomAttribute, XhtmlConstants.AccessKeyCustomAttribute);
            writer.Write("/>");
        }

        private void RenderPostbackUrl(XhtmlMobileTextWriter writer) {
            if ((String)Device["requiresAbsolutePostbackUrl"] == "true") {
                RenderAbsolutePostbackUrl (writer);
                return;
            }
            if (Control.Action.Length > 0) {
                String url = Control.ResolveUrl(PreprocessQueryString(Control.Action));
                writer.Write(url);
            }
            else {
                writer.WriteEncodedUrl(Page.RelativeFilePath);
            }
        }

        private void RenderAbsolutePostbackUrl(XhtmlMobileTextWriter writer) {
            String url = PreprocessQueryString(Control.Action);
            if (url.Length > 0) {
                // Not only do we need to resolve the URL, but we need to make it absolute.
                url = Page.MakePathAbsolute(Control.ResolveUrl(url));
                writer.Write(url);
            }
            else {
                writer.WriteEncodedUrl(Page.AbsoluteFilePath);
            }
        }

        private static void SetControlPageRecursive(Control control, int page)
        {
            MobileControl mc = control as MobileControl;
            if(mc != null)
            {
                mc.FirstPage = page;
                mc.LastPage = page;
            }
            if (control.HasControls())
            {
                foreach (Control child in control.Controls)
                {
                    MobileControl mobileChild = child as MobileControl;
                    if (mobileChild != null)
                    {
                            mobileChild.FirstPage = page;
                            mobileChild.LastPage = page;
                    }
                    else 
                    {
                        SetControlPageRecursive(child, page);
                    }
                }
            }
        }
    }
}
