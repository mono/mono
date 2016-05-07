//------------------------------------------------------------------------------
// <copyright file="UpdatePanelControlTrigger.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.UI {
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.Resources;

    public abstract class UpdatePanelControlTrigger : UpdatePanelTrigger {
        private string _controlID;

        protected UpdatePanelControlTrigger() {
        }

        [
        Category("Behavior"),
        DefaultValue(""),
        IDReferenceProperty(),
        ResourceDescription("UpdatePanelControlTrigger_ControlID"),
        SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID")
        ]
        public string ControlID {
            get {
                return _controlID ?? String.Empty;
            }
            set {
                _controlID = value;
            }
        }

        protected Control FindTargetControl(bool searchNamingContainers) {
            if (String.IsNullOrEmpty(ControlID)) {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.UpdatePanelControlTrigger_NoControlID, Owner.ID));
            }
            Control foundControl = ControlUtil.FindTargetControl(ControlID, Owner, searchNamingContainers);
            if (foundControl == null) {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.UpdatePanelControlTrigger_ControlNotFound, ControlID, Owner.ID));
            }
            return foundControl;
        }
    }
}
