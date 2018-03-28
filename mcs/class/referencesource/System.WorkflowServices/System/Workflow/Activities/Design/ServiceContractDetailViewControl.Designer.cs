//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.Workflow.Activities.Design
{
    internal partial class ServiceContractDetailViewControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServiceContractDetailViewControl));
            this.contractNameLabel = new System.Windows.Forms.Label();
            this.contractNameTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // contractNameLabel
            // 
            resources.ApplyResources(this.contractNameLabel, "contractNameLabel");
            this.contractNameLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.contractNameLabel.Name = "contractNameLabel";
            // 
            // contractNameTextBox
            // 
            resources.ApplyResources(this.contractNameTextBox, "contractNameTextBox");
            this.contractNameTextBox.Name = "contractNameTextBox";
            // 
            // ServiceContractDetailViewControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.contractNameTextBox);
            this.Controls.Add(this.contractNameLabel);
            this.Name = "ServiceContractDetailViewControl";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label contractNameLabel;
        private System.Windows.Forms.TextBox contractNameTextBox;
    }
}
