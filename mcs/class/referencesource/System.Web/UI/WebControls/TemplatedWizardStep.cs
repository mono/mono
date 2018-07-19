//------------------------------------------------------------------------------
// <copyright file="TemplatedWizardStep.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    [
    Bindable(false),
    ControlBuilderAttribute(typeof(WizardStepControlBuilder)),
    ParseChildren(true),
    PersistChildren(false),
    ToolboxItem(false),
    Themeable(true)
    ]

    public class TemplatedWizardStep : WizardStepBase {
        private ITemplate _contentTemplate;
        private Control _contentContainer;
        private ITemplate _navigationTemplate;
        private Control _navigationContainer;


        [
        Browsable(false),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(Wizard)),
        WebSysDescription(SR.TemplatedWizardStep_ContentTemplate)
        ]
        public virtual ITemplate ContentTemplate {
            get {
                return _contentTemplate;
            }
            set {
                _contentTemplate = value;
                if (Owner != null && ControlState > ControlState.Constructed) {
                    Owner.RequiresControlsRecreation();
                }
            }
        }


        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public Control ContentTemplateContainer {
            get {
                return _contentContainer;
            }
            internal set {
                _contentContainer = value;
            }
        }


        [
        Browsable(false),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(Wizard)),
        WebSysDescription(SR.TemplatedWizardStep_CustomNavigationTemplate)
        ]
        public virtual ITemplate CustomNavigationTemplate {
            get {
                return _navigationTemplate;
            }
            set {
                _navigationTemplate = value;
                if (Owner != null && ControlState > ControlState.Constructed) {
                    Owner.RequiresControlsRecreation();
                }
            }
        }


        [
        Browsable(false),
        Bindable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public Control CustomNavigationTemplateContainer {
            get {
                return _navigationContainer;
            }
            internal set {
                _navigationContainer = value;
            }
        }

        [
        Browsable(true)
        ]
        public override string SkinID {
            get {
                return base.SkinID;
            }
            set {
                base.SkinID = value;
            }
        }
    }
}
