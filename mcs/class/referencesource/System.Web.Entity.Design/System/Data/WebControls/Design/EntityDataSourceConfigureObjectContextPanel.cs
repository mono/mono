//------------------------------------------------------------------------------
// <copyright file="EntityDataSourceConfigureObjectContextPanel.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Web.UI.Design.WebControls.Util;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace System.Web.UI.Design.WebControls
{
    internal partial class EntityDataSourceConfigureObjectContextPanel : WizardPanel
    {
        private EntityDataSourceConfigureObjectContext _configureObjectContext;
        private bool _ignoreEvents; // used when a control value is set by the wizard, tells the event handlers to do nothing
        private bool _connectionInEdit; // indicates that a change has been made to the connection and it has not yet been validated
        
        #region Constructors
        internal EntityDataSourceConfigureObjectContextPanel()
        {
            InitializeComponent();
            InitializeUI();
            InitializeTabIndexes();
        }
        #endregion

        #region General initialization
        internal void Register(EntityDataSourceConfigureObjectContext configureObjectContext)
        {
            _configureObjectContext = configureObjectContext;
        }
        #endregion

        #region Control Initialization
        private void InitializeSizes()
        {
            int top = 25;

            _databaseConnectionGroupLabel.Location = new Point(10, top);
            _databaseConnectionGroupLabel.Size = new Size(500, 13);
            top = _databaseConnectionGroupLabel.Bottom;

            _databaseConnectionGroupBox.Location = new Point(13, top);
            _databaseConnectionGroupBox.Size = new Size(503, 124);
            top = 0; // rest of controls in this group are positioned relative to the group box, so top resets

            _namedConnectionRadioButton.Location = new Point(9, top + 20);
            _namedConnectionRadioButton.Size = new Size(116, 17);
            top = _namedConnectionRadioButton.Bottom;

            _namedConnectionComboBox.Location = new Point(25, top + 6);
            _namedConnectionComboBox.Size = new Size(454, 21);
            top = _namedConnectionComboBox.Bottom;

            _connectionStringRadioButton.Location = new Point(9, top + 6);
            _connectionStringRadioButton.Size = new Size(109, 17);
            top = _connectionStringRadioButton.Bottom;

            _connectionStringTextBox.Location = new Point(25, top + 6);
            _connectionStringTextBox.Size = new Size(454, 20);
            top = _databaseConnectionGroupBox.Bottom;

            _containerNameLabel.Location = new Point(10, top + 30);
            _containerNameLabel.Size = new Size(117, 13);
            top = _containerNameLabel.Bottom;

            _containerNameComboBox.Location = new Point(13, top + 3);
            _containerNameComboBox.Size = new Size(502, 21);
            // if any controls are added, top should be reset to _containerNameComboBox.Bottom before adding them here
        }

        private void InitializeTabIndexes()
        {
            _databaseConnectionGroupLabel.TabStop = false;
            _databaseConnectionGroupBox.TabStop = false;
            _namedConnectionComboBox.TabStop = true;
            _connectionStringTextBox.TabStop = true;
            _containerNameLabel.TabStop = false;
            _containerNameComboBox.TabStop = true;
            
            int tabIndex = 0;
            _databaseConnectionGroupLabel.TabIndex = tabIndex += 10;
            _databaseConnectionGroupBox.TabIndex = tabIndex += 10;
            _namedConnectionRadioButton.TabIndex = tabIndex += 10;
            _namedConnectionComboBox.TabIndex = tabIndex += 10;
            _connectionStringRadioButton.TabIndex = tabIndex += 10;
            _connectionStringTextBox.TabIndex = tabIndex += 10;
            _containerNameLabel.TabIndex = tabIndex += 10;
            _containerNameComboBox.TabIndex = tabIndex += 10;            
        }

        private void InitializeUI()
        {
            this._databaseConnectionGroupLabel.Text = Strings.Wizard_ObjectContextPanel_ConnectionStringGroupDescription;
            this._connectionStringRadioButton.Text = Strings.Wizard_ObjectContextPanel_ConnectionStringRadioButton;
            this._connectionStringRadioButton.AccessibleName = Strings.Wizard_ObjectContextPanel_ConnectionStringRadioButtonAccessibleName;
            this._connectionStringTextBox.AccessibleName = Strings.Wizard_ObjectContextPanel_ConnectionStringRadioButtonAccessibleName;
            this._namedConnectionRadioButton.Text = Strings.Wizard_ObjectContextPanel_NamedConnectionRadioButton;
            this._namedConnectionRadioButton.AccessibleName = Strings.Wizard_ObjectContextPanel_NamedConnectionRadioButtonAccessibleName;
            this._namedConnectionComboBox.AccessibleName = Strings.Wizard_ObjectContextPanel_NamedConnectionRadioButtonAccessibleName;
            this._containerNameLabel.Text = Strings.Wizard_ObjectContextPanel_DefaultContainerName;
            this._containerNameComboBox.AccessibleName = Strings.Wizard_ObjectContextPanel_DefaultContainerNameAccessibleName;
            this.Caption = Strings.Wizard_ObjectContextPanel_Caption;
            this.AccessibleName = Strings.Wizard_ObjectContextPanel_Caption;
        }
        #endregion

        #region Control Event Handlers
        private void OnConnectionStringRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (!_ignoreEvents)
            {
                if (_connectionStringRadioButton.Checked)
                {
                    // Update the state of the controls that are associated with the radio buttons
                    _namedConnectionComboBox.Enabled = false;
                    _connectionStringTextBox.Enabled = true;

                    EnterConnectionEditMode();

                    // Update the flag to track if we have text in the box
                    _configureObjectContext.SelectConnectionStringHasValue(!String.IsNullOrEmpty(_connectionStringTextBox.Text));

                    // Move the focus to the associated TextBox
                    _connectionStringTextBox.Select();
                    _connectionStringTextBox.Select(0, _connectionStringTextBox.TextLength);
                }
            }
            // else it's being unchecked, so that means another radio button is being checked and that handler will take care of updating the state
        }

        private void OnConnectionStringTextBox_TextChanged(object sender, EventArgs e)
        {
            if (!_ignoreEvents)
            {
                // If we aren't already in edit mode, move to it that we will know we need to reload metadata if it's needed later
                if (!_connectionInEdit)
                {
                    EnterConnectionEditMode();
                }

                // Update the state of the flag that tracks if there is anything in this TextBox.
                // This will cause the Next button to be disabled if all of the text is removed from the box, otherwise it is enabled
                _configureObjectContext.SelectConnectionStringHasValue(!String.IsNullOrEmpty(_connectionStringTextBox.Text));
            }
        }

        private void OnNamedConnectionRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (!_ignoreEvents)
            {
                if (_namedConnectionRadioButton.Checked)
                {
                    // update the controls that are associated with the radio buttons
                    _namedConnectionComboBox.Enabled = true;
                    _connectionStringTextBox.Enabled = false;

                    EnterConnectionEditMode();

                    // Update flag to indicate if there is a value selected in this box
                    _configureObjectContext.SelectConnectionStringHasValue(_namedConnectionComboBox.SelectedIndex != -1);

                    // Move the focus to the associated ComboBox
                    _namedConnectionComboBox.Select();

                    // If there is a selected NamedConnection, validate the connection string right away
                    // so that we can potentially select the default container name if there is one
                    if (_namedConnectionComboBox.SelectedIndex != -1)
                    {
                        VerifyConnectionString();
                    }
                }
                // else it's being unchecked, so that means another radio button is being checked and that handler will take care of updating the state            
            }
        }

        private void OnNamedConnectionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_ignoreEvents)
            {
                EnterConnectionEditMode();

                // Update flag to indicate if there is a value selected in this box
                _configureObjectContext.SelectConnectionStringHasValue(_namedConnectionComboBox.SelectedIndex != -1);

                // If there is a selected NamedConnection, validate the connection string right away
                // so that we can potentially select the default container name if there is one
                if (_namedConnectionComboBox.SelectedIndex != -1)
                {
                    VerifyConnectionString();
                }
            }
        }

        private void OnContainerNameComboBox_Enter(object sender, EventArgs e)
        {
            if (!_ignoreEvents)
            {
                VerifyConnectionString();
            }
        }

        private void OnContainerNameComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_ignoreEvents)
            {
                _configureObjectContext.SelectContainerName(_containerNameComboBox.SelectedItem as EntityDataSourceContainerNameItem);
            }
        }

        // Move to edit mode so that we will know we need to reload metadata if it's needed later
        private void EnterConnectionEditMode()
        {
            _connectionInEdit = true;
            _containerNameComboBox.SelectedIndex = -1;
        }

        private void LeaveConnectionEditMode()
        {
            _connectionInEdit = false;
        }

        /// <summary>
        /// Verify the selected connection string and load the metadata for it if it is successfully verified
        /// </summary>
        /// <returns>True if the metadata was successfully loaded from the connection string</returns>
        private bool VerifyConnectionString()
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                if (_connectionInEdit)
                {
                    bool isNamedConnection = _namedConnectionRadioButton.Checked;
                    Debug.Assert(!isNamedConnection ? _connectionStringRadioButton.Checked : true, "only expecting either named connection or connection string radio button options");

                    EntityConnectionStringBuilderItem selectedConnection = null;
                    if (isNamedConnection)
                    {
                        if (_namedConnectionComboBox.SelectedIndex != -1)
                        {
                            selectedConnection = _namedConnectionComboBox.SelectedItem as EntityConnectionStringBuilderItem;
                        }
                    }
                    else
                    {
                        // Make a builder item out of the specified connection string. This will do some basic verification on the string.
                        selectedConnection = _configureObjectContext.GetEntityConnectionStringBuilderItem(_connectionStringTextBox.Text);
                    }

                    if (selectedConnection != null)
                    {
                        bool metadataLoaded = _configureObjectContext.SelectConnectionStringBuilder(selectedConnection, true /*resetContainer*/);

                        // If verification failed, try to move the focus back to the appropriate control.
                        if (!metadataLoaded)
                        {
                            if (_namedConnectionRadioButton.Checked)
                            {
                                _namedConnectionComboBox.Select();
                            }
                            else
                            {
                                _connectionStringTextBox.Select();
                                _connectionStringTextBox.Select(0, _connectionStringTextBox.TextLength);
                            }
                        }
                    }

                    // Leave connection edit mode regardless of whether the metadata was loaded or not, because there is no need to keep trying
                    // to validated it over and over again unless the user makes a change that puts it back into edit mode again
                    LeaveConnectionEditMode();
                }

                return true;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
        #endregion

        #region Wizard state management
        public override bool OnNext()
        {
            Debug.Assert(_configureObjectContext.CanEnableNext, "OnNext called when CanEnableNext is false");

            return VerifyConnectionString();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (Visible)
            {
                _configureObjectContext.UpdateWizardState();
            }
        }
        #endregion

        #region Methods for setting control values
        // Expects that the specified builder item is a named connection already in the list, is a full connection string, or is empty
        // If empty, the default is to select the named connection option and don't select anything in the list
        internal void SetConnectionString(EntityConnectionStringBuilderItem connStrBuilderItem)
        {
            Debug.Assert(connStrBuilderItem != null, "expected non-null connStrBuilderItem");

            _ignoreEvents = true;

            // set the state of the ConnectionString radio buttons and associated controls
            bool isNamedConnection = connStrBuilderItem.IsEmpty || connStrBuilderItem.IsNamedConnection; 
           
            _namedConnectionRadioButton.Checked = isNamedConnection;
            _namedConnectionComboBox.Enabled = isNamedConnection;
            _connectionStringRadioButton.Checked = !isNamedConnection;
            _connectionStringTextBox.Enabled = !isNamedConnection;

            // set the value of the control that was just enabled
            if (connStrBuilderItem.IsEmpty)
            {
                _namedConnectionComboBox.SelectedIndex = -1;
                _configureObjectContext.SelectConnectionStringHasValue(false /*connectionStringHasValue*/);
            }
            else if (connStrBuilderItem.IsNamedConnection)
            {
                _namedConnectionComboBox.SelectedItem = connStrBuilderItem;
                _configureObjectContext.SelectConnectionStringHasValue(true /*connectionStringHasValue*/);
            }
            else
            {
                _connectionStringTextBox.Text = connStrBuilderItem.ConnectionString;
                _configureObjectContext.SelectConnectionStringHasValue(!connStrBuilderItem.IsEmpty);
            }

            _ignoreEvents = false;
        }

        internal void SetContainerNames(List<EntityDataSourceContainerNameItem> containerNames)
        {
            _ignoreEvents = true;
            _containerNameComboBox.Items.Clear();
            _containerNameComboBox.Items.AddRange(containerNames.ToArray());
            _ignoreEvents = false;
        }

        internal void SetNamedConnections(List<EntityConnectionStringBuilderItem> namedConnections)
        {
            _ignoreEvents = true;
            _namedConnectionComboBox.Items.AddRange(namedConnections.ToArray());
            _ignoreEvents = false;
        }

        /// <summary>
        /// Expects that selectedContainer is already in the ComboBox list, or should be null 
        /// </summary>
        /// <param name="selectedContainer"></param>
        /// <param name="initialLoad">If this is the initial load, we want to suppress events so that the change does
        /// not cause additional work in panels that listen to the container name changed event</param>
        internal void SetSelectedContainerName(EntityDataSourceContainerNameItem selectedContainer, bool initialLoad)
        {
            if (initialLoad)
            {
                _ignoreEvents = true;
            }
            if (selectedContainer == null)
            {    
                _containerNameComboBox.SelectedIndex = -1;
            }
            else
            {
                _containerNameComboBox.SelectedItem = selectedContainer;
            }
            _ignoreEvents = false;
        }        
        #endregion
    }
}
