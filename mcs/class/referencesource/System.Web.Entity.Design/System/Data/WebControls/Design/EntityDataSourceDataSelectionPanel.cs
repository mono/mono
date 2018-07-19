//------------------------------------------------------------------------------
// <copyright file="EntityDataSourceDataSelectionPanel.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Web.UI.Design.WebControls.Util;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace System.Web.UI.Design.WebControls
{
    internal partial class EntityDataSourceDataSelectionPanel : WizardPanel
    {
        private EntityDataSourceDataSelection _dataSelection;
        private bool _ignoreEvents; // used when a control is set by the wizard, tells the event handlers to do nothing

        public EntityDataSourceDataSelectionPanel()
        {
            InitializeComponent();
            InitializeUI();
            InitializeTabIndexes();
        }

        #region General Initialization
        public void Register(EntityDataSourceDataSelection dataSelection)
        {
            _dataSelection = dataSelection;
        }
        #endregion

        #region Control Initialization
        private void InitializeSizes()
        {
            int top = 0;
            
            _entitySetLabel.Location = new Point(9, top + 8);
            _entitySetLabel.Size = new Size(80, 13);
            top = _entitySetLabel.Bottom;
            
            _entitySetComboBox.Location = new Point(12, top + 3);
            _entitySetComboBox.Size = new Size(502, 21);
            top = _entitySetComboBox.Bottom;

            _entityTypeFilterLabel.Location = new Point(9, top + 6);
            _entityTypeFilterLabel.Size = new Size(118, 13);
            top = _entityTypeFilterLabel.Bottom;

            _entityTypeFilterComboBox.Location = new System.Drawing.Point(12, top + 3);
            _entityTypeFilterComboBox.Size = new System.Drawing.Size(502, 21);
            top = _entityTypeFilterComboBox.Bottom;

            _selectLabel.Location = new Point(9, top + 6);
            _selectLabel.Size = new Size(40, 13);
            top = _selectLabel.Bottom;

            _selectAdvancedTextBox.Location = new Point(12, top + 3);
            _selectAdvancedTextBox.Multiline = true; // set this here so the size will be set properly
            _selectAdvancedTextBox.Size = new Size(502, 137);            
             // don't need to set top here because the next control has the same location and size

            _selectSimpleCheckedListBox.Location = _selectAdvancedTextBox.Location;
            _selectSimpleCheckedListBox.Size = _selectAdvancedTextBox.Size;
            _selectSimpleCheckedListBox.ColumnWidth = 225;
            top = _selectSimpleCheckedListBox.Bottom;

            _insertUpdateDeletePanel.Location = new Point(12, top + 3);
            _insertUpdateDeletePanel.Size = new Size(502, 69);
            top = 0; // position of rest of controls are relative to this panel

            _enableInsertCheckBox.Location = new Point(3, top + 3);
            _enableInsertCheckBox.Size = new Size(141, 17);
            top = _enableInsertCheckBox.Bottom;

            _enableUpdateCheckBox.Location = new Point(3, top + 6);
            _enableUpdateCheckBox.Size = new Size(149, 17);
            top = _enableUpdateCheckBox.Bottom;

            _enableDeleteCheckBox.Location = new Point(3, top + 6);
            _enableDeleteCheckBox.Size = new Size(145, 17);

            // if any controls are added, top should be reset to _insertUpdateDeletePanel.Bottom before adding them here
        }

        private void InitializeTabIndexes()
        {
            _entitySetLabel.TabStop = false;
            _entitySetComboBox.TabStop = true;
            _entityTypeFilterLabel.TabStop = false;
            _entityTypeFilterComboBox.TabStop = true;
            _selectLabel.TabStop = false;
            _selectSimpleCheckedListBox.TabStop = true;
            _selectAdvancedTextBox.TabStop = true;
            _insertUpdateDeletePanel.TabStop = false;
            _enableInsertCheckBox.TabStop = true;
            _enableDeleteCheckBox.TabStop = true;
            _enableUpdateCheckBox.TabStop = true;

            int tabIndex = 0;

            _entitySetLabel.TabIndex = tabIndex += 10;
            _entitySetComboBox.TabIndex = tabIndex += 10;
            _entityTypeFilterLabel.TabIndex = tabIndex += 10;
            _entityTypeFilterComboBox.TabIndex = tabIndex += 10;
            _selectLabel.TabIndex = tabIndex += 10;
            _selectSimpleCheckedListBox.TabIndex = tabIndex += 10;
            _selectAdvancedTextBox.TabIndex = tabIndex += 10;
            _insertUpdateDeletePanel.TabIndex = tabIndex += 10;
            _enableInsertCheckBox.TabIndex = tabIndex += 10;            
            _enableUpdateCheckBox.TabIndex = tabIndex += 10;
            _enableDeleteCheckBox.TabIndex = tabIndex += 10;
        }

        private void InitializeUI()
        {
            _entitySetLabel.Text = Strings.Wizard_DataSelectionPanel_EntitySetName;
            _entitySetComboBox.AccessibleName = Strings.Wizard_DataSelectionPanel_EntitySetNameAccessibleName;
            _entityTypeFilterLabel.Text = Strings.Wizard_DataSelectionPanel_EntityTypeFilter;
            _entityTypeFilterComboBox.AccessibleName = Strings.Wizard_DataSelectionPanel_EntityTypeFilterAccessibleName;
            _selectLabel.Text = Strings.Wizard_DataSelectionPanel_Select;
            _selectSimpleCheckedListBox.AccessibleName = Strings.Wizard_DataSelectionPanel_SelectAccessibleName;
            _selectAdvancedTextBox.AccessibleName = Strings.Wizard_DataSelectionPanel_SelectAccessibleName;
            _enableInsertCheckBox.Text = Strings.Wizard_DataSelectionPanel_Insert;
            _enableInsertCheckBox.AccessibleName = Strings.Wizard_DataSelectionPanel_InsertAccessibleName;
            _enableDeleteCheckBox.Text = Strings.Wizard_DataSelectionPanel_Delete;
            _enableDeleteCheckBox.AccessibleName = Strings.Wizard_DataSelectionPanel_DeleteAccessibleName;
            _enableUpdateCheckBox.Text = Strings.Wizard_DataSelectionPanel_Update;
            _enableUpdateCheckBox.AccessibleName = Strings.Wizard_DataSelectionPanel_UpdateAccessibleName;
            this.Caption = Strings.Wizard_DataSelectionPanel_Caption;
            this.AccessibleName = Strings.Wizard_DataSelectionPanel_Caption;
        }
        #endregion

        #region Control Event Handlers
        private void OnEnableDeleteCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!_ignoreEvents)
            {
                _dataSelection.SelectEnableDelete(_enableDeleteCheckBox.Checked);
                // this property has no effect on the wizard, so don't need to update it
            }
        }

        private void OnEnableInsertCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!_ignoreEvents)
            {
                _dataSelection.SelectEnableInsert(_enableInsertCheckBox.Checked);
                // this property has no effect on the wizard, so don't need to update it
            }
        }

        private void OnEnableUpdateCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!_ignoreEvents)
            {
                _dataSelection.SelectEnableUpdate(_enableUpdateCheckBox.Checked);
                // this property has no effect on the wizard, so don't need to update it
            }
        }
        
        private void OnEntitySetComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_ignoreEvents)
            {
                _dataSelection.SelectEntitySetName(_entitySetComboBox.SelectedItem as EntityDataSourceEntitySetNameItem);
                _dataSelection.UpdateWizardState();
            }
        }

        private void OnEntityTypeFilterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_ignoreEvents)
            {
                _dataSelection.SelectEntityTypeFilter(_entityTypeFilterComboBox.SelectedItem as EntityDataSourceEntityTypeFilterItem);
                _dataSelection.UpdateWizardState();
            }
        }

        private void OnSelectAdvancedTextBox_TextChanged(object sender, EventArgs e)
        {
            if (!_ignoreEvents)
            {
                _dataSelection.SelectAdvancedSelect(_selectAdvancedTextBox.Text);
                _dataSelection.UpdateWizardState();
            }
        }

        private void OnSelectSimpleCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (!_ignoreEvents)
            {
                if (e.NewValue == CheckState.Checked)
                {
                    // if any other box is checked except 'Select All (Entity Value)', clear 'Select All (Entity Value)'
                    if (e.Index != 0)
                    {
                        _selectSimpleCheckedListBox.SetItemChecked(0, false);
                        _dataSelection.SelectEnableInsert(false);
                        _dataSelection.SelectEnableUpdate(false);
                        _dataSelection.SelectEnableDelete(false);
                    }
                    // if 'Select All (Entity Value)' is checked, uncheck all others
                    else
                    {
                        // disable events while we bulk clear the selected items
                        _ignoreEvents = true;
                        // this event occurs before the check state is changed on the current selection, so it won't
                        // uncheck the box that triggered this event
                        foreach (int checkedIndex in _selectSimpleCheckedListBox.CheckedIndices)
                        {
                            _selectSimpleCheckedListBox.SetItemChecked(checkedIndex, false);
                        }
                        _ignoreEvents = false;
                        _dataSelection.ClearAllSelectedProperties();                        
                    }

                    // Add the current index to the list of selected properties in temporary state storage
                    _dataSelection.SelectEntityProperty(e.Index);
                }
                else
                {
                    // Remove the current index from the list of selected properties in temporary state storage.
                    _dataSelection.DeselectEntityProperty(e.Index);                    
                }

                _dataSelection.UpdateInsertUpdateDeleteState();

                // If there are no longer any properties checked at this point, the Finish button will be disabled
                _dataSelection.UpdateWizardState();
            }
        }        
        #endregion

        #region Wizard State Management
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (Visible)
            {
                _dataSelection.UpdateWizardState();
            }
        }
        #endregion


        #region Methods for setting control values
        public void SetEnableInsertUpdateDelete(bool enableInsert, bool enableUpdate, bool enableDelete)
        {
            _ignoreEvents = true;
            _enableInsertCheckBox.Checked = enableInsert;
            _enableUpdateCheckBox.Checked = enableUpdate;
            _enableDeleteCheckBox.Checked = enableDelete;
            _ignoreEvents = false;
        }

        public void SetEnableInsertUpdateDeletePanel(bool enablePanel)
        {
            _ignoreEvents = true;
            _insertUpdateDeletePanel.Enabled = enablePanel;
            _ignoreEvents = false;
        }

        public void SetEntitySetNames(List<EntityDataSourceEntitySetNameItem> entitySetNames)
        {
            _ignoreEvents = true;
            _entitySetComboBox.Items.Clear();
            _entitySetComboBox.Items.AddRange(entitySetNames.ToArray());
            _ignoreEvents = false;
        }

        // Populates the CheckedListBox with the specified entityTypeProperties and checks all of the indexes specified in selectedEntityTypeProperties
        // Expects that 'Select All (Entity Value)' is in the list of properties already
        public void SetEntityTypeProperties(List<string> entityTypeProperties, List<int> selectedEntityTypeProperties)
        {
            Debug.Assert(entityTypeProperties != null && entityTypeProperties.Count > 0, "unexpected null or empty entityTypeProperties");
            Debug.Assert(selectedEntityTypeProperties != null && selectedEntityTypeProperties.Count > 0, "unexpected null or empty selectedEntityTypeProperties");

            _ignoreEvents = true;
            // remove any items currently in the list and replace them with the current list and selected properties
            _selectSimpleCheckedListBox.Items.Clear();
            _selectSimpleCheckedListBox.Items.AddRange(entityTypeProperties.ToArray());

            foreach (int entityProperty in selectedEntityTypeProperties)
            {
                // check all of the items in the list of selected properties
                _selectSimpleCheckedListBox.SetItemChecked(entityProperty, true);
            }

            // Update the controls
            _selectAdvancedTextBox.Visible = false;
            _selectSimpleCheckedListBox.Visible = true;
            _ignoreEvents = false;
        }

        public void SetEntityTypeFilters(List<EntityDataSourceEntityTypeFilterItem> entityTypeFilters)
        {
            _ignoreEvents = true;
            _entityTypeFilterComboBox.Items.Clear();            
            _entityTypeFilterComboBox.Items.AddRange(entityTypeFilters.ToArray());
            _ignoreEvents = false;
        }

        public void SetSelectedEntityTypeFilter(EntityDataSourceEntityTypeFilterItem selectedEntityTypeFilter)
        {
            Debug.Assert(selectedEntityTypeFilter != null, "cannot select null from EntityTypeFilter combobox");

            _ignoreEvents = true;
            _entityTypeFilterComboBox.SelectedItem = selectedEntityTypeFilter;            
            _ignoreEvents = false;
        }

        public void SetSelect(string select)
        {
            _ignoreEvents = true;
            _selectAdvancedTextBox.Text = select;
            _selectAdvancedTextBox.Visible = true;
            _selectSimpleCheckedListBox.Visible = false;
            _ignoreEvents = false;
        }

        public void SetSelectedEntitySetName(EntityDataSourceEntitySetNameItem selectedEntitySet)
        {
            _ignoreEvents = true;
            if (selectedEntitySet == null)
            {
                _entitySetComboBox.SelectedIndex = -1;
            }
            else
            {
                _entitySetComboBox.SelectedItem = selectedEntitySet;
            }
            _ignoreEvents = false;
        }
        #endregion
    }
}
