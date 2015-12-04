//------------------------------------------------------------------------------
// <copyright file="EntityDataSourceConfigureObjectContextPanel.designer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//------------------------------------------------------------------------------

using System.Windows.Forms;
namespace System.Web.UI.Design.WebControls
{
    partial class EntityDataSourceConfigureObjectContextPanel
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

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._databaseConnectionGroupLabel = new System.Windows.Forms.Label();
            this._databaseConnectionGroupBox = new System.Windows.Forms.GroupBox();
            this._namedConnectionRadioButton = new System.Windows.Forms.RadioButton();
            this._namedConnectionComboBox = new System.Windows.Forms.ComboBox();
            this._connectionStringTextBox = new System.Windows.Forms.TextBox();
            this._connectionStringRadioButton = new System.Windows.Forms.RadioButton();
            this._containerNameLabel = new System.Windows.Forms.Label();
            this._containerNameComboBox = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            this.InitializeSizes();

            // 
            // _databaseConnectionGroupLabel
            // 
            this._databaseConnectionGroupLabel.AutoSize = true;
            this._databaseConnectionGroupLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._databaseConnectionGroupLabel.Name = "_databaseConnectionGroupLabel";
            // 
            // _databaseConnectionGroupBox
            // 
            this._databaseConnectionGroupBox.Controls.Add(this._namedConnectionRadioButton);
            this._databaseConnectionGroupBox.Controls.Add(this._namedConnectionComboBox);
            this._databaseConnectionGroupBox.Controls.Add(this._connectionStringRadioButton);
            this._databaseConnectionGroupBox.Controls.Add(this._connectionStringTextBox);
            this._databaseConnectionGroupBox.Name = "_databaseConnectionGroupBox";
            this._databaseConnectionGroupBox.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;             
            // 
            // _namedConnectionRadioButton
            // 
            this._namedConnectionRadioButton.AutoSize = true;
            this._namedConnectionRadioButton.Checked = true;
            this._namedConnectionRadioButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._namedConnectionRadioButton.Name = "_namedConnectionRadioButton";            
            this._namedConnectionRadioButton.UseVisualStyleBackColor = true;
            this._namedConnectionRadioButton.CheckedChanged += new System.EventHandler(this.OnNamedConnectionRadioButton_CheckedChanged);
            // 
            // _namedConnectionComboBox
            // 
            this._namedConnectionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._namedConnectionComboBox.FormattingEnabled = true;
            this._namedConnectionComboBox.Name = "_namedConnectionComboBox";
            this._namedConnectionComboBox.SelectedIndexChanged += new EventHandler(OnNamedConnectionComboBox_SelectedIndexChanged);
            this._namedConnectionComboBox.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            // 
            // _connectionStringRadioButton
            // 
            this._connectionStringRadioButton.AutoSize = true;
            this._connectionStringRadioButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._connectionStringRadioButton.Name = "_connectionStringRadioButton";            
            this._connectionStringRadioButton.UseVisualStyleBackColor = true;
            this._connectionStringRadioButton.CheckedChanged += new System.EventHandler(this.OnConnectionStringRadioButton_CheckedChanged);
            // 
            // _connectionStringTextBox
            // 
            this._connectionStringTextBox.Enabled = false;
            this._connectionStringTextBox.Name = "_connectionStringTextBox";
            this._connectionStringTextBox.TextChanged += new EventHandler(this.OnConnectionStringTextBox_TextChanged);
            this._connectionStringTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            // 
            // _containerNameLabel
            // 
            this._containerNameLabel.AutoSize = true;
            this._containerNameLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._containerNameLabel.Name = "_containerNameLabel";
            // 
            // _containerNameComboBox
            // 
            this._containerNameComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._containerNameComboBox.FormattingEnabled = true;
            this._containerNameComboBox.Name = "_containerNameComboBox";
            this._containerNameComboBox.Enter += new EventHandler(OnContainerNameComboBox_Enter);
            this._containerNameComboBox.SelectedIndexChanged += new System.EventHandler(this.OnContainerNameComboBox_SelectedIndexChanged);
            this._containerNameComboBox.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            // 
            // EntityDataSourceConfigureObjectContextPanel
            // 
            this.Controls.Add(this._databaseConnectionGroupLabel);
            this.Controls.Add(this._databaseConnectionGroupBox);
            this.Controls.Add(this._containerNameLabel);
            this.Controls.Add(this._containerNameComboBox);            
            this.Name = "EntityDataSourceConfigureObjectContextPanel";
            this.Size = new System.Drawing.Size(528, 319);
            this.MinimumSize = this.Size;
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label _databaseConnectionGroupLabel;
        private System.Windows.Forms.GroupBox _databaseConnectionGroupBox;
        private System.Windows.Forms.RadioButton _namedConnectionRadioButton;
        private System.Windows.Forms.ComboBox _namedConnectionComboBox;
        private System.Windows.Forms.RadioButton _connectionStringRadioButton;
        private System.Windows.Forms.TextBox _connectionStringTextBox;
        private System.Windows.Forms.Label _containerNameLabel;
        private System.Windows.Forms.ComboBox _containerNameComboBox;        
    }
}
