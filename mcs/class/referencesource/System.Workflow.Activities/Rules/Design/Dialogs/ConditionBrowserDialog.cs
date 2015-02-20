#region Using directives

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.Activities.Rules;
using System.Workflow.Interop;
using System.Globalization;
using Microsoft.Win32;
using System.Workflow.Activities.Common;

#endregion

namespace System.Workflow.Activities.Rules.Design
{
    #region class ConditionBrowserDialog

    internal sealed class ConditionBrowserDialog : BasicBrowserDialog
    {
        #region members and constructors

        private RuleConditionCollection declarativeConditionCollection;

        public ConditionBrowserDialog(Activity activity, string name)
            : base(activity, name)
        {
            RuleDefinitions rules = ConditionHelper.Load_Rules_DT(activity.Site, Helpers.GetRootActivity(activity));
            if (rules != null)
                this.declarativeConditionCollection = rules.Conditions;

            InitializeListView(this.declarativeConditionCollection, name);
        }

        #endregion

        #region override members

        protected override string GetObjectName(object ruleObject)
        {
            RuleExpressionCondition ruleExpressionCondition = ruleObject as RuleExpressionCondition;

            return ruleExpressionCondition.Name;
        }

        protected override object OnNewInternal()
        {
            using (RuleConditionDialog dlg = new RuleConditionDialog(this.Activity, null))
            {
                if (DialogResult.OK == dlg.ShowDialog(this))
                {
                    RuleExpressionCondition declarativeRuleDefinition = new RuleExpressionCondition();
                    declarativeRuleDefinition.Expression = dlg.Expression;
                    declarativeRuleDefinition.Name = this.CreateNewName();
                    this.declarativeConditionCollection.Add(declarativeRuleDefinition);
                    return declarativeRuleDefinition;
                }
            }
            return null;
        }

        protected override bool OnEditInternal(object currentRuleObject, out object updatedRuleObject)
        {
            RuleExpressionCondition declarativeRuleDefinition = currentRuleObject as RuleExpressionCondition;
            updatedRuleObject = null;

            using (RuleConditionDialog dlg = new RuleConditionDialog(this.Activity, declarativeRuleDefinition.Expression))
            {
                if (DialogResult.OK == dlg.ShowDialog(this))
                {
                    updatedRuleObject = new RuleExpressionCondition(declarativeRuleDefinition.Name, dlg.Expression);

                    this.declarativeConditionCollection.Remove(declarativeRuleDefinition.Name);
                    this.declarativeConditionCollection.Add(updatedRuleObject as RuleExpressionCondition);

                    return true;
                }
            }
            return false;
        }

        protected override string OnRenameInternal(object ruleObject)
        {
            RuleExpressionCondition declarativeRuleDefinition = ruleObject as RuleExpressionCondition;

            using (RenameRuleObjectDialog dlg = new RenameRuleObjectDialog(this.Activity.Site, declarativeRuleDefinition.Name, new RenameRuleObjectDialog.NameValidatorDelegate(IsUniqueName), this))
            {
                if ((dlg.ShowDialog(this) == DialogResult.OK) && (dlg.RuleObjectName != declarativeRuleDefinition.Name))
                {
                    this.declarativeConditionCollection.Remove(declarativeRuleDefinition);
                    declarativeRuleDefinition.Name = dlg.RuleObjectName;
                    this.declarativeConditionCollection.Add(declarativeRuleDefinition);
                    return dlg.RuleObjectName;
                }
            }
            return null;
        }

        protected override void OnDeleteInternal(object ruleObject)
        {
            RuleExpressionCondition declarativeRuleDefinition = ruleObject as RuleExpressionCondition;

            this.declarativeConditionCollection.Remove(declarativeRuleDefinition.Name);
        }

        protected override void UpdateListViewItem(object ruleObject, ListViewItem listViewItem)
        {
            RuleExpressionCondition declarativeRuleDefinition = ruleObject as RuleExpressionCondition;

            ITypeProvider typeProvider = (ITypeProvider)this.Activity.Site.GetService(typeof(ITypeProvider));
            if (typeProvider == null)
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.MissingService, typeof(ITypeProvider).FullName);
                throw new InvalidOperationException(message);
            }

            RuleValidation validator = new RuleValidation(this.Activity, typeProvider, false);
            bool valid = declarativeRuleDefinition.Validate(validator);

            listViewItem.Tag = declarativeRuleDefinition;
            listViewItem.Text = declarativeRuleDefinition.Name;

            string validText = valid ? Messages.Yes : Messages.No;
            if (listViewItem.SubItems.Count == 1)
                listViewItem.SubItems.Add(validText);
            else
                listViewItem.SubItems[1].Text = validText;
        }

        protected override void UpdatePreview(TextBox previewBox, object ruleObject)
        {
            RuleExpressionCondition declarativeRuleDefinition = ruleObject as RuleExpressionCondition;
            if (declarativeRuleDefinition != null && declarativeRuleDefinition.Expression != null)
            {
                RuleExpressionCondition ruleExpressionCondition = new RuleExpressionCondition(declarativeRuleDefinition.Expression);
                NativeMethods.SendMessage(previewBox.Handle, NativeMethods.WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
                previewBox.Lines = ruleExpressionCondition.ToString().Split('\n');
                NativeMethods.SendMessage(previewBox.Handle, NativeMethods.WM_SETREDRAW, new IntPtr(1), IntPtr.Zero);
                previewBox.Invalidate();
            }
            else
            {
                previewBox.Text = string.Empty;
            }
        }

        protected override string DescriptionText { get { return Messages.ConditionDescriptionText; } }
        protected override string TitleText { get { return Messages.ConditionTitleText; } }
        protected override string PreviewLabelText { get { return Messages.ConditionPreviewLabelText; } }
        protected override string ConfirmDeleteMessageText { get { return Messages.ConditionConfirmDeleteMessageText; } }
        protected override string ConfirmDeleteTitleText { get { return Messages.DeleteCondition; } }
        internal override string EmptyNameErrorText { get { return Messages.ConditionEmptyNameErrorText; } }
        internal override string DuplicateNameErrorText { get { return Messages.ConditionDuplicateNameErrorText; } }
        internal override string NewNameLabelText { get { return Messages.ConditionNewNameLableText; } }
        internal override string RenameTitleText { get { return Messages.ConditionRenameTitleText; } }


        // used by RenameConditionDialog
        internal override bool IsUniqueName(string ruleName)
        {
            return (!this.declarativeConditionCollection.Contains(ruleName));
        }

        #endregion

        #region helpers

        private string CreateNewName()
        {
            string newRuleNameBase = Messages.NewConditionName;
            int index = 1;
            while (true)
            {
                string newRuleName = newRuleNameBase + index.ToString(CultureInfo.InvariantCulture);
                if (!this.declarativeConditionCollection.Contains(newRuleName))
                    return newRuleName;
                index++;
            }
        }

        #endregion
    }

    #endregion
}
