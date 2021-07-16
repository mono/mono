//------------------------------------------------------------------------------
// <copyright file="CompleteWizardStep.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.UI;
    

    [
    Browsable(false),
    ]
    public sealed class CompleteWizardStep : TemplatedWizardStep {

        internal override Wizard Owner {
            get {
                return base.Owner;
            }
            set {
                if (value is CreateUserWizard || value == null) {
                    base.Owner = value;
                }
                else {
                    throw new HttpException(System.Web.SR.GetString(System.Web.SR.CompleteWizardStep_OnlyAllowedInCreateUserWizard));
                }
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
                return WizardStepType.Complete;
            }
            set {
                throw new InvalidOperationException(System.Web.SR.GetString(System.Web.SR.CreateUserWizardStep_StepTypeCannotBeSet));
            }
        }

        /// <devdoc>
        /// <para>Gets or sets the title on the <see cref='System.Web.UI.WebControls.Wizard/> .</para>
        /// </devdoc>
        [
        Localizable(true),
        WebSysDefaultValue(System.Web.SR.CreateUserWizard_DefaultCompleteTitleText),
        ]
        public override string Title {
            get {
                string title = TitleInternal;
                return (title != null) ? title : System.Web.SR.GetString(System.Web.SR.CreateUserWizard_DefaultCompleteTitleText);
            }
            set {
                base.Title = value;
            }
        }
    }
}
