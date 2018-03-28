// ---------------------------------------------------------------------------
// Copyright (C) 2006 Microsoft Corporation All Rights Reserved
// ---------------------------------------------------------------------------

#define CODE_ANALYSIS
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Design;

namespace System.Workflow.Activities.Rules.Design
{
    #region class RuleSetDialog

    public partial class RuleSetDialog : Form
    {
        #region members and constructors

        private RuleSet dialogRuleSet;
        private IServiceProvider serviceProvider;
        private Parser ruleParser;
        private const int numCols = 4;
        private bool[] sortOrder = new bool[numCols] { false, false, false, false };

        public RuleSetDialog(Activity activity, RuleSet ruleSet)
        {
            if (activity == null)
                throw (new ArgumentNullException("activity"));

            InitializeDialog(ruleSet);

            ITypeProvider typeProvider;
            this.serviceProvider = activity.Site;
            if (this.serviceProvider != null)
            {
                typeProvider = (ITypeProvider)this.serviceProvider.GetService(typeof(ITypeProvider));
                if (typeProvider == null)
                {
                    string message = string.Format(CultureInfo.CurrentCulture, Messages.MissingService, typeof(ITypeProvider).FullName);
                    throw new InvalidOperationException(message);
                }

                WorkflowDesignerLoader loader = this.serviceProvider.GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
                if (loader != null)
                    loader.Flush();

            }
            else
            {
                // no service provider, so make a TypeProvider that has all loaded Assemblies
                TypeProvider newProvider = new TypeProvider(null);
                foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
                    newProvider.AddAssembly(a);
                typeProvider = newProvider;
            }

            RuleValidation validation = new RuleValidation(activity, typeProvider, false);
            this.ruleParser = new Parser(validation);
        }

        public RuleSetDialog(Type activityType, ITypeProvider typeProvider, RuleSet ruleSet)
        {
            if (activityType == null)
                throw (new ArgumentNullException("activityType"));

            InitializeDialog(ruleSet);

            RuleValidation validation = new RuleValidation(activityType, typeProvider);
            this.ruleParser = new Parser(validation);
        }

        private void InitializeDialog(RuleSet ruleSet)
        {
            InitializeComponent();

            this.conditionTextBox.PopulateAutoCompleteList += new EventHandler<AutoCompletionEventArgs>(PopulateAutoCompleteList);
            this.thenTextBox.PopulateAutoCompleteList += new EventHandler<AutoCompletionEventArgs>(PopulateAutoCompleteList);
            this.elseTextBox.PopulateAutoCompleteList += new EventHandler<AutoCompletionEventArgs>(PopulateAutoCompleteList);

            this.conditionTextBox.PopulateToolTipList += new EventHandler<AutoCompletionEventArgs>(PopulateAutoCompleteList);
            this.thenTextBox.PopulateToolTipList += new EventHandler<AutoCompletionEventArgs>(PopulateAutoCompleteList);
            this.elseTextBox.PopulateToolTipList += new EventHandler<AutoCompletionEventArgs>(PopulateAutoCompleteList);

            this.reevaluationComboBox.Items.Add(Messages.ReevaluationNever);
            this.reevaluationComboBox.Items.Add(Messages.ReevaluationAlways);

            this.chainingBehaviourComboBox.Items.Add(Messages.Sequential);
            this.chainingBehaviourComboBox.Items.Add(Messages.ExplicitUpdateOnly);
            this.chainingBehaviourComboBox.Items.Add(Messages.FullChaining);

            this.HelpRequested += new HelpEventHandler(OnHelpRequested);
            this.HelpButtonClicked += new CancelEventHandler(OnHelpClicked);

            if (ruleSet != null)
                this.dialogRuleSet = ruleSet.Clone();
            else
                this.dialogRuleSet = new RuleSet();

            this.chainingBehaviourComboBox.SelectedIndex = (int)this.dialogRuleSet.ChainingBehavior;

            this.rulesListView.Select();
            foreach (Rule rule in this.dialogRuleSet.Rules)
                this.AddNewItem(rule);

            if (this.rulesListView.Items.Count > 0)
                this.rulesListView.Items[0].Selected = true;
            else
                rulesListView_SelectedIndexChanged(this, new EventArgs());
        }
        #endregion

