//------------------------------------------------------------------------------
// <copyright file="HtmlPageAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web.Mobile;
using System.Web.UI.MobileControls.Adapters;
using System.Security.Permissions;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource
#else
namespace System.Web.UI.MobileControls.Adapters
#endif    

{

    /*
     * HtmlPageAdapter class.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\HtmlPageAdapter.uex' path='docs/doc[@for="HtmlPageAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class HtmlPageAdapter : HtmlControlAdapter, IPageAdapter
    {
        private IList _renderableForms = null;

        private int _optimumPageWeight = 0;
        private const int DefaultPageWeight = 4000;
        private readonly int _defaultPageWeight;
        private const String _postedFromOtherFile = ".";
        private IDictionary _cookielessDataDictionary = null;

        /// <include file='doc\HtmlPageAdapter.uex' path='docs/doc[@for="HtmlPageAdapter.HtmlPageAdapter"]/*' />
        public HtmlPageAdapter() : this(DefaultPageWeight)
        {
        }

        /// <include file='doc\HtmlPageAdapter.uex' path='docs/doc[@for="HtmlPageAdapter.HtmlPageAdapter1"]/*' />
        protected internal HtmlPageAdapter(int defaultPageWeight)
        {
            _defaultPageWeight = defaultPageWeight;
            SetPreferredEncodings(HttpContext.Current);
        }

        /// <include file='doc\HtmlPageAdapter.uex' path='docs/doc[@for="HtmlPageAdapter.OptimumPageWeight"]/*' />
        public virtual int OptimumPageWeight
        {
            get
            {
                if (_optimumPageWeight == 0)
                {
                    _optimumPageWeight = CalculateOptimumPageWeight(_defaultPageWeight);
                }
                return _optimumPageWeight;
            }
        }


        ///////////////////////////////////////////////////////////////////////////
        //  Static method used for determining if device should use
        //  this adapter
        ///////////////////////////////////////////////////////////////////////////

        /// <include file='doc\HtmlPageAdapter.uex' path='docs/doc[@for="HtmlPageAdapter.DeviceQualifies"]/*' />
        public static bool DeviceQualifies(HttpContext context)
        {
            String type = ((MobileCapabilities)context.Request.Browser).PreferredRenderingType;
            bool javascriptSupported = context.Request.Browser.JavaScript;
            bool qualifies = (type == MobileCapabilities.PreferredRenderingTypeHtml32) && javascriptSupported;
            return qualifies;
        }
        
        ///////////////////////////////////////////////////////////////////////////
        //  IControlAdapter implementation
        ///////////////////////////////////////////////////////////////////////////

        /// <include file='doc\HtmlPageAdapter.uex' path='docs/doc[@for="HtmlPageAdapter.Render"]/*' />
        public override void Render(HtmlMobileTextWriter writer)
        {
            writer.BeginResponse();

            if (RequiresPragmaNoCacheHeader())
            {
                Page.Response.AppendHeader("Pragma", "no-cache");
            }

            if (writer.SupportsMultiPart)
            {
                // -1 means it doesn't care about forms' total weight
                _renderableForms = Page.ActiveForm.GetLinkedForms(-1);
                Debug.Assert(_renderableForms != null, "_renderableForms is null");

                foreach (Form form in _renderableForms)
                {
                    RenderForm(writer, form);
                }
            }
            else
            {
                RenderForm(writer, Page.ActiveForm);
            }

            writer.EndResponse();
        }

        private bool RequiresPragmaNoCacheHeader()
        {
            // We need to Assert ASP.NET Low permission because Request.ServerVariables
            // requires it.
            (new AspNetHostingPermission(AspNetHostingPermissionLevel.Low)).Assert();

            String protocol = Page.Request.ServerVariables["SERVER_PROTOCOL"];
            return (protocol == "HTTP/1.0");
        }

        /// <include file='doc\HtmlPageAdapter.uex' path='docs/doc[@for="HtmlPageAdapter.RenderForm"]/*' />
        public virtual void RenderForm(HtmlMobileTextWriter writer, Form form)
        {
            writer.BeginFile(GetFormUrl(form), "text/html", Page.Response.Charset);
            writer.WriteFullBeginTag("html");
            form.RenderControl(writer);
            if (Device.RequiresDBCSCharacter)
            {
                // Insert a comment with a space
                writer.Write("<!--\u3000-->");
            }
            writer.WriteEndTag("html");
            writer.EndFile();
        }

        /// <include file='doc\HtmlPageAdapter.uex' path='docs/doc[@for="HtmlPageAdapter.IsFormRendered"]/*' />
        public virtual bool IsFormRendered(Form form)
        {
            Debug.Assert(_renderableForms != null, "_renderableForms is null");
            return _renderableForms.Contains(form);
        }

        /// <include file='doc\HtmlPageAdapter.uex' path='docs/doc[@for="HtmlPageAdapter.GetFormUrl"]/*' />
        public String GetFormUrl(Form form)
        {
            if (form == Page.ActiveForm)
            {
                return Page.Request.Url.ToString();
            }
            else
            {
                return form.ClientID + ".html";
            }
        }

        /// <include file='doc\HtmlPageAdapter.uex' path='docs/doc[@for="HtmlPageAdapter.RenderPostBackEvent"]/*' />
        public virtual void RenderPostBackEvent(HtmlMobileTextWriter writer, 
                                                String target, 
                                                String argument)
        {
            writer.Write("javascript:__doPostBack('");
            writer.Write(target);
            writer.Write("','");
            writer.Write(argument);
            writer.Write("')");
        }

        /// <include file='doc\HtmlPageAdapter.uex' path='docs/doc[@for="HtmlPageAdapter.RenderUrlPostBackEvent"]/*' />
        public virtual void RenderUrlPostBackEvent(HtmlMobileTextWriter writer,
                                                   String target, 
                                                   String argument)
        {
            writer.WriteEncodedUrl(Page.RelativeFilePath);
            writer.Write("?");

            // Encode ViewStateID=.....&__ET=controlid&__EA=value in URL
            // Note: the encoding needs to be agreed with the page
            // adapter which handles the post back info
            String pageState = Page.ClientViewState;
            if (pageState != null)
            {
                writer.WriteUrlParameter(MobilePage.ViewStateID, pageState);
                writer.Write("&");
            }
            writer.WriteUrlParameter(EventSourceKey, target);
            writer.Write("&");
            writer.WriteUrlParameter(EventArgumentKey, argument);
            RenderHiddenVariablesInUrl(writer);

            // Unique file path suffix is used for identify if query
            // string text is present.  Corresponding code needs to agree
            // on this.  Even if the query string is empty, we still need
            // to output the suffix to indicate this. (this corresponds
            // to the code that handles the postback)
            writer.Write('&');
            writer.Write(Constants.UniqueFilePathSuffixVariable);

            String queryStringText = Page.QueryStringText;
            if (queryStringText != null && queryStringText.Length > 0)
            {
                writer.Write('&');
                writer.Write(queryStringText);
            }
        }

        /// <include file='doc\HtmlPageAdapter.uex' path='docs/doc[@for="HtmlPageAdapter.EventSourceKey"]/*' />
        protected virtual String EventSourceKey
        {
            get
            {
                return MobilePage.HiddenPostEventSourceId;
            }
        }

        /// <include file='doc\HtmlPageAdapter.uex' path='docs/doc[@for="HtmlPageAdapter.EventArgumentKey"]/*' />
        protected virtual String EventArgumentKey
        {
            get
            {
                return MobilePage.HiddenPostEventArgumentId;
            }
        }

        /// <include file='doc\HtmlPageAdapter.uex' path='docs/doc[@for="HtmlPageAdapter.RenderHiddenVariables"]/*' />
        protected void RenderHiddenVariables(HtmlMobileTextWriter writer)
        {
            if (Page.HasHiddenVariables())
            {
                String hiddenVariablePrefix = MobilePage.HiddenVariablePrefix;
                foreach (DictionaryEntry entry in Page.HiddenVariables)
                {
                    if (entry.Value != null)
                    {
                        writer.WriteHiddenField(hiddenVariablePrefix + (String)entry.Key, 
                                            (String)entry.Value);
                    }
                }
            }
        }

        private void RenderHiddenVariablesInUrl(HtmlMobileTextWriter writer)
        {
            if (Page.HasHiddenVariables())
            {
                String hiddenVariablePrefix = MobilePage.HiddenVariablePrefix;
                foreach (DictionaryEntry entry in Page.HiddenVariables)
                {
                    writer.Write("&");
                    writer.WriteUrlParameter(hiddenVariablePrefix + (String)entry.Key,
                                             (String)entry.Value);
                }
            }
        }

        /// <include file='doc\HtmlPageAdapter.uex' path='docs/doc[@for="HtmlPageAdapter.RenderPostBackHeader"]/*' />
        public virtual void RenderPostBackHeader(HtmlMobileTextWriter writer, Form form)
        {
            bool postBack = form.Action.Length == 0;

            RenderPageState(writer);

            writer.WriteHiddenField(EventSourceKey, postBack ? "" : _postedFromOtherFile);
            writer.WriteHiddenField(EventArgumentKey, "");
            RenderHiddenVariables(writer);

            writer.Write("<script language=javascript><!--\r\n");
            writer.Write("function __doPostBack(target, argument){\r\n");
            writer.Write("  var theform = document.");
            writer.Write(form.ClientID);
            writer.Write("\r\n");
            if (form.Action.Length > 0)
            {
                writer.Write("  theform.action = \"\"\r\n");
            }
            writer.Write("  theform.");
            writer.Write(EventSourceKey);
            writer.Write(".value = target\r\n");
            writer.Write("  theform.");
            writer.Write(EventArgumentKey);
            writer.Write(".value = argument\r\n");
            writer.Write("  theform.submit()\r\n");
            writer.Write("}\r\n");
            writer.Write("// -->\r\n");
            writer.Write("</script>\r\n");
        }

        ///////////////////////////////////////////////////////////////////////////
        //  IPageAdapter implementation
        ///////////////////////////////////////////////////////////////////////////

        private MobilePage _page;
        
        /// <include file='doc\HtmlPageAdapter.uex' path='docs/doc[@for="HtmlPageAdapter.Page"]/*' />
        public override MobilePage Page
        {
            get
            {
                return _page;
            }
            set
            {
                _page = value;
            }
        }

        /// <include file='doc\HtmlPageAdapter.uex' path='docs/doc[@for="HtmlPageAdapter.CookielessDataDictionary"]/*' />
        public IDictionary CookielessDataDictionary
        {
            get
            {
                return _cookielessDataDictionary;
            }

            set
            {
                _cookielessDataDictionary = value;
            }
        }

        private bool _persistCookielessData = true;
        /// <include file='doc\HtmlPageAdapter.uex' path='docs/doc[@for="HtmlPageAdapter.PersistCookielessData"]/*' />
        public bool PersistCookielessData
        {
            get
            {
                return _persistCookielessData;
            }
            
            set
            {
                _persistCookielessData = value;
            }
        }

        /// <include file='doc\HtmlPageAdapter.uex' path='docs/doc[@for="HtmlPageAdapter.CreateTextWriter"]/*' />
        public virtual HtmlTextWriter CreateTextWriter(TextWriter writer)
        {
            return new HtmlMobileTextWriter(writer, Device);
        }

        /// <include file='doc\HtmlPageAdapter.uex' path='docs/doc[@for="HtmlPageAdapter.DeterminePostBackMode"]/*' />
        public virtual NameValueCollection DeterminePostBackMode(
            HttpRequest request,
            String postEventSourceID,
            String postEventArgumentID,
            NameValueCollection baseCollection)
        {
            if (baseCollection != null && 
                baseCollection[EventSourceKey] == _postedFromOtherFile)
            {
                return null;
            }
            return baseCollection;
        }

        /// <include file='doc\HtmlPageAdapter.uex' path='docs/doc[@for="HtmlPageAdapter.CacheVaryByHeaders"]/*' />
        public virtual IList CacheVaryByHeaders
        {
            get
            {
                return null;
            }
        }

        /// <include file='doc\HtmlPageAdapter.uex' path='docs/doc[@for="HtmlPageAdapter.HandleError"]/*' />
        public virtual bool HandleError(Exception e, HtmlTextWriter writer)
        {
            return false;
        }

        internal void RenderPageState(HtmlMobileTextWriter writer)
        {
            String viewState = Page.ClientViewState;
            if (viewState != null)
            {
                writer.WriteHiddenField(MobilePage.ViewStateID, viewState);
            }
        }

        /// <include file='doc\HtmlPageAdapter.uex' path='docs/doc[@for="HtmlPageAdapter.HandlePagePostBackEvent"]/*' />
        public virtual bool HandlePagePostBackEvent(String eventSource, String eventArgument)
        {
            return false;
        }


        internal static void SetPreferredEncodings(HttpContext context) {
            // context can be null when in design mode
            if (context == null) {
                return;
            }

            //handle preferred response and request encoding here
            MobileCapabilities caps = (MobileCapabilities)context.Request.Browser;
            if(caps != null) {
                string preferredResponseEncoding = (string)caps["preferredResponseEncoding"];
                string preferredRequestEncoding =  (string)caps["preferredRequestEncoding"];

                if(preferredResponseEncoding != null && preferredResponseEncoding.Length > 0) {
                    context.Response.ContentEncoding = Encoding.GetEncoding(preferredResponseEncoding);
                }

                if(preferredRequestEncoding != null && preferredRequestEncoding.Length > 0) {
                    context.Request.ContentEncoding = Encoding.GetEncoding(preferredRequestEncoding);
                }
            }
        }
    }

}
