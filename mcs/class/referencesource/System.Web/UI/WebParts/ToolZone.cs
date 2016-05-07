//------------------------------------------------------------------------------
// <copyright file="ToolZone.cs" company="Microsoft">
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

    public abstract class ToolZone : WebZone, IPostBackEventHandler {

        private const string headerCloseEventArgument = "headerClose";

        private const int baseIndex = 0;
        private const int editUIStyleIndex = 1;
        private const int headerCloseVerbIndex = 3;
        private const int headerVerbStyleIndex = 4;
        private const int instructionTextStyleIndex = 5;
        private const int labelStyleIndex = 6;
        private const int viewStateArrayLength = 7;

        private Style _editUIStyle;
        private WebPartVerb _headerCloseVerb;
        private Style _headerVerbStyle;
        private Style _instructionTextStyle;
        private Style _labelStyle;

        private WebPartDisplayModeCollection _associatedDisplayModes;

        protected ToolZone(ICollection associatedDisplayModes) {
            if ((associatedDisplayModes == null) || (associatedDisplayModes.Count == 0)) {
                throw new ArgumentNullException("associatedDisplayModes");
            }

            _associatedDisplayModes = new WebPartDisplayModeCollection();
            foreach (WebPartDisplayMode mode in associatedDisplayModes) {
                _associatedDisplayModes.Add(mode);
            }
            _associatedDisplayModes.SetReadOnly(SR.ToolZone_DisplayModesReadOnly);
        }

        protected ToolZone(WebPartDisplayMode associatedDisplayMode) {
            if (associatedDisplayMode == null) {
                throw new ArgumentNullException("associatedDisplayMode");
            }

            _associatedDisplayModes = new WebPartDisplayModeCollection();
            _associatedDisplayModes.Add(associatedDisplayMode);
            _associatedDisplayModes.SetReadOnly(SR.ToolZone_DisplayModesReadOnly);
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public WebPartDisplayModeCollection AssociatedDisplayModes {
            get {
                return _associatedDisplayModes;
            }
        }

        protected virtual bool Display {
            get {
                if (WebPartManager != null) {
                    WebPartDisplayModeCollection associatedDisplayModes = AssociatedDisplayModes;

                    if (associatedDisplayModes != null) {
                        return associatedDisplayModes.Contains(WebPartManager.DisplayMode);
                    }
                }
                return false;
            }
        }

        [
        DefaultValue(null),
        NotifyParentProperty(true),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Styles"),
        WebSysDescription(SR.ToolZone_EditUIStyle),
        ]
        public Style EditUIStyle {
            get {
                if (_editUIStyle == null) {
                    _editUIStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager)_editUIStyle).TrackViewState();
                    }
                }

                return _editUIStyle;
            }
        }

        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Verbs"),
        WebSysDescription(SR.ToolZone_HeaderCloseVerb),
        ]
        public virtual WebPartVerb HeaderCloseVerb {
            get {
                if (_headerCloseVerb == null) {
                    _headerCloseVerb = new WebPartHeaderCloseVerb();
                    _headerCloseVerb.EventArgument = headerCloseEventArgument;
                    if (IsTrackingViewState) {
                        ((IStateManager)_headerCloseVerb).TrackViewState();
                    }
                }
                return _headerCloseVerb;
            }
        }

        [
        DefaultValue(null),
        NotifyParentProperty(true),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Styles"),
        WebSysDescription(SR.ToolZone_HeaderVerbStyle),
        ]
        public Style HeaderVerbStyle {
            get {
                if (_headerVerbStyle == null) {
                    _headerVerbStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager)_headerVerbStyle).TrackViewState();
                    }
                }

                return _headerVerbStyle;
            }
        }

        [
        // Must use WebSysDefaultValue instead of DefaultValue, since it is overridden in extending classes
        Localizable(true),
        WebSysDefaultValue(""),
        WebCategory("Behavior"),
        WebSysDescription(SR.ToolZone_InstructionText),
        ]
        public virtual string InstructionText {
            get {
                string s = (string)ViewState["InstructionText"];
                return((s == null) ? String.Empty : s);
            }
            set {
                ViewState["InstructionText"] = value;
            }
        }

        [
        DefaultValue(null),
        NotifyParentProperty(true),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Styles"),
        WebSysDescription(SR.ToolZone_InstructionTextStyle),
        ]
        public Style InstructionTextStyle {
            get {
                if (_instructionTextStyle == null) {
                    _instructionTextStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager)_instructionTextStyle).TrackViewState();
                    }
                }

                return _instructionTextStyle;
            }
        }

        [
        DefaultValue(null),
        NotifyParentProperty(true),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Styles"),
        WebSysDescription(SR.ToolZone_LabelStyle),
        ]
        public Style LabelStyle {
            get {
                if (_labelStyle == null) {
                    _labelStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager)_labelStyle).TrackViewState();
                    }
                }

                return _labelStyle;
            }
        }

        [
        Bindable(false),
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override bool Visible {
            get {
                return Display && base.Visible;
            }
            set {
                if (!DesignMode) {
                    throw new InvalidOperationException(SR.GetString(SR.ToolZone_CantSetVisible));
                }
            }
        }

        protected abstract void Close();

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
                if (myState[editUIStyleIndex] != null) {
                    ((IStateManager) EditUIStyle).LoadViewState(myState[editUIStyleIndex]);
                }
                if (myState[headerCloseVerbIndex] != null) {
                    ((IStateManager) HeaderCloseVerb).LoadViewState(myState[headerCloseVerbIndex]);
                }
                if (myState[headerVerbStyleIndex] != null) {
                    ((IStateManager) HeaderVerbStyle).LoadViewState(myState[headerVerbStyleIndex]);
                }
                if (myState[instructionTextStyleIndex] != null) {
                    ((IStateManager) InstructionTextStyle).LoadViewState(myState[instructionTextStyleIndex]);
                }
                if (myState[labelStyleIndex] != null) {
                    ((IStateManager) LabelStyle).LoadViewState(myState[labelStyleIndex]);
                }
            }
        }

        protected virtual void OnDisplayModeChanged(object sender, WebPartDisplayModeEventArgs e) {
        }

        /// <internalonly/>
        protected internal override void OnInit(EventArgs e) {
            base.OnInit(e);

            WebPartManager webPartManager = WebPartManager;
            if (webPartManager != null) {
                webPartManager.DisplayModeChanged += new WebPartDisplayModeEventHandler(OnDisplayModeChanged);
                webPartManager.SelectedWebPartChanged += new WebPartEventHandler(OnSelectedWebPartChanged);
            }
        }

        protected virtual void OnSelectedWebPartChanged(object sender, WebPartEventArgs e) {
        }

        protected virtual void RaisePostBackEvent(string eventArgument) {
            ValidateEvent(UniqueID, eventArgument);

            if (String.Equals(eventArgument, headerCloseEventArgument, StringComparison.OrdinalIgnoreCase) &&
                HeaderCloseVerb.Visible && HeaderCloseVerb.Enabled) {
                Close();
            }
        }

        protected override void RenderFooter(HtmlTextWriter writer) {
            writer.AddStyleAttribute(HtmlTextWriterStyle.Margin, "4px");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            RenderVerbs(writer);
            writer.RenderEndTag();  // Div
        }

        protected override void RenderHeader(HtmlTextWriter writer) {
            // Render title bar
            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "2");
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");

            TitleStyle headerStyle = HeaderStyle;
            if (!headerStyle.IsEmpty) {
                // Apply font and forecolor from HeaderStyle to inner table
                Style style = new Style();
                if (!headerStyle.ForeColor.IsEmpty) {
                    style.ForeColor = headerStyle.ForeColor;
                }
                style.Font.CopyFrom(headerStyle.Font);
                if (!headerStyle.Font.Size.IsEmpty) {
                    // If the font size is specified on the HeaderStyle, force the font size to 100%,
                    // so it inherits the font size from its parent in IE compatibility mode. I would
                    // think that "1em" would work here as well, but "1em" doesn't work when you change
                    // the font size in the browser.
                    style.Font.Size = new FontUnit(new Unit(100, UnitType.Percentage));
                }
                if (!style.IsEmpty) {
                    style.AddAttributesToRender(writer, this);
                }
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Table);

            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            // Copied from Panel.cs
            // 
            HorizontalAlign hAlign = headerStyle.HorizontalAlign;
            if (hAlign != HorizontalAlign.NotSet) {
                TypeConverter hac = TypeDescriptor.GetConverter(typeof(HorizontalAlign));
                writer.AddAttribute(HtmlTextWriterAttribute.Align, hac.ConvertToString(hAlign));
            }

            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
            writer.AddStyleAttribute(HtmlTextWriterStyle.WhiteSpace, "nowrap");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write(HeaderText);
            writer.RenderEndTag();  // Td

            WebPartVerb headerCloseVerb = HeaderCloseVerb;
            if (headerCloseVerb.Visible) {
                writer.AddStyleAttribute(HtmlTextWriterStyle.WhiteSpace, "nowrap");
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                ZoneLinkButton closeButton = new ZoneLinkButton(this, headerCloseVerb.EventArgument);
                closeButton.Text = headerCloseVerb.Text;
                closeButton.ImageUrl = headerCloseVerb.ImageUrl;
                closeButton.ToolTip = headerCloseVerb.Description;
                closeButton.Enabled = headerCloseVerb.Enabled;
                closeButton.Page = Page;
                closeButton.ApplyStyle(HeaderVerbStyle);
                closeButton.RenderControl(writer);
                writer.RenderEndTag();  // Td
            }

            writer.RenderEndTag();  // Tr
            writer.RenderEndTag();  // Table
        }

        protected virtual void RenderVerbs(HtmlTextWriter writer) {
        }

        internal void RenderVerbsInternal(HtmlTextWriter writer, ICollection verbs) {
            ArrayList visibleVerbs = new ArrayList();
            foreach (WebPartVerb verb in verbs) {
                if (verb.Visible) {
                    visibleVerbs.Add(verb);
                }
            }

            // Render &nbsp; between each pair of verbs (VSWhidbey 77709)
            if (visibleVerbs.Count > 0) {
                bool firstVerb = true;
                foreach (WebPartVerb verb in visibleVerbs) {
                    if (!firstVerb) {
                        writer.Write("&nbsp;");
                    }
                    RenderVerb(writer, verb);
                    firstVerb = false;
                }
            }
        }

        protected virtual void RenderVerb(HtmlTextWriter writer, WebPartVerb verb) {
            string eventArgument = verb.EventArgument;
            WebControl verbControl;
            if (VerbButtonType == ButtonType.Button) {
                ZoneButton button = new ZoneButton(this, eventArgument);
                button.Text = verb.Text;
                verbControl = button;
            } else {
                ZoneLinkButton linkButton = new ZoneLinkButton(this, eventArgument);
                linkButton.Text = verb.Text;
                if (VerbButtonType == ButtonType.Image) {
                    linkButton.ImageUrl = verb.ImageUrl;
                }
                verbControl = linkButton;
            }

            verbControl.ApplyStyle(VerbStyle);
            verbControl.ToolTip = verb.Description;
            verbControl.Enabled = verb.Enabled;
            verbControl.Page = Page;
            verbControl.RenderControl(writer);
        }

        protected override object SaveViewState() {
            object[] myState = new object[viewStateArrayLength];

            myState[baseIndex] = base.SaveViewState();
            myState[editUIStyleIndex] = (_editUIStyle != null) ? ((IStateManager)_editUIStyle).SaveViewState() : null;
            myState[headerCloseVerbIndex] = (_headerCloseVerb != null) ? ((IStateManager)_headerCloseVerb).SaveViewState() : null;
            myState[headerVerbStyleIndex] = (_headerVerbStyle != null) ? ((IStateManager)_headerVerbStyle).SaveViewState() : null;
            myState[instructionTextStyleIndex] = (_instructionTextStyle != null) ? ((IStateManager)_instructionTextStyle).SaveViewState() : null;
            myState[labelStyleIndex] = (_labelStyle != null) ? ((IStateManager)_labelStyle).SaveViewState() : null;

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

            if (_editUIStyle != null) {
                ((IStateManager) _editUIStyle).TrackViewState();
            }
            if (_headerCloseVerb != null) {
                ((IStateManager) _headerCloseVerb).TrackViewState();
            }
            if (_headerVerbStyle != null) {
                ((IStateManager) _headerVerbStyle).TrackViewState();
            }
            if (_instructionTextStyle != null) {
                ((IStateManager) _instructionTextStyle).TrackViewState();
            }
            if (_labelStyle != null) {
                ((IStateManager) _labelStyle).TrackViewState();
            }
        }

        #region Implementation of IPostBackEventHandler
        /// <internalonly/>
        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument) {
            RaisePostBackEvent(eventArgument);
        }
        #endregion
    }
}

