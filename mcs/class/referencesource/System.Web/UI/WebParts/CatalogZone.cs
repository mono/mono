//------------------------------------------------------------------------------
// <copyright file="CatalogZone.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.Util;

    [
    Designer("System.Web.UI.Design.WebControls.WebParts.CatalogZoneDesigner, " + AssemblyRef.SystemDesign),
    SupportsEventValidation,
    ]
    public class CatalogZone : CatalogZoneBase {

        private ITemplate _zoneTemplate;

        protected override CatalogPartCollection CreateCatalogParts() {
            CatalogPartCollection catalogParts = new CatalogPartCollection();

            if (_zoneTemplate != null) {
                // PERF: Instantiate the template into a special control, that does nothing when a child control
                // is added.  This is more performant because the child control is never parented to the temporary
                // control, it's ID is never generated, etc.
                Control container = new NonParentingControl();

                _zoneTemplate.InstantiateIn(container);
                if (container.HasControls()) {
                    foreach (Control control in container.Controls) {
                        CatalogPart part = control as CatalogPart;

                        if (part != null) {
                            catalogParts.Add(part);
                        }
                        else {
                            LiteralControl literal = control as LiteralControl;
                            // Throw an exception if it is *not* a literal containing only whitespace
                            // Don't throw an exception in the designer, since we want only the offending
                            // control to render as an error block, not the whole CatalogZone.
                            if (((literal == null) || (literal.Text.Trim().Length != 0)) && !DesignMode) {
                                throw new InvalidOperationException(SR.GetString(SR.CatalogZone_OnlyCatalogParts, ID));
                            }
                        }
                    }
                }
            }

            return catalogParts;
        }

        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(CatalogZone)),
        TemplateInstance(TemplateInstance.Single)
        ]
        public virtual ITemplate ZoneTemplate {
            get {
                return _zoneTemplate;
            }
            set {
                InvalidateCatalogParts();
                _zoneTemplate = value;
            }
        }
    }
}
