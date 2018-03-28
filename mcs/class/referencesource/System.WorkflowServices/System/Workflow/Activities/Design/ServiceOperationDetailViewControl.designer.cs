//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Activities.Design
{
    partial class ServiceOperationDetailViewControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServiceOperationDetailViewControl));
            this.oneWayCheckBox = new System.Windows.Forms.CheckBox();
            this.parametersGrid = new System.Windows.Forms.DataGridView();
            this.operationNameLabel = new System.Windows.Forms.Label();
            this.operationNameTextBox = new System.Windows.Forms.TextBox();
            this.operationTabControl = new System.Windows.Forms.TabControl();
            this.parametersTabPage = new System.Windows.Forms.TabPage();
            this.operationsToolStrip = new System.Windows.Forms.ToolStrip();
            this.moveParameterDownButton = new System.Windows.Forms.ToolStripButton();
            this.moveParameterUpButton = new System.Windows.Forms.ToolStripButton();
            this.RemoveParameterButton = new System.Windows.Forms.ToolStripButton();
            this.addParameterButton = new System.Windows.Forms.ToolStripButton();
            this.propertiesTabPage = new System.Windows.Forms.TabPage();
            this.protectionLevelComboBox = new System.Windows.Forms.ComboBox();
            this.protectionLevelLabel = new System.Windows.Forms.Label();
            this.permissionsTab = new System.Windows.Forms.TabPage();
            this.permissionRoleLabel = new System.Windows.Forms.Label();
            this.permissionNameLabel = new System.Windows.Forms.Label();
            this.permissionNameTextBox = new System.Windows.Forms.TextBox();
            this.permissionRoleTextBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.parametersGrid)).BeginInit();
            this.operationTabControl.SuspendLayout();
            this.parametersTabPage.SuspendLayout();
            this.operationsToolStrip.SuspendLayout();
            this.propertiesTabPage.SuspendLayout();
            this.permissionsTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // oneWayCheckBox
            // 
            resources.ApplyResources(this.oneWayCheckBox, "oneWayCheckBox");
            this.oneWayCheckBox.Name = "oneWayCheckBox";
            this.oneWayCheckBox.UseVisualStyleBackColor = true;
            // 
            // parametersGrid
            // 
            this.parametersGrid.AllowUserToAddRows = false;
            this.parametersGrid.AllowUserToDeleteRows = false;
            this.parametersGrid.AllowUserToResizeRows = false;
            this.parametersGrid.BackgroundColor = System.Drawing.SystemColors.Window;
            this.parametersGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.parametersGrid.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.parametersGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.parametersGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            resources.ApplyResources(this.parametersGrid, "parametersGrid");
            this.parametersGrid.Name = "parametersGrid";
            this.parametersGrid.ReadOnly = true;
            this.parametersGrid.RowHeadersVisible = false;
            this.parametersGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.parametersGrid.ShowEditingIcon = false;
            this.parametersGrid.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;

            // 
            // operationNameLabel
            // 
            resources.ApplyResources(this.operationNameLabel, "operationNameLabel");
            this.operationNameLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.operationNameLabel.Name = "operationNameLabel";
            // 
            // operationNameTextBox
            // 
            resources.ApplyResources(this.operationNameTextBox, "operationNameTextBox");
            this.operationNameTextBox.Name = "operationNameTextBox";
            // 
            // operationTabControl
            // 
            this.operationTabControl.Controls.Add(this.parametersTabPage);
            this.operationTabControl.Controls.Add(this.propertiesTabPage);
            this.operationTabControl.Controls.Add(this.permissionsTab);
            resources.ApplyResources(this.operationTabControl, "operationTabControl");
            this.operationTabControl.Name = "operationTabControl";
            this.operationTabControl.SelectedIndex = 0;
            // 
            // parametersTabPage
            // 
            this.parametersTabPage.Controls.Add(this.operationsToolStrip);
            this.parametersTabPage.Controls.Add(this.parametersGrid);
            resources.ApplyResources(this.parametersTabPage, "parametersTabPage");
            this.parametersTabPage.Name = "parametersTabPage";
            this.parametersTabPage.UseVisualStyleBackColor = true;
            // 
            // operationsToolStrip
            // 
            this.operationsToolStrip.BackColor = System.Drawing.SystemColors.Window;
            this.operationsToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.operationsToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.moveParameterDownButton,
            this.moveParameterUpButton,
            this.RemoveParameterButton,
            this.addParameterButton});
            resources.ApplyResources(this.operationsToolStrip, "operationsToolStrip");
            this.operationsToolStrip.Name = "operationsToolStrip";
            this.operationsToolStrip.Stretch = true;
            this.operationsToolStrip.TabStop = true;
            // 
            // moveParameterDownButton
            // 
            this.moveParameterDownButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.moveParameterDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.moveParameterDownButton.Image = global::System.ServiceModel.ImageResources.Down;
            resources.ApplyResources(this.moveParameterDownButton, "moveParameterDownButton");
            this.moveParameterDownButton.Name = "moveParameterDownButton";
            this.moveParameterDownButton.Click += new System.EventHandler(this.moveParameterDownButton_Click);
            // 
            // moveParameterUpButton
            // 
            this.moveParameterUpButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.moveParameterUpButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.moveParameterUpButton.Image = global::System.ServiceModel.ImageResources.Up;
            resources.ApplyResources(this.moveParameterUpButton, "moveParameterUpButton");
            this.moveParameterUpButton.Name = "moveParameterUpButton";
            this.moveParameterUpButton.Click += new System.EventHandler(this.moveParameterUpButton_Click);
            // 
            // RemoveParameterButton
            // 
            this.RemoveParameterButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.RemoveParameterButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.RemoveParameterButton.Image = global::System.ServiceModel.ImageResources.Delete;
            this.RemoveParameterButton.Name = "RemoveParameterButton";
            resources.ApplyResources(this.RemoveParameterButton, "RemoveParameterButton");
            this.RemoveParameterButton.Click += new System.EventHandler(this.RemoveParameterButton_Click);
            // 
            // addParameterButton
            // 
            this.addParameterButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.addParameterButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.addParameterButton.Image = global::System.ServiceModel.ImageResources.Add;
            resources.ApplyResources(this.addParameterButton, "addParameterButton");
            this.addParameterButton.Name = "addParameterButton";
            this.addParameterButton.Click += new System.EventHandler(this.addParameterButton_Click);
            // 
            // propertiesTabPage
            // 
            this.propertiesTabPage.Controls.Add(this.protectionLevelComboBox);
            this.propertiesTabPage.Controls.Add(this.protectionLevelLabel);
            this.propertiesTabPage.Controls.Add(this.oneWayCheckBox);
            resources.ApplyResources(this.propertiesTabPage, "propertiesTabPage");
            this.propertiesTabPage.Name = "propertiesTabPage";
            this.propertiesTabPage.UseVisualStyleBackColor = true;
            // 
            // protectionLevelComboBox
            // 
            this.protectionLevelComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.protectionLevelComboBox, "protectionLevelComboBox");
            this.protectionLevelComboBox.FormattingEnabled = true;
            this.protectionLevelComboBox.Name = "protectionLevelComboBox";
            // 
            // protectionLevelLabel
            // 
            resources.ApplyResources(this.protectionLevelLabel, "protectionLevelLabel");
            this.protectionLevelLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.protectionLevelLabel.Name = "protectionLevelLabel";
            // 
            // permissionsTab
            // 
            this.permissionsTab.Controls.Add(this.permissionRoleTextBox);
            this.permissionsTab.Controls.Add(this.permissionNameTextBox);
            this.permissionsTab.Controls.Add(this.permissionNameLabel);
            this.permissionsTab.Controls.Add(this.permissionRoleLabel);
            resources.ApplyResources(this.permissionsTab, "permissionsTab");
            this.permissionsTab.Name = "permissionsTab";
            this.permissionsTab.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            resources.ApplyResources(this.permissionRoleLabel, "label1");
            this.permissionRoleLabel.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(this.permissionNameLabel, "label2");
            this.permissionNameLabel.Name = "label2";
            // 
            // textBox1
            // 
            resources.ApplyResources(this.permissionNameTextBox, "textBox1");
            this.permissionNameTextBox.Name = "textBox1";
            // 
            // textBox2
            // 
            resources.ApplyResources(this.permissionRoleTextBox, "textBox2");
            this.permissionRoleTextBox.Name = "textBox2";
            // 
            // ServiceOperationDetailViewControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.operationTabControl);
            this.Controls.Add(this.operationNameTextBox);
            this.Controls.Add(this.operationNameLabel);
            resources.ApplyResources(this, "$this");
            this.Name = "ServiceOperationDetailViewControl";
            ((System.ComponentModel.ISupportInitialize)(this.parametersGrid)).EndInit();
            this.operationTabControl.ResumeLayout(false);
            this.parametersTabPage.ResumeLayout(false);
            this.parametersTabPage.PerformLayout();
            this.operationsToolStrip.ResumeLayout(false);
            this.operationsToolStrip.PerformLayout();
            this.propertiesTabPage.ResumeLayout(false);
            this.propertiesTabPage.PerformLayout();
            this.permissionsTab.ResumeLayout(false);
            this.permissionsTab.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox oneWayCheckBox;
        private System.Windows.Forms.DataGridView parametersGrid;
        private System.Windows.Forms.Label operationNameLabel;
        private System.Windows.Forms.TextBox operationNameTextBox;
        private System.Windows.Forms.TabControl operationTabControl;
        private System.Windows.Forms.TabPage parametersTabPage;
        private System.Windows.Forms.TabPage propertiesTabPage;
        private System.Windows.Forms.ComboBox protectionLevelComboBox;
        private System.Windows.Forms.Label protectionLevelLabel;
        private System.Windows.Forms.ToolStrip operationsToolStrip;
        private System.Windows.Forms.ToolStripButton RemoveParameterButton;
        private System.Windows.Forms.ToolStripButton addParameterButton;
        private System.Windows.Forms.ToolStripButton moveParameterDownButton;
        private System.Windows.Forms.ToolStripButton moveParameterUpButton;
        private System.Windows.Forms.TabPage permissionsTab;
        private System.Windows.Forms.Label permissionNameLabel;
        private System.Windows.Forms.Label permissionRoleLabel;
        private System.Windows.Forms.TextBox permissionRoleTextBox;
        private System.Windows.Forms.TextBox permissionNameTextBox;
    }
}
