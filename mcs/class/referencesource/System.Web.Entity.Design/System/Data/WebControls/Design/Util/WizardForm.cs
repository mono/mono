//------------------------------------------------------------------------------
// <copyright file="WizardForm.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Design;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace System.Web.UI.Design.WebControls.Util
{
    /// <devdoc>
    /// Represents a wizard used to guide users through configuration processes.
    /// </devdoc>
    internal abstract class WizardForm : TaskFormBase
    {

        private System.Windows.Forms.Button _nextButton;
        private System.Windows.Forms.Button _previousButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _finishButton;
        private System.Windows.Forms.TableLayoutPanel _wizardButtonsTableLayoutPanel;
        private System.Windows.Forms.Label _dummyLabel1;
        private System.Windows.Forms.Label _dummyLabel2;
        private System.Windows.Forms.Label _dummyLabel3;

        System.Collections.Generic.Stack<WizardPanel> _panelHistory;
        private WizardPanel _initialPanel;

        /// <devdoc>
        /// Creates a new WizardForm with a given service provider.
        /// </devdoc>
        public WizardForm(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _panelHistory = new System.Collections.Generic.Stack<WizardPanel>();

            InitializeComponent();
            InitializeUI();
        }

        /// <devdoc>
        /// The Finish button of the wizard.
        /// </devdoc>
        public System.Windows.Forms.Button FinishButton
        {
            get
            {
                return _finishButton;
            }
        }

        /// <devdoc>
        /// The Next button of the wizard.
        /// </devdoc>
        public System.Windows.Forms.Button NextButton
        {
            get
            {
                return _nextButton;
            }
        }

        /// <devdoc>
        /// The Back button of the wizard.
        /// </devdoc>
        public System.Windows.Forms.Button PreviousButton
        {
            get
            {
                return _previousButton;
            }
        }

        #region Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {

            this._wizardButtonsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this._previousButton = new System.Windows.Forms.Button();
            this._nextButton = new System.Windows.Forms.Button();
            this._dummyLabel2 = new System.Windows.Forms.Label();
            this._finishButton = new System.Windows.Forms.Button();
            this._dummyLabel3 = new System.Windows.Forms.Label();
            this._cancelButton = new System.Windows.Forms.Button();
            this._dummyLabel1 = new System.Windows.Forms.Label();
            this._wizardButtonsTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();

            // 
            // _wizardButtonsTableLayoutPanel
            // 
            this._wizardButtonsTableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._wizardButtonsTableLayoutPanel.AutoSize = true;
            this._wizardButtonsTableLayoutPanel.ColumnCount = 7;
            this._wizardButtonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this._wizardButtonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 3F));
            this._wizardButtonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this._wizardButtonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 7F));
            this._wizardButtonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this._wizardButtonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 7F));
            this._wizardButtonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this._wizardButtonsTableLayoutPanel.Controls.Add(this._previousButton);
            this._wizardButtonsTableLayoutPanel.Controls.Add(this._dummyLabel1);
            this._wizardButtonsTableLayoutPanel.Controls.Add(this._nextButton);
            this._wizardButtonsTableLayoutPanel.Controls.Add(this._dummyLabel2);
            this._wizardButtonsTableLayoutPanel.Controls.Add(this._finishButton);
            this._wizardButtonsTableLayoutPanel.Controls.Add(this._dummyLabel3);
            this._wizardButtonsTableLayoutPanel.Controls.Add(this._cancelButton);
            this._wizardButtonsTableLayoutPanel.Location = new System.Drawing.Point(205, 394);
            this._wizardButtonsTableLayoutPanel.Name = "_wizardButtonsTableLayoutPanel";
            this._wizardButtonsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._wizardButtonsTableLayoutPanel.Size = new System.Drawing.Size(317, 20);
            this._wizardButtonsTableLayoutPanel.TabIndex = 100;            
            // 
            // _previousButton
            // 
            this._previousButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._previousButton.AutoSize = true;
            this._previousButton.Enabled = false;
            this._previousButton.Location = new System.Drawing.Point(0, 0);
            this._previousButton.Margin = new System.Windows.Forms.Padding(0);
            this._previousButton.MinimumSize = new System.Drawing.Size(75, 23);
            this._previousButton.Name = "_previousButton";
            this._previousButton.TabIndex = 10;
            this._previousButton.Click += new System.EventHandler(this.OnPreviousButtonClick);
            // 
            // _dummyLabel1
            // 
            this._dummyLabel1.Location = new System.Drawing.Point(75, 0);
            this._dummyLabel1.Margin = new System.Windows.Forms.Padding(0);
            this._dummyLabel1.Name = "_dummyLabel1";
            this._dummyLabel1.Size = new System.Drawing.Size(3, 0);
            this._dummyLabel1.TabIndex = 20;
            // 
            // _nextButton
            // 
            this._nextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._nextButton.AutoSize = true;
            this._nextButton.Location = new System.Drawing.Point(78, 0);
            this._nextButton.Margin = new System.Windows.Forms.Padding(0);
            this._nextButton.MinimumSize = new System.Drawing.Size(75, 23);
            this._nextButton.Name = "_nextButton";
            this._nextButton.TabIndex = 30;
            this._nextButton.Click += new System.EventHandler(this.OnNextButtonClick);
            // 
            // _dummyLabel2
            // 
            this._dummyLabel2.Location = new System.Drawing.Point(153, 0);
            this._dummyLabel2.Margin = new System.Windows.Forms.Padding(0);
            this._dummyLabel2.Name = "_dummyLabel2";
            this._dummyLabel2.Size = new System.Drawing.Size(7, 0);
            this._dummyLabel2.TabIndex = 40;
            // 
            // _finishButton
            // 
            this._finishButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._finishButton.AutoSize = true;
            this._finishButton.Enabled = false;
            this._finishButton.Location = new System.Drawing.Point(160, 0);
            this._finishButton.Margin = new System.Windows.Forms.Padding(0);
            this._finishButton.MinimumSize = new System.Drawing.Size(75, 23);
            this._finishButton.Name = "_finishButton";
            this._finishButton.TabIndex = 50;
            this._finishButton.Click += new System.EventHandler(this.OnFinishButtonClick);
            // 
            // _dummyLabel3
            // 
            this._dummyLabel3.Location = new System.Drawing.Point(235, 0);
            this._dummyLabel3.Margin = new System.Windows.Forms.Padding(0);
            this._dummyLabel3.Name = "_dummyLabel3";
            this._dummyLabel3.Size = new System.Drawing.Size(7, 0);
            this._dummyLabel3.TabIndex = 60;
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.AutoSize = true;
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(242, 0);
            this._cancelButton.Margin = new System.Windows.Forms.Padding(0);
            this._cancelButton.MinimumSize = new System.Drawing.Size(75, 23);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.TabIndex = 70;
            this._cancelButton.Click += new System.EventHandler(this.OnCancelButtonClick);
            // 
            // TaskForm
            // 
            this.AcceptButton = this._nextButton;
            this.CancelButton = this._cancelButton;
            this.Controls.Add(this._wizardButtonsTableLayoutPanel);
            
            this._wizardButtonsTableLayoutPanel.ResumeLayout(false);
            this._wizardButtonsTableLayoutPanel.PerformLayout();

            InitializeForm();

            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        /// <devdoc>
        /// Called after InitializeComponent to perform additional actions that
        /// are not supported by the designer.
        /// </devdoc>
        private void InitializeUI()
        {
            _cancelButton.Text = Strings.CancelButton;
            _cancelButton.AccessibleName = Strings.CancelButtonAccessibleName;
            _nextButton.Text = Strings.Wizard_NextButton;
            _nextButton.AccessibleName = Strings.Wizard_NextButtonAccessibleName;
            _previousButton.Text = Strings.Wizard_PreviousButton;
            _previousButton.AccessibleName = Strings.Wizard_PreviousButtonAccessibleName;
            _finishButton.Text = Strings.Wizard_FinishButton;
            _finishButton.AccessibleName = Strings.Wizard_FinishButtonAccessibleName;
        }

        /// <devdoc>
        /// Goes to the next panel in the wizard.
        /// </devdoc>
        public void NextPanel()
        {
            WizardPanel currentPanel = _panelHistory.Peek();
            if (currentPanel.OnNext())
            {
                currentPanel.Hide();
                WizardPanel nextPanel = currentPanel.NextPanel;
                if (nextPanel != null)
                {
                    RegisterPanel(nextPanel);
                    _panelHistory.Push(nextPanel);
                    OnPanelChanging(new WizardPanelChangingEventArgs(currentPanel));
                    ShowPanel(nextPanel);
                }
            }
        }

        /// <devdoc>
        /// Click event handler for the Cancel button.
        /// </devdoc>
        protected virtual void OnCancelButtonClick(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <devdoc>
        /// Raises the InitialActivated event.
        /// </devdoc>
        protected override void OnInitialActivated(EventArgs e)
        {
            base.OnInitialActivated(e);

            if (_initialPanel != null)
            {
                RegisterPanel(_initialPanel);
                _panelHistory.Push(_initialPanel);
                ShowPanel(_initialPanel);
            }
        }

        /// <devdoc>
        /// Click event handler for the Finish button.
        /// </devdoc>
        protected virtual void OnFinishButtonClick(object sender, System.EventArgs e)
        {
            WizardPanel currentPanel = _panelHistory.Peek();
            if (currentPanel.OnNext())
            {
                // Call OnComplete for every panel on the stack
                WizardPanel[] panels = _panelHistory.ToArray();
                Array.Reverse(panels);
                foreach (WizardPanel panel in panels)
                {
                    panel.OnComplete();
                }

                DialogResult = DialogResult.OK;
                Close();
            }
        }

        /// <devdoc>
        /// Click event handler for the Next button.
        /// </devdoc>
        protected virtual void OnNextButtonClick(object sender, System.EventArgs e)
        {
            NextPanel();
        }

        /// <devdoc>
        /// Called when a panel is about to change.
        /// </devdoc>
        protected virtual void OnPanelChanging(WizardPanelChangingEventArgs e)
        {
        }

        /// <devdoc>
        /// Click event handler for the Previous button.
        /// </devdoc>
        protected virtual void OnPreviousButtonClick(object sender, System.EventArgs e)
        {
            PreviousPanel();
        }

        /// <devdoc>
        /// Goes to the back panel in the wizard.
        /// </devdoc>
        public void PreviousPanel()
        {
            if (_panelHistory.Count > 1)
            {
                WizardPanel currentPanel = _panelHistory.Pop();
                WizardPanel backPanel = _panelHistory.Peek();
                currentPanel.OnPrevious();
                currentPanel.Hide();
                OnPanelChanging(new WizardPanelChangingEventArgs(currentPanel));
                ShowPanel(backPanel);
            }
        }

        /// <devdoc>
        /// Registers a panel for use in this wizard.
        /// </devdoc>
        internal void RegisterPanel(WizardPanel panel)
        {
            if (!TaskPanel.Controls.Contains(panel))
            {
                panel.Dock = DockStyle.Fill;
                panel.SetParentWizard(this);
                panel.Hide();
                TaskPanel.Controls.Add(panel);
            }
        }

        /// <devdoc>
        /// Initializes a WizardForm to use an ordered array of panels.
        /// </devdoc>
        protected void SetPanels(WizardPanel[] panels)
        {
            if ((panels != null) && (panels.Length > 0))
            {
                RegisterPanel(panels[0]);
                _initialPanel = panels[0];

                for (int i = 0; i < panels.Length - 1; i++)
                {
                    RegisterPanel(panels[i + 1]);
                    panels[i].NextPanel = panels[i + 1];
                }
            }
        }

        /// <devdoc>
        /// Shows the panel specified by the given index.
        /// </devdoc>
        private void ShowPanel(WizardPanel panel)
        {
            if (_panelHistory.Count == 1)
            {
                // If we're on the first panel, disable the back button
                PreviousButton.Enabled = false;
            }
            else
            {
                // Otherwise, enable it
                PreviousButton.Enabled = true;
            }

            if (panel.NextPanel == null)
            {
                // If we're on the last panel, change the button text
                NextButton.Enabled = false;
            }
            else
            {
                NextButton.Enabled = true;
            }

            // Show the specified panel
            panel.Show();

            // Set the description and caption of the task bar
            AccessibleDescription = panel.Caption;
            CaptionLabel.Text = panel.Caption;

            if (IsHandleCreated)
            {
                // Refresh the screen
                Invalidate();
            }

            // Focus the newly shown panel
            panel.Focus();
        }
    }
}

