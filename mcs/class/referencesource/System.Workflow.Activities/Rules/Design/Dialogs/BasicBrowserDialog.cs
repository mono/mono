#region Using directives

using System;
using System.CodeDom;
using System.Collections;
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
using System.Workflow.Activities.Rules;
using System.Workflow.Interop;
using System.Globalization;
using System.Workflow.Activities.Common;

#endregion

namespace System.Workflow.Activities.Rules.Design
{
    #region class BasicBrowserDialog

    internal abstract partial class BasicBrowserDialog : Form
    {
        #region members and constructors

        private Activity activity;
        private string name;
        private IServiceProvider serviceProvider;

        protected BasicBrowserDialog(Activity activity, string name)
        {
            if (activity == null)
                throw (new ArgumentNullException("activity"));

            this.activity = activity;

            InitializeComponent();

            // set captions
            this.descriptionLabel.Text = DescriptionText;
            this.Text = TitleText;
            this.previewLabel.Text = PreviewLabelText;

            this.newRuleToolStripButton.Enabled = true;
            this.name = name;

            serviceProvider = activity.Site;

            //Set dialog fonts
            IUIService uisvc = (IUIService)activity.Site.GetService(typeof(IUIService));
            if (uisvc != null)
                this.Font = (Font)uisvc.Styles["DialogFont"];

            HelpRequested += new HelpEventHandler(OnHelpRequested);
            HelpButtonClicked += new CancelEventHandler(OnHelpClicked);

            this.rulesListView.Select();
        }

        protected Activity Activity
        {
            get
            {
                return this.activity;
            }
        }

        #endregion

        #region public properties

        public string SelectedName
        {
            get
            {
                return this.name;
            }
        }

        #endregion

        #region event handlers

