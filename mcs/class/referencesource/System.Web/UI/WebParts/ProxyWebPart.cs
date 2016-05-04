//------------------------------------------------------------------------------
// <copyright file="ProxyWebPart.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.Util;

    [
    ToolboxItem(false)
    ]
    public abstract class ProxyWebPart : WebPart {

        private string _originalID;
        private string _originalTypeName;
        private string _originalPath;
        private string _genericWebPartID;

        protected ProxyWebPart(WebPart webPart) {
            if (webPart == null) {
                throw new ArgumentNullException("webPart");
            }

            GenericWebPart genericWebPart = webPart as GenericWebPart;
            if (genericWebPart != null) {
                Control childControl = genericWebPart.ChildControl;
                if (childControl == null) {
                    throw new ArgumentException(SR.GetString(SR.PropertyCannotBeNull, "ChildControl"), "webPart");
                }

                _originalID = childControl.ID;
                if (String.IsNullOrEmpty(_originalID)) {
                    throw new ArgumentException(SR.GetString(SR.PropertyCannotBeNullOrEmptyString, "ChildControl.ID"), "webPart");
                }

                Type originalType;
                UserControl childUserControl = childControl as UserControl;
                if (childUserControl != null) {
                    originalType = typeof(UserControl);
                    _originalPath = childUserControl.AppRelativeVirtualPath;
                }
                else {
                    originalType = childControl.GetType();
                }
                _originalTypeName = WebPartUtil.SerializeType(originalType);
                _genericWebPartID = genericWebPart.ID;
                if (String.IsNullOrEmpty(_genericWebPartID)) {
                    throw new ArgumentException(SR.GetString(SR.PropertyCannotBeNullOrEmptyString, "ID"), "webPart");
                }
                ID = _genericWebPartID;
            }
            else {
                _originalID = webPart.ID;
                if (String.IsNullOrEmpty(_originalID)) {
                    throw new ArgumentException(SR.GetString(SR.PropertyCannotBeNullOrEmptyString, "ID"), "webPart");
                }
                _originalTypeName = WebPartUtil.SerializeType(webPart.GetType());
                ID = _originalID;
            }
        }

        protected ProxyWebPart(string originalID, string originalTypeName, string originalPath, string genericWebPartID) {
            if (String.IsNullOrEmpty(originalID)) {
                throw ExceptionUtil.ParameterNullOrEmpty("originalID");
            }
            if (String.IsNullOrEmpty(originalTypeName)) {
                throw ExceptionUtil.ParameterNullOrEmpty("originalTypeName");
            }

            // If you specify a path, this must be a GenericWebPart so genericWebPartID should not be null
            if (!String.IsNullOrEmpty(originalPath) && String.IsNullOrEmpty(genericWebPartID)) {
                throw ExceptionUtil.ParameterNullOrEmpty("genericWebPartID");
            }

            _originalID = originalID;
            _originalTypeName = originalTypeName;
            _originalPath = originalPath;
            _genericWebPartID = genericWebPartID;
            if (!String.IsNullOrEmpty(genericWebPartID)) {
                ID = _genericWebPartID;
            }
            else {
                ID = _originalID;
            }
        }

        public string GenericWebPartID {
            get {
                return (_genericWebPartID != null) ? _genericWebPartID : String.Empty;
            }
        }

        // Seal the ID property so we can set it in the constructor without an FxCop violation.
        public sealed override string ID {
            get {
                return base.ID;
            }
            set {
                base.ID = value;
            }
        }

        public string OriginalID {
            get {
                return (_originalID != null) ? _originalID : String.Empty;
            }
        }

        public string OriginalTypeName {
            get {
                return (_originalTypeName != null) ? _originalTypeName : String.Empty;
            }
        }

        public string OriginalPath {
            get {
                return (_originalPath != null) ? _originalPath : String.Empty;
            }
        }

        // Accept any ControlState, but do nothing with it, since it is actually the ControlState
        // for the WebPart we are replacing.
        protected internal override void LoadControlState(object savedState) {
        }

        // Accept any ViewState, but do nothing with it, since it is actually the ViewState
        // for the WebPart we are replacing.
        protected override void LoadViewState(object savedState) {
        }

        // Do not save any ControlState, since the ProxyWebPart itself does not need to save state
        // between requests.
        protected internal override object SaveControlState() {
            // Call base in case it has some side-effects that the control relies on
            base.SaveControlState();
            return null;
        }

        // Do not save any ViewState, since the ProxyWebPart itself does not need to save state
        // between requests.
        protected override object SaveViewState() {
            // Call base in case it has some side-effects that the control relies on
            base.SaveViewState();
            return null;
        }

    }
}

