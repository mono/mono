namespace System.Workflow.Activities.Rules.Design
{
    partial class RuleConditionDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RuleConditionDialog));
            this.okCancelTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.headerLabel = new System.Windows.Forms.Label();
            this.headerPictureBox = new System.Windows.Forms.PictureBox();
            this.conditionTextBox = new System.Workflow.Activities.Rules.Design.IntellisenseTextBox();
            this.conditionLabel = new System.Windows.Forms.Label();
            this.conditionErrorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.okCancelTableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.headerPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.conditionErrorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // okCancelTableLayoutPanel
            // 
            resources.ApplyResources(this.okCancelTableLayoutPanel, "okCancelTableLayoutPanel");
            this.okCancelTableLayoutPanel.CausesValidation = false;
            this.okCancelTableLayoutPanel.Controls.Add(this.okButton, 0, 0);
            this.okCancelTableLayoutPanel.Controls.Add(this.cancelButton, 1, 0);
            this.okCancelTableLayoutPanel.Name = "okCancelTableLayoutPanel";
            // 
            // okButton
            // 
            resources.ApplyResources(this.okButton, "okButton");
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Name = "okButton";
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            resources.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.CausesValidation = false;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Name = "cancelButton";
            // 
            // headerLabel
            // 
            resources.ApplyResources(this.headerLabel, "headerLabel");
            this.headerLabel.Name = "headerLabel";
            // 
            // headerPictureBox
            // 
            resources.ApplyResources(this.headerPictureBox, "headerPictureBox");
            this.headerPictureBox.Name = "headerPictureBox";
            this.headerPictureBox.TabStop = false;
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
            // conditionErrorProvider
            // 
            this.conditionErrorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.conditionErrorProvider.ContainerControl = this;
            // 
            // RuleConditionDialog
            // 
            this.AcceptButton = this.okButton;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.Controls.Add(this.conditionLabel);
            this.Controls.Add(this.conditionTextBox);
            this.Controls.Add(this.okCancelTableLayoutPanel);
            this.Controls.Add(this.headerLabel);
            this.Controls.Add(this.headerPictureBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.HelpButton = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RuleConditionDialog";
            this.ShowInTaskbar = false;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RuleConditionDialog_FormClosing);
            this.okCancelTableLayoutPanel.ResumeLayout(false);
            this.okCancelTableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.headerPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.conditionErrorProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel okCancelTableLayoutPanel;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label headerLabel;
        private System.Windows.Forms.PictureBox headerPictureBox;
        private IntellisenseTextBox conditionTextBox;
        private System.Windows.Forms.Label conditionLabel;
        private System.Windows.Forms.ErrorProvider conditionErrorProvider;
    }
}
