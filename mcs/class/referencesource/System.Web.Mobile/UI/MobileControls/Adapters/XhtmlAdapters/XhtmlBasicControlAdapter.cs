//------------------------------------------------------------------------------
// <copyright file="XhtmlBasicControlAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Collections;
using System.Security.Permissions;
using System.Text;
using System.Web;
using System.Web.Mobile;
using System.Web.UI;
using System.Web.UI.MobileControls;
using System.Web.UI.MobileControls.Adapters;
using System.Configuration; 
using System.Globalization;
using System.Web.Security;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource.XhtmlAdapters
#else
namespace System.Web.UI.MobileControls.Adapters.XhtmlAdapters
#endif
{

    /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class XhtmlControlAdapter : ControlAdapter {
        private bool _physicalCssClassPushed = false;

        private bool IsRooted(String basepath) {
            return(basepath == null || basepath.Length == 0 || basepath[0] == '/' || basepath[0] == '\\');
        }

        private bool IsRelativeUrl(string url) {
            // If it has a protocol, it's not relative
            if (url.IndexOf(":", StringComparison.Ordinal) != -1)
                return false;

            return !IsRooted(url);
        }


        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.PageAdapter"]/*' />
        protected XhtmlPageAdapter PageAdapter {
            get {
                return Page.Adapter as XhtmlPageAdapter;
            }
        }

        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.Render"]/*' />
        public override void Render(HtmlTextWriter writer) {
            Render((XhtmlMobileTextWriter)writer);
        }

        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.Render1"]/*' />
        public virtual void Render(XhtmlMobileTextWriter writer) {
            RenderChildren(writer);
        }

        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.RenderPostBackEventAsAnchor"]/*' />
        protected virtual void RenderPostBackEventAsAnchor (
            XhtmlMobileTextWriter writer,
            String argument,
            String linkText) {
            RenderPostBackEventAsAnchor(writer, argument, linkText, null /* accessKey */, null /* style */, null /*cssClass */);
        }

        // For convenience in extensibility -not used internally.  The overload with style and cssClass args is
        // to be preferred.  See ASURT 144034.
        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.RenderPostBackEventAsAnchor1"]/*' />
        protected virtual void RenderPostBackEventAsAnchor (
            XhtmlMobileTextWriter writer,
            String argument,
            String linkText, 
            String accessKey) {
            RenderPostBackEventAsAnchor(writer, argument, linkText, accessKey, null /* style */, null /* cssClass */);
        }

        // For Style/CssClass args, see ASURT 144034
        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.RenderPostBackEventAsAnchor2"]/*' />
        protected virtual void RenderPostBackEventAsAnchor (
            XhtmlMobileTextWriter writer,
            String argument,
            String linkText, 
            String accessKey,
            Style style,
            String cssClass) {            
            writer.WriteBeginTag("a");
            writer.Write(" href=\"");
            PageAdapter.RenderUrlPostBackEvent(writer, Control.UniqueID /* target */, argument);
            writer.Write("\" ");
            if (accessKey != null && accessKey.Length > 0) {
                writer.WriteAttribute("accesskey", accessKey);
            }
            if ((String)Device[XhtmlConstants.RequiresXhtmlCssSuppression] != "true") {
                if (CssLocation != StyleSheetLocation.PhysicalFile) {
                    String className = writer.GetCssFormatClassName(style);
                    if (className != null) {
                        writer.WriteAttribute ("class", className);
                    }
                }
                else if (cssClass != null && cssClass.Length > 0) {
                    writer.WriteAttribute ("class", cssClass, true /* encode */);
                }
            }
            writer.Write(">");
            writer.WriteEncodedText(linkText);
            writer.WriteEndTag("a");
        }

        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.ConditionalSetPendingBreakAfterInline"]/*' />
        protected virtual void ConditionalSetPendingBreakAfterInline (XhtmlMobileTextWriter writer) {
            if ((String)Device[XhtmlConstants.BreaksOnInlineElements] == "true") {
                return;
            }
            ConditionalSetPendingBreak(writer);
        }

        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.ConditionalSetPendingBreak"]/*' />
        protected virtual void ConditionalSetPendingBreak (XhtmlMobileTextWriter writer) {
            MobileControl mobileControl = Control as MobileControl;
            if (mobileControl != null && mobileControl.BreakAfter) {
                writer.SetPendingBreak ();
            }
        }

        // Overloads for complex controls like list that compose list items.  For these controls, the accessKey attribute
        // for each link may be different from the accessKey attribute for the control.
        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.RenderBeginLink"]/*' />
        protected virtual void RenderBeginLink(XhtmlMobileTextWriter writer, String target, String accessKey, Style style, String cssClass) {
            RenderBeginLink(writer, target, accessKey, style, cssClass, null /* title */);
        }
        
        
        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.RenderBeginLink"]/*' />
        protected virtual void RenderBeginLink(XhtmlMobileTextWriter writer, String target, String accessKey, Style style, String cssClass, String title) {
            writer.WriteBeginTag("a");
            writer.Write(" href=\"");
            RenderHrefValue (writer, target);
            writer.Write("\"");
            if (accessKey != null && accessKey.Length > 0) {
                writer.WriteAttribute("accesskey", accessKey, true);
            }
            if (CssLocation != StyleSheetLocation.PhysicalFile) {
                String className = writer.GetCssFormatClassName(style);
                if (className != null) {
                    writer.WriteAttribute ("class", className);
                }
            }
            else if (cssClass != null && cssClass.Length > 0) {
                writer.WriteAttribute ("class", cssClass, true /* encode */);
            }
            if (title != null && title.Length > 0) {
                writer.WriteAttribute("title", title, true /* encode */);
            }
            writer.WriteLine(">");            
        }

        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.RenderBeginLink1"]/*' />
        protected virtual void RenderBeginLink(XhtmlMobileTextWriter writer, String target) {
            String attributeValue = ((IAttributeAccessor)Control).GetAttribute(XhtmlConstants.AccessKeyCustomAttribute);
            RenderBeginLink(writer, target, attributeValue, null, null);
        }

        // Writes the href value for RenderBeginLink, depending on whether the target is a new form on the 
        // current page or a standard url (e.g., a new page).
        private void RenderHrefValue (XhtmlMobileTextWriter writer, String target) {
            bool appendCookielessDataDictionary = PageAdapter.PersistCookielessData  && 
                !target.StartsWith("http:", StringComparison.Ordinal) && 
                !target.StartsWith("https:", StringComparison.Ordinal);
            bool queryStringWritten = false;

            // ASURT 144021
            if (target == null || target.Length == 0) {
                target = Page.Response.ApplyAppPathModifier(Control.TemplateSourceDirectory);
            }



            if (target.StartsWith(Constants.FormIDPrefix, StringComparison.Ordinal)) {
                RenderFormNavigationHrefValue (writer, target);
                appendCookielessDataDictionary = false;
            }
            else {
                // For page adapter Control = null.
                if (Control != null) {
                    target = Control.ResolveUrl(target);
                }
                
                // ASURT 147179
                if ((String)Device["requiresAbsolutePostbackUrl"] == "true"
                    && IsRelativeUrl(target)) {
                    String templateSourceDirectory = Page.TemplateSourceDirectory;
                    String prefix = writer.EncodeUrlInternal(Page.Response.ApplyAppPathModifier(Page.TemplateSourceDirectory));
                    if (prefix[prefix.Length - 1] != '/') {
                        prefix = prefix + '/';
                    }
                    target = prefix + target;
                }

                if ((String)Device[XhtmlConstants.SupportsUrlAttributeEncoding] != "false") {
                    writer.WriteEncodedText (target);
                }
                else {
                    writer.Write (target);                
                }
                queryStringWritten = target.IndexOf ('?') != -1;
            }

            if (appendCookielessDataDictionary) {
                RenderCookielessDataDictionaryInQueryString (writer, queryStringWritten);
            }
        }

        // Writes an href postback event with semantics of form navigation (activation).
        private void RenderFormNavigationHrefValue (XhtmlMobileTextWriter writer, String target) {
            String prefix = Constants.FormIDPrefix;
            Debug.Assert (target.StartsWith (prefix, StringComparison.Ordinal));
            String name = target.Substring(prefix.Length);
            Form form = Control.ResolveFormReference(name);
            // EventTarget = Control, EventArg = Form has semantics navigate to (activate) the form.
            PageAdapter.RenderUrlPostBackEvent (writer, Control.UniqueID /* target */, form.UniqueID /* argument */);
        }

        private void RenderCookielessDataDictionaryInQueryString (XhtmlMobileTextWriter writer, bool queryStringWritten) {
            IDictionary dictionary = PageAdapter.CookielessDataDictionary;
            if (dictionary != null) {
                foreach (String name in dictionary.Keys) {
                    if (queryStringWritten) {
                        String amp = (String)Device[XhtmlConstants.SupportsUrlAttributeEncoding] != "false" ? "&amp;" : "&";
                        writer.Write(amp);
                    }
                    else {
                        writer.Write ('?');
                        queryStringWritten = true;
                    }
                    writer.Write (name);
                    writer.Write ('=');
                    writer.Write (dictionary[name]);
                }
            }
        }

        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.RenderEndLink"]/*' />
        protected virtual void RenderEndLink(XhtmlMobileTextWriter writer) {
            writer.WriteEndTag("a");
        }

        /////////////////////////////////////////////////////////////////////////
        //  SECONDARY UI SUPPORT
        /////////////////////////////////////////////////////////////////////////

        internal const int NotSecondaryUIInit = -1;  // For initialization of private consts in derived classes.
        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.NotSecondaryUI"]/*' />
        protected static readonly int NotSecondaryUI = NotSecondaryUIInit;

        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.SecondaryUIMode"]/*' />
        protected virtual int SecondaryUIMode {
            get {
                if (Control == null || Control.Form == null) {
                    return NotSecondaryUI;
                }
                else {
                    return((XhtmlFormAdapter)Control.Form.Adapter).GetSecondaryUIMode(Control);
                }
            }
            set {
                ((XhtmlFormAdapter)Control.Form.Adapter).SetSecondaryUIMode(Control, value);
            }
        }

        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.ExitSecondaryUIMode"]/*' />
        protected virtual void ExitSecondaryUIMode() {
            SecondaryUIMode = NotSecondaryUI;
        }

        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.LoadAdapterState"]/*' />
        public override void LoadAdapterState(Object state) {
            if (state != null) {
                SecondaryUIMode = (int)state;
            }
        }

        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.SaveAdapterState"]/*' />
        public override Object SaveAdapterState() {
            int mode = SecondaryUIMode;
            if (mode != NotSecondaryUI) {
                return mode;
            }
            else {
                return null;
            }
        }

        /////////////////////////////////////////////////////////////////////////
        //  ENTER STYLE SUPPORT:  These methods should, in general, be used in place
        //  of writer.EnterStyle, ExitStyle, etc.  They check whether there is
        //  a cssLocation attribute on the active form, and enter style only if
        //  not.
        /////////////////////////////////////////////////////////////////////////

        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.ConditionalEnterStyle"]/*' />
        protected virtual void ConditionalEnterStyle(XhtmlMobileTextWriter writer, Style style) {
            ConditionalEnterStyle(writer, style, String.Empty);
        }

        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.ConditionalEnterStyle1"]/*' />
        protected virtual void ConditionalEnterStyle(XhtmlMobileTextWriter writer, Style style, String tag) {
            if ((String)Device[XhtmlConstants.RequiresXhtmlCssSuppression] == "true") {
                return;
            }
            if (CssLocation == StyleSheetLocation.PhysicalFile) {
                // Do nothing.  Styles should be handled by CssClass custom attribute.
                return;
            }
            if (tag == null || tag.Length == 0) {
                writer.EnterStyle(style);
            }
            else {
                writer.EnterStyle(style, tag);
            }

        }


        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.ConditionalExitStyle"]/*' />
        protected virtual void ConditionalExitStyle(XhtmlMobileTextWriter writer, Style style)  {
            if ((String)Device[XhtmlConstants.RequiresXhtmlCssSuppression] == "true") {
                return;
            }
            if (CssLocation == StyleSheetLocation.PhysicalFile) {
                // Do nothing.  Styles should be handled by CssClass custom attribute.
                return;
            }
            writer.ExitStyle(style);
        }

        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.ConditionalEnterFormat"]/*' />
        protected virtual void ConditionalEnterFormat(XhtmlMobileTextWriter writer, Style style) {
            if ((String)Device[XhtmlConstants.RequiresXhtmlCssSuppression] == "true") {
                return;
            }
            if (CssLocation == StyleSheetLocation.PhysicalFile) {
                // Do nothing.  Styles should be handled by CssClass custom attribute.
                return;
            }
            writer.EnterFormat(style);
        }

        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.ConditionalExitFormat"]/*' />
        protected virtual void ConditionalExitFormat(XhtmlMobileTextWriter writer, Style style) {
            if ((String)Device[XhtmlConstants.RequiresXhtmlCssSuppression] == "true") {
                return;
            }
            if (CssLocation == StyleSheetLocation.PhysicalFile) {
                // Do nothing.  Styles should be handled by CssClass custom attribute.
                return;
            }
            writer.ExitFormat(style);        
        }

        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.ConditionalEnterLayout"]/*' />
        protected virtual void ConditionalEnterLayout(XhtmlMobileTextWriter writer, Style style) {
            if ((String)Device[XhtmlConstants.RequiresXhtmlCssSuppression] == "true") {
                return;
            }
            if (CssLocation == StyleSheetLocation.PhysicalFile) {
                // Do nothing.  Styles should be handled by CssClass custom attribute.
                return;
            }
            writer.EnterLayout(style);
        }

        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.ConditionalExitLayout"]/*' />
        protected virtual void ConditionalExitLayout(XhtmlMobileTextWriter writer, Style style) {
            if ((String)Device[XhtmlConstants.RequiresXhtmlCssSuppression] == "true") {
                return;
            }
            if (CssLocation == StyleSheetLocation.PhysicalFile) {
                // Do nothing.  Styles should be handled by CssClass custom attribute.
                return;
            }
            writer.ExitLayout(style);        
        }

        /////////////////////////////////////////////////////////////////////////
        // STYLESHEET LOCATION SUPPORT
        // Use for determining whether stylesheet is physical or virtual, and
        // where it is located (application cache, session state, or a physical 
        // directory). 
        /////////////////////////////////////////////////////////////////////////

        // For intelligibility at point of call.
        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.StyleSheetLocationAttributeValue"]/*' />
        protected virtual String StyleSheetLocationAttributeValue {
            get{
                return(String) Page.ActiveForm.CustomAttributes[XhtmlConstants.StyleSheetLocationCustomAttribute];
            }
        }

        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.StyleSheetStorageApplicationSetting"]/*' />
        protected virtual String StyleSheetStorageApplicationSetting {
            get {
                return(String) ConfigurationManager.AppSettings[XhtmlConstants.CssStateLocationAppSettingKey];
            }
        }

        // Add new supported markups here.
        private Doctype _documentType = Doctype.NotSet;
        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.DocumentType"]/*' />
        protected virtual Doctype DocumentType {
            get{
                if (_documentType != Doctype.NotSet) {
                    return _documentType;
                }
                if ((String)Device[XhtmlConstants.RequiresOnEnterForward] == "true") {
                    return Doctype.Wml20;
                }
                // Use capability rather than preferred rendering type header for accuracy.
                String browserCap = Device[XhtmlConstants.InternalStyleConfigSetting];
                // Send internal styles by default.
                if (browserCap == null || !String.Equals(browserCap, "false", StringComparison.OrdinalIgnoreCase))
                    return _documentType = Doctype.XhtmlMobileProfile;
                else
                    return _documentType = Doctype.XhtmlBasic;
            }
        }

        private StyleSheetLocation _cssLocation = StyleSheetLocation.NotSet;
        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.CssLocation"]/*' />
        protected virtual StyleSheetLocation CssLocation {
            get {
                if (_cssLocation != StyleSheetLocation.NotSet) {
                    return _cssLocation;
                }
                if (StyleSheetLocationAttributeValue != null && StyleSheetLocationAttributeValue.Length > 0) {
                    return _cssLocation = StyleSheetLocation.PhysicalFile;
                }
                if (DocumentType == Doctype.XhtmlMobileProfile  || DocumentType == Doctype.Wml20) {
                    return _cssLocation = StyleSheetLocation.Internal;
                }
                // if (String.Compare(StyleSheetStorageApplicationSetting, XhtmlConstants.CacheStyleSheetValue, ((true /* ignore case */) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)) == 0) {
                if (String.Compare(StyleSheetStorageApplicationSetting, XhtmlConstants.CacheStyleSheetValue, StringComparison.OrdinalIgnoreCase) == 0) {
                    return _cssLocation = StyleSheetLocation.ApplicationCache;
                }
                return _cssLocation = StyleSheetLocation.SessionState;      
            }
        }

        /////////////////////////////////////////////////////////////////////////
        // CSSCLASS CUSTOM ATTRIBUTE / PHYSICAL STYLESHEET SUPPORT
        // The cssClass custom attribute should only be honored if we are using a physical
        // stylesheet.
        /////////////////////////////////////////////////////////////////////////

        // The use of _physicalCssClassPushed precludes nesting calls to ConditionalRenderClassAttribute, ConditionalRenderOpeningSpanElement,
        // ConditionalRenderClosingSpanElement, etc.  The ConditionalRenderOpening call and ConditionalRenderClosing call must be paired without 
        // nesting.  ConditionalRenderClassAttribute is to be paired with ConditionalPopPhysicalCssClass (without nesting).

        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.ConditionalRenderClassAttribute"]/*' />
        protected virtual void ConditionalRenderClassAttribute(XhtmlMobileTextWriter writer) {
            if ((String)Device[XhtmlConstants.RequiresXhtmlCssSuppression] == "true") {
                return;
            }
            String classAttribute = (String) Control.CustomAttributes[XhtmlConstants.CssClassCustomAttribute];
            if (CssLocation == StyleSheetLocation.PhysicalFile && 
                classAttribute != null && 
                classAttribute.Length > 0 &&
                writer.DiffersFromCurrentPhysicalCssClass(classAttribute)) {
                writer.WriteAttribute("class", classAttribute);
                writer.PushPhysicalCssClass(classAttribute);
                Debug.Assert(!_physicalCssClassPushed, "These calls should not be nested.");
                _physicalCssClassPushed = true;
            }
        }

        private bool _physicalSpanClassOpen = false;
        // Render opening <span class=...> in case the stylesheet location has been specified as a physical file.
        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.ConditionalRenderOpeningSpanElement"]/*' />
        protected virtual void ConditionalRenderOpeningSpanElement(XhtmlMobileTextWriter writer) {
            if ((String)Device[XhtmlConstants.RequiresXhtmlCssSuppression] == "true") {
                return;
            }
            String classAttribute = (String) Control.CustomAttributes[XhtmlConstants.CssClassCustomAttribute];
            if (CssLocation == StyleSheetLocation.PhysicalFile && 
                classAttribute != null && 
                classAttribute.Length > 0 &&
                writer.DiffersFromCurrentPhysicalCssClass(classAttribute)) {
                writer.WriteLine();
                writer.WriteBeginTag("span");
                writer.WriteAttribute("class", classAttribute, true /*encode*/);
                writer.Write(">");
                writer.PushPhysicalCssClass(classAttribute);
                _physicalSpanClassOpen = true;
                Debug.Assert(!_physicalCssClassPushed, "These calls should not be nested.");
                _physicalCssClassPushed = true;
            }
        }

        // Render closing </span> in case the stylesheet location has been specified as a physical file.
        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.ConditionalRenderClosingSpanElement"]/*' />
        protected virtual void ConditionalRenderClosingSpanElement(XhtmlMobileTextWriter writer) {            
            String classAttribute = (String) Control.CustomAttributes[XhtmlConstants.CssClassCustomAttribute];
            if (_physicalSpanClassOpen) {
                Debug.Assert(_physicalCssClassPushed, "_physicalSpanClassOpen implies _physicalCssClassPushed");
                writer.WriteEndTag("span");
                writer.WriteLine();
                ConditionalPopPhysicalCssClass(writer); // resets _physicalCssClassPushed
                _physicalSpanClassOpen = false;
            }
        }

        // Render opening <div class=...> in case the stylesheet location has been specified as a physical file.
        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.ConditionalRenderOpeningDivElement"]/*' />
        protected virtual void ConditionalRenderOpeningDivElement(XhtmlMobileTextWriter writer) {
            if ((String)Device[XhtmlConstants.RequiresXhtmlCssSuppression] == "true") {
                return;
            }
            String classAttribute = (String) Control.CustomAttributes[XhtmlConstants.CssClassCustomAttribute];
            if (CssLocation == StyleSheetLocation.PhysicalFile) {
                writer.WriteLine();
                if ((String)Device["usePOverDiv"] == "true") {
                    writer.WriteBeginTag("p");
                }
                else {
                    writer.WriteBeginTag("div");
                }
                if (classAttribute != null && 
                    classAttribute.Length > 0 &&
                    writer.DiffersFromCurrentPhysicalCssClass(classAttribute)) {
                    writer.WriteAttribute("class", classAttribute, true);
                    writer.PushPhysicalCssClass(classAttribute);
                    Debug.Assert(!_physicalCssClassPushed, "These calls should not be nested.");
                    _physicalCssClassPushed = true;
                }
                writer.Write(">");
            }
        }

        // Render closing </div> in case the stylesheet location has been specified as a physical file.
        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.ConditionalRenderClosingDivElement"]/*' />
        protected virtual void ConditionalRenderClosingDivElement(XhtmlMobileTextWriter writer) {
            if ((String)Device[XhtmlConstants.RequiresXhtmlCssSuppression] == "true") {
                return;
            }
            String classAttribute = (String) Control.CustomAttributes[XhtmlConstants.CssClassCustomAttribute];
            if (CssLocation == StyleSheetLocation.PhysicalFile) {
                writer.WriteLine();
                if ((String)Device["usePOverDiv"] == "true") {
                    writer.WriteEndTag("p");
                }
                else {
                    writer.WriteEndTag("div");
                }
                writer.WriteLine();
                ConditionalPopPhysicalCssClass(writer);
            }
        }

        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.ConditionalPopPhysicalCssClass"]/*' />
        protected virtual void ConditionalPopPhysicalCssClass(XhtmlMobileTextWriter writer) {
            if (_physicalCssClassPushed) {
                writer.PopPhysicalCssClass();
                _physicalCssClassPushed = false;
            }
        }

        /////////////////////////////////////////////////////////////////////////
        // GENERAL CUSTOM ATTRIBUTE SUPPORT
        /////////////////////////////////////////////////////////////////////////

        // Plays same role as HtmlAdapter.AddCustomAttribute.  Named for consistency
        // within Xhtml adapter set.
        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.ConditionalRenderCustomAttribute"]/*' />
        protected virtual void ConditionalRenderCustomAttribute(XhtmlMobileTextWriter writer,
            String attributeName) {
            ConditionalRenderCustomAttribute(writer, attributeName, attributeName);
        }

        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.ConditionalRenderCustomAttribute1"]/*' />
        protected virtual void ConditionalRenderCustomAttribute(XhtmlMobileTextWriter writer,
            String attributeName, String markupAttributeName) {
            String attributeValue = ((IAttributeAccessor)Control).GetAttribute(attributeName);
            if (attributeValue != null && attributeValue.Length > 0) {
                writer.WriteAttribute(markupAttributeName, attributeValue, true);
            }
        }

        // Utilities to increase intelligibility
        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.GetCustomAttributeValue"]/*' />
        protected virtual String GetCustomAttributeValue(String attributeName) {
            return((IAttributeAccessor)Control).GetAttribute(attributeName);
        }

        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.GetCustomAttributeValue1"]/*' />
        protected virtual String GetCustomAttributeValue(MobileControl control, String attributeName) {
            return((IAttributeAccessor)control).GetAttribute(attributeName);
        }

        /////////////////////////////////////////////////////////////////////////
        // SPECIALIZED UTILITY METHODS FOR LIST SELECTIONLIST OBJECTLIST
        /////////////////////////////////////////////////////////////////////////

        // tagname can be any of table, ul, ol.  See the list adapters for examples.
        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.RenderOpeningListTag"]/*' />
        protected virtual void RenderOpeningListTag(XhtmlMobileTextWriter writer, String tagName) {            
            String classAttribute = (String) Control.CustomAttributes[XhtmlConstants.CssClassCustomAttribute];
            if (CssLocation == StyleSheetLocation.PhysicalFile && (String)Device[XhtmlConstants.RequiresXhtmlCssSuppression] != "true") {
                writer.WritePendingBreak();
                writer.WriteBeginTag(tagName);            
                if (classAttribute != null && 
                    classAttribute.Length > 0 &&
                    writer.DiffersFromCurrentPhysicalCssClass(classAttribute)) {
                    writer.WriteAttribute("class", classAttribute, true);
                    writer.PushPhysicalCssClass(classAttribute);
                    Debug.Assert(!_physicalCssClassPushed, "These calls should not be nested.");
                    _physicalCssClassPushed = true;
                }
                writer.Write(">");
            }
            else if ((String)Device[XhtmlConstants.RequiresXhtmlCssSuppression] != "true") {
                writer.WritePendingBreak();
                StyleFilter filter = writer.CurrentStyleClass.GetFilter(Style);
                writer.EnterStyle(new XhtmlFormatStyleClass(Style, filter), tagName);
            }
            else {
                writer.WritePendingBreak();
                writer.WriteFullBeginTag(tagName);
            }
        }

        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.RenderClosingListTag"]/*' />
        protected virtual void RenderClosingListTag(XhtmlMobileTextWriter writer, String tagName) {
            if (CssLocation == StyleSheetLocation.PhysicalFile  && (String)Device[XhtmlConstants.RequiresXhtmlCssSuppression] != "true") {
                writer.WriteEndTag(tagName);
                ConditionalPopPhysicalCssClass(writer);
            }
            else if ((String)Device[XhtmlConstants.RequiresXhtmlCssSuppression] != "true") {
                writer.ExitStyle(Style);
            }
            else {
                writer.WriteEndTag(tagName);
            }
        }

        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.ClearPendingBreakIfDeviceBreaksOnBlockLevel"]/*' />
        protected virtual void ClearPendingBreakIfDeviceBreaksOnBlockLevel(XhtmlMobileTextWriter writer) {
            if ((String)Device[XhtmlConstants.BreaksOnBlockElements] != "false") {
                writer.ClearPendingBreak();
            }
        }

        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.ConditionalClearPendingBreak"]/*' />
        protected virtual void ConditionalClearPendingBreak(XhtmlMobileTextWriter writer) {
            if ((String)Device[XhtmlConstants.BreaksOnInlineElements] == "true") {
                writer.ClearPendingBreak();
            }
        }

        // Required for a very rare device case where <select> cannot follow <table>.
        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.ConditionalClearCachedEndTag"]/*' />
        protected virtual void ConditionalClearCachedEndTag(XhtmlMobileTextWriter writer, String s) {
            if (s != null && s.Length > 0) {
                writer.ClearCachedEndTag ();
            }
        }

        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.RenderAsHiddenInputField"]/*' />
        protected virtual void RenderAsHiddenInputField(XhtmlMobileTextWriter writer) {
        }

        // Renders hidden variables for IPostBackDataHandlers which are
        // not displayed due to pagination or secondary UI.
        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.RenderOffPageVariables"]/*' />
        protected void RenderOffPageVariables(XhtmlMobileTextWriter writer, Control control, int page) {
            if (control.HasControls()) {
                foreach (Control child in control.Controls) {
                    // Note: Control.Form != null.
                    if (!child.Visible || child == Control.Form.Header || child == Control.Form.Footer) {
                        continue;
                    }

                    MobileControl mobileCtl = child as MobileControl;

                    if (mobileCtl != null) {
                        if (mobileCtl.IsVisibleOnPage(page)
                            && (mobileCtl == ((XhtmlFormAdapter)mobileCtl.Form.Adapter).SecondaryUIControl ||
                            null == ((XhtmlFormAdapter)mobileCtl.Form.Adapter).SecondaryUIControl)) {
                            if (mobileCtl.FirstPage == mobileCtl.LastPage) {
                                // Entire control is visible on this page, so no need to look
                                // into children.
                                continue;
                            }

                            // Control takes up more than one page, so it may be possible that
                            // its children are on a different page, so we'll continue to
                            // fall through into children.
                        }
                        else if (mobileCtl is IPostBackDataHandler) {
                            XhtmlControlAdapter adapter = mobileCtl.Adapter as XhtmlControlAdapter;
                            if (adapter != null) {
                                adapter.RenderAsHiddenInputField(writer);
                            }
                        }
                    }
                    RenderOffPageVariables(writer, child, page);
                }
            }
        }

        private static string AMP = "&";
        /// <include file='doc\XhtmlBasicControlAdapter.uex' path='docs/doc[@for="XhtmlControlAdapter.PreprocessQueryString"]/*' />
        protected String PreprocessQueryString(String queryString) {
            StringBuilder processedString = new StringBuilder();
            int offset  = 0;
            int pos     = 0;
            int length = queryString.Length;

            // ASURT 143419
            while (offset < length) {
                pos = queryString.IndexOf(AMP, offset, StringComparison.Ordinal);                
                if (pos == -1) {
                    processedString.Append(queryString.Substring(offset, queryString.Length - offset));
                    break;
                }

                if (pos != 0) {
                    processedString.Append(queryString.Substring(offset, pos - offset + 1));
                }
                do {
                    pos++;
                } while (pos < length && queryString[pos] == '&');
                offset = pos;
            }
            queryString = processedString.ToString();

            // ASURT 144130
            if (PageAdapter.PersistCookielessData == false) {
                queryString = RemoveQueryStringPair(FormsAuthentication.FormsCookieName, queryString);    
            }

            // ASURT 145389
            queryString = RemoveQueryStringPair("x-up-destcharset", queryString);
            return queryString;
        }

        private String RemoveQueryStringPair(String name, String queryString) {
            int pos = queryString.IndexOf(name, StringComparison.Ordinal);
            if (pos != -1) {
                int pos2 = queryString.IndexOf(AMP, pos, StringComparison.Ordinal);

                if (pos2 != -1) {
                    queryString = queryString.Remove(pos, pos2-pos+1);
                }
                else {
                    pos2 = queryString.IndexOf(AMP, StringComparison.Ordinal);
                    if ((pos2 != -1) && (pos2 < pos)) {
                        queryString = queryString.Substring(0, pos-1);
                    }
                    else {
                        queryString = queryString.Substring(0, pos);
                    }
                }
            }
            return queryString;
        }
    }
}
