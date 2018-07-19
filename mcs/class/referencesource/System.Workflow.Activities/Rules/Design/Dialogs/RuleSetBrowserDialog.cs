using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using System.Workflow.Activities.Rules;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.Interop;
using System.Workflow.Activities.Common;

namespace System.Workflow.Activities.Rules.Design
{
    #region class RuleSetBrowserDialog

    internal sealed class RuleSetBrowserDialog : BasicBrowserDialog
    {
        #region members and constructors

        private RuleSetCollection ruleSetCollection;

        public RuleSetBrowserDialog(Activity activity, string name)
            : base(activity, name)
        {
            RuleDefinitions rules = ConditionHelper.Load_Rules_DT(activity.Site, Helpers.GetRootActivity(activity));
            if (rules != null)
                this.ruleSetCollection = rules.RuleSets;

            InitializeListView(this.ruleSetCollection, name);
        }

        #endregion

        #region override members

        protected override string GetObjectName(object ruleObject)
        {
            RuleSet ruleSet = ruleObject as RuleSet;

            return ruleSet.Name;
        }

        protected override object OnNewInternal()
        {
            using (RuleSetDialog dlg = new RuleSetDialog(this.Activity, null))
            {
                if (DialogResult.OK == dlg.ShowDialog(this))
                {
                    RuleSet ruleSet = dlg.RuleSet;
                    ruleSet.Name = ruleSetCollection.GenerateRuleSetName();
                    this.ruleSetCollection.Add(ruleSet);
                    return ruleSet;
                }
            }
            return null;
        }

        protected override bool OnEditInternal(object currentRuleObject, out object updatedRuleObject)
        {
            RuleSet ruleSet = currentRuleObject as RuleSet;
            updatedRuleObject = null;

            using (RuleSetDialog dlg = new RuleSetDialog(this.Activity, ruleSet))
            {
                if (DialogResult.OK == dlg.ShowDialog())
                {
                    this.ruleSetCollection.Remove(ruleSet.Name);
                    this.ruleSetCollection.Add(dlg.RuleSet);
                    updatedRuleObject = dlg.RuleSet;
                    return true;
                }
            }
            return false;
        }

        protected override string OnRenameInternal(object ruleObject)
        {
            RuleSet ruleSet = ruleObject as RuleSet;

            using (RenameRuleObjectDialog dlg = new RenameRuleObjectDialog(this.Activity.Site, ruleSet.Name, new RenameRuleObjectDialog.NameValidatorDelegate(IsUniqueName), this))
            {
                if ((dlg.ShowDialog(this) == DialogResult.OK) && (dlg.RuleObjectName != ruleSet.Name))
                {
                    this.ruleSetCollection.Remove(ruleSet);
                    ruleSet.Name = dlg.RuleObjectName;
                    this.ruleSetCollection.Add(ruleSet);
                    return dlg.RuleObjectName;
                }
            }
            return null;
        }

        protected override void OnDeleteInternal(object ruleObject)
        {
            RuleSet ruleSet = ruleObject as RuleSet;

            this.ruleSetCollection.Remove(ruleSet.Name);
        }

        protected override void UpdateListViewItem(object ruleObject, ListViewItem listViewItem)
        {
            RuleSet ruleSet = ruleObject as RuleSet;

            ValidationManager manager = new ValidationManager(this.Activity.Site);
            ITypeProvider typeProvider = (ITypeProvider)manager.GetService(typeof(ITypeProvider));
            RuleValidation validation = new RuleValidation(this.Activity, typeProvider, false);

            bool valid;
            using (WorkflowCompilationContext.CreateScope(manager))
            {
                valid = ruleSet.Validate(validation);
            }

            listViewItem.Tag = ruleSet;
            listViewItem.Text = ruleSet.Name;
            string validText = valid ? Messages.Yes : Messages.No;
            if (listViewItem.SubItems.Count == 1)
                listViewItem.SubItems.Add(validText);
            else
                listViewItem.SubItems[1].Text = validText;
        }

        protected override void UpdatePreview(TextBox previewBox, object ruleObject)
        {
            RuleSet ruleSet = ruleObject as RuleSet;

            NativeMethods.SendMessage(previewBox.Handle, NativeMethods.WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
            previewBox.Lines = DesignerHelpers.GetRuleSetPreview(ruleSet).Split('\n');
            NativeMethods.SendMessage(previewBox.Handle, NativeMethods.WM_SETREDRAW, new IntPtr(1), IntPtr.Zero);
            previewBox.Invalidate();
        }

        protected override string DescriptionText { get { return Messages.RuleSetDescriptionText; } }
        protected override string TitleText { get { return Messages.RuleSetTitleText; } }
        protected override string PreviewLabelText { get { return Messages.RuleSetPreviewLabelText; } }
        protected override string ConfirmDeleteMessageText { get { return Messages.RuleSetConfirmDeleteMessageText; } }
        protected override string ConfirmDeleteTitleText { get { return Messages.DeleteRuleSet; } }
        internal override string EmptyNameErrorText { get { return Messages.RuleSetEmptyNameErrorText; } }
        internal override string DuplicateNameErrorText { get { return Messages.RuleSetDuplicateNameErrorText; } }
        internal override string NewNameLabelText { get { return Messages.RuleSetNewNameLableText; } }
        internal override string RenameTitleText { get { return Messages.RuleSetRenameTitleText; } }


        // used by RenameConditionDialog
        internal override bool IsUniqueName(string ruleName)
        {
            return (!this.ruleSetCollection.Contains(ruleName));
        }

        #endregion
    }

    #endregion
}
