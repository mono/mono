//------------------------------------------------------------------------------
// <copyright file="DeclarativeCatalogPart.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Web.Security;
    using System.Web.Util;

    /// <devdoc>
    /// </devdoc>
    [
    Designer("System.Web.UI.Design.WebControls.WebParts.DeclarativeCatalogPartDesigner, " + AssemblyRef.SystemDesign),
    ]
    public sealed class DeclarativeCatalogPart : CatalogPart {

        private ITemplate _webPartsTemplate;
        private WebPartDescriptionCollection _descriptions;
        private string _webPartsListUserControlPath;

        [
        WebSysDefaultValue(SR.DeclarativeCatalogPart_PartTitle),
        ]
        public override string Title {
            get {
                string s = (string)ViewState["Title"];
                return (s != null) ? s : SR.GetString(SR.DeclarativeCatalogPart_PartTitle);
            }
            set {
                ViewState["Title"] = value;
            }
        }

        [
        DefaultValue(""),
        Editor("System.Web.UI.Design.UserControlFileEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        Themeable(false),
        UrlProperty(),
        WebCategory("Behavior"),
        WebSysDescription(SR.DeclarativeCatlaogPart_WebPartsListUserControlPath),
        ]
        public string WebPartsListUserControlPath {
            get {
                return (_webPartsListUserControlPath != null) ? _webPartsListUserControlPath : String.Empty;
            }
            set {
                _webPartsListUserControlPath = value;
                // Reset the collection of available web parts so it will be recreated
                _descriptions = null;
            }
        }

        /// <devdoc>
        /// I don't know any reason this should be a single-instance template.
        /// </devdoc>
        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(DeclarativeCatalogPart)),
        // NOT single-instance template for now
        ]
        public ITemplate WebPartsTemplate {
            get {
                return _webPartsTemplate;
            }
            set {
                _webPartsTemplate = value;
                // Reset the collection of available web parts so it will be recreated
                _descriptions = null;
            }
        }

        private void AddControlToDescriptions(Control control, ArrayList descriptions) {
            WebPart webPart = control as WebPart;
            if ((webPart == null) && !(control is LiteralControl)) {
                // Fix for DesignMode
                if (WebPartManager != null) {
                    webPart = WebPartManager.CreateWebPart(control);
                }
                else {
                    webPart = WebPartManager.CreateWebPartStatic(control);
                }
            }

            // Fix for DesignMode
            if (webPart != null && (WebPartManager == null || WebPartManager.IsAuthorized(webPart))) {
                WebPartDescription description = new WebPartDescription(webPart);
                descriptions.Add(description);
            }
        }

        public override WebPartDescriptionCollection GetAvailableWebPartDescriptions() {
            if (_descriptions == null) {
                LoadAvailableWebParts();
            }
            return _descriptions;
        }

        public override WebPart GetWebPart(WebPartDescription description) {
            if (description == null) {
                throw new ArgumentNullException("description");
            }

            WebPartDescriptionCollection webPartDescriptions = GetAvailableWebPartDescriptions();
            if (!webPartDescriptions.Contains(description)) {
                throw new ArgumentException(SR.GetString(SR.CatalogPart_UnknownDescription), "description");
            }

            return description.WebPart;
        }

        private void LoadAvailableWebParts() {
            ArrayList descriptions = new ArrayList();

            if (WebPartsTemplate != null) {
                Control container = new NonParentingControl();
                WebPartsTemplate.InstantiateIn(container);
                if (container.HasControls()) {
                    // Copy container.Controls to a temporary array, since adding the control to the
                    // descriptions may cause it to be reparented to a GenericWebPart, which would
                    // modify the container.Controls collection.
                    Control[] controls = new Control[container.Controls.Count];
                    container.Controls.CopyTo(controls, 0);
                    foreach (Control control in controls) {
                        AddControlToDescriptions(control, descriptions);
                    }
                }
            }

            string webPartsListUserControlPath = WebPartsListUserControlPath;
            if (!String.IsNullOrEmpty(webPartsListUserControlPath) && !DesignMode) {
                // Page.LoadControl() throws a null ref exception at design-time
                Control userControl = Page.LoadControl(webPartsListUserControlPath);
                if (userControl != null && userControl.HasControls()) {
                    // Copy userControl.Controls to a temporary array, since adding the control to the
                    // descriptions may cause it to be reparented to a GenericWebPart, which would
                    // modify the userControl.Controls collection.
                    Control[] controls = new Control[userControl.Controls.Count];
                    userControl.Controls.CopyTo(controls, 0);
                    foreach (Control control in controls) {
                        AddControlToDescriptions(control, descriptions);
                    }
                }
            }

            _descriptions = new WebPartDescriptionCollection(descriptions);
        }


        // Override Render to render nothing by default, since the CatalogPartChrome renders the
        // AvailableWebParts.  A CatalogPart only needs to render something if it wants
        // additional rendering above the AvailableWebParts.
        protected internal override void Render(HtmlTextWriter writer) {
        }

        #region Overriden to hide in the designer (VSWhidbey 353577)
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Themeable(false)]
        public override string AccessKey {
            get { return base.AccessKey; }
            set { base.AccessKey = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Themeable(false)]
        public override Color BackColor {
            get { return base.BackColor; }
            set { base.BackColor = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Themeable(false)]
        public override string BackImageUrl {
            get { return base.BackImageUrl; }
            set { base.BackImageUrl = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Themeable(false)]
        public override Color BorderColor {
            get { return base.BorderColor; }
            set { base.BorderColor = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Themeable(false)]
        public override BorderStyle BorderStyle {
            get { return base.BorderStyle; }
            set { base.BorderStyle = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Themeable(false)]
        public override Unit BorderWidth {
            get { return base.BorderWidth; }
            set { base.BorderWidth = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Themeable(false), CssClassProperty()]
        public override string CssClass {
            get { return base.CssClass; }
            set { base.CssClass = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Themeable(false)]
        public override string DefaultButton {
            get { return base.DefaultButton; }
            set { base.DefaultButton = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Themeable(false)]
        public override ContentDirection Direction {
            get { return base.Direction; }
            set { base.Direction = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Themeable(false)]
        public override bool Enabled {
            get { return base.Enabled; }
            set { base.Enabled = value; }
        }

        [Browsable(false), DefaultValue(false), EditorBrowsable(EditorBrowsableState.Never), Themeable(false)]
        public override bool EnableTheming {
            get { return false; }
            set { throw new NotSupportedException(SR.GetString(SR.NoThemingSupport, this.GetType().Name)); }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Themeable(false)]
        public override FontInfo Font {
            get { return base.Font; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Themeable(false)]
        public override Color ForeColor {
            get { return base.ForeColor; }
            set { base.ForeColor = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Themeable(false)]
        public override string GroupingText {
            get { return base.GroupingText; }
            set { base.GroupingText = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Themeable(false)]
        public override Unit Height {
            get { return base.Height; }
            set { base.Height = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Themeable(false)]
        public override HorizontalAlign HorizontalAlign {
            get { return base.HorizontalAlign; }
            set { base.HorizontalAlign = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Themeable(false)]
        public override ScrollBars ScrollBars {
            get { return base.ScrollBars; }
            set { base.ScrollBars = value; }
        }

        [Browsable(false), DefaultValue(""), EditorBrowsable(EditorBrowsableState.Never), Themeable(false)]
        public override string SkinID {
            get { return String.Empty; }
            set { throw new NotSupportedException(SR.GetString(SR.NoThemingSupport, this.GetType().Name)); }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Themeable(false)]
        public override short TabIndex {
            get { return base.TabIndex; }
            set { base.TabIndex = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Themeable(false)]
        public override string ToolTip {
            get { return base.ToolTip; }
            set { base.ToolTip = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Themeable(false)]
        public override bool Visible {
            get { return base.Visible; }
            set { base.Visible = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Themeable(false)]
        public override Unit Width {
            get { return base.Width; }
            set { base.Width = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Themeable(false)]
        public override bool Wrap {
            get { return base.Wrap; }
            set { base.Wrap = value; }
        }
        #endregion
    }
}

