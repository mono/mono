//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.Workflow.Activities.Design
{
    internal partial class ServiceOperationViewControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServiceOperationViewControl));
            this.backgroundPanel = new System.Workflow.Activities.Design.GradientPanel();
            this.isImplementedPictureBox = new System.Windows.Forms.PictureBox();
            this.operationIconPictureBox = new System.Windows.Forms.PictureBox();
            this.operationNameLabel = new System.Windows.Forms.Label();
            this.backgroundPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.isImplementedPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.operationIconPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // backgroundPanel
            // 
            resources.ApplyResources(this.backgroundPanel, "backgroundPanel");
            this.backgroundPanel.BackColor = System.Drawing.SystemColors.Window;
            this.backgroundPanel.BaseColor = System.Drawing.Color.White;
            this.backgroundPanel.BorderColor = System.Drawing.Color.White;
            this.backgroundPanel.Controls.Add(this.isImplementedPictureBox);
            this.backgroundPanel.Controls.Add(this.operationIconPictureBox);
            this.backgroundPanel.Controls.Add(this.operationNameLabel);
            this.backgroundPanel.DropShadow = false;
            this.backgroundPanel.Glossy = true;
            this.backgroundPanel.LightingColor = System.Drawing.Color.White;
            this.backgroundPanel.Name = "backgroundPanel";
            this.backgroundPanel.Radius = 3;
            // 
            // isImplementedPictureBox
            // 
            this.isImplementedPictureBox.BackColor = System.Drawing.Color.Transparent;
            this.isImplementedPictureBox.Image = global::System.ServiceModel.ImageResources.Check;
            resources.ApplyResources(this.isImplementedPictureBox, "isImplementedPictureBox");
            this.isImplementedPictureBox.Name = "isImplementedPictureBox";
            this.isImplementedPictureBox.TabStop = false;
            // 
            // operationIconPictureBox
            // 
            this.operationIconPictureBox.BackColor = System.Drawing.Color.Transparent;
            this.operationIconPictureBox.Image = global::System.ServiceModel.ImageResources.Operation;
            resources.ApplyResources(this.operationIconPictureBox, "operationIconPictureBox");
            this.operationIconPictureBox.Name = "operationIconPictureBox";
            this.operationIconPictureBox.TabStop = false;
            // 
            // operationNameLabel
            // 
            this.operationNameLabel.AutoEllipsis = true;
            this.operationNameLabel.BackColor = System.Drawing.Color.Transparent;
            this.operationNameLabel.ForeColor = System.Drawing.SystemColors.WindowText;
            resources.ApplyResources(this.operationNameLabel, "operationNameLabel");
            this.operationNameLabel.Name = "operationNameLabel";
            // 
            // ServiceOperationViewControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.backgroundPanel);
            this.Name = "ServiceOperationViewControl";
            resources.ApplyResources(this, "$this");
            this.backgroundPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.isImplementedPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.operationIconPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private GradientPanel backgroundPanel;
        private System.Windows.Forms.Label operationNameLabel;
        private System.Windows.Forms.PictureBox operationIconPictureBox;
        private System.Windows.Forms.PictureBox isImplementedPictureBox;

    }
}
