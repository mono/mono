//------------------------------------------------------------------------------
// <copyright file="CatalogZoneBase.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.Util;

    public abstract class CatalogZoneBase : ToolZone, IPostBackDataHandler {

        private CatalogPartCollection _catalogParts;

        private string[] _selectedCheckBoxValues;
        private string _selectedZoneID;
        private string _selectedCatalogPartID;

        private const int baseIndex = 0;
        private const int addVerbIndex = 1;
        private const int closeVerbIndex = 2;
        private const int partLinkStyleIndex = 3;
        private const int selectedPartLinkStyleIndex = 4;
        private const int viewStateArrayLength = 5;

        // Use same baseIndex as above
        private const int selectedCatalogPartIDIndex = 1;
        private const int controlStateArrayLength = 2;

        private WebPartVerb _addVerb;
        private WebPartVerb _closeVerb;
        private Style _partLinkStyle;
        private Style _selectedPartLinkStyle;
        private CatalogPartChrome _catalogPartChrome;

        private const string addEventArgument = "add";
        private const string closeEventArgument = "close";
        private const string selectEventArgument = "select";

        protected CatalogZoneBase() : base(WebPartManager.CatalogDisplayMode) {
        }

        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Verbs"),
        WebSysDescription(SR.CatalogZoneBase_AddVerb),
        ]
        public virtual WebPartVerb AddVerb {
            get {
                if (_addVerb == null) {
                    _addVerb = new WebPartCatalogAddVerb();
                    _addVerb.EventArgument = addEventArgument;
                    if (IsTrackingViewState) {
                        ((IStateManager)_addVerb).TrackViewState();
                    }
                }

                return _addVerb;
            }
        }

        internal string CheckBoxName {
            get {
                return UniqueID + ID_SEPARATOR + "_checkbox";
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public CatalogPartChrome CatalogPartChrome {
            get {
                if (_catalogPartChrome == null) {
                    _catalogPartChrome = CreateCatalogPartChrome();
                }
                return _catalogPartChrome;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public CatalogPartCollection CatalogParts {
            get {
                if (_catalogParts == null) {
                    CatalogPartCollection catalogParts = CreateCatalogParts();

                    // Verify that each CatalogPart has a nonempty ID.  Don't throw an exception in the designer,
                    // since we want only the offending control to render as an error block, not the whole CatalogZone.
                    if (!DesignMode) {
                        foreach (CatalogPart catalogPart in catalogParts) {
                            if (String.IsNullOrEmpty(catalogPart.ID)) {
                                throw new InvalidOperationException(SR.GetString(SR.CatalogZoneBase_NoCatalogPartID));
                            }
                        }
                    }

                    _catalogParts = catalogParts;

                    // Call EnsureChildControls to parent the CatalogParts and set the WebPartManager, and Zone
                    EnsureChildControls();
                }

                return _catalogParts;
            }
        }

        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Verbs"),
        WebSysDescription(SR.CatalogZoneBase_CloseVerb),
        ]
        public virtual WebPartVerb CloseVerb {
            get {
                if (_closeVerb == null) {
                    _closeVerb = new WebPartCatalogCloseVerb();
                    _closeVerb.EventArgument = closeEventArgument;
                    if (IsTrackingViewState) {
                        ((IStateManager)_closeVerb).TrackViewState();
                    }
                }

                return _closeVerb;
            }
        }

        [
        WebSysDefaultValue(SR.CatalogZoneBase_DefaultEmptyZoneText)
        ]
        public override string EmptyZoneText {
            // Must look at viewstate directly instead of the property in the base class,
            // so we can distinguish between an unset property and a property set to String.Empty.
            get {
                string s = (string)ViewState["EmptyZoneText"];
                return((s == null) ? SR.GetString(SR.CatalogZoneBase_DefaultEmptyZoneText) : s);
            }
            set {
                ViewState["EmptyZoneText"] = value;
            }
        }

        [
        WebSysDefaultValue(SR.CatalogZoneBase_HeaderText),
        ]
        public override string HeaderText {
            get {
                string s = (string)ViewState["HeaderText"];
                return((s == null) ? SR.GetString(SR.CatalogZoneBase_HeaderText) : s);
            }
            set {
                ViewState["HeaderText"] = value;
            }
        }

        [
        WebSysDefaultValue(SR.CatalogZoneBase_InstructionText),
        ]
        public override string InstructionText {
            get {
                string s = (string)ViewState["InstructionText"];
                return((s == null) ? SR.GetString(SR.CatalogZoneBase_InstructionText) : s);
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
        WebSysDescription(SR.CatalogZoneBase_PartLinkStyle),
        ]
        public Style PartLinkStyle {
            get {
                if (_partLinkStyle == null) {
                    _partLinkStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager)_partLinkStyle).TrackViewState();
                    }
                }

                return _partLinkStyle;
            }
        }

        [
        DefaultValue(""),
        Themeable(false),
        WebCategory("Behavior"),
        WebSysDescription(SR.CatalogZoneBase_SelectedCatalogPartID),
        ]
        public string SelectedCatalogPartID {
            get {
                if (String.IsNullOrEmpty(_selectedCatalogPartID)) {
                    if (DesignMode) {
                        return String.Empty;
                    }
                    else {
                        CatalogPartCollection catalogParts = CatalogParts;
                        if (catalogParts != null && catalogParts.Count > 0) {
                            return catalogParts[0].ID;
                        } else {
                            return String.Empty;
                        }
                    }
                }
                else {
                    return _selectedCatalogPartID;
                }
            }
            set {
                _selectedCatalogPartID = value;
            }
        }

        // 
        private CatalogPart SelectedCatalogPart {
            get {
                CatalogPartCollection catalogParts = CatalogParts;
                if (catalogParts != null && catalogParts.Count > 0) {
                    if (String.IsNullOrEmpty(_selectedCatalogPartID)) {
                        return catalogParts[0];
                    }
                    else {
                        return catalogParts[_selectedCatalogPartID];
                    }
                }
                else {
                    // If there are no catalog parts, return null
                    return null;
                }
            }
        }

        [
        Localizable(true),
        WebSysDefaultValue(SR.CatalogZoneBase_DefaultSelectTargetZoneText),
        WebCategory("Behavior"),
        WebSysDescription(SR.CatalogZoneBase_SelectTargetZoneText),
        ]
        public virtual string SelectTargetZoneText {
            get {
                string s = (string)ViewState["SelectTargetZoneText"];
                return((s == null) ? SR.GetString(SR.CatalogZoneBase_DefaultSelectTargetZoneText) : s);
            }
            set {
                ViewState["SelectTargetZoneText"] = value;
            }
        }

        [
        DefaultValue(null),
        NotifyParentProperty(true),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Styles"),
        WebSysDescription(SR.CatalogZoneBase_SelectedPartLinkStyle),
        ]
        public Style SelectedPartLinkStyle {
            get {
                if (_selectedPartLinkStyle == null) {
                    _selectedPartLinkStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager)_selectedPartLinkStyle).TrackViewState();
                    }
                }

                return _selectedPartLinkStyle;
            }
        }

        [
        DefaultValue(true),
        WebCategory("Behavior"),
        WebSysDescription(SR.CatalogZoneBase_ShowCatalogIcons),
        ]
        public virtual bool ShowCatalogIcons {
            get {
                object b = ViewState["ShowCatalogIcons"];
                return (b != null) ? (bool)b : true;
            }
            set {
                ViewState["ShowCatalogIcons"] = value;
            }
        }

        private string ZonesID {
            get {
                return UniqueID + ID_SEPARATOR + "_zones";
            }
        }

        private void AddSelectedWebParts() {
            WebPartZoneBase selectedZone = null;
            if (WebPartManager != null) {
                selectedZone = WebPartManager.Zones[_selectedZoneID];
            }

            CatalogPart selectedCatalogPart = SelectedCatalogPart;
            WebPartDescriptionCollection availableWebParts = null;
            if (selectedCatalogPart != null) {
                availableWebParts = selectedCatalogPart.GetAvailableWebPartDescriptions();
            }

            if (selectedZone != null && selectedZone.AllowLayoutChange &&
                _selectedCheckBoxValues != null && availableWebParts != null) {
                ArrayList selectedWebParts = new ArrayList();

                // Fetch all of the WebParts before calling AddWebPart() on any of them.
                // This is necessary if the CatalogPart would refresh its list of
                // AvailableWebPartDescriptions in response to adding a WebPart.
                // PageCatalogPart is an example of this. (VSWhidbey 337539)
                for (int i = 0; i < _selectedCheckBoxValues.Length; i++) {
                    string value = _selectedCheckBoxValues[i];
                    WebPartDescription webPartDescription = availableWebParts[value];
                    if (webPartDescription != null) {
                        WebPart part = selectedCatalogPart.GetWebPart(webPartDescription);
                        if (part != null) {
                            selectedWebParts.Add(part);
                        }
                    }
                }

                AddWebParts(selectedWebParts, selectedZone);
            }
        }

        private void AddWebParts(ArrayList webParts, WebPartZoneBase zone) {
            // Add web parts from the list in reverse order, so they appear in the zone in the same
            // order they were returned from the catalog part. (VSWhidbey 77750)
            webParts.Reverse();

            foreach (WebPart part in webParts) {
                WebPartZoneBase targetZone = zone;
                if (part.AllowZoneChange == false && part.Zone != null) {
                    targetZone = part.Zone;
                }

                // WebPartManager is checked for null in AddWebParts()
                Debug.Assert(WebPartManager != null);
                // Add new parts to the top of the Zone, so the user will see them without scrolling the page
                WebPartManager.AddWebPart(part, targetZone, 0);
            }
        }

        protected override void Close() {
            if (WebPartManager != null) {
                WebPartManager.DisplayMode = WebPartManager.BrowseDisplayMode;
            }
        }

        protected virtual CatalogPartChrome CreateCatalogPartChrome() {
            return new CatalogPartChrome(this);
        }

        protected abstract CatalogPartCollection CreateCatalogParts();

        /// <internalonly/>
        protected internal override void CreateChildControls() {
            Controls.Clear();

            // 
            foreach (CatalogPart catalogPart in CatalogParts) {
                catalogPart.SetWebPartManager(WebPartManager);
                catalogPart.SetZone(this);
                Controls.Add(catalogPart);
            }
        }

        internal string GetCheckBoxID(string value) {
            return ClientID + ClientIDSeparator + "_checkbox" + ClientIDSeparator + value;
        }

        // Called by a derived class if the list of CatalogParts changes, and they want CreateCatalogParts()
        // to be called again.
        protected void InvalidateCatalogParts() {
            _catalogParts = null;
            ChildControlsCreated = false;
        }

        /// <devdoc>
        /// Loads the control state for those properties that should persist across postbacks
        /// even when EnableViewState=false.
        /// </devdoc>
        protected internal override void LoadControlState(object savedState) {
            if (savedState == null) {
                base.LoadControlState(null);
            }
            else {
                object[] myState = (object[])savedState;
                if (myState.Length != controlStateArrayLength) {
                    throw new ArgumentException(SR.GetString(SR.Invalid_ControlState));
                }

                base.LoadControlState(myState[baseIndex]);
                if (myState[selectedCatalogPartIDIndex] != null) {
                    _selectedCatalogPartID = (string)myState[selectedCatalogPartIDIndex];
                }
            }
        }

        protected virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection) {

            string selectedCheckBoxValues = postCollection[CheckBoxName];
            if (!String.IsNullOrEmpty(selectedCheckBoxValues)) {

                //Validate postback reference if exists in the postdata.
                ValidateEvent(CheckBoxName);

                _selectedCheckBoxValues = selectedCheckBoxValues.Split(',');
            }

            _selectedZoneID = postCollection[ZonesID];

            // Do not raise a changed event
            return false;
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
                if (myState[addVerbIndex] != null) {
                    ((IStateManager) AddVerb).LoadViewState(myState[addVerbIndex]);
                }
                if (myState[closeVerbIndex] != null) {
                    ((IStateManager) CloseVerb).LoadViewState(myState[closeVerbIndex]);
                }
                if (myState[partLinkStyleIndex] != null) {
                    ((IStateManager) PartLinkStyle).LoadViewState(myState[partLinkStyleIndex]);
                }
                if (myState[selectedPartLinkStyleIndex] != null) {
                    ((IStateManager) SelectedPartLinkStyle).LoadViewState(myState[selectedPartLinkStyleIndex]);
                }
            }
        }

        protected internal override void OnInit(EventArgs e) {
            base.OnInit(e);

            Page page = Page;
            Debug.Assert(page != null);
            if (page != null) {
                page.RegisterRequiresControlState(this);
            }
        }

        // We don't need to handle WebPartManager.DisplayModeChanged in this class.
        // We need it in EditorZoneBase since the available editor parts changes when the
        // WebPartToEdit changes, but the list of catalog parts never changes
        // when the DisplayMode changes.

        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);
            CatalogPartChrome.PerformPreRender();
            Page.RegisterRequiresPostBack(this);
        }

        protected override void RaisePostBackEvent(string eventArgument) {
            string[] eventArguments = eventArgument.Split(ID_SEPARATOR);

            if ((eventArguments.Length == 2) && (eventArguments[0] == selectEventArgument)) {
                SelectedCatalogPartID = eventArguments[1];
            }
            else if (String.Equals(eventArgument, addEventArgument, StringComparison.OrdinalIgnoreCase)) {
                if (AddVerb.Visible && AddVerb.Enabled) {
                    AddSelectedWebParts();
                }
            }
            else if (String.Equals(eventArgument, closeEventArgument, StringComparison.OrdinalIgnoreCase)) {
                if (CloseVerb.Visible && CloseVerb.Enabled) {
                    Close();
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

            CatalogPartCollection catalogParts = CatalogParts;
            if (catalogParts != null && catalogParts.Count > 0) {
                bool firstCell = true;
                // Only render links if there is more than 1 catalog part (VSWhidbey 77672)
                if (catalogParts.Count > 1) {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    firstCell = false;
                    RenderCatalogPartLinks(writer);
                    writer.RenderEndTag();  // Td
                    writer.RenderEndTag();  // Tr
                }

                CatalogPartChrome chrome = CatalogPartChrome;
                if (DesignMode) {
                    foreach (CatalogPart catalogPart in catalogParts) {
                        RenderCatalogPart(writer, catalogPart, chrome, ref firstCell);
                    }
                }
                else {
                    CatalogPart selectedCatalogPart = SelectedCatalogPart;
                    if (selectedCatalogPart != null) {
                        RenderCatalogPart(writer, selectedCatalogPart, chrome, ref firstCell);
                    }
                }

                writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                // Mozilla renders padding on an empty TD without this attribute
                writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, "0");

                // Add an extra row with height of 100%, to Microsoft up any extra space
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

        private void RenderCatalogPart(HtmlTextWriter writer, CatalogPart catalogPart, CatalogPartChrome chrome, ref bool firstCell) {
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            if (!firstCell) {
                writer.AddStyleAttribute(HtmlTextWriterStyle.PaddingTop, "0");
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            firstCell = false;

            chrome.RenderCatalogPart(writer, catalogPart);

            writer.RenderEndTag();  // Td
            writer.RenderEndTag();  // Tr
        }

        protected virtual void RenderCatalogPartLinks(HtmlTextWriter writer) {
            RenderInstructionText(writer);

            CatalogPart selectedCatalogPart = SelectedCatalogPart;
            foreach (CatalogPart catalogPart in CatalogParts) {
                WebPartDescriptionCollection availableWebParts = catalogPart.GetAvailableWebPartDescriptions();
                int count = ((availableWebParts != null) ? availableWebParts.Count : 0);

                string displayTitle = catalogPart.DisplayTitle;
                // 
                string text = displayTitle + " (" + count.ToString(CultureInfo.CurrentCulture) + ")";

                if (catalogPart == selectedCatalogPart) {
                    Label label = new Label();
                    label.Text = text;
                    label.Page = Page;
                    label.ApplyStyle(SelectedPartLinkStyle);
                    label.RenderControl(writer);
                }
                else {
                    Debug.Assert(!String.IsNullOrEmpty(catalogPart.ID));
                    string eventArgument = selectEventArgument + ID_SEPARATOR + catalogPart.ID;

                    ZoneLinkButton linkButton = new ZoneLinkButton(this, eventArgument);
                    linkButton.Text = text;
                    linkButton.ToolTip = SR.GetString(SR.CatalogZoneBase_SelectCatalogPart, displayTitle);
                    linkButton.Page = Page;
                    linkButton.ApplyStyle(PartLinkStyle);
                    linkButton.RenderControl(writer);
                }

                writer.WriteBreak();
            }

            writer.WriteBreak();
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

        protected override void RenderFooter(HtmlTextWriter writer) {
            writer.AddStyleAttribute(HtmlTextWriterStyle.Margin, "4px");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            DropDownList zonesDropDownList = new DropDownList();
            zonesDropDownList.ClientIDMode = ClientIDMode.AutoID;
            zonesDropDownList.ID = ZonesID;

            // Populate the DropDownList
            if (DesignMode) {
                // Add sample zone to dropdown
                zonesDropDownList.Items.Add(SR.GetString(SR.Zone_SampleHeaderText));
            }
            else {
                if (WebPartManager != null && WebPartManager.Zones != null) {
                    foreach (WebPartZoneBase zone in WebPartManager.Zones) {
                        if (zone.AllowLayoutChange) {
                            Debug.Assert(!String.IsNullOrEmpty(zone.ID));
                            ListItem item = new ListItem(zone.DisplayTitle, zone.ID);
                            if (String.Equals(zone.ID, _selectedZoneID, StringComparison.OrdinalIgnoreCase)) {
                                item.Selected = true;
                            }
                            zonesDropDownList.Items.Add(item);
                        }
                    }
                }
            }

            LabelStyle.AddAttributesToRender(writer, this);
            // Only render the "for" attribute if we are going to render the associated DropDownList (VSWhidbey 541458)
            if (zonesDropDownList.Items.Count > 0) {
                writer.AddAttribute(HtmlTextWriterAttribute.For, zonesDropDownList.ClientID);
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Label);
            writer.Write(SelectTargetZoneText);
            writer.RenderEndTag();

            // Render &nbsp; before the DropDownList (VSWhidbey 77709)
            writer.Write("&nbsp;");

            zonesDropDownList.ApplyStyle(EditUIStyle);
            // Do not render empty DropDownList (VSWhidbey 534498)
            if (zonesDropDownList.Items.Count > 0) {
                zonesDropDownList.RenderControl(writer);
            }

            writer.Write("&nbsp;");

            RenderVerbs(writer);

            writer.RenderEndTag();  // Div
        }

        private void RenderInstructionText(HtmlTextWriter writer) {
            string instructionText = InstructionText;
            if (!String.IsNullOrEmpty(instructionText)) {
                Label label = new Label();
                label.Text = instructionText;
                label.Page = Page;
                label.ApplyStyle(InstructionTextStyle);
                label.RenderControl(writer);
                writer.WriteBreak();
                writer.WriteBreak();
            }
        }

        protected override void RenderVerbs(HtmlTextWriter writer) {
            int count = 0;
            bool originalAddVerbEnabled = false;

            CatalogPart selectedCatalogPart = SelectedCatalogPart;
            if (selectedCatalogPart != null) {
                WebPartDescriptionCollection availableWebParts = selectedCatalogPart.GetAvailableWebPartDescriptions();
                count = ((availableWebParts != null) ? availableWebParts.Count : 0);
            }

            // If the current CatalogPart has no WebPartDescriptions, disable the AddVerb
            if (count == 0) {
                originalAddVerbEnabled = AddVerb.Enabled;
                AddVerb.Enabled = false;
            }

            try {
                RenderVerbsInternal(writer, new WebPartVerb[] {AddVerb, CloseVerb});
            }
            finally {
                if (count == 0) {
                    AddVerb.Enabled = originalAddVerbEnabled;
                }
            }
        }

        /// <devdoc>
        /// Saves the control state for those properties that should persist across postbacks
        /// even when EnableViewState=false.
        /// </devdoc>
        protected internal override object SaveControlState() {
            object[] myState = new object[controlStateArrayLength];

            myState[baseIndex] = base.SaveControlState();
            if (!String.IsNullOrEmpty(_selectedCatalogPartID)) {
                myState[selectedCatalogPartIDIndex] = _selectedCatalogPartID;
            }

            for (int i=0; i < controlStateArrayLength; i++) {
                if (myState[i] != null) {
                    return myState;
                }
            }

            // More performant to return null than an array of null values
            return null;
        }

        protected override object SaveViewState() {
            object[] myState = new object[viewStateArrayLength];

            myState[baseIndex] = base.SaveViewState();
            myState[addVerbIndex] = (_addVerb != null) ? ((IStateManager)_addVerb).SaveViewState() : null;
            myState[closeVerbIndex] = (_closeVerb != null) ? ((IStateManager)_closeVerb).SaveViewState() : null;
            myState[partLinkStyleIndex] = (_partLinkStyle != null) ? ((IStateManager)_partLinkStyle).SaveViewState() : null;
            myState[selectedPartLinkStyleIndex] = (_selectedPartLinkStyle != null) ? ((IStateManager)_selectedPartLinkStyle).SaveViewState() : null;

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

            if (_addVerb != null) {
                ((IStateManager) _addVerb).TrackViewState();
            }
            if (_closeVerb != null) {
                ((IStateManager) _closeVerb).TrackViewState();
            }
            if (_partLinkStyle != null) {
                ((IStateManager) _partLinkStyle).TrackViewState();
            }
            if (_selectedPartLinkStyle != null) {
                ((IStateManager) _selectedPartLinkStyle).TrackViewState();
            }
        }

        #region Implementation of IPostBackDataHandler
        bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection) {
            return LoadPostData(postDataKey, postCollection);
        }

        void IPostBackDataHandler.RaisePostDataChangedEvent() {
        }
        #endregion
    }
}