        #region public members
        public RuleSet RuleSet
        {
            get
            {
                return this.dialogRuleSet;
            }
        }

        #endregion

        #region event handlers

        private void PopulateAutoCompleteList(object sender, AutoCompletionEventArgs e)
        {
            e.AutoCompleteValues = this.ruleParser.GetExpressionCompletions(e.Prefix);
        }


        private void rulesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.rulesListView.SelectedItems.Count > 0)
            {
                Rule rule = this.rulesListView.SelectedItems[0].Tag as Rule;

                this.nameTextBox.Enabled = true;
                this.activeCheckBox.Enabled = true;
                this.reevaluationComboBox.Enabled = true;
                this.priorityTextBox.Enabled = true;
                this.conditionTextBox.Enabled = true;
                this.thenTextBox.Enabled = true;
                this.elseTextBox.Enabled = true;

                this.nameTextBox.Text = rule.Name;
                this.activeCheckBox.Checked = rule.Active;
                this.reevaluationComboBox.SelectedIndex = (int)rule.ReevaluationBehavior;
                this.priorityTextBox.Text = rule.Priority.ToString(CultureInfo.CurrentCulture);

                //Condition 
                this.conditionTextBox.Text = rule.Condition != null ? rule.Condition.ToString().Replace("\n", "\r\n") : string.Empty;
                try
                {
                    this.ruleParser.ParseCondition(this.conditionTextBox.Text);
                    conditionErrorProvider.SetError(this.conditionTextBox, string.Empty);
                }
                catch (RuleSyntaxException ex)
                {
                    conditionErrorProvider.SetError(this.conditionTextBox, ex.Message);
                }

                //Then
                this.thenTextBox.Text = GetActionsString(rule.ThenActions);
                try
                {
                    this.ruleParser.ParseStatementList(this.thenTextBox.Text);
                    thenErrorProvider.SetError(this.thenTextBox, string.Empty);
                }
                catch (RuleSyntaxException ex)
                {
                    thenErrorProvider.SetError(this.thenTextBox, ex.Message);
                }

                //Else
                this.elseTextBox.Text = GetActionsString(rule.ElseActions);
                try
                {
                    this.ruleParser.ParseStatementList(this.elseTextBox.Text);
                    elseErrorProvider.SetError(this.elseTextBox, string.Empty);
                }
                catch (RuleSyntaxException ex)
                {
                    elseErrorProvider.SetError(this.elseTextBox, ex.Message);
                }

                this.deleteToolStripButton.Enabled = true;
            }
            else
            {
                this.nameTextBox.Text = string.Empty;
                this.activeCheckBox.Checked = false;
                this.reevaluationComboBox.Text = string.Empty;
                this.priorityTextBox.Text = string.Empty;
                this.conditionTextBox.Text = string.Empty;
                this.thenTextBox.Text = string.Empty;
                this.elseTextBox.Text = string.Empty;

                this.nameTextBox.Enabled = false;
                this.activeCheckBox.Enabled = false;
                this.reevaluationComboBox.Enabled = false;
                this.priorityTextBox.Enabled = false;
                this.conditionTextBox.Enabled = false;
                this.thenTextBox.Enabled = false;
                this.elseTextBox.Enabled = false;
                conditionErrorProvider.SetError(this.conditionTextBox, string.Empty);
                thenErrorProvider.SetError(this.thenTextBox, string.Empty);
                elseErrorProvider.SetError(this.elseTextBox, string.Empty);

                this.deleteToolStripButton.Enabled = false;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void conditionTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (this.rulesListView.SelectedItems.Count == 0)
                return;
            try
            {
                Rule rule = this.rulesListView.SelectedItems[0].Tag as Rule;
                RuleCondition ruleCondition = this.ruleParser.ParseCondition(this.conditionTextBox.Text);
                rule.Condition = ruleCondition;
                if (!string.IsNullOrEmpty(this.conditionTextBox.Text))
                    this.conditionTextBox.Text = ruleCondition.ToString().Replace("\n", "\r\n");
                UpdateItem(this.rulesListView.SelectedItems[0], rule);
                conditionErrorProvider.SetError(this.conditionTextBox, string.Empty);
            }
            catch (Exception ex)
            {
                conditionErrorProvider.SetError(this.conditionTextBox, ex.Message);
                DesignerHelpers.DisplayError(Messages.Error_ConditionParser + "\n" + ex.Message, this.Text, this.serviceProvider);
                e.Cancel = true;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void thenTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (this.rulesListView.SelectedItems.Count == 0)
                return;
            try
            {
                Rule rule = this.rulesListView.SelectedItems[0].Tag as Rule;
                List<RuleAction> ruleThenActions = this.ruleParser.ParseStatementList(this.thenTextBox.Text);
                this.thenTextBox.Text = GetActionsString(ruleThenActions);
                rule.ThenActions.Clear();
                foreach (RuleAction ruleAction in ruleThenActions)
                    rule.ThenActions.Add(ruleAction);
                UpdateItem(this.rulesListView.SelectedItems[0], rule);
                thenErrorProvider.SetError(this.thenTextBox, string.Empty);
            }
            catch (Exception ex)
            {
                thenErrorProvider.SetError(this.thenTextBox, ex.Message);
                DesignerHelpers.DisplayError(Messages.Error_ActionsParser + "\n" + ex.Message, this.Text, this.serviceProvider);
                e.Cancel = true;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void elseTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (this.rulesListView.SelectedItems.Count == 0)
                return;
            try
            {
                Rule rule = (Rule)this.rulesListView.SelectedItems[0].Tag;
                List<RuleAction> ruleElseActions = this.ruleParser.ParseStatementList(this.elseTextBox.Text);
                this.elseTextBox.Text = GetActionsString(ruleElseActions);
                rule.ElseActions.Clear();
                foreach (RuleAction ruleAction in ruleElseActions)
                    rule.ElseActions.Add(ruleAction);
                UpdateItem(this.rulesListView.SelectedItems[0], rule);
                elseErrorProvider.SetError(this.elseTextBox, string.Empty);
            }
            catch (Exception ex)
            {
                elseErrorProvider.SetError(this.elseTextBox, ex.Message);
                DesignerHelpers.DisplayError(Messages.Error_ActionsParser + "\n" + ex.Message, this.Text, this.serviceProvider);
                e.Cancel = true;
            }
        }

        private void newRuleToolStripButton_Click(object sender, EventArgs e)
        {
            // verify to run validation first
            if (this.rulesToolStrip.Focus())
            {
                Rule newRule = new Rule();
                newRule.Name = CreateNewName();
                this.dialogRuleSet.Rules.Add(newRule);
                ListViewItem listViewItem = AddNewItem(newRule);
                listViewItem.Selected = true;
                listViewItem.Focused = true;
                int index = rulesListView.Items.IndexOf(listViewItem);
                rulesListView.EnsureVisible(index);
            }
        }

        private void deleteToolStripButton_Click(object sender, EventArgs e)
        {
            IntellisenseTextBox itb = this.ActiveControl as IntellisenseTextBox;
            if (itb != null)
                itb.HideIntellisenceDropDown();
            MessageBoxOptions mbo = (MessageBoxOptions)0;
            if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
                mbo = MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading;
            DialogResult dr = MessageBox.Show(this, Messages.RuleConfirmDeleteMessageText, Messages.DeleteRule,
                MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, mbo);
            if (dr == DialogResult.OK)
            {
                Rule rule = this.rulesListView.SelectedItems[0].Tag as Rule;

                int selectionIndex = this.rulesListView.SelectedIndices[0];
                this.dialogRuleSet.Rules.Remove(rule);
                this.rulesListView.Items.RemoveAt(selectionIndex);
                if (this.rulesListView.Items.Count > 0)
                {
                    int newSelectionIndex = Math.Min(selectionIndex, this.rulesListView.Items.Count - 1);
                    this.rulesListView.Items[newSelectionIndex].Selected = true;
                }
            }
        }

        private void nameTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (this.rulesListView.SelectedItems.Count > 0)
            {
                Rule rule = this.rulesListView.SelectedItems[0].Tag as Rule;
                if (this.nameTextBox.Text != rule.Name)
                {
                    string ruleName = this.nameTextBox.Text;
                    if (string.IsNullOrEmpty(ruleName))
                    {
                        e.Cancel = true;
                        DesignerHelpers.DisplayError(Messages.Error_RuleNameIsEmpty, this.Text, this.serviceProvider);
                    }
                    else if (rule.Name == ruleName)
                    {
                        this.nameTextBox.Text = ruleName;
                    }
                    else if (!IsUniqueIdentifier(ruleName))
                    {
                        e.Cancel = true;
                        DesignerHelpers.DisplayError(Messages.Error_DuplicateRuleName, this.Text, this.serviceProvider);
                    }
                    else
                    {
                        rule.Name = ruleName;
                        UpdateItem(this.rulesListView.SelectedItems[0], rule);
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void priorityTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (this.rulesListView.SelectedItems.Count > 0)
            {
                Rule rule = this.rulesListView.SelectedItems[0].Tag as Rule;
                try
                {
                    rule.Priority = int.Parse(this.priorityTextBox.Text, CultureInfo.CurrentCulture);
                    UpdateItem(this.rulesListView.SelectedItems[0], rule);
                }
                catch
                {
                    e.Cancel = true;
                    DesignerHelpers.DisplayError(Messages.Error_InvalidPriority, this.Text, this.serviceProvider);
                }
            }
        }

        private void activeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rulesListView.SelectedItems.Count > 0)
            {
                Rule rule = this.rulesListView.SelectedItems[0].Tag as Rule;
                rule.Active = this.activeCheckBox.Checked;
                UpdateItem(this.rulesListView.SelectedItems[0], rule);
            }
        }

        private void reevaluationComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.rulesListView.SelectedItems.Count > 0)
            {
                Rule rule = this.rulesListView.SelectedItems[0].Tag as Rule;
                rule.ReevaluationBehavior = (RuleReevaluationBehavior)this.reevaluationComboBox.SelectedIndex;
                UpdateItem(this.rulesListView.SelectedItems[0], rule);
            }
        }

