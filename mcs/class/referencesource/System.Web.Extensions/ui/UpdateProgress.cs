//------------------------------------------------------------------------------
// <copyright file="UpdateProgress.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.UI {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Globalization;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.Script;
    using System.Web.Resources;
    using System.Web.Util;

    [
    DefaultProperty("AssociatedUpdatePanelID"),
    Designer("System.Web.UI.Design.UpdateProgressDesigner, " + AssemblyRef.SystemWebExtensionsDesign),
    ParseChildren(true),
    PersistChildren(false),
    ToolboxBitmap(typeof(EmbeddedResourceFinder), "System.Web.Resources.UpdateProgress.bmp")
    ]
    public class UpdateProgress : Control, IAttributeAccessor, IScriptControl {

        private AttributeCollection _attributes;
        private ITemplate _progressTemplate;
        private Control _progressTemplateContainer;
        private int _displayAfter = 500;
        private bool _dynamicLayout = true;
        private string _associatedUpdatePanelID;

        [
        Category("Behavior"),
        DefaultValue(""),
        IDReferenceProperty(typeof(UpdatePanel)),
        ResourceDescription("UpdateProgress_AssociatedUpdatePanelID"),
        SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID"),
        TypeConverter("System.Web.UI.Design.UpdateProgressAssociatedUpdatePanelIDConverter")
        ]
        public string AssociatedUpdatePanelID {
            get {
                if (_associatedUpdatePanelID == null) {
                    return String.Empty;
                }
                return _associatedUpdatePanelID;
            }
            set {
                _associatedUpdatePanelID = value;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.WebControl_Attributes)
        ]
        public AttributeCollection Attributes {
            get {
                if (_attributes == null) {
                    StateBag bag = new StateBag(true /* ignoreCase */);
                    _attributes = new AttributeCollection(bag);
                }
                return _attributes;
            }
        }

        public override ControlCollection Controls {
            get {
                EnsureChildControls();
                return base.Controls;
            }
        }

        [
        DefaultValue(500),
        ResourceDescription("UpdateProgress_DisplayAfter"),
        Category("Behavior")
        ]
        public int DisplayAfter {
            get {
                return _displayAfter;
            }
            set {
                if (value < 0) {
                    throw new ArgumentOutOfRangeException(AtlasWeb.UpdateProgress_DisplayAfterInvalid);
                }
                _displayAfter = value;
            }
        }

        [
        Browsable(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        ResourceDescription("UpdateProgress_ProgressTemplate"),
        ]
        public ITemplate ProgressTemplate {
            get {
                return _progressTemplate;
            }
            set {
                _progressTemplate = value;
            }
        }

        [
        DefaultValue(true),
        ResourceDescription("UpdateProgress_DynamicLayout"),
        Category("Behavior")
        ]
        public bool DynamicLayout {
            get {
                return _dynamicLayout;
            }
            set {
                _dynamicLayout = value;
            }
        }

        private ScriptManager ScriptManager {
            get {
                ScriptManager scriptManager = ScriptManager.GetCurrent(Page);
                if (scriptManager == null) {
                    throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.Common_ScriptManagerRequired, ID));
                }
                return scriptManager;
            }
        }

        protected internal override void CreateChildControls() {
            // Set up the progress template
            if (_progressTemplate != null) {
                _progressTemplateContainer = new Control();
                _progressTemplate.InstantiateIn(_progressTemplateContainer);
                Controls.Add(_progressTemplateContainer);
            }
        }

        public override void DataBind() {
            EnsureChildControls();
            base.DataBind();
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            ScriptManager.RegisterScriptControl(this);
        }

        protected internal override void Render(HtmlTextWriter writer) {
            EnsureChildControls();

            writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
            if (_dynamicLayout) {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
            }
            else {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Visibility, "hidden");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "block");
            }
            if (_attributes != null) {
                _attributes.AddAttributes(writer);
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            base.Render(writer);
            writer.RenderEndTag(); // div

            if (!DesignMode) {
                ScriptManager.RegisterScriptDescriptors(this);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "Matches IScriptControl interface.")]
        protected virtual IEnumerable<ScriptReference> GetScriptReferences() {
            yield break;
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "Matches IScriptControl interface.")]
        protected virtual IEnumerable<ScriptDescriptor> GetScriptDescriptors() {
            // Don't render any scripts when partial rendering is not enabled
            if (Page != null && ScriptManager.SupportsPartialRendering && Visible) {
                ScriptControlDescriptor desc = new ScriptControlDescriptor("Sys.UI._UpdateProgress", ClientID);
                string updatePanelClientID = null;
                if (!String.IsNullOrEmpty(AssociatedUpdatePanelID)) {
                    // Try both the NamingContainer and the Page
                    UpdatePanel c = ControlUtil.FindTargetControl(AssociatedUpdatePanelID, this, true) as UpdatePanel;
                    if (c != null)
                        updatePanelClientID = c.ClientID;
                    else {
                        throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.UpdateProgress_NoUpdatePanel, AssociatedUpdatePanelID));
                    }
                }
                desc.AddProperty("associatedUpdatePanelId", updatePanelClientID);
                desc.AddProperty("dynamicLayout", DynamicLayout);
                desc.AddProperty("displayAfter", DisplayAfter);
                yield return desc;
            }

            yield break;
        }

        string IAttributeAccessor.GetAttribute(string key) {
            return (_attributes != null) ? _attributes[key] : null;
        }

        void IAttributeAccessor.SetAttribute(string key, string value) {
            Attributes[key] = value;
        }

        #region IScriptControl Members

        IEnumerable<ScriptReference> IScriptControl.GetScriptReferences() {
            return GetScriptReferences();
        }

        IEnumerable<ScriptDescriptor> IScriptControl.GetScriptDescriptors() {
            return GetScriptDescriptors();
        }

        #endregion
    }
}

