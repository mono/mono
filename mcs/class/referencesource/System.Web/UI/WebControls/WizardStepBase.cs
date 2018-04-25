//------------------------------------------------------------------------------
// <copyright file="WizardStepBase.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    public sealed class WizardStepControlBuilder : ControlBuilder {
        internal override void SetParentBuilder(ControlBuilder parentBuilder) {
            // Do not check containment at designtime or in a skin file.
            if (Parser.FInDesigner || Parser is PageThemeParser)
                return;

            if (parentBuilder.ControlType == null ||
                !(typeof(WizardStepCollection).IsAssignableFrom(parentBuilder.ControlType))) {
                throw new HttpException(SR.GetString(SR.WizardStep_WrongContainment));
            }

            base.SetParentBuilder(parentBuilder);
        }
    }

    [
    Bindable(false),
    ControlBuilderAttribute(typeof(WizardStepControlBuilder)),
    ToolboxItem(false)
    ]
    public abstract class WizardStepBase : View {
        private Wizard _owner;

        [
        WebCategory("Behavior"),
        Themeable(false),
        Filterable(false),
        DefaultValue(true),
        WebSysDescription(SR.WizardStep_AllowReturn)
        ]
        public virtual bool AllowReturn {
            get {
                object o = ViewState["AllowReturn"];
                return o == null? true : (bool)o;
            }
            set {
                ViewState["AllowReturn"] = value;
            }
        }

        /// <devdoc>
        ///    <para>Gets and sets a value indicating whether theme is enabled.</para>
        /// </devdoc>
        [
        Browsable(true)
        ]
        public override bool EnableTheming {
            get {
                return base.EnableTheming;
            }
            set {
                base.EnableTheming = value;
            }
        }

        public override string ID {
            get {
                return base.ID;
            }
            set {
                // VSWhidbey 407127. Need to validate control ID here since WiardStepBase does not have a designer.
                if (Owner != null && Owner.DesignMode) {
                    if (!CodeGenerator.IsValidLanguageIndependentIdentifier(value)) {
                        throw new ArgumentException(SR.GetString(SR.Invalid_identifier, value));
                    }

                    if (value != null && value.Equals(Owner.ID, StringComparison.OrdinalIgnoreCase)) {
                        throw new ArgumentException(SR.GetString(SR.Id_already_used, value));
                    }

                    foreach (WizardStepBase step in Owner.WizardSteps) {
                        if (step == this) {
                            continue;
                        }

                        if (step.ID != null && step.ID.Equals(value, StringComparison.OrdinalIgnoreCase)) {
                            throw new ArgumentException(SR.GetString(SR.Id_already_used, value));
                        }
                    }
                }

                base.ID = value;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebCategory("Appearance"),
        WebSysDescription(SR.WizardStep_Name)
        ]
        public virtual String Name {
            get {
                if (!String.IsNullOrEmpty(Title)) {
                    return Title;
                }

                if (!String.IsNullOrEmpty(ID)) {
                    return ID;
                }

                return null;
            }
        }

        internal virtual Wizard Owner {
            get {
                return _owner;
            }
            set {
                _owner = value;
            }
        }

        [
        WebCategory("Behavior"),
        DefaultValue(WizardStepType.Auto),
        WebSysDescription(SR.WizardStep_StepType)
        ]
        public virtual WizardStepType StepType {
            get {
                object o = ViewState["StepType"];
                return o == null? WizardStepType.Auto : (WizardStepType)o;
            }
            set {
                if (value < WizardStepType.Auto || value > WizardStepType.Step) {
                    throw new ArgumentOutOfRangeException("value");
                }

                if (StepType != value) {
                    ViewState["StepType"] = value;
                    if (Owner != null) {
                        Owner.OnWizardStepsChanged();
                    }
                }
            }
        }

        /// <devdoc>
        /// <para>Gets or sets the title on the <see cref='System.Web.UI.WebControls.Wizard/> .</para>
        /// </devdoc>
        [
        DefaultValue(""),
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDescription(SR.WizardStep_Title)
        ]
        public virtual string Title {
            get {
                string s = (string)ViewState["Title"];
                return((s == null) ? String.Empty : s);
            }
            set {
                if (Title != value) {
                    ViewState["Title"] = value;
                    if (Owner != null) {
                        Owner.OnWizardStepsChanged();
                    }
                }
            }
        }

        internal string TitleInternal {
            get {
                return (string)ViewState["Title"];
            }
        }

        [
        Browsable(false),
        EditorBrowsable(EditorBrowsableState.Advanced),
        WebCategory("Appearance"),
        ]
        public Wizard Wizard {
            get {
                return Owner;
            }
        }

        // VSWhidbey 397000, need to notify the Owner of type change so
        // the sidebar can be re-databound correctly.
        protected override void LoadViewState(object savedState) {
            if (savedState != null) {
                base.LoadViewState(savedState);

                if (Owner != null &&
                    (ViewState["Title"] != null || ViewState["StepType"] != null)) {
                    Owner.OnWizardStepsChanged();
                }
            }
        }

        protected internal override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            if (Owner == null && !DesignMode) {
                throw new InvalidOperationException(SR.GetString(SR.WizardStep_WrongContainment));
            }
        }

        protected internal override void RenderChildren(HtmlTextWriter writer) {
            if (!Owner.ShouldRenderChildControl) {
                return;
            }

            base.RenderChildren(writer);
        }
    }
}
