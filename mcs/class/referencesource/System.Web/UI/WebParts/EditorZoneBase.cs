//------------------------------------------------------------------------------
// <copyright file="EditorZoneBase.cs" company="Microsoft">
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

    public abstract class EditorZoneBase : ToolZone {

        private EditorPartCollection _editorParts;

        private const int baseIndex = 0;
        private const int applyVerbIndex = 1;
        private const int cancelVerbIndex = 2;
        private const int okVerbIndex = 3;
        private const int viewStateArrayLength = 4;

        private WebPartVerb _applyVerb;
        private WebPartVerb _cancelVerb;
        private WebPartVerb _okVerb;

        private bool _applyError;

        private EditorPartChrome _editorPartChrome;

        private const string applyEventArgument = "apply";
        private const string cancelEventArgument = "cancel";
        private const string okEventArgument = "ok";

        protected EditorZoneBase() : base(WebPartManager.EditDisplayMode) {
        }

        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Verbs"),
        WebSysDescription(SR.EditorZoneBase_ApplyVerb),
        ]
        public virtual WebPartVerb ApplyVerb {
            get {
                if (_applyVerb == null) {
                    _applyVerb = new WebPartEditorApplyVerb();
                    _applyVerb.EventArgument = applyEventArgument;
                    if (IsTrackingViewState) {
                        ((IStateManager)_applyVerb).TrackViewState();
                    }
                }
                return _applyVerb;
            }
        }

        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Verbs"),
        WebSysDescription(SR.EditorZoneBase_CancelVerb),
        ]
        public virtual WebPartVerb CancelVerb {
            get {
                if (_cancelVerb == null) {
                    _cancelVerb = new WebPartEditorCancelVerb();
                    _cancelVerb.EventArgument = cancelEventArgument;
                    if (IsTrackingViewState) {
                        ((IStateManager)_cancelVerb).TrackViewState();
                    }
                }

                return _cancelVerb;
            }
        }

        protected override bool Display {
            get {
                return (base.Display && WebPartToEdit != null);
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public EditorPartChrome EditorPartChrome {
            get {
                if (_editorPartChrome == null) {
                    _editorPartChrome = CreateEditorPartChrome();
                }
                return _editorPartChrome;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public EditorPartCollection EditorParts {
            get {
                if (_editorParts == null) {
                    WebPart webPartToEdit = WebPartToEdit;
                    EditorPartCollection webPartEditorParts = null;
                    if (webPartToEdit != null && webPartToEdit is IWebEditable) {
                        webPartEditorParts = ((IWebEditable)webPartToEdit).CreateEditorParts();
                    }

                    EditorPartCollection editorParts = new EditorPartCollection(webPartEditorParts, CreateEditorParts());

                    // Verify that each EditorPart has a nonempty ID.  Don't throw an exception in the designer,
                    // since we want only the offending control to render as an error block, not the whole CatalogZone.
                    if (!DesignMode) {
                        foreach (EditorPart editorPart in editorParts) {
                            if (String.IsNullOrEmpty(editorPart.ID)) {
                                throw new InvalidOperationException(SR.GetString(SR.EditorZoneBase_NoEditorPartID));
                            }
                        }
                    }

                    _editorParts = editorParts;

                    // Call EnsureChildControls to parent the EditorParts and set the WebPartToEdit,
                    // WebPartManager, and Zone
                    EnsureChildControls();
                }

                return _editorParts;
            }
        }

        [
        WebSysDefaultValue(SR.EditorZoneBase_DefaultEmptyZoneText)
        ]
        public override string EmptyZoneText {
            // Must look at viewstate directly instead of the property in the base class,
            // so we can distinguish between an unset property and a property set to String.Empty.
            get {
                string s = (string)ViewState["EmptyZoneText"];
                return((s == null) ? SR.GetString(SR.EditorZoneBase_DefaultEmptyZoneText) : s);
            }
            set {
                ViewState["EmptyZoneText"] = value;
            }
        }

        [
        Localizable(true),
        WebCategory("Behavior"),
        WebSysDefaultValue(SR.EditorZoneBase_DefaultErrorText),
        WebSysDescription(SR.EditorZoneBase_ErrorText),
        ]
        public virtual string ErrorText {
            get {
                string s = (string)ViewState["ErrorText"];
                return((s == null) ? SR.GetString(SR.EditorZoneBase_DefaultErrorText) : s);
            }
            set {
                ViewState["ErrorText"] = value;
            }
        }

        [
        WebSysDefaultValue(SR.EditorZoneBase_DefaultHeaderText)
        ]
        public override string HeaderText {
            get {
                string s = (string)ViewState["HeaderText"];
                return((s == null) ? SR.GetString(SR.EditorZoneBase_DefaultHeaderText) : s);
            }
            set {
                ViewState["HeaderText"] = value;
            }
        }

        [
        WebSysDefaultValue(SR.EditorZoneBase_DefaultInstructionText),
        ]
        public override string InstructionText {
            get {
                string s = (string)ViewState["InstructionText"];
                return((s == null) ? SR.GetString(SR.EditorZoneBase_DefaultInstructionText) : s);
            }
            set {
                ViewState["InstructionText"] = value;
            }
        }

        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Verbs"),
        WebSysDescription(SR.EditorZoneBase_OKVerb),
        ]
        public virtual WebPartVerb OKVerb {
            get {
                if (_okVerb == null) {
                    _okVerb = new WebPartEditorOKVerb();
                    _okVerb.EventArgument = okEventArgument;
                    if (IsTrackingViewState) {
                        ((IStateManager)_okVerb).TrackViewState();
                    }
                }
                return _okVerb;
            }
        }

        protected WebPart WebPartToEdit {
            get {
                if (WebPartManager != null && WebPartManager.DisplayMode == WebPartManager.EditDisplayMode) {
                    return WebPartManager.SelectedWebPart;
                }
                else {
                    return null;
                }
            }
        }

        private void ApplyAndSyncChanges() {
            WebPart webPartToEdit = WebPartToEdit;
            Debug.Assert(webPartToEdit != null);
            if (webPartToEdit != null) {
                EditorPartCollection editorParts = EditorParts;
                foreach (EditorPart editorPart in editorParts) {
                    if (editorPart.Display && editorPart.Visible && editorPart.ChromeState == PartChromeState.Normal) {
                        if (!editorPart.ApplyChanges()) {
                            _applyError = true;
                        }
                    }
                }
                if (!_applyError) {
                    foreach (EditorPart editorPart in editorParts) {
                        editorPart.SyncChanges();
                    }
                }
            }
        }

        /// <devdoc>
        /// Returns the Page to normal view.  Does not call ApplyChanges to any EditorParts.
        /// </devdoc>
        protected override void Close() {
            if (WebPartManager != null) {
                WebPartManager.EndWebPartEditing();
            }
        }

        /// <internalonly/>
        protected internal override void CreateChildControls() {
            ControlCollection controls = Controls;
            controls.Clear();

            WebPart webPartToEdit = WebPartToEdit;
            foreach (EditorPart editorPart in EditorParts) {
                // webPartToEdit will be null if WebPartManager is null
                if (webPartToEdit != null) {
                    editorPart.SetWebPartToEdit(webPartToEdit);
                    editorPart.SetWebPartManager(WebPartManager);
                }
                editorPart.SetZone(this);
                controls.Add(editorPart);
            }
        }

        protected virtual EditorPartChrome CreateEditorPartChrome() {
            return new EditorPartChrome(this);
        }

        protected abstract EditorPartCollection CreateEditorParts();

        // Called by a derived class if the list of EditorParts changes, and they want CreateEditorParts()
        // to be called again.
        protected void InvalidateEditorParts() {
            _editorParts = null;
            ChildControlsCreated = false;
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
                if (myState[applyVerbIndex] != null) {
                    ((IStateManager) ApplyVerb).LoadViewState(myState[applyVerbIndex]);
                }
                if (myState[cancelVerbIndex] != null) {
                    ((IStateManager) CancelVerb).LoadViewState(myState[cancelVerbIndex]);
                }
                if (myState[okVerbIndex] != null) {
                    ((IStateManager) OKVerb).LoadViewState(myState[okVerbIndex]);
                }
            }
        }

        protected override void OnDisplayModeChanged(object sender, WebPartDisplayModeEventArgs e) {
            InvalidateEditorParts();
            base.OnDisplayModeChanged(sender, e);
        }

        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            EditorPartChrome.PerformPreRender();
        }

        protected override void OnSelectedWebPartChanged(object sender, WebPartEventArgs e) {
            if (WebPartManager != null && WebPartManager.DisplayMode == WebPartManager.EditDisplayMode) {
                InvalidateEditorParts();

                // SelectedWebPartChanged is raised when a WebPart is entering or exiting Edit mode.
                // We only want to call SyncChanges when a WebPart is entering Edit mode
                // (e.WebPart will be non-null).
                if (e.WebPart != null) {
                    foreach (EditorPart editorPart in EditorParts) {
                        editorPart.SyncChanges();
                    }
                }
            }

            base.OnSelectedWebPartChanged(sender, e);
        }

        protected override void RaisePostBackEvent(string eventArgument) {
            if (String.Equals(eventArgument, applyEventArgument, StringComparison.OrdinalIgnoreCase)) {
                if (ApplyVerb.Visible && ApplyVerb.Enabled && WebPartToEdit != null) {
                    ApplyAndSyncChanges();
                }
            }
            else if (String.Equals(eventArgument, cancelEventArgument, StringComparison.OrdinalIgnoreCase)) {
                if (CancelVerb.Visible && CancelVerb.Enabled && WebPartToEdit != null) {
                    Close();
                }
            }
            else if (String.Equals(eventArgument, okEventArgument, StringComparison.OrdinalIgnoreCase)) {
                if (OKVerb.Visible && OKVerb.Enabled && WebPartToEdit != null) {
                    ApplyAndSyncChanges();
                    if (!_applyError) {
                        // Only close the EditorZone if there were no errors applying the EditorParts
                        Close();
                    }
                }
            }
            else {
                base.RaisePostBackEvent(eventArgument);
            }
        }

        protected internal override void Render(HtmlTextWriter writer) {
            if (Page != null) {
                Page.VerifyRenderingInServerForm(this);
            }

            base.Render(writer);
        }

        protected override void RenderBody(HtmlTextWriter writer) {
            RenderBodyTableBeginTag(writer);
            if (DesignMode) {
                RenderDesignerRegionBeginTag(writer, Orientation.Vertical);
            }

            if (HasControls()) {
                bool firstCell = true;

                RenderInstructionText(writer, ref firstCell);

                if (_applyError) {
                    RenderErrorText(writer, ref firstCell);
                }

                EditorPartChrome chrome = EditorPartChrome;
                foreach (EditorPart editorPart in EditorParts) {
                    if ((!editorPart.Display) || (!editorPart.Visible)) {
                        continue;
                    }

                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                    if (!firstCell) {
                        writer.AddStyleAttribute(HtmlTextWriterStyle.PaddingTop, "0");
                    }
                    else {
                        firstCell = false;
                    }
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);

                    chrome.RenderEditorPart(writer, editorPart);

                    writer.RenderEndTag();  // Td
                    writer.RenderEndTag();  // Tr
                }

                writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                // Mozilla renders padding on an empty TD without this attribute
                writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, "0");

                // Add an extra row with height of 100%, to [....] up any extra space
                // if the height of the zone is larger than its contents
                // Mac IE needs height=100% set on <td> instead of <tr>
                writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "100%");

                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.RenderEndTag(); // Td
                writer.RenderEndTag(); // Tr
            }
            else {
                RenderEmptyZoneText(writer);
            }

            if (DesignMode) {
                RenderDesignerRegionEndTag(writer);
            }
            RenderBodyTableEndTag(writer);
        }

        private void RenderEmptyZoneText(HtmlTextWriter writer) {
            string emptyZoneText = EmptyZoneText;
            if (!String.IsNullOrEmpty(emptyZoneText)) {
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                writer.AddAttribute(HtmlTextWriterAttribute.Valign, "top");

                Style emptyZoneTextStyle = EmptyZoneTextStyle;
                if (!emptyZoneTextStyle.IsEmpty) {
                    emptyZoneTextStyle.AddAttributesToRender(writer, this);
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Td);

                writer.Write(emptyZoneText);

                writer.RenderEndTag();  // Td
                writer.RenderEndTag();  // Tr
            }
        }

        private void RenderErrorText(HtmlTextWriter writer, ref bool firstCell) {
            string errorText = ErrorText;
            if (!String.IsNullOrEmpty(errorText)) {
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                firstCell = false;

                Label label = new Label();
                label.Text = errorText;
                label.Page = Page;
                label.ApplyStyle(ErrorStyle);
                label.RenderControl(writer);

                writer.RenderEndTag();  // Td
                writer.RenderEndTag();  // Tr
            }
        }

        private void RenderInstructionText(HtmlTextWriter writer, ref bool firstCell) {
            string instructionText = InstructionText;
            if (!String.IsNullOrEmpty(instructionText)) {
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                firstCell = false;

                Label label = new Label();
                label.Text = instructionText;
                label.Page = Page;
                label.ApplyStyle(InstructionTextStyle);
                label.RenderControl(writer);

                writer.RenderEndTag();  // Td
                writer.RenderEndTag();  // Tr
            }
        }

        protected override void RenderVerbs(HtmlTextWriter writer) {
            RenderVerbsInternal(writer, new WebPartVerb[] {OKVerb, CancelVerb, ApplyVerb});
        }

        protected override object SaveViewState() {
            object[] myState = new object[viewStateArrayLength];

            myState[baseIndex] = base.SaveViewState();
            myState[applyVerbIndex] = (_applyVerb != null) ? ((IStateManager)_applyVerb).SaveViewState() : null;
            myState[cancelVerbIndex] = (_cancelVerb != null) ? ((IStateManager)_cancelVerb).SaveViewState() : null;
            myState[okVerbIndex] = (_okVerb != null) ? ((IStateManager)_okVerb).SaveViewState() : null;

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

            if (_applyVerb != null) {
                ((IStateManager) _applyVerb).TrackViewState();
            }
            if (_cancelVerb != null) {
                ((IStateManager) _cancelVerb).TrackViewState();
            }
            if (_okVerb != null) {
                ((IStateManager) _okVerb).TrackViewState();
            }
        }
    }
}

