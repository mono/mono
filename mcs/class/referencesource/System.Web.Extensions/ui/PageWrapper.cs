//------------------------------------------------------------------------------
// <copyright file="PageWrapper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.UI {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web;

    internal sealed class PageWrapper : IPage {
        private readonly Page _page;

        public PageWrapper(Page page) {
            Debug.Assert(page != null);
            _page = page;
        }

        #region IPage Members
        string IPage.AppRelativeVirtualPath {
            get {
                return _page.AppRelativeVirtualPath;
            }
        }

        IDictionary<String, String> IPage.HiddenFieldsToRender {
            get {
                return _page._hiddenFieldsToRender;
            }
        }

        IClientScriptManager IPage.ClientScript {
            get {
                return new ClientScriptManagerWrapper(_page.ClientScript);
            }
        }

        bool IPage.EnableEventValidation {
            get {
                return _page.EnableEventValidation;
            }
        }

        IHtmlForm IPage.Form {
            get {
                if (_page.Form != null) {
                    return new HtmlFormWrapper(_page.Form);
                }
                return null;
            }
        }

        HtmlHead IPage.Header {
            get {
                return _page.Header;
            }
        }

        bool IPage.IsPostBack {
            get {
                return _page.IsPostBack;
            }
        }

        bool IPage.IsValid {
            get {
                return _page.IsValid;
            }
        }

        IDictionary IPage.Items {
            get {
                return _page.Items;
            }
        }

        HttpRequestBase IPage.Request {
            get {
                return new HttpRequestWrapper(_page.Request);
            }
        }

        HttpResponseInternalBase IPage.Response {
            get {
                return new HttpResponseInternalWrapper(_page.Response);
            }
        }

        HttpServerUtilityBase IPage.Server {
            get {
                return new HttpServerUtilityWrapper(_page.Server);
            }
        }

        string IPage.Title {
            get {
                return _page.Title;
            }
            set {
                _page.Title = value;
            }
        }

        event EventHandler IPage.Error {
            add {
                _page.Error += value;
            }
            remove {
                _page.Error -= value;
            }
        }

        event EventHandler IPage.InitComplete {
            add {
                _page.InitComplete += value;
            }
            remove {
                _page.InitComplete -= value;
            }
        }

        event EventHandler IPage.LoadComplete {
            add {
                _page.LoadComplete += value;
            }
            remove {
                _page.LoadComplete -= value;
            }
        }

        void IPage.RegisterRequiresViewStateEncryption() {
            _page.RegisterRequiresViewStateEncryption();
        }

        void IPage.SetFocus(Control control) {
            _page.SetFocus(control);
        }

        void IPage.SetFocus(string clientID) {
            _page.SetFocus(clientID);
        }

        event EventHandler IPage.PreRender {
            add {
                _page.PreRender += value;
            }
            remove {
                _page.PreRender -= value;
            }
        }

        event EventHandler IPage.PreRenderComplete {
            add {
                _page.PreRenderComplete += value;
            }
            remove {
                _page.PreRenderComplete -= value;
            }
        }

        void IPage.SetPostFormRenderDelegate(RenderMethod renderMethod) {
            _page.SetPostFormRenderDelegate(renderMethod);
        }        

        void IPage.SetRenderMethodDelegate(RenderMethod renderMethod) {
            _page.SetRenderMethodDelegate(renderMethod);
        }

        void IPage.Validate(string validationGroup) {
            _page.Validate(validationGroup);
        }

        void IPage.VerifyRenderingInServerForm(Control control) {
            _page.VerifyRenderingInServerForm(control);
        }
        #endregion
    }
}
