//------------------------------------------------------------------------------
// <copyright file="HtmlFormAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.MobileControls;
using System.Web.UI.MobileControls.Adapters;
using System.Drawing;
using System.Security.Permissions;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource
#else
namespace System.Web.UI.MobileControls.Adapters
#endif

{

    /*
     * HtmlFormAdapter class.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\HtmlFormAdapter.uex' path='docs/doc[@for="HtmlFormAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class HtmlFormAdapter : HtmlControlAdapter
    {
        private static readonly String _contentTypeMetaTag = "<meta http-equiv=\"Content-Type\" content=\"{0}; charset={1}\">\r\n";

        /// <include file='doc\HtmlFormAdapter.uex' path='docs/doc[@for="HtmlFormAdapter.Control"]/*' />
        protected new Form Control
        {
            get
            {
                return (Form)base.Control;
            }
        }

        /// <include file='doc\HtmlFormAdapter.uex' path='docs/doc[@for="HtmlFormAdapter.Render"]/*' />
        public override void Render(HtmlMobileTextWriter writer)
        {
            Color backColor = (Color)Style[Style.BackColorKey, true];
            String title = Control.Title;
            bool isTitleEmpty = String.IsNullOrEmpty(title);

            bool willWriteHeadElements =
                !isTitleEmpty ||
                RenderExtraHeadElements(null);

            if (willWriteHeadElements)
            {
                writer.Write("\r\n");
                writer.WriteFullBeginTag("head");
            }

            if (!isTitleEmpty)
            {
                writer.Write("\r\n");
                writer.WriteFullBeginTag("title");
                writer.Write(title);
                writer.WriteEndTag("title");
                writer.Write("\r\n");
            }

            _renderPager = true;

            RenderExtraHeadElements(writer);

            if (willWriteHeadElements)
            {
                writer.WriteEndTag("head");
                writer.Write("\r\n");
            }

            IDictionary bodyAttributes = new ListDictionary();
            if ((backColor != Color.Empty) && (writer.RenderBodyColor))
            {
                bodyAttributes.Add("bgcolor", ColorTranslator.ToHtml(backColor));
            }
            RenderBodyTag(writer, bodyAttributes);

            bool formTagRequired = ShouldRenderFormTag();
            if (formTagRequired)
            {
                writer.WriteBeginTag("form");
                writer.WriteAttribute("id", Control.ClientID);
                writer.WriteAttribute("name", Control.ClientID);
                writer.WriteAttribute("method", Control.Method.ToString().ToLower(CultureInfo.InvariantCulture));
                writer.Write(" action=\"");

                if (Control.Action.Length > 0)
                {
                    // AUI 3652
                    String url = Control.ResolveUrl(Control.Action);

                    if (!Device.SupportsQueryStringInFormAction)
                    {
                        // If query string is not supported, we don't write
                        // it here, and the query string will be added as
                        // hidden variables later.
                        int i = url.IndexOf('?');
                        if (i != -1)
                        {
                            url = url.Substring(0, i);
                        }
                    }

                    writer.Write(url);
                }
                else
                {
                    writer.WriteEncodedUrl(Page.RelativeFilePath);

                    if (Device.SupportsQueryStringInFormAction)
                    {
                        writer.Write("?");
                        writer.Write(Page.UniqueFilePathSuffix);
                        if (Control.Method != FormMethod.Get)
                        {
                            String queryStringText = Page.QueryStringText;
                            if (queryStringText != null && queryStringText.Length > 0)
                            {
                                writer.Write('&');
                                writer.Write(queryStringText);
                            }
                        }
                    }
                }

                writer.Write("\"");
                writer.Write(">\r\n");

                PageAdapter.RenderPostBackHeader(writer, Control);

                // Renders hidden variables for IPostBackDataHandlers which are
                // not displayed due to pagination or secondary UI.
                RenderOffPageVariables(writer, Control, Control.CurrentPage);
            }

            writer.EnterStyle(Style);

            writer.BeforeFirstControlWritten = true;

            MobileControl secondaryUIControl = SecondaryUIControl as MobileControl;

            if (secondaryUIControl != null && secondaryUIControl.Form == Control)
            {
                bool secondaryUIInHeaderOrFooter = IsControlInFormHeader(secondaryUIControl) 
                    || IsControlInFormFooter(secondaryUIControl);


                SetControlPageRecursive(secondaryUIControl, -1);
                if(Control.Header != null && !secondaryUIInHeaderOrFooter)
                {
                     Control.Header.RenderControl(writer);
                }
                secondaryUIControl.RenderControl(writer);
                if(Control.Footer != null && !secondaryUIInHeaderOrFooter)
                {
                    Control.Footer.RenderControl(writer);
                }
            }
            else
            {

                bool pagerRendered = false;
                if(Control.HasControls())
                {
                    foreach(Control child in Control.Controls)
                    {
                        if(Control.Footer == child)
                        {
                            RenderPager(writer);
                            pagerRendered = true;
                        }
                        child.RenderControl(writer);
                    }
                }
                if(!pagerRendered)
                {
                    RenderPager(writer);
                }
            }

            writer.ExitStyle(Style, false);

            if (formTagRequired)
            {
                if (!Device.SupportsQueryStringInFormAction)
                {
                    // Add query string parameters at the end of the form if
                    // there are any
                    RenderQueryParametersAsHiddenFields(writer);
                }
                writer.WriteEndTag("form");
            }
            writer.WriteEndTag("body");
        }

        // Return true if actually wrote any head elements.  If called with
        // null, this returns whether head elements would be written if called
        // with a real writer.
        /// <include file='doc\HtmlFormAdapter.uex' path='docs/doc[@for="HtmlFormAdapter.RenderExtraHeadElements"]/*' />
        protected virtual bool RenderExtraHeadElements(HtmlMobileTextWriter writer)
        {
            bool result = false;
            
            String metaTagName = Device.RequiredMetaTagNameValue;
            if (metaTagName != null)
            {
                if (writer != null)
                {
                    writer.Write("<meta NAME=\"" + metaTagName + "\" CONTENT=\"True\">\r\n");
                }
                result = true;
            }

            String charset = Page.Response.Charset;
            if (Device.RequiresContentTypeMetaTag &&
                charset != null && charset.Length > 0)
            {
                if (writer != null)
                {
                    writer.Write(String.Format(CultureInfo.InvariantCulture, _contentTypeMetaTag, Device.PreferredRenderingMime, charset));
                }
                result = true;
            }
            
            Form form = this.Control as Form;
            if(writer == null)
            {
                if((form != null) && (form.Script != null))
                {
                    result = true;
                }
            }
            else if ((form != null) && (form.Script != null))
            {
                 foreach(Control childControl in form.Script.Controls)
                 {
                     LiteralControl lc = childControl as LiteralControl;
                     if(lc != null)
                     {
                         writer.Write(lc.Text);
                     }
                     else
                     {
                         DataBoundLiteralControl dlc = childControl as DataBoundLiteralControl;
                         if(dlc != null)
                         {
                             writer.Write(dlc.Text);
                         }
                     }
                 }
                 result = true;
            }
            return result;
        }

        /// <include file='doc\HtmlFormAdapter.uex' path='docs/doc[@for="HtmlFormAdapter.RenderBodyTag"]/*' />
        protected virtual void RenderBodyTag(HtmlMobileTextWriter writer, IDictionary attributes)
        {
            writer.WriteBeginTag("body");
            foreach (DictionaryEntry entry in attributes)
            {
                writer.WriteAttribute((String)entry.Key, (String)entry.Value, true);
            }
            writer.WriteLine(">");
        }

        /// <include file='doc\HtmlFormAdapter.uex' path='docs/doc[@for="HtmlFormAdapter.ShouldRenderFormTag"]/*' />
        protected virtual bool ShouldRenderFormTag()
        {
            return true;
        }

        private bool _renderPager = true;

        /// <include file='doc\HtmlFormAdapter.uex' path='docs/doc[@for="HtmlFormAdapter.DisablePager"]/*' />
        protected internal void DisablePager()
        {
            _renderPager = false;
        }

        /// <include file='doc\HtmlFormAdapter.uex' path='docs/doc[@for="HtmlFormAdapter.RenderPager"]/*' />
        protected virtual void RenderPager(HtmlMobileTextWriter writer)
        {
            if(!_renderPager)
            {
                return;
            }
            PagerStyle pagerStyle = Control.PagerStyle;

            int pageCount = Control.PageCount;
            if (pageCount <= 1)
            {
                return;
            }
            int page = Control.CurrentPage;
            String text = pagerStyle.GetPageLabelText(page, pageCount);

            if((page > 1) || (text.Length > 0) || (page < pageCount))
            {
                writer.EnterStyle(pagerStyle);
            }

            if (page > 1)
            {
                RenderPagerTag(writer, page - 1,
                               pagerStyle.GetPreviousPageText(page));
                writer.Write(" ");
            }

            if (text.Length > 0)
            {
                writer.WriteEncodedText(text);
                writer.Write(" ");
            }

            if (page < pageCount)
            {
                RenderPagerTag(writer, page + 1,
                               pagerStyle.GetNextPageText(page));
            }

            if((page > 1) || (text.Length > 0) || (page < pageCount))
            {
                writer.ExitStyle(pagerStyle, true);
            }
        }

        /// <include file='doc\HtmlFormAdapter.uex' path='docs/doc[@for="HtmlFormAdapter.RenderPagerTag"]/*' />
        protected internal virtual void RenderPagerTag(
            HtmlMobileTextWriter writer,
            int pageToNavigate,
            String text)
        {
            RenderPostBackEventAsAnchor(writer, pageToNavigate.ToString(CultureInfo.InvariantCulture),
                                        text);
        }

        private void RenderQueryParametersAsHiddenFields(
            HtmlMobileTextWriter writer)
        {
            String action = Page.ActiveForm.Action;
            int indexOfQueryStringText = action.IndexOf('?');
            String queryString = Page.QueryStringText;

            if (indexOfQueryStringText != -1 ||
                queryString == null ||
                queryString.Length > 0 ||
                Control.Method == FormMethod.Get)
            {
                // We use __ufps as the delimiter in the collection.
                writer.WriteHiddenField(
                    Constants.UniqueFilePathSuffixVariable.Substring(
                        0, Constants.UniqueFilePathSuffixVariable.Length - 1),
                    String.Empty);

                // If there is some query string on the Action attribute,
                // it takes the precedence.
                if (indexOfQueryStringText != -1)
                {
                    queryString = action.Substring(indexOfQueryStringText + 1);
                }

                if (queryString != null && queryString.Length != 0)
                {
                    NameValueCollection collection =
                            ParseQueryStringIntoCollection(queryString);

                    for (int i = 0; i < collection.Count; i++)
                    {
                        writer.WriteHiddenField(collection.GetKey(i),
                                                collection.Get(i));
                    }
                }
            }
        }

        private NameValueCollection ParseQueryStringIntoCollection(
            String queryString)
        {
            Debug.Assert(queryString != null && queryString.Length > 0,
                         "queryString is null or empty");

            NameValueCollection collection = new NameValueCollection();

            int l = queryString.Length;
            int i = 0;

            while (i < l)
            {
                // find next & while noting first = on the way (and if there are more)
                int si = i;
                int ti = -1;

                while (i < l)
                {
                    char b = queryString[i];

                    if (b == '=')
                    {
                        if (ti < 0)
                        {
                            ti = i;
                        }
                    }
                    else if (b == '&')
                    {
                        break;
                    }

                    i++;
                }

                // extract the name / value pair
                String name, value;
                if (ti >= 0)
                {
                    name  = Page.Server.UrlDecode(queryString.Substring(si, ti-si));
                    value = Page.Server.UrlDecode(queryString.Substring(ti+1, i-ti-1));
                }
                else
                {
                    name = null;
                    value = Page.Server.UrlDecode(queryString.Substring(si, i-si));
                }

                // add name / value pair to the collection
                collection.Add(name, value);

                // trailing '&'
                if (i == l-1 && queryString[i] == '&')
                {
                    collection.Add(null, "");
                }

                i++;
            }

            return collection;
        }

        /////////////////////////////////////////////////////////////////////////
        //  SECONDARY UI SUPPORT
        /////////////////////////////////////////////////////////////////////////

        private Control _secondaryUIControl;
        private int _secondaryUIMode;

        internal int GetSecondaryUIMode(Control control)
        {
            return (control != null && _secondaryUIControl == control) ? _secondaryUIMode : NotSecondaryUI;
        }

        internal void SetSecondaryUIMode(Control control, int mode)
        {
            if (mode != NotSecondaryUI)
            {
                if (_secondaryUIControl != null && _secondaryUIControl != control)
                {
                    throw new Exception(
                        SR.GetString(SR.FormAdapterMultiControlsAttemptSecondaryUI));
                }
                _secondaryUIControl = control;
                _secondaryUIMode = mode;
                return;
            }

            if (control == _secondaryUIControl)
            {
                _secondaryUIControl = null;
            }
        }

        internal Control SecondaryUIControl
        {
            get
            {
                return _secondaryUIControl;
            }
        }

        //identical to method in wmlformadapter
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

        private bool IsControlInFormHeader(MobileControl control)
        {
            return IsAncestor(Control.Header, control);
        }

        private bool IsControlInFormFooter(MobileControl control)
        {
            return IsAncestor(Control.Footer, control);
        }

        private bool IsAncestor(MobileControl ancestor, MobileControl descendant)
        {
            for (Control i = descendant; i != null; i = i.Parent)
            {
                if (i == ancestor)
                {
                    return true;
                }
            }
            return false;
        }
    }
}

