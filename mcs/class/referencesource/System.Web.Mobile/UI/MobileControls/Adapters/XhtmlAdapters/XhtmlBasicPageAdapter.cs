//------------------------------------------------------------------------------
// <copyright file="XhtmlBasicPageAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Web;
using System.Collections;
using System.Collections.Specialized;
using System.Web.UI;
using System.IO;
using System.Security.Permissions;
using System.Text;
using System.Web.Mobile;
using System.Web.UI.MobileControls;
using System.Web.UI.MobileControls.Adapters;
using System.Diagnostics;
using System.Globalization;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource.XhtmlAdapters
#else
namespace System.Web.UI.MobileControls.Adapters.XhtmlAdapters
#endif
{

    /// <include file='doc\XhtmlBasicPageAdapter.uex' path='docs/doc[@for="XhtmlPageAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class XhtmlPageAdapter : XhtmlControlAdapter, IPageAdapter {
        private static readonly TimeSpan _cacheExpirationTime = new TimeSpan(0, 20, 0);
        private const int DefaultPageWeight = 4000;

        private IDictionary _cookielessDataDictionary = null;
        private int _defaultPageWeight = DefaultPageWeight;
        private int _optimumPageWeight = 0;
        private MobilePage _page;
        private bool _persistCookielessData = true;
        private bool _pushedCssClassForBody = false;

        public XhtmlPageAdapter() {
            HtmlPageAdapter.SetPreferredEncodings(HttpContext.Current);
        }

        /// <include file='doc\XhtmlBasicPageAdapter.uex' path='docs/doc[@for="XhtmlPageAdapter.CacheVaryByHeaders"]/*' />
        public virtual IList CacheVaryByHeaders {
            get {
                return null;
            }
        }

        /// <include file='doc\XhtmlBasicPageAdapter.uex' path='docs/doc[@for="XhtmlPageAdapter.CookielessDataDictionary"]/*' />
        public IDictionary CookielessDataDictionary {
            get {
                return _cookielessDataDictionary;
            }

            set {
                _cookielessDataDictionary = value;
            }
        }

        /// <include file='doc\XhtmlBasicPageAdapter.uex' path='docs/doc[@for="XhtmlPageAdapter.EventArgumentKey"]/*' />
        public virtual String EventArgumentKey {
            get {
                return Constants.EventArgumentID;
            }
        }

        /// <include file='doc\XhtmlBasicPageAdapter.uex' path='docs/doc[@for="XhtmlPageAdapter.EventSourceKey"]/*' />
        public virtual String EventSourceKey {
            get {
                return Constants.EventSourceID;
            }
        }

        /// <include file='doc\XhtmlBasicPageAdapter.uex' path='docs/doc[@for="XhtmlPageAdapter.OptimumPageWeight"]/*' />
        public virtual int OptimumPageWeight {
            get {
                if (_optimumPageWeight == 0) {
                    _optimumPageWeight = CalculateOptimumPageWeight(_defaultPageWeight);
                }
                return _optimumPageWeight;
            }
        }

        /// <include file='doc\XhtmlBasicPageAdapter.uex' path='docs/doc[@for="XhtmlPageAdapter.Page"]/*' />
        public override MobilePage Page {
            get {
                return _page;
            }
            set {
                _page = value;
            }
        }

        /// <include file='doc\XhtmlBasicPageAdapter.uex' path='docs/doc[@for="XhtmlPageAdapter.PersistCookielessData"]/*' />
        public bool PersistCookielessData {
            get {
                return _persistCookielessData;
            }
            set {
                _persistCookielessData = value;
            }
        }

        // Helper function to add multiple values for the same key
        private void AddValues(NameValueCollection sourceCollection,
            String sourceKey,
            NameValueCollection targetCollection) {
            String [] values = sourceCollection.GetValues(sourceKey);
            foreach (String value in values) {
                targetCollection.Add(sourceKey, value);
            }
        }

        private NameValueCollection CollectionFromForm(
            NameValueCollection form,
            String postEventSourceID,
            String postEventArgumentID) {
            int i;
            int count = form.Count;
            NameValueCollection collection = new NameValueCollection();
            bool isPostBack = false;

            for (i = 0; i < count; i++) {
                String name = form.GetKey(i);
                if (name == null || name.Length == 0) {
                    continue;
                }

                // Pager navigation is rendered by buttons which have the
                // targeted page number appended to the form id after
                // PagePrefix which is a constant string to identify this
                // special case.  E.g. ControlID__PG_2
                int index = name.LastIndexOf(Constants.PagePrefix, StringComparison.Ordinal);
                if (index != -1) {
                    // We need to associate the form id with the event source
                    // id and the page number with the event argument id in
                    // order to have the event raised properly by ASP.NET
                    int pageBeginPos = index + Constants.PagePrefix.Length;
                    collection.Add(postEventSourceID,
                        name.Substring(0, index));
                    collection.Add(postEventArgumentID,
                        name.Substring(pageBeginPos,
                        name.Length - pageBeginPos));
                    continue;
                }

                // This is to determine if the request is a postback from
                // the same mobile page.
                if (name == MobilePage.ViewStateID ||
                    name == EventSourceKey) {
                    isPostBack = true;
                }

                // Default case, just preserve the value(s)
                AddValues(form, name, collection);
            }

            if (collection.Count == 0 || !isPostBack) {
                // Returning null to indicate this is not a postback
                return null;
            }
            else {
                return collection;
            }
        }

        private NameValueCollection CollectionFromQueryString(
            NameValueCollection queryString,
            String postEventSourceID,
            String postEventArgumentID) {
            NameValueCollection collection = new NameValueCollection();
            bool isPostBack = false;

            for (int i = 0; i < queryString.Count; i++) {
                String name = queryString.GetKey(i);

                // ASSUMPTION: In query string, besides the expected
                // name/value pairs (ViewStateID, EventSource and
                // EventArgument), there are hidden variables, control
                // id/value pairs (if the form submit method is GET), unique
                // file path suffix variable and custom query string text.
                // They will be in the above order if any of them present.
                // Hidden variables and control id/value pairs should be added
                // back to the collection intactly, but the other 2 items
                // should not be added to the collection.

                // name can be null if there is a query name without equal
                // sign appended.  We should just ignored it in this case.
                if (name == null) {
                    continue;
                }
                else if (name == MobilePage.ViewStateID) {
                    collection.Add(MobilePage.ViewStateID, queryString.Get(i));
                    isPostBack = true;
                }
                else if (name == Constants.EventSourceID) {
                    collection.Add(postEventSourceID, queryString.Get(i));
                    isPostBack = true;
                }
                else if (name == Constants.EventArgumentID) {
                    collection.Add(postEventArgumentID, queryString.Get(i));
                }
                else if (Constants.UniqueFilePathSuffixVariable.StartsWith(name, StringComparison.Ordinal)) {
                    // At this point we know that the rest of them is
                    // the custom query string text, so we are done.
                    break;
                }
                else {
                    AddValues(queryString, name, collection);
                }
            }

            if (collection.Count == 0 || !isPostBack) {
                // Returning null to indicate this is not a postback
                return null;
            }
            else {
                return collection;
            }
        }

        private void ConditionalRenderLinkElement (XhtmlMobileTextWriter writer) {
            if (DoesDeviceRequireCssSuppression()) {
                return;
            }

            String cssLocation = (String) Page.ActiveForm.CustomAttributes[XhtmlConstants.StyleSheetLocationCustomAttribute];
            if (cssLocation != null &&
                cssLocation.Length != 0) {
                writer.WriteBeginTag ("link");
                writer.WriteAttribute ("type", "text/css");
                writer.WriteAttribute ("rel", "stylesheet");
                writer.WriteAttribute("href", cssLocation, true);
                writer.WriteLine("/>");
            }
            else if (!writer.IsStyleSheetEmpty () && CssLocation!=StyleSheetLocation.Internal) {
                writer.WriteLine ();
                writer.WriteBeginTag ("link");
                writer.WriteAttribute ("type", "text/css");
                writer.WriteAttribute ("rel", "stylesheet");
                String queryStringValue = GetCssQueryStringValue(writer);
                writer.Write(" href=\"" + XhtmlConstants.CssMappedFileName + "?" + XhtmlConstants.CssQueryStringName + "=" + queryStringValue + "\"/>");
                writer.WriteLine();
            }
        }

        private void ConditionalRenderStyleElement (XhtmlMobileTextWriter writer) {
            if (!writer.IsStyleSheetEmpty () && CssLocation == StyleSheetLocation.Internal) {
                bool requiresComments = (String)Device["requiresCommentInStyleElement"] == "true";
                writer.WriteLine();
                writer.WriteBeginTag("style");
                writer.Write(" type=\"text/css\">");
                writer.WriteLine();
                if (requiresComments) {
                    writer.WriteLine("<!--");
                }
                writer.Write(writer.GetStyles());
                if (requiresComments) {
                    writer.WriteLine("-->");
                }
                writer.WriteEndTag("style");
                writer.WriteLine(); 
            }
        }

        /// <include file='doc\XhtmlBasicPageAdapter.uex' path='docs/doc[@for="XhtmlPageAdapter.CreateTextWriter"]/*' />
        public virtual HtmlTextWriter CreateTextWriter( TextWriter writer) {
            return new XhtmlMobileTextWriter (writer, Device);
        }

        // Similar to CHTML.
        /// <include file='doc\XhtmlBasicPageAdapter.uex' path='docs/doc[@for="XhtmlPageAdapter.DeterminePostBackMode"]/*' />
        public virtual NameValueCollection DeterminePostBackMode(
            HttpRequest request,
            String postEventSourceID,
            String postEventArgumentID,
            NameValueCollection baseCollection) {

            if (baseCollection != null && baseCollection[EventSourceKey] == XhtmlConstants.PostedFromOtherFile) {
                return null;
            }
            else if (request == null) {
                return baseCollection;
            }
            else if (String.Compare(request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase) == 0) {
                return CollectionFromForm(request.Form,
                    postEventSourceID,
                    postEventArgumentID);
            }
            else if (request.QueryString.Count == 0) {
                return baseCollection;
            }
            else {
                return CollectionFromQueryString(request.QueryString,
                    postEventSourceID,
                    postEventArgumentID);
            }
        }

        /// <include file='doc\XhtmlBasicPageAdapter.uex' path='docs/doc[@for="XhtmlPageAdapter.DeviceQualifies"]/*' />
        public static bool DeviceQualifies(HttpContext context) {
            String type = ((MobileCapabilities)context.Request.Browser).PreferredRenderingType;
            return String.Equals(type, "xhtml-basic", StringComparison.OrdinalIgnoreCase) || 
                String.Equals(type, "xhtml-mp", StringComparison.OrdinalIgnoreCase) ||
                String.Equals(type, "wml20", StringComparison.OrdinalIgnoreCase);
        }

        private bool DoesDeviceRequireCssSuppression() {
            return(String)Device[XhtmlConstants.RequiresXhtmlCssSuppression] == "true";
        }

        private String GetCssQueryStringValue(XhtmlMobileTextWriter writer) {
            if (CssLocation == StyleSheetLocation.ApplicationCache) {
                // Initialize the cache key
                writer.SetCacheKey(Page.Cache);                
                return writer.CacheKey;
            }
            else if (CssLocation == StyleSheetLocation.SessionState) {
                writer.SetSessionKey(Page.Session);
                return writer.SessionKey;
            }
            Debug.Assert (StyleSheetLocationAttributeValue != null);
            return StyleSheetLocationAttributeValue;        
        }

        /// <include file='doc\XhtmlBasicPageAdapter.uex' path='docs/doc[@for="XhtmlPageAdapter.HandleError"]/*' />
        public virtual bool HandleError (Exception e, HtmlTextWriter writer) {
            return false;
        }

        /// <include file='doc\XhtmlBasicPageAdapter.uex' path='docs/doc[@for="XhtmlPageAdapter.HandlePagePostBackEvent"]/*' />
        public virtual bool HandlePagePostBackEvent (string eventSource, string eventArgument) {
            return false;
        }

        /// <include file='doc\XhtmlBasicPageAdapter.uex' path='docs/doc[@for="XhtmlPageAdapter.InitWriterState"]/*' />
        protected virtual void InitWriterState(XhtmlMobileTextWriter writer) {
            writer.UseDivsForBreaks = (String)Device[XhtmlConstants.BreaksOnInlineElements] == "true";
            writer.SuppressNewLine = (String)Device[XhtmlConstants.RequiresNewLineSuppression] == "true";
            writer.SupportsNoWrapStyle = (String)Device[XhtmlConstants.SupportsNoWrapStyle] != "false";
        }
        
        /// <include file='doc\XhtmlBasicPageAdapter.uex' path='docs/doc[@for="XhtmlPageAdapter.OnPreRender"]/*' />
        public override void OnPreRender(EventArgs e) {
            if (Page.ActiveForm.Paginate && Page.ActiveForm.Action.Length > 0) {
                Page.ActiveForm.Paginate = false;
            }
            base.OnPreRender(e);
        }
        
        /// <include file='doc\XhtmlBasicPageAdapter.uex' path='docs/doc[@for="XhtmlPageAdapter.Render"]/*' />
        public override void Render (XhtmlMobileTextWriter writer) {
            writer.BeginResponse ();
            if (Page.Request.Browser["requiresPragmaNoCacheHeader"] == "true") {
                Page.Response.AppendHeader("Pragma", "no-cache");
            }
            InitWriterState(writer);          
            writer.BeginCachedRendering ();
            // For consistency with HTML, we render the form style with body tag.
            RenderOpeningBodyElement(writer);
            // Certain WML 2.0 devices require that we write an onevent onenterforward setvar snippet.
            // We cannot know the relevant variables until after the form is rendered, so we mark this
            // position.  The setvar elements will be inserted into the cached rendering here.
            writer.MarkWmlOnEventLocation ();
            Page.ActiveForm.RenderControl(writer);
            RenderClosingBodyElement(writer);
            writer.ClearPendingBreak ();
            writer.EndCachedRendering ();

            // Note: first and third arguments are not used.
            writer.BeginFile (Page.Request.Url.ToString (), Page.Device.PreferredRenderingMime, Page.Response.Charset);
            String supportsXmlDeclaration = Device["supportsXmlDeclaration"];
            // Invariant culture not needed, included for best practices compliance.
            if (supportsXmlDeclaration == null ||
                !String.Equals(supportsXmlDeclaration, "false", StringComparison.OrdinalIgnoreCase)) {
                writer.WriteXmlDeclaration ();
            }
            writer.WriteDoctypeDeclaration(DocumentType);
            // Review: Hard coded xmlns.
            writer.WriteFullBeginTag ("html xmlns=\"http://www.w3.org/1999/xhtml\"");
            writer.WriteLine ();
            writer.WriteFullBeginTag ("head");
            writer.WriteLine ();
            writer.WriteFullBeginTag ("title");
            if (Page.ActiveForm.Title != null) {
                writer.WriteEncodedText(Page.ActiveForm.Title);
            }
            writer.WriteEndTag ("title");
            ConditionalRenderLinkElement (writer);
            ConditionalRenderStyleElement (writer);
            writer.WriteEndTag ("head");
            writer.WriteLine ();
            writer.WriteCachedMarkup (); // includes body tag.
            writer.WriteLine ();
            writer.WriteEndTag ("html");
            writer.EndFile ();
            if (!DoesDeviceRequireCssSuppression()) {
                if (CssLocation == StyleSheetLocation.ApplicationCache && !writer.IsStyleSheetEmpty()) {
                    // Recall that Page.Cache has application scope
                    Page.Cache.Insert(writer.CacheKey, writer.GetStyles (), null, DateTime.MaxValue, _cacheExpirationTime);
                }
                else if (CssLocation == StyleSheetLocation.SessionState && !writer.IsStyleSheetEmpty()) {
                    Page.Session[writer.SessionKey] = writer.GetStyles();
                }
            }

            writer.EndResponse ();
        }

        private void RenderClosingBodyElement(XhtmlMobileTextWriter writer) {
            Style formStyle = ((ControlAdapter)Page.ActiveForm.Adapter).Style;
            if (CssLocation == StyleSheetLocation.PhysicalFile) {
                writer.WriteEndTag("body");
                if (_pushedCssClassForBody) {
                    writer.PopPhysicalCssClass();
                }
            }
            else if ((String)Device[XhtmlConstants.RequiresXhtmlCssSuppression] != "true" &&
                (String)Device[XhtmlConstants.SupportsBodyClassAttribute] != "false") {
                writer.ExitStyle(formStyle); // writes the closing body element.
            }
            else {
                writer.WriteEndTag ("body");
            }
        }

        private void RenderHiddenVariablesInUrl(XhtmlMobileTextWriter writer) {
            if (Page.HasHiddenVariables()) {
                String hiddenVariablePrefix = MobilePage.HiddenVariablePrefix;
                foreach (DictionaryEntry entry in Page.HiddenVariables) {
                    writer.Write("&amp;");
                    writer.WriteUrlParameter(hiddenVariablePrefix + (String)entry.Key,
                        (String)entry.Value);
                }
            }
        }

        private void RenderOpeningBodyElement(XhtmlMobileTextWriter writer) {
            Form activeForm = Page.ActiveForm;
            Style formStyle = ((ControlAdapter)activeForm.Adapter).Style;
            if (CssLocation == StyleSheetLocation.PhysicalFile) {
                String cssClass = (String) activeForm.CustomAttributes[XhtmlConstants.CssClassCustomAttribute];
                writer.WriteBeginTag("body");
                if (cssClass != null  && (String)Device["supportsBodyClassAttribute"] != "false") {
                    writer.WriteAttribute("class", cssClass, true /* encode */);
                    writer.PushPhysicalCssClass(cssClass);
                    _pushedCssClassForBody = true;
                }
                writer.Write(">");
            }
            else if ((String)Device[XhtmlConstants.RequiresXhtmlCssSuppression] != "true" &&
                (String)Device[XhtmlConstants.SupportsBodyClassAttribute] != "false") {
                writer.EnterStyle(formStyle, "body");
            }
            else {
                writer.WriteFullBeginTag("body");    
                if ((String)Device[XhtmlConstants.RequiresXhtmlCssSuppression] != "true" &&
                    (String)Device[XhtmlConstants.SupportsBodyClassAttribute] == "false") {
                    writer.SetBodyStyle(formStyle);
                }
            }
        }

        /// <include file='doc\XhtmlBasicPageAdapter.uex' path='docs/doc[@for="XhtmlPageAdapter.RenderUrlPostBackEvent"]/*' />
        public virtual void RenderUrlPostBackEvent (XhtmlMobileTextWriter writer,
            String target, 
            String argument) {

            String amp = (String)Device[XhtmlConstants.SupportsUrlAttributeEncoding] == "false"  ? "&" : "&amp;";
            
            if ((String)Device["requiresAbsolutePostbackUrl"] == "true") {
                writer.WriteEncodedUrl(Page.AbsoluteFilePath);
            }
            else {
                writer.WriteEncodedUrl(Page.RelativeFilePath);
            }
            writer.Write ("?");

            // Encode ViewStateID=.....&__ET=controlid&__EA=value in URL
            // Note: the encoding needs to be agreed with the page
            // adapter which handles the post back info
            String pageState = Page.ClientViewState;
            if (pageState != null) {
                writer.WriteUrlParameter (MobilePage.ViewStateID, pageState);
                writer.Write (amp);
            }
            writer.WriteUrlParameter (EventSourceKey, target);
            writer.Write (amp);
            writer.WriteUrlParameter (EventArgumentKey, argument);
            RenderHiddenVariablesInUrl (writer);

            // Unique file path suffix is used for identify if query
            // string text is present.  Corresponding code needs to agree
            // on this.  Even if the query string is empty, we still need
            // to output the suffix to indicate this. (this corresponds
            // to the code that handles the postback)
            writer.Write(amp);
            writer.Write(Constants.UniqueFilePathSuffixVariable);

            String queryStringText = PreprocessQueryString(Page.QueryStringText);

            if (queryStringText != null && queryStringText.Length > 0) {
                writer.Write (amp);
                if ((String)Device[XhtmlConstants.SupportsUrlAttributeEncoding] != "false") {
                    writer.WriteEncodedAttributeValue(queryStringText);
                }
                else {
                    writer.Write (queryStringText);
                }
            }
        }
    }
}
