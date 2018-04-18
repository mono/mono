//------------------------------------------------------------------------------
// <copyright file="EntityDataSourceDataSelectionPanel.designer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//------------------------------------------------------------------------------

using System.Windows.Forms;
namespace System.Web.UI.Design.WebControls
{
    partial class EntityDataSourceDataSelectionPanel
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
            this._entitySetLabel = new System.Windows.Forms.Label();
            this._entitySetComboBox = new System.Windows.Forms.ComboBox();
            this._entityTypeFilterLabel = new System.Windows.Forms.Label();
            this._entityTypeFilterComboBox = new System.Windows.Forms.ComboBox();
            this._selectLabel = new System.Windows.Forms.Label();
            this._selectAdvancedTextBox = new System.Windows.Forms.TextBox();
            this._selectSimpleCheckedListBox = new System.Windows.Forms.CheckedListBox();            
            this._insertUpdateDeletePanel = new System.Windows.Forms.Panel();
            this._enableInsertCheckBox = new System.Windows.Forms.CheckBox();
            this._enableDeleteCheckBox = new System.Windows.Forms.CheckBox();
            this._enableUpdateCheckBox = new System.Windows.Forms.CheckBox();
            this._insertUpdateDeletePanel.SuspendLayout();
            this.SuspendLayout();
            this.InitializeSizes();
            
            //
            // _entitySetLabel
            // 
            this._entitySetLabel.AutoSize = true;
            this._entitySetLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._entitySetLabel.Name = "_entitySetLabel";            
            // 
            // _entitySetComboBox
            // 
            this._entitySetComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._entitySetComboBox.FormattingEnabled = true;
            this._entitySetComboBox.Name = "_entitySetComboBox";
            this._entitySetComboBox.SelectedIndexChanged += new System.EventHandler(this.OnEntitySetComboBox_SelectedIndexChanged);
            this._entitySetComboBox.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            // 
            // _entityTypeFilterLabel
            // 
            this._entityTypeFilterLabel.AutoSize = true;
            this._entityTypeFilterLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._entityTypeFilterLabel.Name = "_entityTypeFilterLabel";
            // 
            // _entityTypeFilterComboBox
            // 
            this._entityTypeFilterComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._entityTypeFilterComboBox.FormattingEnabled = true;
            this._entityTypeFilterComboBox.Name = "_entityTypeFilterComboBox";
            this._entityTypeFilterComboBox.SelectedIndexChanged += new EventHandler(OnEntityTypeFilterComboBox_SelectedIndexChanged);
            this._entityTypeFilterComboBox.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            // 
            // _selectLabel
            // 
            this._selectLabel.AutoSize = true;
            this._selectLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._selectLabel.Name = "_selectLabel";
            // 
            // _selectAdvancedTextBox
            // 
            this._selectAdvancedTextBox.Multiline = true;
            this._selectAdvancedTextBox.Name = "_selectAdvancedTextBox";
            this._selectAdvancedTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._selectAdvancedTextBox.Visible = false;
            this._selectAdvancedTextBox.TextChanged += new EventHandler(OnSelectAdvancedTextBox_TextChanged);
            this._selectAdvancedTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
            // 
            // _selectSimpleCheckedListBox
            // 
            this._selectSimpleCheckedListBox.CheckOnClick = true;            
            this._selectSimpleCheckedListBox.FormattingEnabled = true;
            this._selectSimpleCheckedListBox.HorizontalScrollbar = true;
            this._selectSimpleCheckedListBox.MultiColumn = true;
            this._selectSimpleCheckedListBox.Name = "_selectSimpleCheckedListBox";
            this._selectSimpleCheckedListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(OnSelectSimpleCheckedListBox_ItemCheck);
            this._selectSimpleCheckedListBox.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
            // 
            // _insertUpdateDeletePanel
            // 
            this._insertUpdateDeletePanel.Controls.Add(this._enableInsertCheckBox);            
            this._insertUpdateDeletePanel.Controls.Add(this._enableUpdateCheckBox);
            this._insertUpdateDeletePanel.Controls.Add(this._enableDeleteCheckBox);
            this._insertUpdateDeletePanel.Name = "_insertUpdateDeletePanel";
            this._insertUpdateDeletePanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            // 
            // _enableInsertCheckBox
            // 
            this._enableInsertCheckBox.AutoSize = true;
            this._enableInsertCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._enableInsertCheckBox.Name = "_enableInsertCheckBox";
            this._enableInsertCheckBox.UseVisualStyleBackColor = true;
            this._enableInsertCheckBox.CheckedChanged += new EventHandler(OnEnableInsertCheckBox_CheckedChanged);
            this._enableInsertCheckBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            // 
            // _enableUpdateCheckBox
            // 
            this._enableUpdateCheckBox.AutoSize = true;
            this._enableUpdateCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._enableUpdateCheckBox.Name = "_enableUpdateCheckBox";
            this._enableUpdateCheckBox.UseVisualStyleBackColor = true;
            this._enableUpdateCheckBox.CheckedChanged += new EventHandler(OnEnableUpdateCheckBox_CheckedChanged);
            this._enableUpdateCheckBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            // 
            // _enableDeleteCheckBox
            // 
            this._enableDeleteCheckBox.AutoSize = true;
            this._enableDeleteCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._enableDeleteCheckBox.Name = "_enableDeleteCheckBox";
            this._enableDeleteCheckBox.UseVisualStyleBackColor = true;
            this._enableDeleteCheckBox.CheckedChanged += new EventHandler(OnEnableDeleteCheckBox_CheckedChanged);
            this._enableDeleteCheckBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
                      
            //
            // EntityDataSourceDataSelectionPanel
            //
            this.Controls.Add(this._entitySetLabel);
            this.Controls.Add(this._entitySetComboBox);
            this.Controls.Add(this._entityTypeFilterLabel);
            this.Controls.Add(this._entityTypeFilterComboBox);
            this.Controls.Add(this._selectLabel);
            this.Controls.Add(this._selectSimpleCheckedListBox);
            this.Controls.Add(this._selectAdvancedTextBox);
            this.Controls.Add(this._insertUpdateDeletePanel);
            this.Name = "EntityDataSourceDataSelectionPanel";
            this.Size = new System.Drawing.Size(528, 319);
            this.MinimumSize = this.Size;
            this._insertUpdateDeletePanel.ResumeLayout(false);
            this._insertUpdateDeletePanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label _entitySetLabel;
        private System.Windows.Forms.ComboBox _entitySetComboBox;
        private System.Windows.Forms.Label _entityTypeFilterLabel;
        private System.Windows.Forms.ComboBox _entityTypeFilterComboBox;
        private System.Windows.Forms.Label _selectLabel;
        private System.Windows.Forms.TextBox _selectAdvancedTextBox;
        private System.Windows.Forms.CheckedListBox _selectSimpleCheckedListBox;        
        private System.Windows.Forms.Panel _insertUpdateDeletePanel;
        private System.Windows.Forms.CheckBox _enableInsertCheckBox;
        private System.Windows.Forms.CheckBox _enableDeleteCheckBox;
        private System.Windows.Forms.CheckBox _enableUpdateCheckBox;
        
    }
}
