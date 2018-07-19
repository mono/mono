//------------------------------------------------------------------------------
// <copyright file="WmlPageAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web.Mobile;
using System.Web.UI.MobileControls;
using System.Web.UI.MobileControls.Adapters;
using System.Security.Permissions;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource
#else
namespace System.Web.UI.MobileControls.Adapters
#endif    

{

    /*
     * WmlPageAdapter base class contains wml specific methods.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\WmlPageAdapter.uex' path='docs/doc[@for="WmlPageAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class WmlPageAdapter : WmlControlAdapter, IPageAdapter
    {
        private IList _renderableForms;
        private static readonly String _headerBegin = "<?xml version='1.0'";
        private static readonly String _headerEncoding = " encoding ='{0}'";
        private static readonly String _headerEnd
            = "?>\r\n"
            + "<!DOCTYPE wml PUBLIC '-//WAPFORUM//DTD WML 1.1//EN' 'http://www.wapforum.org/DTD/wml_1.1.xml'>";
        private static readonly String _cacheExpiry
            = "<head>\r\n"
            + "<meta http-equiv=\"Cache-Control\" content=\"max-age=0\" />\r\n"
            + "</head>\r\n";
        private static readonly String _contentType = "text/vnd.wap.wml";

        private IDictionary _cookielessDataDictionary = null;


        ///////////////////////////////////////////////////////////////////////////
        //  Static method used for determining if device should use
        //  this adapter
        ///////////////////////////////////////////////////////////////////////////

        /// <include file='doc\WmlPageAdapter.uex' path='docs/doc[@for="WmlPageAdapter.DeviceQualifies"]/*' />
        public static bool DeviceQualifies(HttpContext context)
        {
            MobileCapabilities capabilities = ((MobileCapabilities)context.Request.Browser);
            String type = capabilities.PreferredRenderingType;
            bool qualifies = (type == MobileCapabilities.PreferredRenderingTypeWml11) ||
                             (type == MobileCapabilities.PreferredRenderingTypeWml12);

            return qualifies;
        }

        ///////////////////////////////////////////////////////////////////////////
        //  IPageAdapter implementation
        ///////////////////////////////////////////////////////////////////////////

        private MobilePage _page;
        
        /// <include file='doc\WmlPageAdapter.uex' path='docs/doc[@for="WmlPageAdapter.Page"]/*' />
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

        /// <include file='doc\WmlPageAdapter.uex' path='docs/doc[@for="WmlPageAdapter.CookielessDataDictionary"]/*' />
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

        private bool _persistCookielessData = false;

        /// <include file='doc\WmlPageAdapter.uex' path='docs/doc[@for="WmlPageAdapter.PersistCookielessData"]/*' />
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

        private int _optimumPageWeight = 0;
        private const int DefaultPageWeight = 800;

        /// <include file='doc\WmlPageAdapter.uex' path='docs/doc[@for="WmlPageAdapter.OptimumPageWeight"]/*' />
        public virtual int OptimumPageWeight
        {
            get
            {
                if (_optimumPageWeight == 0)
                {
                    _optimumPageWeight = CalculateOptimumPageWeight(DefaultPageWeight);
                }
                return _optimumPageWeight;
            }
        }

        /// <include file='doc\WmlPageAdapter.uex' path='docs/doc[@for="WmlPageAdapter.CreateTextWriter"]/*' />
        public virtual HtmlTextWriter CreateTextWriter(TextWriter writer)
        {
            return new WmlMobileTextWriter(writer, Device, Page);
        }

        /// <include file='doc\WmlPageAdapter.uex' path='docs/doc[@for="WmlPageAdapter.DeterminePostBackMode"]/*' />
        public virtual NameValueCollection DeterminePostBackMode(
            HttpRequest request,
            String postEventSourceID,
            String postEventArgumentID,
            NameValueCollection baseCollection)
        {
            NameValueCollection collection = baseCollection;

            if (collection != null)
            {
                if (Device.RequiresSpecialViewStateEncoding)
                {
                    // Reverse the special character replacement done when
                    // writing out the viewstate value.
                    String speciallyEncodedState =
                                    baseCollection[MobilePage.ViewStateID];

                    if (speciallyEncodedState != null)
                    {
                        speciallyEncodedState = EncodeSpecialViewState(speciallyEncodedState);

                        // We need to regenerate the collection since the
                        // original baseCollection is readonly.
                        collection = new NameValueCollection();
                        bool viewStateAdded = false;

                        for (int i = 0; i < baseCollection.Count; i++)
                        {
                            String name = baseCollection.GetKey(i);

                            if (!viewStateAdded && name == MobilePage.ViewStateID)
                            {
                                // This is the viewstate value we want to change.
                                collection.Add(MobilePage.ViewStateID, speciallyEncodedState);
                                viewStateAdded = true;
                            }
                            else
                            {
                                collection.Add(name, baseCollection.Get(i));
                            }
                        }
                    }
                }
            }

            return collection;
        }

        /// <include file='doc\WmlPageAdapter.uex' path='docs/doc[@for="WmlPageAdapter.CacheVaryByHeaders"]/*' />
        public virtual IList CacheVaryByHeaders
        {
            get
            {
                return null;
            }
        }

        /// <include file='doc\WmlPageAdapter.uex' path='docs/doc[@for="WmlPageAdapter.HandleError"]/*' />
        public virtual bool HandleError(Exception e, HtmlTextWriter writer)
        {
            WmlMobileTextWriter wmlWriter = (WmlMobileTextWriter)writer;

            bool renderBackButton = Device.NumberOfSoftkeys > 2 && !Device.HasBackButton;

            //  set mime type, in case init() has not been called yet
            Page.Response.ContentType = _contentType;

            Exception ex;
            if ((e is HttpException && e.InnerException != null) || 
                (e is System.Reflection.TargetInvocationException && e.InnerException != null))
            {
                ex = e.InnerException;
            }
            else
            {
                ex = e;
            }

            if (RequiresUTF8ContentEncoding())
            {
                Page.Response.ContentEncoding = UTF8Encoding;
            }

            wmlWriter.BeginResponse();
            wmlWriter.BeginFile(Page.Request.Url.ToString(), _contentType, Page.Response.Charset);

            RenderXmlHeader(writer);

            // First card.

            writer.WriteFullBeginTag("wml");

            if (Device.SupportsCacheControlMetaTag)
            {
                writer.Write(_cacheExpiry);
            }
            else
            {
                Page.Response.AppendHeader("Cache-Control", "max-age=0");
            }

            writer.WriteFullBeginTag("card");

            writer.WriteFullBeginTag("p");
            writer.WriteFullBeginTag("big");
            writer.WriteFullBeginTag("b");
            wmlWriter.WriteEncodedText(SR.GetString(SR.WmlPageAdapterServerError,
                                       HttpRuntime.AppDomainAppVirtualPath));
            writer.WriteEndTag("b");
            writer.WriteEndTag("big");
            writer.WriteEndTag("p");

            writer.WriteFullBeginTag("p");
            writer.Write("<do type=\"accept\" label=\"");
            writer.Write(GetDefaultLabel(MoreLabel));
            writer.Write("\"><go href=\"#more\" /></do>");
            if (renderBackButton)
            {
                writer.Write("<do type=\"prev\" label=\"");
                writer.Write(GetDefaultLabel(BackLabel));
                writer.WriteLine("\"><prev /></do>");
            }
            wmlWriter.WriteEncodedText(ex.GetType().ToString());
            writer.Write("<br />");
            wmlWriter.WriteEncodedText(ex.Message);
            writer.Write("<br />");
            wmlWriter.WriteEncodedText(SR.GetString(SR.WmlPageAdapterMethod));
            if (ex.TargetSite != null) {
                wmlWriter.WriteEncodedText(ex.TargetSite.Name);
            }
            writer.Write("<br />");
            writer.WriteEndTag("p");

            writer.WriteEndTag("card");

            // Stack trace card.

            writer.Write("<card id=\"more\">");
            writer.Write("<p mode=\"nowrap\">");

            if (renderBackButton)
            {
                writer.Write("<do type=\"prev\" label=\"");
                wmlWriter.WriteEncodedText(GetDefaultLabel(BackLabel));
                writer.WriteLine("\"><prev /></do>");
            }

            String stackTrace = ex.StackTrace;
            if (stackTrace != null) {
                int maximumStackTrace = OptimumPageWeight / 2;
                if (stackTrace.Length > maximumStackTrace)
                {
                    wmlWriter.WriteEncodedText(SR.GetString(SR.WmlPageAdapterStackTrace));
                    writer.Write("<br />");
                    stackTrace = stackTrace.Substring(0, maximumStackTrace);
                }
                else
                {
                    wmlWriter.WriteEncodedText(SR.GetString(SR.WmlPageAdapterPartialStackTrace));
                    writer.Write("<br />");
                }
            
                int lineBegin = 0;
                int lineEnd;
                while (lineBegin < stackTrace.Length)
                {
                    lineEnd = stackTrace.IndexOf("\r\n", lineBegin, StringComparison.Ordinal);
                    if (lineEnd == -1)
                    {
                        lineEnd = stackTrace.Length;
                    }
            
                    wmlWriter.WriteEncodedText(stackTrace.Substring(lineBegin, lineEnd - lineBegin));
                    writer.Write("<br />");
            
                    lineBegin = lineEnd + 2;
                }
            }
            
            writer.WriteEndTag("p");
            writer.WriteEndTag("card");
            writer.WriteEndTag("wml");
            
            wmlWriter.EndFile();
            wmlWriter.EndResponse();
            return true;
        }

        /// <include file='doc\WmlPageAdapter.uex' path='docs/doc[@for="WmlPageAdapter.HandlePagePostBackEvent"]/*' />
        public virtual bool HandlePagePostBackEvent(String eventSource, String eventArgument)
        {
            return false;
        }

        /// <include file='doc\WmlPageAdapter.uex' path='docs/doc[@for="WmlPageAdapter.Render"]/*' />
        public override void Render(WmlMobileTextWriter writer)
        {
            if (RequiresUTF8ContentEncoding())
            {
                Page.Response.ContentEncoding = UTF8Encoding;
            }

            writer.BeginResponse();
            writer.BeginFile(Page.Request.Url.ToString(), _contentType, Page.Response.Charset);

            RenderXmlHeader(writer);

            writer.WriteFullBeginTag("wml");

            if (Device.SupportsCacheControlMetaTag)
            {
                writer.Write(_cacheExpiry);
            }
            else
            {
                Page.Response.AppendHeader("Cache-Control", "max-age=0");
            }

            if (Device.CanCombineFormsInDeck)
            {
                _renderableForms = Page.ActiveForm.GetLinkedForms(OptimumPageWeight);
                Debug.Assert(_renderableForms != null, "_renderableForms is null");
            }
            else
            {
                _renderableForms = new ArrayList();
                _renderableForms.Add(Page.ActiveForm);
            }

            foreach (Form form in _renderableForms)
            {
                RenderForm(writer, form);
            }

            writer.RenderExtraCards();

            writer.WriteEndTag("wml");

            writer.EndFile();
            writer.EndResponse();
        }

        /// <include file='doc\WmlPageAdapter.uex' path='docs/doc[@for="WmlPageAdapter.RenderForm"]/*' />
        protected virtual void RenderForm(WmlMobileTextWriter writer, Form form)
        {
            writer.AnalyzeMode = true;
            form.RenderControl(writer);
            writer.AnalyzeMode = false;
            form.RenderControl(writer);
        }

        // Should be called by controls/adapters after _renderableForms is
        // set in this.Render()
        /// <include file='doc\WmlPageAdapter.uex' path='docs/doc[@for="WmlPageAdapter.IsFormRendered"]/*' />
        public virtual bool IsFormRendered(Form form)
        {
            Debug.Assert(_renderableForms != null, "_renderableForms is null");
            return _renderableForms.Contains(form);
        }

        private void RenderXmlHeader(HtmlTextWriter writer)
        {
            writer.Write(_headerBegin);
            String charset = Page.Response.Charset;
            if (charset != null && charset.Length > 0 && 
                String.Compare(charset, "utf-8", StringComparison.OrdinalIgnoreCase) != 0)
            {
                writer.Write(String.Format(CultureInfo.InvariantCulture, _headerEncoding, charset));
            }
            writer.Write(_headerEnd);
        }

        /// <include file='doc\WmlPageAdapter.uex' path='docs/doc[@for="WmlPageAdapter.RendersMultipleForms"]/*' />
        public virtual bool RendersMultipleForms()
        {
            return _renderableForms.Count > 1;
        }

        // '+' <-> '-'
        // '=' <-> '.'
        // '/' <-> '*'
        private static readonly char[] _specialEncodingChars = new char[64]
        {
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',  '/',  '-', '\0',  '+',  '=',  '*',
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',  '.', '\0', '\0',
        };

        internal String EncodeSpecialViewState(String pageState)
        {
            char[] viewstate = pageState.ToCharArray();

            for (int i = 0; i < viewstate.Length; i++)
            {
                char currentChar = viewstate[i];

                // Only check character replacement if within the range
                if (currentChar < _specialEncodingChars.Length)
                {
                    char encodingChar = _specialEncodingChars[currentChar];
                    if (encodingChar != '\0')
                    {
                        viewstate[i] = encodingChar;
                    }
                }
            }
            return new String(viewstate);
        }

        private static readonly Encoding UTF8Encoding = Encoding.GetEncoding("UTF-8");

        private bool _requiresUTF8ContentEncoding = false;
        private bool _haveRequiresUTF8ContentEncoding = false;

        private bool RequiresUTF8ContentEncoding()
        {
            if (!_haveRequiresUTF8ContentEncoding)
            {
                String RequiresUTF8ContentEncodingString = Device["requiresUTF8ContentEncoding"];
                if (RequiresUTF8ContentEncodingString == null)
                {
                    if (IsKDDIPhone())
                    {
                        _requiresUTF8ContentEncoding = true;
                    }
                }
                else
                {
                    _requiresUTF8ContentEncoding = Convert.ToBoolean(RequiresUTF8ContentEncodingString, CultureInfo.InvariantCulture);
                }
                _haveRequiresUTF8ContentEncoding = true;
            }
            return _requiresUTF8ContentEncoding;
        }

        private bool _requiresValueAttributeInInputTag = false;
        private bool _haveRequiresValueAttributeInInputTag = false;

        internal bool RequiresValueAttributeInInputTag()
        {
            if (!_haveRequiresValueAttributeInInputTag)
            {
                String RequiresValueAttributeInInputTagString = Device["requiresValueAttributeInInputTag"];
                if (RequiresValueAttributeInInputTagString == null)
                {
                    if (IsKDDIPhone())
                    {
                        _requiresValueAttributeInInputTag = true;
                    }
                }
                else
                {
                    _requiresValueAttributeInInputTag = Convert.ToBoolean(RequiresValueAttributeInInputTagString, CultureInfo.InvariantCulture);
                }
                _haveRequiresValueAttributeInInputTag = true;
            }
            return _requiresValueAttributeInInputTag;
        }

        private bool _isKDDIPhone = false;
        private bool _haveIsKDDIPhone = false;

        internal bool IsKDDIPhone()
        {
            if (!_haveIsKDDIPhone)
            {
                if (Device.Browser == "Phone.com" &&
                    Device.MajorVersion == 3 &&
                    Device.MinorVersion < 0.3)
                {
                    String charset = Page.Request.Headers["x-up-devcap-charset"];

                    if (charset != null &&
                        String.Compare(charset, "Shift_JIS", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        _isKDDIPhone = true;
                    }
                }
                _haveIsKDDIPhone = true;
            }
            return _isKDDIPhone;
        }
    }

}


















