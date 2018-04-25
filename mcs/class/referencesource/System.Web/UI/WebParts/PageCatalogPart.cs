//------------------------------------------------------------------------------
// <copyright file="PageCatalogPart.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.Web;
    using System.Web.Security;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    /// <devdoc>
    /// </devdoc>
    [
    Designer("System.Web.UI.Design.WebControls.WebParts.PageCatalogPartDesigner, " + AssemblyRef.SystemDesign),
    ]
    public sealed class PageCatalogPart : CatalogPart {

        private WebPartDescriptionCollection _availableWebPartDescriptions;

        private static readonly WebPartDescriptionCollection DesignModeAvailableWebParts =
            new WebPartDescriptionCollection(new WebPartDescription[] {
                new WebPartDescription("webpart1", String.Format(CultureInfo.CurrentCulture,
                    SR.GetString(SR.CatalogPart_SampleWebPartTitle), "1"), null, null),
                new WebPartDescription("webpart2", String.Format(CultureInfo.CurrentCulture,
                    SR.GetString(SR.CatalogPart_SampleWebPartTitle), "2"), null, null),
                new WebPartDescription("webpart3", String.Format(CultureInfo.CurrentCulture,
                    SR.GetString(SR.CatalogPart_SampleWebPartTitle), "3"), null, null),
            });

        [
        WebSysDefaultValue(SR.PageCatalogPart_PartTitle),
        ]
        public override string Title {
            get {
                string s = (string)ViewState["Title"];
                return (s != null) ? s : SR.GetString(SR.PageCatalogPart_PartTitle);
            }
            set {
                ViewState["Title"] = value;
            }
        }

        public override WebPartDescriptionCollection GetAvailableWebPartDescriptions() {
            if (DesignMode) {
                return DesignModeAvailableWebParts;
            }

            if (_availableWebPartDescriptions == null) {
                WebPartCollection availableWebParts;
                if (WebPartManager != null) {
                    WebPartCollection closedWebParts = GetClosedWebParts();
                    if (closedWebParts != null) {
                        availableWebParts = closedWebParts;
                    }
                    else {
                        availableWebParts = new WebPartCollection();
                    }
                }
                else {
                    availableWebParts = new WebPartCollection();
                }

                ArrayList descriptions = new ArrayList();
                foreach(WebPart part in availableWebParts) {
                    // Do not show UnauthorizedWebParts (VSWhidbey 429514)
                    if (part is UnauthorizedWebPart) {
                        continue;
                    }

                    WebPartDescription description = new WebPartDescription(part);
                    descriptions.Add(description);
                }

                _availableWebPartDescriptions = new WebPartDescriptionCollection(descriptions);
            }

            return _availableWebPartDescriptions;
        }

        private WebPartCollection GetClosedWebParts() {
            // WebPartManager is checked for null in calling code
            Debug.Assert(WebPartManager != null);

            ArrayList closedWebParts = new ArrayList();

            WebPartCollection webParts = WebPartManager.WebParts;
            if (webParts != null) {
                foreach (WebPart part in webParts) {
                    if (part.IsClosed) {
                        closedWebParts.Add(part);
                    }
                }
            }

            return new WebPartCollection(closedWebParts);
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

        /// <internalonly/>
        protected internal override void OnInit(EventArgs e) {
            base.OnInit(e);

            if (WebPartManager != null) {
                WebPartManager.WebPartAdded += new WebPartEventHandler(this.OnWebPartsChanged);
                WebPartManager.WebPartClosed += new WebPartEventHandler(this.OnWebPartsChanged);
                WebPartManager.WebPartDeleted += new WebPartEventHandler(this.OnWebPartsChanged);
            }
        }

        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            // Invalidate cache, since the DisplayTitles may not have been available the first time
            // we created the WebPartDescriptions (VSWhidbey 355573)
            _availableWebPartDescriptions = null;
        }

        private void OnWebPartsChanged(object sender, WebPartEventArgs e) {
            // Invalidate cache
            _availableWebPartDescriptions = null;
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

