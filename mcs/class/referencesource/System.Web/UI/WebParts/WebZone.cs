//------------------------------------------------------------------------------
// <copyright file="WebZone.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.Util;

    /// <devdoc>
    /// Base class for all zone classes in the WebPart framework.  Contains properties used to control the UI
    /// which is common to all zone classes.
    /// </devdoc>
    [
    Designer("System.Web.UI.Design.WebControls.WebParts.WebZoneDesigner, " + AssemblyRef.SystemDesign),
    Bindable(false),
    ]
    public abstract class WebZone : CompositeControl {

        private WebPartManager _webPartManager;

        private const int baseIndex = 0;
        private const int emptyZoneTextStyleIndex = 1;
        private const int footerStyleIndex = 2;
        private const int partStyleIndex = 3;
        private const int partChromeStyleIndex = 4;
        private const int partTitleStyleIndex = 5;
        private const int headerStyleIndex = 6;
        private const int verbStyleIndex = 7;
        private const int errorStyleIndex = 8;
        private const int viewStateArrayLength = 9;

        private Style _emptyZoneTextStyle;
        private TitleStyle _footerStyle;
        private TableStyle _partStyle;
        private Style _partChromeStyle;
        private TitleStyle _partTitleStyle;
        private TitleStyle _headerStyle;
        private Style _verbStyle;
        private Style _errorStyle;

        // Prevent class from being subclassed outside of our assembly
        internal WebZone() {
        }

        /// <devdoc>
        /// The URL of the background image for the control.
        /// </devdoc>
        [
        DefaultValue(""),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty(),
        WebCategory("Appearance"),
        WebSysDescription(SR.WebControl_BackImageUrl)
        ]
        public virtual string BackImageUrl {
            get {
                string s = (string)ViewState["BackImageUrl"];
                return((s == null) ? String.Empty : s);
            }
            set {
                ViewState["BackImageUrl"] = value;
            }
        }

        [
        // Must use WebSysDefaultValue instead of DefaultValue, since it is overridden in extending classes
        Localizable(true),
        WebSysDefaultValue(""),
        WebCategory("Behavior"),
        WebSysDescription(SR.Zone_EmptyZoneText),
        ]
        public virtual string EmptyZoneText {
            get {
                string s = (string)ViewState["EmptyZoneText"];
                return((s == null) ? String.Empty : s);
            }
            set {
                ViewState["EmptyZoneText"] = value;
            }
        }

        [
        DefaultValue(null),
        NotifyParentProperty(true),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Styles"),
        WebSysDescription(SR.Zone_EmptyZoneTextStyle),
        ]
        public Style EmptyZoneTextStyle {
            get {
                if (_emptyZoneTextStyle == null) {
                    _emptyZoneTextStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager)_emptyZoneTextStyle).TrackViewState();
                    }
                }

                return _emptyZoneTextStyle;
            }
        }

        [
        DefaultValue(null),
        NotifyParentProperty(true),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Styles"),
        WebSysDescription(SR.Zone_ErrorStyle),
        ]
        public Style ErrorStyle {
            get {
                if (_errorStyle == null) {
                    _errorStyle = new ErrorStyle();
                    if (IsTrackingViewState) {
                        ((IStateManager)_errorStyle).TrackViewState();
                    }
                }

                return _errorStyle;
            }
        }

        /// <devdoc>
        /// Style for the footer of the zone.
        /// </devdoc>
        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Styles"),
        WebSysDescription(SR.Zone_FooterStyle)
        ]
        public TitleStyle FooterStyle {
            get {
                if (_footerStyle == null) {
                    _footerStyle = new TitleStyle();
                    if (IsTrackingViewState) {
                        ((IStateManager)_footerStyle).TrackViewState();
                    }
                }

                return _footerStyle;
            }
        }

        protected virtual bool HasFooter {
            get {
                return true;
            }
        }

        protected virtual bool HasHeader {
            get {
                return true;
            }
        }

        /// <devdoc>
        /// The header text of the zone.
        /// </devdoc>
        [
        // Must use WebSysDefaultValue instead of DefaultValue, since it is overridden in extending classes
        Localizable(true),
        WebSysDefaultValue(""),
        WebCategory("Appearance"),
        WebSysDescription(SR.Zone_HeaderText)
        ]
        public virtual string HeaderText {
            get {
                string s = (string)ViewState["HeaderText"];
                return((s == null) ? String.Empty : s);
            }
            set {
                ViewState["HeaderText"] = value;
            }
        }

        /// <devdoc>
        /// Style for the title bar of the zone.
        /// </devdoc>
        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Styles"),
        WebSysDescription(SR.Zone_HeaderStyle)
        ]
        public TitleStyle HeaderStyle {
            get {
                if (_headerStyle == null) {
                    _headerStyle = new TitleStyle();
                    if (IsTrackingViewState) {
                        ((IStateManager)_headerStyle).TrackViewState();
                    }
                }

                return _headerStyle;
            }
        }

        /// <devdoc>
        /// Padding for the contained parts.
        /// </devdoc>
        [
        DefaultValue(typeof(Unit), "5px"),
        WebCategory("WebPart"),
        WebSysDescription(SR.Zone_PartChromePadding)
        ]
        public Unit PartChromePadding {
            get {
                object obj = ViewState["PartChromePadding"];
                return (obj == null) ? Unit.Pixel(5) : (Unit)obj;
            }
            set {
                if (value.Value < 0) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["PartChromePadding"] = value;
            }
        }

        /// <devdoc>
        /// Style for the contained parts.
        /// </devdoc>
        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("WebPart"),
        WebSysDescription(SR.Zone_PartChromeStyle)
        ]
        public Style PartChromeStyle {
            get {
                if (_partChromeStyle == null) {
                    _partChromeStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager)_partChromeStyle).TrackViewState();
                    }
                }

                return _partChromeStyle;
            }
        }

        /// <devdoc>
        /// The type of frame/border for the contained parts.
        /// </devdoc>
        [
        DefaultValue(PartChromeType.Default),
        WebCategory("WebPart"),
        WebSysDescription(SR.Zone_PartChromeType)
        ]
        public virtual PartChromeType PartChromeType {
            get {
                object o = ViewState["PartChromeType"];
                return (o != null) ? (PartChromeType)(int)o : PartChromeType.Default;
            }
            set {
                if ((value < PartChromeType.Default) || (value > PartChromeType.BorderOnly)) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["PartChromeType"] = (int)value;
            }
        }

        /// <devdoc>
        /// Style for the contents of the contained parts.
        /// </devdoc>
        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("WebPart"),
        WebSysDescription(SR.Zone_PartStyle)
        ]
        public TableStyle PartStyle {
            get {
                if (_partStyle == null) {
                    _partStyle = new TableStyle();
                    if (IsTrackingViewState) {
                        ((IStateManager)_partStyle).TrackViewState();
                    }
                }

                return _partStyle;
            }
        }

        /// <devdoc>
        /// Style for the title bars of the contained parts.
        /// </devdoc>
        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("WebPart"),
        WebSysDescription(SR.Zone_PartTitleStyle)
        ]
        public TitleStyle PartTitleStyle {
            get {
                if (_partTitleStyle == null) {
                    _partTitleStyle = new TitleStyle();
                    if (IsTrackingViewState) {
                        ((IStateManager)_partTitleStyle).TrackViewState();
                    }
                }

                return _partTitleStyle;
            }
        }

        protected override HtmlTextWriterTag TagKey {
            get {
                return HtmlTextWriterTag.Table;
            }
        }

        // Padding = -1 means we will not render anything for the cellpadding attribute
        [
        DefaultValue(2),
        WebCategory("Layout"),
        WebSysDescription(SR.Zone_Padding),
        ]
        public virtual int Padding {
            get {
                object obj = ViewState["Padding"];
                return (obj == null) ? 2 : (int) obj;
            }
            set {
                if (value < -1) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["Padding"] = value;
            }
        }

        // Called by WebPartZoneBase, EditorZoneBase, CatalogZoneBase, and ConnectionsZone.
        internal void RenderBodyTableBeginTag(HtmlTextWriter writer) {
            // 


            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            int padding = Padding;
            if (padding >= 0) {
                writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, padding.ToString(CultureInfo.InvariantCulture));
            }
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");

            // Copied from Panel.cs
            // 
            string backImageUrl = BackImageUrl;
            // Whidbey 12856
            if (backImageUrl.Trim().Length > 0)
                writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundImage,"url(" + ResolveClientUrl(backImageUrl) + ")");

            // Needed if Zone HeaderText is wider than contained Parts
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");

            // Needed if the height of the Zone is taller than the height of its contents
            writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "100%");

            writer.RenderBeginTag(HtmlTextWriterTag.Table);
        }

        // Called by WebPartZoneBase, EditorZoneBase, CatalogZoneBase, and ConnectionsZone.
        internal static void RenderBodyTableEndTag(HtmlTextWriter writer) {
            writer.RenderEndTag();  // Table
        }

        // Called by WebPartZoneBase, EditorZoneBase, and CatalogZoneBase.
        internal void RenderDesignerRegionBeginTag(HtmlTextWriter writer, Orientation orientation) {
            writer.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            if (orientation == Orientation.Horizontal) {
                writer.AddStyleAttribute(HtmlTextWriterStyle.WhiteSpace, "nowrap");
            }
            writer.AddAttribute(HtmlTextWriterAttribute.DesignerRegion, "0");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, Padding.ToString(CultureInfo.InvariantCulture));
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
            if (orientation == Orientation.Vertical) {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
            }
            else {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "100%");
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Table);
        }

        // Called by WebPartZoneBase, EditorZoneBase, and CatalogZoneBase.
        internal static void RenderDesignerRegionEndTag(HtmlTextWriter writer) {
            writer.RenderEndTag(); // Table
            writer.RenderEndTag(); // Td
            writer.RenderEndTag(); // Tr
        }

        protected internal bool RenderClientScript {
            get {
                bool renderClientScript = false;
                if (DesignMode) {
                    renderClientScript = true;
                }
                else if (WebPartManager != null) {
                    renderClientScript = WebPartManager.RenderClientScript;
                }
                return renderClientScript;
            }
        }

        /// <devdoc>
        /// The type of the button rendered for each verb.
        /// </devdoc>
        [
        DefaultValue(ButtonType.Button),
        WebCategory("Appearance"),
        WebSysDescription(SR.Zone_VerbButtonType),
        ]
        public virtual ButtonType VerbButtonType {
            get {
                object obj = ViewState["VerbButtonType"];
                return (obj == null) ? ButtonType.Button : (ButtonType)obj;
            }
            set {
                if (value < ButtonType.Button || value > ButtonType.Link) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["VerbButtonType"] = value;
            }
        }

        [
        DefaultValue(null),
        NotifyParentProperty(true),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Styles"),
        WebSysDescription(SR.Zone_VerbStyle),
        ]
        public Style VerbStyle {
            get {
                if (_verbStyle == null) {
                    _verbStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager)_verbStyle).TrackViewState();
                    }
                }

                return _verbStyle;
            }
        }

        /// <devdoc>
        /// The effective chrome type of a part, taking into consideration the PartChromeType
        /// of the zone and the DisplayMode of the page.
        /// </devdoc>
        public virtual PartChromeType GetEffectiveChromeType(Part part) {
            if (part == null) {
                throw new ArgumentNullException("part");
            }

            PartChromeType chromeType = part.ChromeType;
            if (chromeType == PartChromeType.Default) {
                PartChromeType partChromeType = PartChromeType;
                if (partChromeType == PartChromeType.Default) {
                    chromeType = PartChromeType.TitleAndBorder;
                }
                else {
                    chromeType = partChromeType;
                }
            }

            Debug.Assert(chromeType != PartChromeType.Default);
            return chromeType;
        }

        protected override void LoadViewState(object savedState) {
            if (savedState == null) {
                base.LoadViewState(null);
            }
            else {
                object[] myState = (object[]) savedState;
                if (myState.Length != viewStateArrayLength) {
                    throw new ArgumentException(SR.GetString(SR.ViewState_InvalidViewState));
                }

                base.LoadViewState(myState[baseIndex]);
                if (myState[emptyZoneTextStyleIndex] != null) {
                    ((IStateManager) EmptyZoneTextStyle).LoadViewState(myState[emptyZoneTextStyleIndex]);
                }
                if (myState[footerStyleIndex] != null) {
                    ((IStateManager) FooterStyle).LoadViewState(myState[footerStyleIndex]);
                }
                if (myState[partStyleIndex] != null) {
                    ((IStateManager) PartStyle).LoadViewState(myState[partStyleIndex]);
                }
                if (myState[partChromeStyleIndex] != null) {
                    ((IStateManager) PartChromeStyle).LoadViewState(myState[partChromeStyleIndex]);
                }
                if (myState[partTitleStyleIndex] != null) {
                    ((IStateManager) PartTitleStyle).LoadViewState(myState[partTitleStyleIndex]);
                }
                if (myState[headerStyleIndex] != null) {
                    ((IStateManager) HeaderStyle).LoadViewState(myState[headerStyleIndex]);
                }
                if (myState[verbStyleIndex] != null) {
                    ((IStateManager) VerbStyle).LoadViewState(myState[verbStyleIndex]);
                }
                if (myState[errorStyleIndex] != null) {
                    ((IStateManager) ErrorStyle).LoadViewState(myState[errorStyleIndex]);
                }
            }
        }

        protected internal override void OnInit(EventArgs e) {
            base.OnInit(e);

            Page page = Page;
            Debug.Assert(page != null);
            if (page != null) {
                if (page.ControlState >= ControlState.Initialized && !DesignMode) {
                    throw new InvalidOperationException(SR.GetString(SR.Zone_AddedTooLate));
                }

                if (!DesignMode) {
                    _webPartManager = WebPartManager.GetCurrentWebPartManager(page);
                    if (_webPartManager == null) {
                        throw new InvalidOperationException(SR.GetString(SR.WebPartManagerRequired));
                    }

                    _webPartManager.RegisterZone(this);
                }
            }
        }

        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            Control parent = Parent;
            Debug.Assert(parent != null);
            if (parent != null && (parent is WebZone || parent is Part)) {
                throw new InvalidOperationException(SR.GetString(SR.Zone_InvalidParent));
            }
        }

        public override void RenderBeginTag(HtmlTextWriter writer) {
            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");

            // On Mac IE, if height is not set, render height:1px, so the table sizes to contents.
            // Otherwise, Mac IE may give the table an arbitrary height (equal to the width of its contents).
            if (!DesignMode &&
                Page != null &&
                Page.Request.Browser.Type == "IE5" &&
                Page.Request.Browser.Platform == "MacPPC" &&
                (!ControlStyleCreated || ControlStyle.Height == Unit.Empty)) {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "1px");
            }

            // Render <table>
            base.RenderBeginTag(writer);
        }

        protected internal override void RenderContents(HtmlTextWriter writer) {
            if (HasHeader) {
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                TitleStyle headerStyle = HeaderStyle;
                if (!headerStyle.IsEmpty) {
                    headerStyle.AddAttributesToRender(writer, this);
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                RenderHeader(writer);
                writer.RenderEndTag();  // Td
                writer.RenderEndTag();  // Tr
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            // We want the body to fill the height of the zone, and squish the header and footer
            // to the size of their contents
            // Mac IE needs height=100% set on <td> instead of <tr>
            writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "100%");

            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            RenderBody(writer);
            writer.RenderEndTag();  // Td
            writer.RenderEndTag();  // Tr

            if (HasFooter) {
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                TitleStyle footerStyle = FooterStyle;
                if (!footerStyle.IsEmpty) {
                    footerStyle.AddAttributesToRender(writer, this);
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                RenderFooter(writer);
                writer.RenderEndTag();  // Td
                writer.RenderEndTag();  // Tr
            }
        }

        protected virtual void RenderHeader(HtmlTextWriter writer) {
        }

        protected virtual void RenderBody(HtmlTextWriter writer) {
        }

        protected virtual void RenderFooter(HtmlTextWriter writer) {
        }

        protected override object SaveViewState() {
            object[] myState = new object[viewStateArrayLength];

            myState[baseIndex] = base.SaveViewState();
            myState[emptyZoneTextStyleIndex] = (_emptyZoneTextStyle != null) ? ((IStateManager)_emptyZoneTextStyle).SaveViewState() : null;
            myState[footerStyleIndex] = (_footerStyle != null) ? ((IStateManager)_footerStyle).SaveViewState() : null;
            myState[partStyleIndex] = (_partStyle != null) ? ((IStateManager)_partStyle).SaveViewState() : null;
            myState[partChromeStyleIndex] = (_partChromeStyle != null) ? ((IStateManager)_partChromeStyle).SaveViewState() : null;
            myState[partTitleStyleIndex] = (_partTitleStyle != null) ? ((IStateManager)_partTitleStyle).SaveViewState() : null;
            myState[headerStyleIndex] = (_headerStyle != null) ? ((IStateManager)_headerStyle).SaveViewState() : null;
            myState[verbStyleIndex] = (_verbStyle != null) ? ((IStateManager)_verbStyle).SaveViewState() : null;
            myState[errorStyleIndex] = (_errorStyle != null) ? ((IStateManager)_errorStyle).SaveViewState() : null;

            for (int i=0; i < viewStateArrayLength; i++) {
                if (myState[i] != null) {
                    return myState;
                }
            }

            // More performant to return null than an array of null values
            return null;
        }

        protected override void TrackViewState() {
            base.TrackViewState();

            if (_emptyZoneTextStyle != null) {
                ((IStateManager) _emptyZoneTextStyle).TrackViewState();
            }
            if (_footerStyle != null) {
                ((IStateManager) _footerStyle).TrackViewState();
            }
            if (_partStyle != null) {
                ((IStateManager) _partStyle).TrackViewState();
            }
            if (_partChromeStyle != null) {
                ((IStateManager) _partChromeStyle).TrackViewState();
            }
            if (_partTitleStyle != null) {
                ((IStateManager) _partTitleStyle).TrackViewState();
            }
            if (_headerStyle != null) {
                ((IStateManager) _headerStyle).TrackViewState();
            }
            if (_verbStyle != null) {
                ((IStateManager) _verbStyle).TrackViewState();
            }
            if (_errorStyle != null) {
                ((IStateManager) _errorStyle).TrackViewState();
            }
        }

        protected WebPartManager WebPartManager {
            get {
                return _webPartManager;
            }
        }
    }
}
