//------------------------------------------------------------------------------
// <copyright file="ExtenderControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.UI {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.Resources;
    using System.Web.Util;

    [
    DefaultProperty("TargetControlID"),
    Designer("System.Web.UI.Design.ExtenderControlDesigner, " + AssemblyRef.SystemWebExtensionsDesign),
    NonVisualControl(),
    ParseChildren(true),
    PersistChildren(false),
    ToolboxItem("System.Web.UI.Design.ExtenderControlToolboxItem, " + AssemblyRef.SystemWebExtensionsDesign),
    ]
    public abstract class ExtenderControl : Control, IExtenderControl {
        private string _targetControlID;
        private IScriptManagerInternal _scriptManager;
        private new IPage _page;

        protected ExtenderControl() {
        }

        internal ExtenderControl(IScriptManagerInternal scriptManager, IPage page) {
            _scriptManager = scriptManager;
            _page = page;
        }

        private IPage IPage {
            get {
                if (_page != null) {
                    return _page;
                }
                else {
                    Page page = Page;
                    if (page == null) {
                        throw new InvalidOperationException(AtlasWeb.Common_PageCannotBeNull);
                    }
                    return new PageWrapper(page);
                }
            }
        }

        private IScriptManagerInternal ScriptManager {
            get {
                if (_scriptManager == null) {
                    Page page = Page;
                    if (page == null) {
                        throw new InvalidOperationException(AtlasWeb.Common_PageCannotBeNull);
                    }
                    _scriptManager = System.Web.UI.ScriptManager.GetCurrent(page);
                    if (_scriptManager == null) {
                        throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                            AtlasWeb.Common_ScriptManagerRequired, ID));
                    }
                }
                return _scriptManager;
            }
        }

        [
        Category("Behavior"),
        DefaultValue(""),
        IDReferenceProperty,
        ResourceDescription("ExtenderControl_TargetControlID"),
        SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID"),
        ]
        public string TargetControlID {
            get {
                return (_targetControlID == null) ? String.Empty : _targetControlID;
            }
            set {
                _targetControlID = value;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override bool Visible {
            get {
                return base.Visible;
            }
            set {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Finds the closest UpdatePanel in the parent controls of the specified control.
        /// </summary>
        /// <param name="control">The control for which we want the closest UpdatePanel.</param>
        /// <returns>An UpdatePanel or null if none was found.</returns>
        private static UpdatePanel FindUpdatePanel(Control control) {
            Control parent = control.Parent;
            while (parent != null) {
                UpdatePanel parentPanel = parent as UpdatePanel;
                if (parentPanel != null) {
                    return parentPanel;
                }
                parent = parent.Parent;
            }
            return null;
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);
            RegisterWithScriptManager();
        }

        private void RegisterWithScriptManager() {
            if (String.IsNullOrEmpty(TargetControlID)) {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                    AtlasWeb.ExtenderControl_TargetControlIDEmpty, ID));
            }

            Control targetControl = FindControl(TargetControlID);
            if (targetControl == null) {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                    AtlasWeb.ExtenderControl_TargetControlIDInvalid, ID, TargetControlID));
            }

            // Check if the target control and its extender are in the same UpdatePanel if any:
            if (FindUpdatePanel(this) != FindUpdatePanel(targetControl)) {
                throw new InvalidOperationException(AtlasWeb.ExtenderControl_TargetControlDifferentUpdatePanel);
            }

            ScriptManager.RegisterExtenderControl(this, targetControl);
        }

        protected internal override void Render(HtmlTextWriter writer) {
            base.Render(writer);

            // DevDiv 97460: ScriptDescriptors only render if in server form, verify to avoid silently failing.
            IPage.VerifyRenderingInServerForm(this);

            // ScriptManager cannot be found in DesignMode, so do not attempt to register scripts.
            if (!DesignMode) {
                ScriptManager.RegisterScriptDescriptors(this);
            }
        }

        protected abstract IEnumerable<ScriptDescriptor> GetScriptDescriptors(Control targetControl);

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "Implementation will likely return a new collection, which is too slow for a property")]
        protected abstract IEnumerable<ScriptReference> GetScriptReferences();

        #region IExtenderControl Members
        IEnumerable<ScriptDescriptor> IExtenderControl.GetScriptDescriptors(Control targetControl) {
            return GetScriptDescriptors(targetControl);
        }

        IEnumerable<ScriptReference> IExtenderControl.GetScriptReferences() {
            return GetScriptReferences();
        }
        #endregion
    }
}
