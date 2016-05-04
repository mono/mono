//------------------------------------------------------------------------------
// <copyright file="WmlFormAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Web.UI.MobileControls;
using System.Web.UI.MobileControls.Adapters;
using System.Web.Security;
using System.Security.Permissions;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource
#else
namespace System.Web.UI.MobileControls.Adapters
#endif    

{

    /*
     * WmlFormAdapter base class contains wml specific methods.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\WmlFormAdapter.uex' path='docs/doc[@for="WmlFormAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class WmlFormAdapter : WmlControlAdapter
    {
        private IDictionary _postBackVariables = null;

        /// <include file='doc\WmlFormAdapter.uex' path='docs/doc[@for="WmlFormAdapter.Control"]/*' />
        protected new Form Control
        {
            get
            {
                return (Form)base.Control;
            }
        }

        /// <include file='doc\WmlFormAdapter.uex' path='docs/doc[@for="WmlFormAdapter.Render"]/*' />
        public override void Render(WmlMobileTextWriter writer)
        {
            String formsAuthCookieName = FormsAuthentication.FormsCookieName;
            if(!Device.SupportsRedirectWithCookie)
            {
                if(!String.IsNullOrEmpty(formsAuthCookieName))
                {
                    HttpContext.Current.Response.Cookies.Remove(formsAuthCookieName);
                }
            }

            writer.BeginForm(Control);
            if (Page.Adapter.PersistCookielessData &&
                Device.CanRenderOneventAndPrevElementsTogether &&
                !String.IsNullOrEmpty(formsAuthCookieName) &&
                Control == Page.ActiveForm )
            {
                IDictionary dictionary = PageAdapter.CookielessDataDictionary;
                if(dictionary != null)
                {
                    String value = (String)dictionary[formsAuthCookieName];
                    if(!String.IsNullOrEmpty(value))
                    {
                        writer.AddFormVariable("__facn", value, false);
                    }
                }
            }
            MobileControl secondaryUIControl = SecondaryUIControl as MobileControl;
            writer.EnterLayout(Style);
            if (secondaryUIControl != null && secondaryUIControl.Form == Control)
            {
                SetControlPageRecursive(secondaryUIControl, -1);
                secondaryUIControl.RenderControl(writer);
            }
            else
            {
                if (Control.HasControls())
                {
                    Panel header = Control.Header;
                    Panel footer = Control.Footer;

                    if (header != null)
                    {
                        writer.BeginCustomMarkup();
                        header.RenderControl(writer);
                        writer.EndCustomMarkup();
                    }

                    foreach(Control control in Control.Controls)
                    {
                        if (control != header && control != footer)
                        {
                            control.RenderControl(writer);
                        }
                    }

                    RenderPager(writer);

                    if (footer != null)
                    {
                        writer.BeginCustomMarkup();
                        footer.RenderControl(writer);
                        writer.EndCustomMarkup();
                    }
                }
                else
                {
                    RenderPager(writer);
                }
            }
            writer.ExitLayout(Style);
            writer.EndForm();
        }

        /// <include file='doc\WmlFormAdapter.uex' path='docs/doc[@for="WmlFormAdapter.RenderPager"]/*' />
        protected virtual void RenderPager(WmlMobileTextWriter writer)
        {
            PagerStyle pagerStyle = Control.PagerStyle;
            int pageCount = Control.PageCount;
            if (pageCount <= 1)
            {
                return;
            }
            int page = Control.CurrentPage;

            writer.EnterStyle(pagerStyle);
            if (page < pageCount)
            {
                String nextPageText = pagerStyle.GetNextPageText(page);
                RenderPostBackEvent(writer, 
                                    (page + 1).ToString(CultureInfo.InvariantCulture), 
                                    writer.IsValidSoftkeyLabel(nextPageText) ? nextPageText 
                                                                             : GetDefaultLabel(NextLabel),
                                    true,
                                    nextPageText, 
                                    true);
            }

            if (page > 1)
            {
                String prevPageText = pagerStyle.GetPreviousPageText(page);
                RenderPostBackEvent(writer, 
                                    (page - 1).ToString(CultureInfo.InvariantCulture), 
                                    writer.IsValidSoftkeyLabel(prevPageText) ? prevPageText
                                                                             : GetDefaultLabel(PreviousLabel),
                                    true,
                                    prevPageText, 
                                    true);
            }
            writer.ExitStyle(pagerStyle);
        }

        /// <include file='doc\WmlFormAdapter.uex' path='docs/doc[@for="WmlFormAdapter.CalculatePostBackVariables"]/*' />
        public virtual IDictionary CalculatePostBackVariables()
        {
            if (_postBackVariables == null)
            {
                _postBackVariables = new ListDictionary();
                BuildControlPostBacksRecursive(Control);
            }
            return _postBackVariables;
        }

        private void BuildControlPostBacksRecursive(Control control)
        {
            if (control is IPostBackDataHandler
                && !(control is IPostBackEventHandler)
                && control.Visible && control != Control)
            {
                MobileControl mobileCtl = control as MobileControl;

                if (mobileCtl != null && !mobileCtl.IsVisibleOnPage(Control.CurrentPage))
                {
                    String s = GetControlPostBackValue(mobileCtl);
                    if (s != null)
                    {
                        _postBackVariables[control] = s;
                    }
                }
                else
                {
                    _postBackVariables[control] = null;
                }
            }

            if (control.HasControls())
            {
                foreach (Control child in control.Controls)
                {
                    BuildControlPostBacksRecursive(child);
                }
            }
        }

        /// <include file='doc\WmlFormAdapter.uex' path='docs/doc[@for="WmlFormAdapter.HandlePostBackEvent"]/*' />
        public override bool HandlePostBackEvent(String eventArgument)
        {
            String eventSource = eventArgument;
            int comma = eventSource.IndexOf(',');
            if (comma == -1)
            {
                eventArgument = null;
            }
            else
            {
                eventArgument = eventSource.Substring(comma + 1);
                eventSource = eventSource.Substring(0, comma);
            }

            if (eventSource.Length > 0)
            {
                Control sourceControl = Page.FindControl(eventSource);
                if (sourceControl != null && sourceControl is IPostBackEventHandler)
                {
                    ((IPostBackEventHandler)sourceControl).RaisePostBackEvent(eventArgument);
                }
            }

            return true;
        }

        /// <include file='doc\WmlFormAdapter.uex' path='docs/doc[@for="WmlFormAdapter.RenderExtraCardElements"]/*' />
        protected internal virtual void RenderExtraCardElements(WmlMobileTextWriter writer)
        {
            Form form = this.Control as Form;
            if((form != null) && (form.Script != null))
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
            }
         }

        /// <include file='doc\WmlFormAdapter.uex' path='docs/doc[@for="WmlFormAdapter.RenderCardTag"]/*' />
        protected internal virtual void RenderCardTag(WmlMobileTextWriter writer, IDictionary attributes)
        {
            writer.WriteBeginTag("card");
            if (attributes != null)
            {
                foreach (DictionaryEntry entry in attributes)
                {
                    writer.WriteAttribute((String)entry.Key, (String)entry.Value, true);
                }
            }
            writer.WriteLine(">");
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

        //identical to method in htmlformadapter
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
