//------------------------------------------------------------------------------
// <copyright file="ErrorWebPart.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.Util;

    [
    ToolboxItem(false)
    ]
    public class ErrorWebPart : ProxyWebPart, ITrackingPersonalizable {

        private string _errorMessage;

        // No constructor that takes a WebPart, since we ony use the ErrorWebPart when the original
        // WebPart could not be instantiated.

        public ErrorWebPart(string originalID, string originalTypeName, string originalPath, string genericWebPartID) :
            base(originalID, originalTypeName, originalPath, genericWebPartID) {
        }

        public string ErrorMessage {
            get {
                return (_errorMessage != null) ? _errorMessage : String.Empty;
            }
            set {
                _errorMessage = value;
            }
        }

        protected override void AddAttributesToRender(HtmlTextWriter writer) {
            WebPartZoneBase zone = Zone;
            if (zone != null && !zone.ErrorStyle.IsEmpty) {
                zone.ErrorStyle.AddAttributesToRender(writer, this);
            }

            base.AddAttributesToRender(writer);
        }

        // Can be overridden by derived classes to set properties
        protected virtual void EndLoadPersonalization() {
            // We don't really need to set AllowEdit, since EditorPart.Display has
            // a special case for ErrorWebPart.  However, let's set it to false anyway
            // for consistency.
            AllowEdit = false;

            // We want to force the user to see the ErrorWebPart, and we don't want to allow
            // them to hide or minimize it.
            ChromeState = PartChromeState.Normal;
            Hidden = false;
            AllowHide = false;
            AllowMinimize = false;

            // There is no reason to allow exporting an ErrorWebPart.
            ExportMode = WebPartExportMode.None;

            // We never call IsAuthorized() on ErrorWebParts, so there is no reason for
            // AuthorizationFilter to be set.
            AuthorizationFilter = String.Empty;
        }

        protected internal override void RenderContents(HtmlTextWriter writer) {
            string errorMessage = ErrorMessage;
            if (!String.IsNullOrEmpty(errorMessage)) {
                writer.WriteEncodedText(SR.GetString(SR.ErrorWebPart_ErrorText, errorMessage));
            }
        }

        #region ITrackingPersonalizable implementation
        // It doesn't really matter what we return from this property, since this codepath will
        // never be reached for the ErrorWebPart.  However, we return true since we will never need
        // the framework to diff our properties.
        bool ITrackingPersonalizable.TracksChanges {
            get {
                return true;
            }
        }

        void ITrackingPersonalizable.BeginLoad() {
        }

        void ITrackingPersonalizable.BeginSave() {
        }

        void ITrackingPersonalizable.EndLoad() {
            EndLoadPersonalization();
        }

        void ITrackingPersonalizable.EndSave() {
        }
        #endregion
    }
}

