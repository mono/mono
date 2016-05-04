//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.Workflow.Activities.Design
{
    partial class ServiceContractViewControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServiceContractViewControl));
            this.contractNameLabel = new System.Windows.Forms.Label();
            this.contractIconPictureBox = new System.Windows.Forms.PictureBox();
            this.backgroundPanel = new System.Workflow.Activities.Design.GradientPanel();
            ((System.ComponentModel.ISupportInitialize)(this.contractIconPictureBox)).BeginInit();
            this.backgroundPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // contractNameLabel
            // 
            this.contractNameLabel.AutoEllipsis = true;
            this.contractNameLabel.BackColor = System.Drawing.Color.Transparent;
            this.contractNameLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            resources.ApplyResources(this.contractNameLabel, "contractNameLabel");
            this.contractNameLabel.Name = "contractNameLabel";
            // 
            // contractIconPictureBox
            // 
            this.contractIconPictureBox.BackColor = System.Drawing.Color.Transparent;
            this.contractIconPictureBox.Image = global::System.ServiceModel.ImageResources.Contract;
            resources.ApplyResources(this.contractIconPictureBox, "contractIconPictureBox");
            this.contractIconPictureBox.Name = "contractIconPictureBox";
            this.contractIconPictureBox.TabStop = false;
            // 
            // backgroundPanel
            // 
            this.backgroundPanel.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.backgroundPanel.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.backgroundPanel.Controls.Add(this.contractNameLabel);
            this.backgroundPanel.Controls.Add(this.contractIconPictureBox);
            resources.ApplyResources(this.backgroundPanel, "backgroundPanel");
            this.backgroundPanel.DropShadow = false;
            this.backgroundPanel.Glossy = false;
            this.backgroundPanel.LightingColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.backgroundPanel.Name = "backgroundPanel";
            this.backgroundPanel.Radius = 2;
            // 
            // ServiceContractViewControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.backgroundPanel);
            this.Name = "ServiceContractViewControl";
            resources.ApplyResources(this, "$this");
            ((System.ComponentModel.ISupportInitialize)(this.contractIconPictureBox)).EndInit();
            this.backgroundPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label contractNameLabel;
        private System.Windows.Forms.PictureBox contractIconPictureBox;
        private GradientPanel backgroundPanel;
    }
}
