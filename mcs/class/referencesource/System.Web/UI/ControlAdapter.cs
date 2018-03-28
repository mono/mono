//How to set the _control

//------------------------------------------------------------------------------
// <copyright file="ControlAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Adapters {
    using System;
    using System.ComponentModel;
    using System.Security.Permissions;

    /* Defines the properties, methods, and events shared by all server control
     * adapters in the Web Forms page framework.
    */
    public abstract class ControlAdapter {
        private HttpBrowserCapabilities _browser = null;

        internal Control _control; //control associated with this adapter

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        protected Control Control {
            get {
                return _control;
            }
        }

        /* Indicates the page on which the associated control resides.
        */
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        protected Page Page {
            get {
                if(Control != null)
                    return Control.Page;
                return null;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        protected PageAdapter PageAdapter {
            get {
                if(Control != null && Control.Page != null)
                    return Control.Page.PageAdapter;
                return null;
            }
        }

        protected HttpBrowserCapabilities Browser {
            get {
                if (_browser == null) {
                    if (Page.RequestInternal != null) {
                        _browser = Page.RequestInternal.Browser;
                    }
                    else {
                        /* VSWhidbey 83667: In post-cache substitution, Page.Request 
                         * would not be available. Then we try to 
                         * use the more expensive way to access the current 
                         * context and get the request handle.
                         */
                        HttpContext context = HttpContext.Current;
                        if (context != null && context.Request != null) {
                            _browser = context.Request.Browser;
                        }
                    }
                }

                return _browser;
            }
        }

        protected internal virtual void OnInit(EventArgs e) {
            Control.OnInit(e);
        }

        protected internal virtual void OnLoad(EventArgs e) {
            Control.OnLoad(e);
        }

        protected internal virtual void OnPreRender(EventArgs e) {
            Control.OnPreRender(e);
        }

        protected internal virtual void Render(HtmlTextWriter writer) {
            //
            if(_control != null) {
                _control.Render(writer);
            }
        }

        protected virtual void RenderChildren(HtmlTextWriter writer) {
            if(_control != null) {
                _control.RenderChildren(writer);
            }
        }

        protected internal virtual void OnUnload(EventArgs e) {
            Control.OnUnload(e);
        }

        protected internal virtual void BeginRender(HtmlTextWriter writer) {
            writer.BeginRender();
        }

        protected internal virtual void CreateChildControls() {
            Control.CreateChildControls();
        }

        protected internal virtual void EndRender(HtmlTextWriter writer) {
            writer.EndRender();
        }

        protected internal virtual void LoadAdapterControlState(object state) {
        }
        
        protected internal virtual void LoadAdapterViewState(object state) {
        }

        protected internal virtual object SaveAdapterControlState() {
            return null;
        }
        
        protected internal virtual object SaveAdapterViewState() {
            return null;
        }
    }
}
