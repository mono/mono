//------------------------------------------------------------------------------
// <copyright file="HtmlControlAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Collections;
using System.Diagnostics;
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
     * HtmlControlAdapter base class contains html specific methods.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\HtmlControlAdapter.uex' path='docs/doc[@for="HtmlControlAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class HtmlControlAdapter : System.Web.UI.MobileControls.Adapters.ControlAdapter
    {
        /// <include file='doc\HtmlControlAdapter.uex' path='docs/doc[@for="HtmlControlAdapter.PageAdapter"]/*' />
        protected HtmlPageAdapter PageAdapter
        {
            get
            {
                return ((HtmlPageAdapter)Page.Adapter);
            }
        }

        /// <include file='doc\HtmlControlAdapter.uex' path='docs/doc[@for="HtmlControlAdapter.FormAdapter"]/*' />
        protected HtmlFormAdapter FormAdapter
        {
            get
            {
                return (HtmlFormAdapter)Control.Form.Adapter;
            }
        }

        /// <include file='doc\HtmlControlAdapter.uex' path='docs/doc[@for="HtmlControlAdapter.RequiresFormTag"]/*' />
        public virtual bool RequiresFormTag
        {
            get
            {
                return false;
            }
        }

        /// <include file='doc\HtmlControlAdapter.uex' path='docs/doc[@for="HtmlControlAdapter.Render"]/*' />
        public override void Render(HtmlTextWriter writer)
        {
            HtmlMobileTextWriter htmlWriter = (HtmlMobileTextWriter)writer;
            Render(htmlWriter);
        }

        /// <include file='doc\HtmlControlAdapter.uex' path='docs/doc[@for="HtmlControlAdapter.Render1"]/*' />
        public virtual void Render(HtmlMobileTextWriter writer)
        {
            RenderChildren(writer);
        }

        /// <include file='doc\HtmlControlAdapter.uex' path='docs/doc[@for="HtmlControlAdapter.RenderPostBackEventReference"]/*' />
        protected void RenderPostBackEventReference(HtmlMobileTextWriter writer, String argument)
        {
            PageAdapter.RenderPostBackEvent(writer, Control.UniqueID, argument);
        }

        /// <include file='doc\HtmlControlAdapter.uex' path='docs/doc[@for="HtmlControlAdapter.RenderPostBackEventAsAttribute"]/*' />
        protected void RenderPostBackEventAsAttribute(
            HtmlMobileTextWriter writer, 
            String attributeName, 
            String argument)
        {
            writer.Write(" ");
            writer.Write(attributeName);
            writer.Write("=\"");
            RenderPostBackEventReference(writer, argument);
            writer.Write("\" ");
        }

        /// <include file='doc\HtmlControlAdapter.uex' path='docs/doc[@for="HtmlControlAdapter.RenderPostBackEventAsAnchor"]/*' />
        protected void RenderPostBackEventAsAnchor(
            HtmlMobileTextWriter writer,
            String argument,
            String linkText)
        {
            writer.EnterStyle(Style);
            writer.WriteBeginTag("a");
            RenderPostBackEventAsAttribute(writer, "href", argument);
            writer.Write(">");
            writer.WriteText(linkText, true);
            writer.WriteEndTag("a");
            writer.ExitStyle(Style);
        }

        /// <include file='doc\HtmlControlAdapter.uex' path='docs/doc[@for="HtmlControlAdapter.RenderBeginLink"]/*' />
        protected void RenderBeginLink(HtmlMobileTextWriter writer, String target)
        {
            bool queryStringWritten = false;
            bool appendCookieless = (PageAdapter.PersistCookielessData)  && 
                    (!( (target.StartsWith("http:", StringComparison.Ordinal)) || (target.StartsWith("https:", StringComparison.Ordinal)) ));
            writer.WriteBeginTag("a");
            writer.Write(" href=\"");

            String targetUrl = null;
            String prefix = Constants.FormIDPrefix;
            if (target.StartsWith(prefix, StringComparison.Ordinal))
            {
                String name = target.Substring(prefix.Length);
                Form form = Control.ResolveFormReference(name);

                if (writer.SupportsMultiPart)
                {
                    if (form != null && PageAdapter.IsFormRendered(form))
                    {
                        targetUrl = PageAdapter.GetFormUrl(form);
                    }
                }
                
                if (targetUrl == null)
                {
                    RenderPostBackEventReference(writer, form.UniqueID);
                    appendCookieless = false;
                }
                else
                {
                    writer.Write(targetUrl);
                    queryStringWritten = targetUrl.IndexOf('?') != -1;
                }
            }
            else
            {
                MobileControl control = Control;

                // There is some adapter that Control is not set.  And we
                // don't do any url resolution then.  E.g. a page adapter
                if (control != null)
                {
                    // AUI 3652
                    target = control.ResolveUrl(target);
                }

                writer.Write(target);
                queryStringWritten = target.IndexOf('?') != -1;
            }

            IDictionary dictionary = PageAdapter.CookielessDataDictionary;
            if((dictionary != null) && (appendCookieless))
            {
                foreach(String name in dictionary.Keys)
                {
                    if(queryStringWritten)
                    {
                        writer.Write('&');
                    }
                    else
                    {
                        writer.Write('?');
                        queryStringWritten = true;
                    }
                    writer.Write(name);
                    writer.Write('=');
                    writer.Write(dictionary[name]);
                }
            }

            writer.Write("\"");
            AddAttributes(writer);
            writer.Write(">");
        }

        /// <include file='doc\HtmlControlAdapter.uex' path='docs/doc[@for="HtmlControlAdapter.RenderEndLink"]/*' />
        protected void RenderEndLink(HtmlMobileTextWriter writer)
        {
            writer.WriteEndTag("a");
        }

        // Can be used by adapter that allow its subclass to add more
        // specific attributes
        /// <include file='doc\HtmlControlAdapter.uex' path='docs/doc[@for="HtmlControlAdapter.AddAttributes"]/*' />
        protected virtual void AddAttributes(HtmlMobileTextWriter writer)
        {
        }

        // Can be used by adapter that adds the custom attribute "accesskey"
        /// <include file='doc\HtmlControlAdapter.uex' path='docs/doc[@for="HtmlControlAdapter.AddAccesskeyAttribute"]/*' />
        protected virtual void AddAccesskeyAttribute(HtmlMobileTextWriter writer)
        {
            if (Device.SupportsAccesskeyAttribute)
            {
                AddCustomAttribute(writer, "accesskey");
            }
        }

        // Can be used by adapter that adds custom attributes for
        // multi-media functionalities

        private readonly static String [] _multiMediaAttributes =
            { "src",
              "soundstart",
              "loop",
              "volume",
              "vibration",
              "viblength" };

        /// <include file='doc\HtmlControlAdapter.uex' path='docs/doc[@for="HtmlControlAdapter.AddJPhoneMultiMediaAttributes"]/*' />
        protected virtual void AddJPhoneMultiMediaAttributes(
            HtmlMobileTextWriter writer)
        {
            if (Device.SupportsJPhoneMultiMediaAttributes)
            {
                for (int i = 0; i < _multiMediaAttributes.Length; i++)
                {
                    AddCustomAttribute(writer, _multiMediaAttributes[i]);
                }
            }
        }

        private void AddCustomAttribute(HtmlMobileTextWriter writer,
                                        String attributeName)
        {
            String attributeValue = ((IAttributeAccessor)Control).GetAttribute(attributeName);
            if (!String.IsNullOrEmpty(attributeValue))
            {
                writer.WriteAttribute(attributeName, attributeValue);
            }
        }

        /// <include file='doc\HtmlControlAdapter.uex' path='docs/doc[@for="HtmlControlAdapter.RenderAsHiddenInputField"]/*' />
        protected virtual void RenderAsHiddenInputField(HtmlMobileTextWriter writer)
        {
        }

        // Renders hidden variables for IPostBackDataHandlers which are
        // not displayed due to pagination or secondary UI.
        internal void RenderOffPageVariables(HtmlMobileTextWriter writer, Control ctl, int page)
        {
            if (ctl.HasControls())
            {
                foreach (Control child in ctl.Controls)
                {
                    // Note: Control.Form != null.
                    if (!child.Visible || child == Control.Form.Header || child == Control.Form.Footer)
                    {
                        continue;
                    }

                    MobileControl mobileCtl = child as MobileControl;

                    if (mobileCtl != null)
                    {
                        if (mobileCtl.IsVisibleOnPage(page)
                            && (mobileCtl == ((HtmlFormAdapter)mobileCtl.Form.Adapter).SecondaryUIControl ||
                            null == ((HtmlFormAdapter)mobileCtl.Form.Adapter).SecondaryUIControl))
                        {
                            if (mobileCtl.FirstPage == mobileCtl.LastPage)
                            {
                                // Entire control is visible on this page, so no need to look
                                // into children.
                                continue;
                            }

                            // Control takes up more than one page, so it may be possible that
                            // its children are on a different page, so we'll continue to
                            // fall through into children.
                        }
                        else if (mobileCtl is IPostBackDataHandler)
                        {
                            HtmlControlAdapter adapter = mobileCtl.Adapter as HtmlControlAdapter;
                            if (adapter != null)
                            {
                                adapter.RenderAsHiddenInputField(writer);
                            }
                        }
                    }

                    RenderOffPageVariables(writer, child, page);
                }
            }
        }

        /////////////////////////////////////////////////////////////////////
        //  SECONDARY UI SUPPORT
        /////////////////////////////////////////////////////////////////////

        internal const int NotSecondaryUIInit = -1;  // For initialization of private consts in derived classes.
        /// <include file='doc\HtmlControlAdapter.uex' path='docs/doc[@for="HtmlControlAdapter.NotSecondaryUI"]/*' />
        protected static readonly int NotSecondaryUI = NotSecondaryUIInit;

        /// <include file='doc\HtmlControlAdapter.uex' path='docs/doc[@for="HtmlControlAdapter.SecondaryUIMode"]/*' />
        protected int SecondaryUIMode
        {
            get
            {
                if (Control == null || Control.Form == null) 
                {
                    return NotSecondaryUI;
                }
                else
                {
                    return ((HtmlFormAdapter)Control.Form.Adapter).GetSecondaryUIMode(Control);
                }
            }
            set
            {
                ((HtmlFormAdapter)Control.Form.Adapter).SetSecondaryUIMode(Control, value);
            }
        }

        /// <include file='doc\HtmlControlAdapter.uex' path='docs/doc[@for="HtmlControlAdapter.ExitSecondaryUIMode"]/*' />
        protected void ExitSecondaryUIMode()
        {
            SecondaryUIMode = NotSecondaryUI;
        }

        /// <include file='doc\HtmlControlAdapter.uex' path='docs/doc[@for="HtmlControlAdapter.LoadAdapterState"]/*' />
        public override void LoadAdapterState(Object state)
        {
            if (state != null)
            {
                SecondaryUIMode = (int)state;
            }
        }

        /// <include file='doc\HtmlControlAdapter.uex' path='docs/doc[@for="HtmlControlAdapter.SaveAdapterState"]/*' />
        public override Object SaveAdapterState()
        {
            int mode = SecondaryUIMode;
            if (mode != NotSecondaryUI) 
            {
                return mode;
            }
            else
            {
                return null;
            }
        }
    }

}
