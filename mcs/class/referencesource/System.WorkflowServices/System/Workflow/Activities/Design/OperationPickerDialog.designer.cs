//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.Workflow.Activities.Design
{
    partial class OperationPickerDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OperationPickerDialog));
            this.operationsToolStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.importContractButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.addOperationButton = new System.Windows.Forms.ToolStripButton();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.operationsPanel = new System.Workflow.Activities.Design.GradientPanel();
            this.operationsListBox = new System.Workflow.Activities.Design.RichListBox();
            this.detailsViewPanel = new System.Workflow.Activities.Design.GradientPanel();
            this.footerPanel = new System.Workflow.Activities.Design.GradientPanel();
            this.operationsToolStrip.SuspendLayout();
            this.operationsPanel.SuspendLayout();
            this.footerPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // operationsToolStrip
            // 
            this.operationsToolStrip.BackColor = System.Drawing.SystemColors.Control;
            this.operationsToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.operationsToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.importContractButton,
            this.toolStripButton1,
            this.addOperationButton});
            resources.ApplyResources(this.operationsToolStrip, "operationsToolStrip");
            this.operationsToolStrip.Name = "operationsToolStrip";
            this.operationsToolStrip.Stretch = true;
            this.operationsToolStrip.TabStop = true;
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            resources.ApplyResources(this.toolStripLabel1, "toolStripLabel1");
            // 
            // importContractButton
            // 
            this.importContractButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.importContractButton.Image = global::System.ServiceModel.ImageResources.Import;
            resources.ApplyResources(this.importContractButton, "importContractButton");
            this.importContractButton.Name = "importContractButton";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton1.Image = global::System.ServiceModel.ImageResources.AddContract;
            resources.ApplyResources(this.toolStripButton1, "toolStripButton1");
            this.toolStripButton1.Name = "toolStripButton1";
            // 
            // addOperationButton
            // 
            this.addOperationButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.addOperationButton.Image = global::System.ServiceModel.ImageResources.AddOperation;
            resources.ApplyResources(this.addOperationButton, "addOperationButton");
            this.addOperationButton.Name = "addOperationButton";
            // 
            // okButton
            // 
            resources.ApplyResources(this.okButton, "okButton");
            this.okButton.Name = "okButton";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // operationsPanel
            // 
            resources.ApplyResources(this.operationsPanel, "operationsPanel");
            this.operationsPanel.BaseColor = System.Drawing.SystemColors.Window;
            this.operationsPanel.BorderColor = System.Drawing.SystemColors.ControlDark;
            this.operationsPanel.Controls.Add(this.operationsListBox);
            this.operationsPanel.DropShadow = false;
            this.operationsPanel.Glossy = false;
            this.operationsPanel.LightingColor = System.Drawing.SystemColors.Window;
            this.operationsPanel.Name = "operationsPanel";
            this.operationsPanel.Radius = 3;
            this.operationsPanel.Padding = new System.Windows.Forms.Padding(3);
            // 
            // operationsListBox
            // 
            this.operationsListBox.BackColor = System.Drawing.SystemColors.Window;
            this.operationsListBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.operationsListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.operationsListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.operationsListBox.Editable = false;
            resources.ApplyResources(this.operationsListBox, "operationsListBox");
            this.operationsListBox.FormattingEnabled = true;
            this.operationsListBox.Name = "operationsListBox";
            this.operationsListBox.SelectedItemViewControl = null;
            // 
            // detailsViewPanel
            // 
            resources.ApplyResources(this.detailsViewPanel, "detailsViewPanel");
            this.detailsViewPanel.BackColor = System.Drawing.Color.Transparent;
            this.detailsViewPanel.BaseColor = System.Drawing.SystemColors.Control;
            this.detailsViewPanel.BorderColor = System.Drawing.Color.Transparent;
            this.detailsViewPanel.DropShadow = false;
            this.detailsViewPanel.Glossy = false;
            this.detailsViewPanel.LightingColor = System.Drawing.SystemColors.Control;
            this.detailsViewPanel.Name = "detailsViewPanel";
            this.detailsViewPanel.Radius = 3;
            // 
            // footerPanel
            // 
            this.footerPanel.BackColor = System.Drawing.SystemColors.Control;
            this.footerPanel.BaseColor = System.Drawing.Color.Transparent;
            this.footerPanel.BorderColor = System.Drawing.SystemColors.ControlDark;
            this.footerPanel.Controls.Add(this.cancelButton);
            this.footerPanel.Controls.Add(this.okButton);
            resources.ApplyResources(this.footerPanel, "footerPanel");
            this.footerPanel.DropShadow = false;
            this.footerPanel.Glossy = false;
            this.footerPanel.LightingColor = System.Drawing.Color.Transparent;
            this.footerPanel.Name = "footerPanel";
            this.footerPanel.Radius = 1;
            // 
            // OperationPickerDialog
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13f);
            this.AutoSize = true;
            this.AcceptButton = this.okButton;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.CancelButton = this.cancelButton;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.operationsToolStrip);
            this.Controls.Add(this.operationsPanel);
            this.Controls.Add(this.detailsViewPanel);
            this.Controls.Add(this.footerPanel);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.HelpButton = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OperationPickerDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Load += new System.EventHandler(this.OperationPickerDialogLoad);
            this.operationsToolStrip.ResumeLayout(false);
            this.operationsToolStrip.PerformLayout();
            this.operationsPanel.ResumeLayout(false);
            this.footerPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip operationsToolStrip;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripButton importContractButton;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripButton addOperationButton;
        private GradientPanel operationsPanel;
        private RichListBox operationsListBox;
        private GradientPanel detailsViewPanel;
        private GradientPanel footerPanel;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;


    }
}