        private void chainingBehaviourComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.dialogRuleSet.ChainingBehavior = (RuleChainingBehavior)this.chainingBehaviourComboBox.SelectedIndex;
        }

        private void rulesListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column < numCols)
            {
                this.rulesListView.ListViewItemSorter = new ListViewItemComparer(e.Column, sortOrder[e.Column]);
                sortOrder[e.Column] = !sortOrder[e.Column];
            }
        }

        #endregion

        #region private helpers

        private ListViewItem AddNewItem(Rule rule)
        {
            ListViewItem listViewItem = new ListViewItem(new string[] { rule.Name, String.Empty, String.Empty, String.Empty, String.Empty });
            this.rulesListView.Items.Add(listViewItem);
            listViewItem.Tag = rule;
            UpdateItem(listViewItem, rule);

            return listViewItem;
        }

        private void UpdateItem(ListViewItem listViewItem, Rule rule)
        {
            listViewItem.SubItems[0].Text = rule.Name;
            listViewItem.SubItems[1].Text = rule.Priority.ToString(CultureInfo.CurrentCulture);
            listViewItem.SubItems[2].Text = (string)this.reevaluationComboBox.Items[(int)rule.ReevaluationBehavior];
            listViewItem.SubItems[3].Text = rule.Active.ToString(CultureInfo.CurrentCulture);
            listViewItem.SubItems[4].Text = DesignerHelpers.GetRulePreview(rule);
        }

        private string CreateNewName()
        {
            string newRuleNameBase = Messages.NewRuleName;
            int index = 1;
            while (true)
            {
                string newRuleName = newRuleNameBase + index.ToString(CultureInfo.InvariantCulture);
                if (IsUniqueIdentifier(newRuleName))
                    return newRuleName;
                index++;
            }
        }

        private bool IsUniqueIdentifier(string name)
        {
            foreach (Rule rule1 in this.dialogRuleSet.Rules)
            {
                if (rule1.Name == name)
                    return false;
            }
            return true;
        }

        private static string GetActionsString(IList<RuleAction> actions)
        {
            if (actions == null)
                throw new ArgumentNullException("actions");

            bool first = true;
            StringBuilder actionsText = new StringBuilder();

            foreach (RuleAction ruleAction in actions)
            {
                if (!first)
                    actionsText.Append("\r\n");
                else
                    first = false;

                actionsText.Append(ruleAction.ToString());
            }

            return actionsText.ToString();
        }

        private static void SetCaretAt(TextBoxBase textBox, int position)
        {
            textBox.Focus();
            textBox.SelectionStart = position;
            textBox.SelectionLength = 0;
            textBox.ScrollToCaret();
        }

        #endregion

        #region class ListViewItemComparer

        private class ListViewItemComparer : IComparer
        {
            private int col;
            private bool ascending;

            public ListViewItemComparer(int column, bool ascending)
            {
                this.col = column;
                this.ascending = ascending;
            }

            public int Compare(object x, object y)
            {
                int retval = 0;
                ListViewItem item1 = (ListViewItem)x;
                ListViewItem item2 = (ListViewItem)y;
                if (this.col == 1)
                {
                    // looking at priority
                    int val1 = 0;
                    int val2 = 0;
                    int.TryParse(item1.SubItems[col].Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out val1);
                    int.TryParse(item2.SubItems[col].Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out val2);
                    if (val1 != val2)
                        retval = (val2 - val1);
                    else
                        // priorities are the same, so sort on name (column 0)
                        retval = String.Compare(item1.SubItems[0].Text, item2.SubItems[0].Text, StringComparison.CurrentCulture);
                }
                else
                {
                    retval = String.Compare(item1.SubItems[col].Text, item2.SubItems[col].Text, StringComparison.CurrentCulture);
                }
                return ascending ? retval : -retval;
            }
        }

        #endregion

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData.Equals(Keys.Escape))
            {
                this.conditionTextBox.Validating -= this.conditionTextBox_Validating;
                this.thenTextBox.Validating -= this.thenTextBox_Validating;
                this.elseTextBox.Validating -= this.elseTextBox_Validating;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void OnHelpClicked(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            ShowHelp();
        }


        private void OnHelpRequested(object sender, HelpEventArgs e)
        {
            ShowHelp();
        }


        private void ShowHelp()
        {
            if (serviceProvider != null)
            {
                IHelpService helpService = serviceProvider.GetService(typeof(IHelpService)) as IHelpService;
                if (helpService != null)
                {
                    helpService.ShowHelpFromKeyword(this.GetType().FullName + ".UI");
                }
                else
                {
                    IUIService uisvc = serviceProvider.GetService(typeof(IUIService)) as IUIService;
                    if (uisvc != null)
                        uisvc.ShowError(Messages.NoHelp);
                }
            }
            else
            {
                IUIService uisvc = (IUIService)GetService(typeof(IUIService));
                if (uisvc != null)
                    uisvc.ShowError(Messages.NoHelp);
            }
        }
    }

    #endregion

}
