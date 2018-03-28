namespace System.Workflow.Activities.Rules.Design
{
    partial class RuleSetDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RuleSetDialog));
            this.nameColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.rulesListView = new System.Windows.Forms.ListView();
            this.priorityColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.reevaluationCountColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.activeColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.rulePreviewColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.rulesGroupBox = new System.Windows.Forms.GroupBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.chainingLabel = new System.Windows.Forms.Label();
            this.chainingBehaviourComboBox = new System.Windows.Forms.ComboBox();
            this.rulesToolStrip = new System.Windows.Forms.ToolStrip();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.newRuleToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.buttonOK = new System.Windows.Forms.Button();
            this.ruleGroupBox = new System.Windows.Forms.GroupBox();
            this.reevaluationComboBox = new System.Windows.Forms.ComboBox();
            this.elseTextBox = new System.Workflow.Activities.Rules.Design.IntellisenseTextBox();
            this.elseLabel = new System.Windows.Forms.Label();
            this.thenTextBox = new System.Workflow.Activities.Rules.Design.IntellisenseTextBox();
            this.thenLabel = new System.Windows.Forms.Label();
            this.conditionTextBox = new System.Workflow.Activities.Rules.Design.IntellisenseTextBox();
            this.conditionLabel = new System.Windows.Forms.Label();
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.nameLabel = new System.Windows.Forms.Label();
            this.activeCheckBox = new System.Windows.Forms.CheckBox();
            this.reevaluationLabel = new System.Windows.Forms.Label();
            this.priorityTextBox = new System.Windows.Forms.TextBox();
            this.priorityLabel = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.headerTextLabel = new System.Windows.Forms.Label();
            this.pictureBoxHeader = new System.Windows.Forms.PictureBox();
            this.okCancelTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.conditionErrorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.thenErrorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.elseErrorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.rulesGroupBox.SuspendLayout();
            this.panel1.SuspendLayout();
            this.rulesToolStrip.SuspendLayout();
            this.ruleGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxHeader)).BeginInit();
            this.okCancelTableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.conditionErrorProvider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.thenErrorProvider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.elseErrorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // nameColumnHeader
            // 
            this.nameColumnHeader.Name = "nameColumnHeader";
            resources.ApplyResources(this.nameColumnHeader, "nameColumnHeader");
            // 
            // rulesListView
            // 
            this.rulesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameColumnHeader,
            this.priorityColumnHeader,
            this.reevaluationCountColumnHeader,
            this.activeColumnHeader,
            this.rulePreviewColumnHeader});
            resources.ApplyResources(this.rulesListView, "rulesListView");
            this.rulesListView.FullRowSelect = true;
            this.rulesListView.HideSelection = false;
            this.rulesListView.MultiSelect = false;
            this.rulesListView.Name = "rulesListView";
            this.rulesListView.UseCompatibleStateImageBehavior = false;
            this.rulesListView.View = System.Windows.Forms.View.Details;
            this.rulesListView.SelectedIndexChanged += new System.EventHandler(this.rulesListView_SelectedIndexChanged);
            this.rulesListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.rulesListView_ColumnClick);
            // 
            // priorityColumnHeader
            // 
            resources.ApplyResources(this.priorityColumnHeader, "priorityColumnHeader");
            // 
            // reevaluationCountColumnHeader
            // 
            resources.ApplyResources(this.reevaluationCountColumnHeader, "reevaluationCountColumnHeader");
            // 
            // activeColumnHeader
            // 
            resources.ApplyResources(this.activeColumnHeader, "activeColumnHeader");
            // 
            // rulePreviewColumnHeader
            // 
            resources.ApplyResources(this.rulePreviewColumnHeader, "rulePreviewColumnHeader");
            // 
            // rulesGroupBox
            // 
            this.rulesGroupBox.Controls.Add(this.panel1);
            resources.ApplyResources(this.rulesGroupBox, "rulesGroupBox");
            this.rulesGroupBox.Name = "rulesGroupBox";
            this.rulesGroupBox.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.chainingLabel);
            this.panel1.Controls.Add(this.chainingBehaviourComboBox);
            this.panel1.Controls.Add(this.rulesToolStrip);
            this.panel1.Controls.Add(this.rulesListView);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // chainingLabel
            // 
            resources.ApplyResources(this.chainingLabel, "chainingLabel");
            this.chainingLabel.Name = "chainingLabel";
            // 
            // chainingBehaviourComboBox
            // 
            this.chainingBehaviourComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.chainingBehaviourComboBox.FormattingEnabled = true;
            resources.ApplyResources(this.chainingBehaviourComboBox, "chainingBehaviourComboBox");
            this.chainingBehaviourComboBox.Name = "chainingBehaviourComboBox";
            this.chainingBehaviourComboBox.SelectedIndexChanged += new System.EventHandler(this.chainingBehaviourComboBox_SelectedIndexChanged);
            // 
            // rulesToolStrip
            // 
            this.rulesToolStrip.BackColor = System.Drawing.SystemColors.Control;
            this.rulesToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.rulesToolStrip.ImageList = this.imageList;
            this.rulesToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newRuleToolStripButton,
            this.toolStripSeparator1,
            this.deleteToolStripButton});
            resources.ApplyResources(this.rulesToolStrip, "rulesToolStrip");
            this.rulesToolStrip.Name = "rulesToolStrip";
            this.rulesToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.rulesToolStrip.TabStop = true;
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "NewRule.bmp");
            this.imageList.Images.SetKeyName(1, "RenameRule.bmp");
            this.imageList.Images.SetKeyName(2, "Delete.bmp");
            // 
            // newRuleToolStripButton
            // 
            resources.ApplyResources(this.newRuleToolStripButton, "newRuleToolStripButton");
            this.newRuleToolStripButton.Name = "newRuleToolStripButton";
            this.newRuleToolStripButton.Click += new System.EventHandler(this.newRuleToolStripButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // deleteToolStripButton
            // 
            resources.ApplyResources(this.deleteToolStripButton, "deleteToolStripButton");
            this.deleteToolStripButton.Name = "deleteToolStripButton";
            this.deleteToolStripButton.Click += new System.EventHandler(this.deleteToolStripButton_Click);
            // 
            // buttonOK
            // 
            resources.ApplyResources(this.buttonOK, "buttonOK");
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Name = "buttonOK";
            // 
            // ruleGroupBox
            // 
            this.ruleGroupBox.Controls.Add(this.reevaluationComboBox);
            this.ruleGroupBox.Controls.Add(this.elseTextBox);
            this.ruleGroupBox.Controls.Add(this.elseLabel);
            this.ruleGroupBox.Controls.Add(this.thenTextBox);
            this.ruleGroupBox.Controls.Add(this.thenLabel);
            this.ruleGroupBox.Controls.Add(this.conditionTextBox);
            this.ruleGroupBox.Controls.Add(this.conditionLabel);
            this.ruleGroupBox.Controls.Add(this.nameTextBox);
            this.ruleGroupBox.Controls.Add(this.nameLabel);
            this.ruleGroupBox.Controls.Add(this.activeCheckBox);
            this.ruleGroupBox.Controls.Add(this.reevaluationLabel);
            this.ruleGroupBox.Controls.Add(this.priorityTextBox);
            this.ruleGroupBox.Controls.Add(this.priorityLabel);
            resources.ApplyResources(this.ruleGroupBox, "ruleGroupBox");
            this.ruleGroupBox.Name = "ruleGroupBox";
            this.ruleGroupBox.TabStop = false;
            // 
            // reevaluationComboBox
            // 
            this.reevaluationComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.reevaluationComboBox.FormattingEnabled = true;
            resources.ApplyResources(this.reevaluationComboBox, "reevaluationComboBox");
            this.reevaluationComboBox.Name = "reevaluationComboBox";
            this.reevaluationComboBox.SelectedIndexChanged += new System.EventHandler(this.reevaluationComboBox_SelectedIndexChanged);
            // 
            // elseTextBox
            // 
            this.elseTextBox.AcceptsReturn = true;
            resources.ApplyResources(this.elseTextBox, "elseTextBox");
            this.elseTextBox.Name = "elseTextBox";
            this.elseTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.elseTextBox_Validating);
            // 
            // elseLabel
            // 
            resources.ApplyResources(this.elseLabel, "elseLabel");
            this.elseLabel.Name = "elseLabel";
            // 
            // thenTextBox
            // 
            this.thenTextBox.AcceptsReturn = true;
            resources.ApplyResources(this.thenTextBox, "thenTextBox");
            this.thenTextBox.Name = "thenTextBox";
            this.thenTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.thenTextBox_Validating);
            // 
            // thenLabel
            // 
            resources.ApplyResources(this.thenLabel, "thenLabel");
            this.thenLabel.Name = "thenLabel";
            // 
            // conditionTextBox
            // 
            this.conditionTextBox.AcceptsReturn = true;
            resources.ApplyResources(this.conditionTextBox, "conditionTextBox");
            this.conditionTextBox.Name = "conditionTextBox";
            this.conditionTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.conditionTextBox_Validating);
            // 
            // conditionLabel
            // 
            resources.ApplyResources(this.conditionLabel, "conditionLabel");
            this.conditionLabel.Name = "conditionLabel";
            // 
            // nameTextBox
            // 
            resources.ApplyResources(this.nameTextBox, "nameTextBox");
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.nameTextBox_Validating);
            // 
            // nameLabel
            // 
            resources.ApplyResources(this.nameLabel, "nameLabel");
            this.nameLabel.Name = "nameLabel";
            // 
            // activeCheckBox
            // 
            resources.ApplyResources(this.activeCheckBox, "activeCheckBox");
            this.activeCheckBox.Name = "activeCheckBox";
            this.activeCheckBox.CheckedChanged += new System.EventHandler(this.activeCheckBox_CheckedChanged);
            // 
            // reevaluationLabel
            // 
            resources.ApplyResources(this.reevaluationLabel, "reevaluationLabel");
            this.reevaluationLabel.Name = "reevaluationLabel";
            // 
            // priorityTextBox
            // 
            resources.ApplyResources(this.priorityTextBox, "priorityTextBox");
            this.priorityTextBox.Name = "priorityTextBox";
            this.priorityTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.priorityTextBox_Validating);
            // 
            // priorityLabel
            // 
            resources.ApplyResources(this.priorityLabel, "priorityLabel");
            this.priorityLabel.Name = "priorityLabel";
            // 
            // buttonCancel
            // 
            resources.ApplyResources(this.buttonCancel, "buttonCancel");
            this.buttonCancel.CausesValidation = false;
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // headerTextLabel
            // 
            resources.ApplyResources(this.headerTextLabel, "headerTextLabel");
            this.headerTextLabel.Name = "headerTextLabel";
            // 
            // pictureBoxHeader
            // 
            resources.ApplyResources(this.pictureBoxHeader, "pictureBoxHeader");
            this.pictureBoxHeader.Name = "pictureBoxHeader";
            this.pictureBoxHeader.TabStop = false;
            // 
            // okCancelTableLayoutPanel
            // 
            resources.ApplyResources(this.okCancelTableLayoutPanel, "okCancelTableLayoutPanel");
            this.okCancelTableLayoutPanel.CausesValidation = false;
            this.okCancelTableLayoutPanel.Controls.Add(this.buttonOK, 0, 0);
            this.okCancelTableLayoutPanel.Controls.Add(this.buttonCancel, 1, 0);
            this.okCancelTableLayoutPanel.Name = "okCancelTableLayoutPanel";
            // 
            // conditionErrorProvider
            // 
            this.conditionErrorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.conditionErrorProvider.ContainerControl = this;
            // 
            // thenErrorProvider
            // 
            this.thenErrorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.thenErrorProvider.ContainerControl = this;
            // 
            // elseErrorProvider
            // 
            this.elseErrorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.elseErrorProvider.ContainerControl = this;
            // 
            // RuleSetDialog
            // 
            this.AcceptButton = this.buttonOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.Controls.Add(this.ruleGroupBox);
            this.Controls.Add(this.headerTextLabel);
            this.Controls.Add(this.pictureBoxHeader);
            this.Controls.Add(this.okCancelTableLayoutPanel);
            this.Controls.Add(this.rulesGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.HelpButton = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RuleSetDialog";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.rulesGroupBox.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.rulesToolStrip.ResumeLayout(false);
            this.rulesToolStrip.PerformLayout();
            this.ruleGroupBox.ResumeLayout(false);
            this.ruleGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxHeader)).EndInit();
            this.okCancelTableLayoutPanel.ResumeLayout(false);
            this.okCancelTableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.conditionErrorProvider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.thenErrorProvider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.elseErrorProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        void buttonCancel_Click(object sender, EventArgs e)
        {
            this.conditionTextBox.Validating -= this.conditionTextBox_Validating;
            this.thenTextBox.Validating -= this.thenTextBox_Validating;
            this.elseTextBox.Validating -= this.elseTextBox_Validating;
        }

        #endregion

        private System.Windows.Forms.ColumnHeader nameColumnHeader;
        private System.Windows.Forms.ListView rulesListView;
        private System.Windows.Forms.ColumnHeader priorityColumnHeader;
        private System.Windows.Forms.ColumnHeader reevaluationCountColumnHeader;
        private System.Windows.Forms.ColumnHeader activeColumnHeader;
        private System.Windows.Forms.ColumnHeader rulePreviewColumnHeader;
        private System.Windows.Forms.GroupBox rulesGroupBox;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ToolStrip rulesToolStrip;
        private System.Windows.Forms.ToolStripButton newRuleToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton deleteToolStripButton;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.GroupBox ruleGroupBox;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label headerTextLabel;
        private System.Windows.Forms.PictureBox pictureBoxHeader;
        private System.Windows.Forms.TableLayoutPanel okCancelTableLayoutPanel;
        private System.Windows.Forms.CheckBox activeCheckBox;
        private System.Windows.Forms.Label reevaluationLabel;
        private System.Windows.Forms.TextBox priorityTextBox;
        private System.Windows.Forms.Label priorityLabel;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.ErrorProvider conditionErrorProvider;
        private System.Windows.Forms.ErrorProvider thenErrorProvider;
        private System.Windows.Forms.ErrorProvider elseErrorProvider;
        private IntellisenseTextBox elseTextBox;
        private System.Windows.Forms.Label elseLabel;
        private IntellisenseTextBox thenTextBox;
        private System.Windows.Forms.Label thenLabel;
        private IntellisenseTextBox conditionTextBox;
        private System.Windows.Forms.Label conditionLabel;
        private System.Windows.Forms.ComboBox reevaluationComboBox;
        private System.Windows.Forms.ComboBox chainingBehaviourComboBox;
        private System.Windows.Forms.Label chainingLabel;
    }
}
