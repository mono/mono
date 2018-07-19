//------------------------------------------------------------------------------
// <copyright file="CreateUserWizardStep.cs" company="Microsoft">
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
    Browsable(false),
    ]
    public sealed class CreateUserWizardStep : TemplatedWizardStep {

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public override bool AllowReturn {
            get {
                return AllowReturnInternal;
            }
            set {
                throw new InvalidOperationException(SR.GetString(SR.CreateUserWizardStep_AllowReturnCannotBeSet));
            }
        }

        internal bool AllowReturnInternal {
            get {
                object val = ViewState["AllowReturnInternal"];
                return((val == null) ? true : (bool)val);
            }
            set {
                ViewState["AllowReturnInternal"] = value;
            }
        }

        internal override Wizard Owner {
            get {
                return base.Owner;
            }
            set {
                if (value is CreateUserWizard || value == null) {
                    base.Owner = value;
                }
                else {
                    throw new HttpException(SR.GetString(SR.CreateUserWizardStep_OnlyAllowedInCreateUserWizard));
                }
            }
        }


        /// <devdoc>
        /// <para>Gets or sets the title on the <see cref='System.Web.UI.WebControls.Wizard/> .</para>
        /// </devdoc>
        [
        Localizable(true),
        WebSysDefaultValue(SR.CreateUserWizard_DefaultCreateUserTitleText),
        ]
        public override string Title {
            get {
                string title = TitleInternal;
                return (title != null) ? title : SR.GetString(SR.CreateUserWizard_DefaultCreateUserTitleText);
            }
            set {
                base.Title = value;
            }
        }


        [
        Browsable(false),
        Themeable(false),
        Filterable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public override WizardStepType StepType {
            get {
                return base.StepType;
            }
            set {
                throw new InvalidOperationException(SR.GetString(SR.CreateUserWizardStep_StepTypeCannotBeSet));
            }
        }
    }
}
