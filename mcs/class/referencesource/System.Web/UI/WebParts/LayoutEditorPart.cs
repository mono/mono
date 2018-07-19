//------------------------------------------------------------------------------
// <copyright file="LayoutEditorPart.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public sealed class LayoutEditorPart : EditorPart {

        private DropDownList _chromeState;
        private DropDownList _zone;
        // 

        private TextBox _zoneIndex;

        private string _chromeStateErrorMessage;
        private string _zoneIndexErrorMessage;

        private const int TextBoxColumns = 10;
        private const int MinZoneIndex = 0;

        private bool CanChangeChromeState {
            get {
                WebPart webPart = WebPartToEdit;
                if (!webPart.Zone.AllowLayoutChange) {
                    return false;
                }
                else if (!webPart.AllowMinimize) {
                    // If AllowMinimize is false, the dropdown can be used to restore the WebPart
                    // but not to minimize the WebPart.
                    return (webPart.ChromeState == PartChromeState.Minimized);
                }
                else {
                    return true;
                }
            }
        }

        private bool CanChangeZone {
            get {
                WebPart webPart = WebPartToEdit;
                WebPartZoneBase currentZone = webPart.Zone;
                return (currentZone.AllowLayoutChange && webPart.AllowZoneChange);
            }
        }

        private bool CanChangeZoneIndex {
            get {
                return WebPartToEdit.Zone.AllowLayoutChange;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Themeable(false)]
        public override string DefaultButton {
            get { return base.DefaultButton; }
            set { base.DefaultButton = value; }
        }

        public override bool Display {
            get {
                // Don't return base.Display, since we want to show the layout editor part even
                // if AllowEdit is false on the WebPartToEdit, or the WebPartToEdit is a ProxyWebPart.
                return true;
            }
        }

        private bool HasError {
            get {
                return (_chromeStateErrorMessage != null || _zoneIndexErrorMessage != null);
            }
        }

        [
        WebSysDefaultValue(SR.LayoutEditorPart_PartTitle),
        ]
        public override string Title {
            get {
                string s = (string)ViewState["Title"];
                return (s != null) ? s : SR.GetString(SR.LayoutEditorPart_PartTitle);
            }
            set {
                ViewState["Title"] = value;
            }
        }

        public override bool ApplyChanges() {
            WebPart webPart = WebPartToEdit;
            if (webPart != null) {
                EnsureChildControls();

                try {
                    if (CanChangeChromeState) {
                        TypeConverter chromeStateConverter = TypeDescriptor.GetConverter(typeof(PartChromeState));
                        webPart.ChromeState = (PartChromeState)chromeStateConverter.ConvertFromString(_chromeState.SelectedValue);
                    }
                }
                catch (Exception e) {
                    _chromeStateErrorMessage = CreateErrorMessage(e.Message);
                }

                int zoneIndex = webPart.ZoneIndex;
                if (CanChangeZoneIndex) {
                    if (Int32.TryParse(_zoneIndex.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out zoneIndex)) {
                        if (zoneIndex < MinZoneIndex) {
                            _zoneIndexErrorMessage = SR.GetString(SR.EditorPart_PropertyMinValue, MinZoneIndex.ToString(CultureInfo.CurrentCulture));
                        }
                    }
                    else {
                        _zoneIndexErrorMessage = SR.GetString(SR.EditorPart_PropertyMustBeInteger);
                    }
                }

                WebPartZoneBase oldZone = webPart.Zone;
                WebPartZoneBase newZone = oldZone;
                if (CanChangeZone) {
                    newZone = WebPartManager.Zones[_zone.SelectedValue];
                }

                // Do not call MoveWebPart if the WebPart is currently in the correct position (VSWhidbey 374634)
                if (_zoneIndexErrorMessage == null && oldZone.AllowLayoutChange && newZone.AllowLayoutChange &&
                    (webPart.Zone != newZone || webPart.ZoneIndex != zoneIndex)) {
                    try {
                        WebPartManager.MoveWebPart(webPart, newZone, zoneIndex);
                    }
                    catch (Exception e) {
                        // Zone and ZoneIndex are set at the same time.  Use the _zoneIndexErrorMessage, since it is
                        // more likely that a bogus ZoneIndex would cause an error.
                        _zoneIndexErrorMessage = CreateErrorMessage(e.Message);
                    }
                }
            }

            return !HasError;
        }

        protected internal override void CreateChildControls() {
            ControlCollection controls = Controls;
            controls.Clear();

            TypeConverter chromeStateConverter = TypeDescriptor.GetConverter(typeof(PartChromeState));
            _chromeState = new DropDownList();
            _chromeState.Items.Add(new ListItem(SR.GetString(SR.PartChromeState_Normal),
                                               chromeStateConverter.ConvertToString(PartChromeState.Normal)));
            _chromeState.Items.Add(new ListItem(SR.GetString(SR.PartChromeState_Minimized),
                                               chromeStateConverter.ConvertToString(PartChromeState.Minimized)));
            controls.Add(_chromeState);

            // Add all zones to dropdown in CreateChildControls.  Items will be selected and/or disabled
            // in SyncChanges.  Assumes no Zones are added or removed during page execution.
            _zone = new DropDownList();
            WebPartManager manager = WebPartManager;
            if (manager != null) {
                WebPartZoneCollection zones = manager.Zones;
                if (zones != null) {
                    foreach (WebPartZoneBase zone in zones) {
                        ListItem item = new ListItem(zone.DisplayTitle, zone.ID);
                        _zone.Items.Add(item);
                    }
                }
            }
            controls.Add(_zone);

            _zoneIndex = new TextBox();
            _zoneIndex.Columns = TextBoxColumns;
            controls.Add(_zoneIndex);

            // We don't need viewstate enabled on our child controls.  Disable for perf.
            foreach (Control c in controls) {
                c.EnableViewState = false;
            }
        }

        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            // We want to synchronize the EditorPart to the state of the WebPart on every page load,
            // so we stay current if the WebPart changes in the background.
            if (Display && Visible && !HasError) {
                SyncChanges();
            }
        }

        protected internal override void RenderContents(HtmlTextWriter writer) {
            if (Page != null) {
                Page.VerifyRenderingInServerForm(this);
            }

            // HACK: Need this for child controls to be created at design-time when control is inside template
            EnsureChildControls();

            if (DesignMode) {
                // Add sample zone to dropdown
                _zone.Items.Add(SR.GetString(SR.Zone_SampleHeaderText));
            }

            string[] propertyDisplayNames = new string[] {
                SR.GetString(SR.LayoutEditorPart_ChromeState),
                SR.GetString(SR.LayoutEditorPart_Zone),
                SR.GetString(SR.LayoutEditorPart_ZoneIndex),
            };

            WebControl[] propertyEditors = new WebControl[] {
                _chromeState,
                _zone,
                _zoneIndex,
            };

            string[] errorMessages = new string[] {
                _chromeStateErrorMessage,
                null,
                _zoneIndexErrorMessage,
            };

            RenderPropertyEditors(writer, propertyDisplayNames, null /* propertyDescriptions */,
                                  propertyEditors, errorMessages);
        }

        public override void SyncChanges() {
            WebPart webPart = WebPartToEdit;

            Debug.Assert(webPart != null);
            if (webPart != null) {
                WebPartZoneBase currentZone = webPart.Zone;
                bool allowLayoutChange = currentZone.AllowLayoutChange;

                EnsureChildControls();

                TypeConverter chromeStateConverter = TypeDescriptor.GetConverter(typeof(PartChromeState));
                _chromeState.SelectedValue = chromeStateConverter.ConvertToString(webPart.ChromeState);
                _chromeState.Enabled = CanChangeChromeState;

                WebPartManager manager = WebPartManager;
                Debug.Assert(manager != null);
                if (manager != null) {
                    WebPartZoneCollection zones = manager.Zones;
                    bool allowZoneChange = webPart.AllowZoneChange;

                    _zone.ClearSelection();
                    foreach (ListItem item in _zone.Items) {
                        string zoneID = item.Value;
                        WebPartZoneBase zone = zones[zoneID];
                        if (zone == currentZone || (allowZoneChange && zone.AllowLayoutChange)) {
                            item.Enabled = true;
                        }
                        else {
                            item.Enabled = false;
                        }

                        if (zone == currentZone) {
                            item.Selected = true;
                        }
                    }

                    _zone.Enabled = CanChangeZone;
                }

                _zoneIndex.Text = webPart.ZoneIndex.ToString(CultureInfo.CurrentCulture);
                _zoneIndex.Enabled = CanChangeZoneIndex;
            }
        }
    }
}
