
namespace System.Workflow.ComponentModel.Design
{
    partial class ActivityBindForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ActivityBindForm));
            this.dummyPanel = new System.Windows.Forms.Panel();
            this.cancelButton = new System.Windows.Forms.Button();
            this.OKButton = new System.Windows.Forms.Button();
            this.buttonTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.helpTextBox = new System.Windows.Forms.TextBox();
            this.createField = new System.Windows.Forms.RadioButton();
            this.createProperty = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.bindTabControl = new System.Windows.Forms.TabControl();
            this.existingMemberPage = new System.Windows.Forms.TabPage();
            this.newMemberPage = new System.Windows.Forms.TabPage();
            this.newMemberHelpTextBox = new System.Windows.Forms.TextBox();
            this.memberNameLabel = new System.Windows.Forms.Label();
            this.memberNameTextBox = new System.Windows.Forms.TextBox();
            this.buttonTableLayoutPanel.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.bindTabControl.SuspendLayout();
            this.existingMemberPage.SuspendLayout();
            this.newMemberPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // dummyPanel
            // 
            resources.ApplyResources(this.dummyPanel, "dummyPanel");
            this.dummyPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dummyPanel.Name = "dummyPanel";
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // OKButton
            // 
            resources.ApplyResources(this.OKButton, "OKButton");
            this.OKButton.Name = "OKButton";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // buttonTableLayoutPanel
            // 
            resources.ApplyResources(this.buttonTableLayoutPanel, "buttonTableLayoutPanel");
            this.buttonTableLayoutPanel.Controls.Add(this.OKButton, 0, 0);
            this.buttonTableLayoutPanel.Controls.Add(this.cancelButton, 1, 0);
            this.buttonTableLayoutPanel.Name = "buttonTableLayoutPanel";
            // 
            // helpTextBox
            // 
            resources.ApplyResources(this.helpTextBox, "helpTextBox");
            this.helpTextBox.Name = "helpTextBox";
            this.helpTextBox.ReadOnly = true;
            // 
            // createField
            // 
            resources.ApplyResources(this.createField, "createField");
            this.createField.Checked = true;
            this.createField.Name = "createField";
            this.createField.TabStop = true;
            this.createField.UseVisualStyleBackColor = true;
            // 
            // createProperty
            // 
            resources.ApplyResources(this.createProperty, "createProperty");
            this.createProperty.Name = "createProperty";
            this.createProperty.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.createField);
            this.groupBox1.Controls.Add(this.createProperty);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // bindTabControl
            // 
            resources.ApplyResources(this.bindTabControl, "bindTabControl");
            this.bindTabControl.Controls.Add(this.existingMemberPage);
            this.bindTabControl.Controls.Add(this.newMemberPage);
            this.bindTabControl.Name = "bindTabControl";
            this.bindTabControl.SelectedIndex = 0;
            // 
            // existingMemberPage
            // 
            this.existingMemberPage.Controls.Add(this.dummyPanel);
            this.existingMemberPage.Controls.Add(this.helpTextBox);
            resources.ApplyResources(this.existingMemberPage, "existingMemberPage");
            this.existingMemberPage.Name = "existingMemberPage";
            this.existingMemberPage.UseVisualStyleBackColor = true;
            // 
            // newMemberPage
            // 
            this.newMemberPage.Controls.Add(this.memberNameLabel);
            this.newMemberPage.Controls.Add(this.memberNameTextBox);
            this.newMemberPage.Controls.Add(this.groupBox1);
            this.newMemberPage.Controls.Add(this.newMemberHelpTextBox);
            resources.ApplyResources(this.newMemberPage, "newMemberPage");
            this.newMemberPage.Name = "newMemberPage";
            this.newMemberPage.UseVisualStyleBackColor = true;
            // 
            // newMemberHelpTextBox
            // 
            resources.ApplyResources(this.newMemberHelpTextBox, "newMemberHelpTextBox");
            this.newMemberHelpTextBox.Name = "newMemberHelpTextBox";
            this.newMemberHelpTextBox.ReadOnly = true;
            // 
            // memberNameLabel
            // 
            resources.ApplyResources(this.memberNameLabel, "memberNameLabel");
            this.memberNameLabel.Name = "memberNameLabel";
            // 
            // memberNameTextBox
            // 
            resources.ApplyResources(this.memberNameTextBox, "memberNameTextBox");
            this.memberNameTextBox.Name = "memberNameTextBox";
            // 
            // ActivityBindForm
            // 
            this.AcceptButton = this.OKButton;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.Controls.Add(this.bindTabControl);
            this.Controls.Add(this.buttonTableLayoutPanel);
            this.HelpButton = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ActivityBindForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.ActivityBindForm_HelpButtonClicked);
            this.Load += new System.EventHandler(this.ActivityBindForm_Load);
            this.buttonTableLayoutPanel.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.bindTabControl.ResumeLayout(false);
            this.existingMemberPage.ResumeLayout(false);
            this.existingMemberPage.PerformLayout();
            this.newMemberPage.ResumeLayout(false);
            this.newMemberPage.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel dummyPanel;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.TableLayoutPanel buttonTableLayoutPanel;
        private System.Windows.Forms.TextBox helpTextBox;
        private System.Windows.Forms.RadioButton createProperty;
        private System.Windows.Forms.RadioButton createField;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TabControl bindTabControl;
        private System.Windows.Forms.TabPage existingMemberPage;
        private System.Windows.Forms.TabPage newMemberPage;
        private System.Windows.Forms.TextBox newMemberHelpTextBox;
        private System.Windows.Forms.Label memberNameLabel;
        private System.Windows.Forms.TextBox memberNameTextBox;
    }
}

