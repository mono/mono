//------------------------------------------------------------------------------
// <copyright file="WebPartZone.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    /// <devdoc>
    /// Zone that hosts WebPart controls, and contains a template to specify the contained WebParts.
    /// </devdoc>
    [
    Designer("System.Web.UI.Design.WebControls.WebParts.WebPartZoneDesigner, " + AssemblyRef.SystemDesign),
    SupportsEventValidation,
    ]
    public class WebPartZone : WebPartZoneBase {

        private ITemplate _zoneTemplate;
        private bool _registrationComplete;

        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateInstance(TemplateInstance.Single)
        ]
        public virtual ITemplate ZoneTemplate {
            get {
                return _zoneTemplate;
            }
            set {
                if (!DesignMode) {
                    if (_registrationComplete) {
                        throw new InvalidOperationException(SR.GetString(SR.WebPart_SetZoneTemplateTooLate));
                    }
                }
                _zoneTemplate = value;
            }
        }

        private void AddWebPartToList(WebPartCollection webParts, Control control) {
            WebPart part = control as WebPart;

            // We used to throw an exception if the template contained a non-whitespace literal.
            // However, sometimes Venus would insert <br /> tags between the server controls
            // in the template.  So, we now just ignore all literals.
            if ((part == null) && !(control is LiteralControl)) {
                WebPartManager manager = WebPartManager;
                if (manager != null) {
                    part = manager.CreateWebPart(control);
                }
                else {
                    part = WebPartManager.CreateWebPartStatic(control);
                }
            }

            if (part != null) {
                webParts.Add(part);
            }
        }

        protected internal override WebPartCollection GetInitialWebParts() {
            WebPartCollection webParts = new WebPartCollection();

            if (ZoneTemplate != null) {
                // PERF: Instantiate the template into a special control, that does nothing when a child control
                // is added.  This is more performant because the child control is never parented to the temporary
                // control, it's ID is never generated, etc.
                Control container = new NonParentingControl();
                ZoneTemplate.InstantiateIn(container);

                if (container.HasControls()) {
                    ControlCollection controls = container.Controls;
                    foreach (Control control in controls) {
                        if (control is ContentPlaceHolder) {
                            if (control.HasControls()) {
                                Control[] children = new Control[control.Controls.Count];
                                control.Controls.CopyTo(children, 0);
                                foreach (Control child in children) {
                                    AddWebPartToList(webParts, child);
                                }
                            }
                        }
                        else {
                            AddWebPartToList(webParts, control);
                        }
                    }
                }
            }

            return webParts;
        }

        protected internal override void OnInit(EventArgs e) {
            base.OnInit(e);
            _registrationComplete = true;
        }
    }
}

