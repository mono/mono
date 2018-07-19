//------------------------------------------------------------------------------
// <copyright file="CatalogPart.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Web.UI;

    /// <devdoc>
    /// Provides default rendering and part selection UI
    /// </devdoc>
    [
    Bindable(false),
    Designer("System.Web.UI.Design.WebControls.WebParts.CatalogPartDesigner, " + AssemblyRef.SystemDesign),
    ]
    public abstract class CatalogPart : Part {

        private WebPartManager _webPartManager;
        private CatalogZoneBase _zone;

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public string DisplayTitle {
            get {
                string displayTitle = Title;
                if (String.IsNullOrEmpty(displayTitle)) {
                    displayTitle = SR.GetString(SR.Part_Untitled);
                }
                return displayTitle;
            }
        }

        protected WebPartManager WebPartManager {
            get {
                return _webPartManager;
            }
        }

        protected CatalogZoneBase Zone {
            get {
                return _zone;
            }
        }

        public abstract WebPartDescriptionCollection GetAvailableWebPartDescriptions();

        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        protected override IDictionary GetDesignModeState() {
            IDictionary state = new HybridDictionary(1);
            state["Zone"] = Zone;
            return state;
        }

        public abstract WebPart GetWebPart(WebPartDescription description);

        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            if (Zone == null) {
                throw new InvalidOperationException(SR.GetString(SR.CatalogPart_MustBeInZone, ID));
            }
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        protected override void SetDesignModeState(IDictionary data) {
            if (data != null) {
                object o = data["Zone"];
                if (o != null) {
                    SetZone((CatalogZoneBase)o);
                }
            }
        }

        internal void SetWebPartManager(WebPartManager webPartManager) {
            _webPartManager = webPartManager;
        }

        internal void SetZone(CatalogZoneBase zone) {
            _zone = zone;
        }
    }
}
