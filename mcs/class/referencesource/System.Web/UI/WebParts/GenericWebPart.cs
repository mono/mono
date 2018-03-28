//------------------------------------------------------------------------------
// <copyright file="GenericWebPart.cs" company="Microsoft">
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

    /// <devdoc>
    /// A WebPart that can wrap any other generic server control, and provide it
    /// with "WebPart-ness."
    /// 1. Implements several properties if not set on the WebPart by looking for an
    /// attribute on the contained control.
    /// 2. Implement IWebEditable to allow the PropertyGridEditorPart to tunnel-in
    /// and browse the contained control.
    /// </devdoc>
    [
    ToolboxItem(false)
    ]
    public class GenericWebPart : WebPart {

        internal const string IDPrefix = "gwp";
        private Control _childControl;
        private IWebPart _childIWebPart;
        private string _subtitle;

        /// <devdoc>
        /// Intializes an instance of GenericWebPart with the control it is to wrap.
        /// </devdoc>
        protected internal GenericWebPart(Control control) {
            if (control == null) {
                throw new ArgumentNullException("control");
            }
            if (control is WebPart) {
                throw new ArgumentException(SR.GetString(SR.GenericWebPart_CannotWrapWebPart), "control");
            }
            if (control is BasePartialCachingControl) {
                throw new ArgumentException(SR.GetString(SR.GenericWebPart_CannotWrapOutputCachedControl), "control");
            }
            if (String.IsNullOrEmpty(control.ID)) {
                throw new ArgumentException(SR.GetString(SR.GenericWebPart_NoID, control.GetType().FullName));
            }

            ID = IDPrefix + control.ID;
            _childControl = control;
            _childIWebPart = _childControl as IWebPart;
            CopyChildAttributes();
        }

        public override string CatalogIconImageUrl {
            get {
                if (_childIWebPart != null) {
                    return _childIWebPart.CatalogIconImageUrl;
                }
                else {
                    return base.CatalogIconImageUrl;
                }
            }
            set {
                if (_childIWebPart != null) {
                    _childIWebPart.CatalogIconImageUrl = value;
                }
                else {
                    base.CatalogIconImageUrl = value;
                }
            }
        }

        public Control ChildControl {
            get {
                Debug.Assert(_childControl != null, "ChildControl cannot be null.");
                return _childControl;
            }
        }

        public override string Description {
            get {
                if (_childIWebPart != null) {
                    return _childIWebPart.Description;
                }
                else {
                    return base.Description;
                }
            }
            set {
                if (_childIWebPart != null) {
                    _childIWebPart.Description = value;
                }
                else {
                    base.Description = value;
                }
            }
        }

        public override Unit Height {
            get {
                WebControl c = ChildControl as WebControl;
                if (c != null) {
                    return c.Height;
                }
                else {
                    return base.Height;
                }
            }
            set {
                WebControl c = ChildControl as WebControl;
                if (c != null) {
                    c.Height = value;
                }
                else {
                    base.Height = value;
                }
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

        public override string Subtitle {
            get {
                if (_childIWebPart != null) {
                    return _childIWebPart.Subtitle;
                }
                else {
                    return (_subtitle != null ? _subtitle : String.Empty);
                }
            }
        }

        public override string Title {
            get {
                if (_childIWebPart != null) {
                    return _childIWebPart.Title;
                }
                else {
                    return base.Title;
                }
            }
            set {
                if (_childIWebPart != null) {
                    _childIWebPart.Title = value;
                }
                else {
                    base.Title = value;
                }
            }
        }

        public override string TitleIconImageUrl {
            get {
                if (_childIWebPart != null) {
                    return _childIWebPart.TitleIconImageUrl;
                }
                else {
                    return base.TitleIconImageUrl;
                }
            }
            set {
                if (_childIWebPart != null) {
                    _childIWebPart.TitleIconImageUrl = value;
                }
                else {
                    base.TitleIconImageUrl = value;
                }
            }
        }

        public override string TitleUrl {
            get {
                if (_childIWebPart != null) {
                    return _childIWebPart.TitleUrl;
                }
                else {
                    return base.TitleUrl;
                }
            }
            set {
                if (_childIWebPart != null) {
                    _childIWebPart.TitleUrl = value;
                }
                else {
                    base.TitleUrl = value;
                }
            }
        }

        public override WebPartVerbCollection Verbs {
            get {
                if (ChildControl != null) {
                    IWebActionable webActionableChildControl = ChildControl as IWebActionable;
                    if (webActionableChildControl != null) {
                        return new WebPartVerbCollection(base.Verbs, webActionableChildControl.Verbs);
                    }
                }
                return base.Verbs;
            }
        }

        public override object WebBrowsableObject {
            get {
                IWebEditable webEditableChildControl = ChildControl as IWebEditable;
                if (webEditableChildControl != null) {
                    return webEditableChildControl.WebBrowsableObject;
                }
                else {
                    return ChildControl;
                }
            }
        }

        public override Unit Width {
            get {
                WebControl c = ChildControl as WebControl;
                if (c != null) {
                    return c.Width;
                }
                else {
                    return base.Width;
                }
            }
            set {
                WebControl c = ChildControl as WebControl;
                if (c != null) {
                    c.Width = value;
                }
                else {
                    base.Width = value;
                }
            }
        }

        private void CopyChildAttributes() {
            // Copy the attribute values from the ChildControl to the GenericWebPart properties.
            IAttributeAccessor childAttributeAccessor = ChildControl as IAttributeAccessor;
            if (childAttributeAccessor != null) {
                base.AuthorizationFilter = childAttributeAccessor.GetAttribute("AuthorizationFilter");
                base.CatalogIconImageUrl = childAttributeAccessor.GetAttribute("CatalogIconImageUrl");
                base.Description = childAttributeAccessor.GetAttribute("Description");

                string exportMode = childAttributeAccessor.GetAttribute("ExportMode");
                if (exportMode != null) {
                    base.ExportMode = (WebPartExportMode)(Util.GetEnumAttribute(
                        "ExportMode", exportMode, typeof(WebPartExportMode)));
                }

                // Don't need to check base.Subtitle, since we always want to use the Subtitle on the
                // ChildControl if it is present.  Also, the property is not settable on WebPart, so we
                // know that base.Subtitle will always be String.Empty.
                _subtitle = childAttributeAccessor.GetAttribute("Subtitle");

                base.Title = childAttributeAccessor.GetAttribute("Title");
                base.TitleIconImageUrl = childAttributeAccessor.GetAttribute("TitleIconImageUrl");
                base.TitleUrl = childAttributeAccessor.GetAttribute("TitleUrl");
            }

            // Remove all the attributes from the ChildControl, whether or not they were copied
            // to the GenericWebPart property.  We want to remove the attributes so they are not
            // rendered on the ChildControl.  (VSWhidbey 313674)
            WebControl childWebControl = ChildControl as WebControl;
            if (childWebControl != null) {
                // If the ChildControl is a WebControl, we want to completely remove the attributes.
                childWebControl.Attributes.Remove("AuthorizationFilter");
                childWebControl.Attributes.Remove("CatalogIconImageUrl");
                childWebControl.Attributes.Remove("Description");
                childWebControl.Attributes.Remove("ExportMode");
                childWebControl.Attributes.Remove("Subtitle");
                childWebControl.Attributes.Remove("Title");
                childWebControl.Attributes.Remove("TitleIconImageUrl");
                childWebControl.Attributes.Remove("TitleUrl");
            }
            else if (childAttributeAccessor != null) {
                // If the ChildControl is not a WebControl, we cannot remove the attributes, so we set
                // them to null instead.
                childAttributeAccessor.SetAttribute("AuthorizationFilter", null);
                childAttributeAccessor.SetAttribute("CatalogIconImageUrl", null);
                childAttributeAccessor.SetAttribute("Description", null);
                childAttributeAccessor.SetAttribute("ExportMode", null);
                childAttributeAccessor.SetAttribute("Subtitle", null);
                childAttributeAccessor.SetAttribute("Title", null);
                childAttributeAccessor.SetAttribute("TitleIconImageUrl", null);
                childAttributeAccessor.SetAttribute("TitleUrl", null);
            }
        }

        protected internal override void CreateChildControls() {
            ((GenericWebPartControlCollection)Controls).AddGenericControl(ChildControl);
        }

        protected override ControlCollection CreateControlCollection() {
            return new GenericWebPartControlCollection(this);
        }

        public override EditorPartCollection CreateEditorParts() {
            IWebEditable webEditableChildControl = ChildControl as IWebEditable;
            if (webEditableChildControl != null) {
                return new EditorPartCollection(base.CreateEditorParts(), webEditableChildControl.CreateEditorParts());
            }
            else {
                return base.CreateEditorParts();
            }
        }

        protected internal override void Render(HtmlTextWriter writer) {
            // Copied from CompositeControl.Render()
            if (DesignMode) {
                EnsureChildControls();
            }

            RenderContents(writer);
        }

        private sealed class GenericWebPartControlCollection : ControlCollection {
            public GenericWebPartControlCollection(GenericWebPart owner) : base(owner) {
                SetCollectionReadOnly(SR.GenericWebPart_CannotModify);
            }

            /// <devdoc>
            /// Allows adding the generic control to be wrapped.
            /// </devdoc>
            public void AddGenericControl(Control control) {
                string originalError = SetCollectionReadOnly(null);
                // Extra try-catch block to prevent elevation of privilege attack via exception filter
                try {
                    try {
                        Clear();
                        Add(control);
                    }
                    finally {
                        SetCollectionReadOnly(originalError);
                    }
                }
                catch {
                    throw;
                }
            }
        }
    }
}
