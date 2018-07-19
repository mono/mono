#region Using directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;

#endregion

namespace System.Workflow.Activities.Rules.Design
{
    internal partial class RenameRuleObjectDialog : Form
    {
        public delegate bool NameValidatorDelegate(string name);

        private string name;
        private IServiceProvider serviceProvider;
        private NameValidatorDelegate nameValidator;
        private BasicBrowserDialog parent;

        public RenameRuleObjectDialog(IServiceProvider serviceProvider, string oldName, NameValidatorDelegate nameValidator, BasicBrowserDialog parent)
        {
            if (oldName == null)
                throw (new ArgumentNullException("oldName"));
            if (serviceProvider == null)
                throw (new ArgumentNullException("serviceProvider"));
            if (nameValidator == null)
                throw (new ArgumentNullException("nameValidator"));

            this.serviceProvider = serviceProvider;
            this.name = oldName;
            this.nameValidator = nameValidator;
            this.parent = parent;
            InitializeComponent();

            this.ruleNameTextBox.Text = oldName;
            this.Text = parent.RenameTitleText;
            this.newNamelabel.Text = parent.NewNameLabelText;

            this.Icon = null;

            //Set dialog fonts
            IUIService uisvc = (IUIService)this.serviceProvider.GetService(typeof(IUIService));
            if (uisvc != null)
                this.Font = (Font)uisvc.Styles["DialogFont"];

        }

        public string RuleObjectName
        {
            get
            {
                return this.name;
            }
        }

        private void OnCancel(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void OnOk(object sender, EventArgs e)
        {
            string newName = this.ruleNameTextBox.Text;

            if (newName.Trim().Length == 0)
            {
                string errorMessage = parent.EmptyNameErrorText;
                IUIService uisvc = (IUIService)this.serviceProvider.GetService(typeof(IUIService));
                if (uisvc != null)
                    uisvc.ShowError(errorMessage);
                else
                    MessageBox.Show(errorMessage, Messages.InvalidConditionNameCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, DetermineOptions(sender));

                this.DialogResult = DialogResult.None;
            }
            else if (this.name != newName && !nameValidator(newName))
            {
                string errorMessage = parent.DuplicateNameErrorText;
                IUIService uisvc = (IUIService)this.serviceProvider.GetService(typeof(IUIService));
                if (uisvc != null)
                    uisvc.ShowError(errorMessage);
                else
                    MessageBox.Show(errorMessage, Messages.InvalidConditionNameCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, DetermineOptions(sender));

                this.DialogResult = DialogResult.None;
            }
            else
            {
                this.name = newName;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private static MessageBoxOptions DetermineOptions(object sender)
        {
            MessageBoxOptions options = (MessageBoxOptions)0;
            Control someControl = sender as Control;
            RightToLeft rightToLeftValue = RightToLeft.Inherit;

            while ((rightToLeftValue == RightToLeft.Inherit) && (someControl != null))
            {
                rightToLeftValue = someControl.RightToLeft;
                someControl = someControl.Parent;
            }

            if (rightToLeftValue == RightToLeft.Yes)
            {
                options = MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign;
            }
            return options;
        }
    }
}