        private void OnCancel(object sender, EventArgs e)
        {
            this.name = null;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void OnOk(object sender, EventArgs e)
        {
            object ruleObject = this.rulesListView.SelectedItems[0].Tag;
            this.name = this.GetObjectName(ruleObject);

            this.DialogResult = DialogResult.OK;
            this.Close();
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

        private void OnNew(object sender, EventArgs e)
        {
            try
            {
                this.OnComponentChanging();
                object newObject = OnNewInternal();
                if (newObject != null)
                {
                    using (new WaitCursor())
                    {
                        ListViewItem listViewItem = this.rulesListView.Items.Add(new ListViewItem());
                        this.UpdateListViewItem(newObject, listViewItem);
                        listViewItem.Selected = true;
                        this.OnComponentChanged();
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                DesignerHelpers.DisplayError(ex.Message, this.Text, this.activity.Site);
            }
        }

        private void OnEdit(object sender, EventArgs e)
        {
            try
            {
                this.OnComponentChanging();
                object updatedRuleObject = null;
                object ruleObject = this.rulesListView.SelectedItems[0].Tag;
                if (OnEditInternal(ruleObject, out updatedRuleObject))
                {
                    using (new WaitCursor())
                    {
                        this.UpdateListViewItem(updatedRuleObject, this.rulesListView.SelectedItems[0]);
                        this.UpdatePreview(this.previewRichTextBox, updatedRuleObject);
                        this.OnComponentChanged();
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                DesignerHelpers.DisplayError(ex.Message, this.Text, this.activity.Site);
            }
        }

        private void OnRename(object sender, EventArgs e)
        {
            try
            {
                this.OnComponentChanging();
                object ruleObject = this.rulesListView.SelectedItems[0].Tag;
                string newName = OnRenameInternal(ruleObject);
                if (newName != null)
                {
                    using (new WaitCursor())
                    {
                        ListViewItem selectedItem = this.rulesListView.SelectedItems[0];
                        selectedItem.Text = newName;
                        this.OnComponentChanged();
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                DesignerHelpers.DisplayError(ex.Message, this.Text, this.activity.Site);
            }
        }

        private void OnDelete(object sender, EventArgs e)
        {
            MessageBoxOptions mbo = (MessageBoxOptions)0;
            if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
                mbo = MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading;
            DialogResult dr = MessageBox.Show(this, this.ConfirmDeleteMessageText, this.ConfirmDeleteTitleText,
                MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, mbo);
            if (dr == DialogResult.OK)
            {
                using (new WaitCursor())
                {
                    object ruleObject = this.rulesListView.SelectedItems[0].Tag;

                    try
                    {
                        this.OnComponentChanging();
                        int selectionIndex = this.rulesListView.SelectedIndices[0];
                        object selectedRuleObject = null;
                        OnDeleteInternal(ruleObject);
                        this.rulesListView.Items.RemoveAt(selectionIndex);
                        if (this.rulesListView.Items.Count > 0)
                        {
                            int newSelectionIndex = Math.Min(selectionIndex, this.rulesListView.Items.Count - 1);
                            this.rulesListView.Items[newSelectionIndex].Selected = true;
                            selectedRuleObject = this.rulesListView.Items[newSelectionIndex].Tag;
                        }
                        this.UpdatePreview(this.previewRichTextBox, selectedRuleObject);
                        this.OnComponentChanged();
                    }
                    catch (InvalidOperationException ex)
                    {
                        DesignerHelpers.DisplayError(ex.Message, this.Text, this.activity.Site);
                    }
                }
            }
        }

        private void OnItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
                e.Item.Focused = true;

            OnToolbarStatus();
            this.okButton.Enabled = e.IsSelected;

            object currentRuleObject = null;
            if (e.IsSelected)
                currentRuleObject = e.Item.Tag;

            UpdatePreview(this.previewRichTextBox, currentRuleObject);
        }

        private void OnDoubleClick(object sender, EventArgs e)
        {
            if (this.rulesListView.SelectedItems.Count > 0)
                this.OnOk(sender, e);
        }

        private void OnToolbarStatus()
        {
            if (this.rulesListView.SelectedItems.Count == 1)
            {
                this.editToolStripButton.Enabled = true;
                this.renameToolStripButton.Enabled = true;
                this.deleteToolStripButton.Enabled = true;
            }
            else
            {
                this.editToolStripButton.Enabled = false;
                this.renameToolStripButton.Enabled = false;
                this.deleteToolStripButton.Enabled = false;
            }
        }

        #endregion

        #region helpers

        private bool OnComponentChanging()
        {
            bool canChange = true;
            ISite site = ((IComponent)this.activity).Site;
            IComponentChangeService changeService = (IComponentChangeService)site.GetService(typeof(IComponentChangeService));

            if (changeService != null)
            {
                try
                {
                    changeService.OnComponentChanging(this.activity, null);
                }
                catch (CheckoutException coEx)
                {
                    if (coEx == CheckoutException.Canceled)
                        canChange = false;
                    else
                        throw;
                }
            }
            return canChange;
        }

        private void OnComponentChanged()
        {
            ISite site = ((IComponent)this.activity).Site;
            IComponentChangeService changeService = (IComponentChangeService)site.GetService(typeof(IComponentChangeService));

            if (changeService != null)
                changeService.OnComponentChanged(this.activity, null, null, null);

            ConditionHelper.Flush_Rules_DT(site, Helpers.GetRootActivity(this.activity));
        }

        protected void InitializeListView(IList list, string selectedName)
        {
            foreach (object ruleObject in list)
            {
                ListViewItem listViewItem = this.rulesListView.Items.Add(new ListViewItem());
                this.UpdateListViewItem(ruleObject, listViewItem);
                if (GetObjectName(ruleObject) == selectedName)
                    listViewItem.Selected = true;
            }

            if (this.rulesListView.SelectedItems.Count == 0)
                OnToolbarStatus();
        }

        #endregion

        #region override methods

        protected abstract string GetObjectName(object ruleObject);

        //commands
        protected abstract object OnNewInternal();
        protected abstract bool OnEditInternal(object currentRuleObject, out object updatedRuleObject);
        protected abstract void OnDeleteInternal(object ruleObject);
        protected abstract string OnRenameInternal(object ruleObject);

        // populating controls
        protected abstract void UpdateListViewItem(object ruleObject, ListViewItem listViewItem);
        protected abstract void UpdatePreview(TextBox previewTextBox, object ruleObject);

        // captions
        protected abstract string DescriptionText { get; }
        protected abstract string TitleText { get; }
        protected abstract string PreviewLabelText { get; }
        protected abstract string ConfirmDeleteMessageText { get; }
        protected abstract string ConfirmDeleteTitleText { get; }
        internal abstract string EmptyNameErrorText { get; }
        internal abstract string DuplicateNameErrorText { get; }
        internal abstract string NewNameLabelText { get; }
        internal abstract string RenameTitleText { get; }

        internal abstract bool IsUniqueName(string ruleName);

        #endregion

        private class WaitCursor : IDisposable
        {
            private Cursor oldCursor;
            public WaitCursor()
            {
                Application.DoEvents(); // Force redraw before waiting
                oldCursor = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
            }
            public void Dispose()
            {
                Cursor.Current = oldCursor;
            }
        }
    }

    #endregion
}
