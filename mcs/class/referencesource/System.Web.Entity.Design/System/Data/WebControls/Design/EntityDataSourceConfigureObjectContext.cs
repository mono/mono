//------------------------------------------------------------------------------
// <copyright file="EntityDataSourceConfigureObjectContext.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//
// Manages the properties that can be set on the first page of the wizard
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Web.UI.Design.WebControls.Util;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;

namespace System.Web.UI.Design.WebControls
{
    // delegate for event handler to process notifications when the DefaultContainerName is changed
    internal delegate void EntityDataSourceContainerChangedEventHandler(object sender, EntityDataSourceContainerNameItem newContainerName);

    internal class EntityDataSourceConfigureObjectContext
    {
        #region Private readonly fields
        private readonly EntityDataSourceConfigureObjectContextPanel _panel;
        private readonly EntityDataSourceDesignerHelper _helper;
        #endregion

        #region Private writeable fields
        private EntityDataSourceContainerChangedEventHandler _containerNameChanged; // used to notify the DataSelection panel that a change has been made
        #endregion

        #region Private fields for temporary storage of property values
        private EntityConnectionStringBuilderItem _selectedConnectionStringBuilder;
        private bool _connectionStringHasValue;
        private List<EntityConnectionStringBuilderItem> _namedConnections;
        private List<EntityDataSourceContainerNameItem> _containerNames;
        private EntityDataSourceContainerNameItem _selectedContainerName;
        private readonly EntityDataSourceState _entityDataSourceState;        
        private readonly EntityDataSourceWizardForm _wizardForm;
        #endregion

        #region Constructors
        internal EntityDataSourceConfigureObjectContext(EntityDataSourceConfigureObjectContextPanel panel, EntityDataSourceWizardForm wizardForm, EntityDataSourceDesignerHelper helper, EntityDataSourceState entityDataSourceState)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                _panel = panel;
                _helper = helper;

                // Explicitly load metadata here to ensure that we get the latest changes in the project
                _helper.ReloadResources();

                _panel.Register(this);

                _wizardForm = wizardForm;
                _entityDataSourceState = entityDataSourceState;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
        #endregion

        #region Events
        internal event EntityDataSourceContainerChangedEventHandler ContainerNameChanged
        {
            add
            {
                _containerNameChanged += value;
            }
            remove
            {
                _containerNameChanged -= value;
            }
        }

        // Fires the event to notify that a container has been chosen from the list
        private void OnContainerNameChanged(EntityDataSourceContainerNameItem selectedContainerName)
        {
            if (_containerNameChanged != null)
            {
                _containerNameChanged(this, selectedContainerName);
            }
        }
        
        #endregion        

        #region Methods to manage temporary state and wizard contents
        // Save current wizard settings back to the EntityDataSourceState
        internal void SaveState()
        {
            SaveConnectionString();
            SaveContainerName();            
        }

        // Load the initial state of the wizard
        internal void LoadState()
        {
            LoadConnectionStrings();
            LoadContainerNames(_entityDataSourceState.DefaultContainerName, true /*initialLoad*/);            
        }

        #region DefaultContainerName
        /// <summary>
        /// Populates the DefaultContainerName ComboBox with all of the EntityContainers in the loaded metadata
        /// If the specified DefaultContainerName property on the data source control is not empty and 'initialLoad' is true, 
        /// it is added to the list and selected in the control
        /// </summary>
        /// <param name="containerName">The container name to find</param>
        /// <param name="initialLoad">if true, this is the initial load so the container name is added to the list if it is not found.</param>
        private void LoadContainerNames(string containerName, bool initialLoad)
        {
            // Get a list of EntityContainers from the metadata in the connection string
            _containerNames = _helper.GetContainerNames(false /*sortResults*/);
            
            // Try to find the specified container in list
            _selectedContainerName = FindContainerName(containerName, initialLoad /*addIfNotFound*/);

            // Sort the list now, after we may have added a new entry above
            _containerNames.Sort();

            // Update the controls
            _panel.SetContainerNames(_containerNames);
            _panel.SetSelectedContainerName(_selectedContainerName, initialLoad /*initialLoad*/);
        }

        /// <summary>
        /// Find the current container in the current list of containers
        /// </summary>
        /// <param name="containerName">The container name to find</param>
        /// <param name="addIfNotFound">if true, adds the container name to the list if it is not found.</param>
        /// <returns></returns>
        private EntityDataSourceContainerNameItem FindContainerName(string containerName, bool addIfNotFound)
        {
            Debug.Assert(_containerNames != null, "_containerNames have already been initialized and should not be null");

            if (!String.IsNullOrEmpty(containerName))
            {
                EntityDataSourceContainerNameItem containerToSelect = null;
                foreach (EntityDataSourceContainerNameItem containerNameItem in _containerNames)
                {
                    // Ignore case here when searching the list for a matching item, but set the temporary state property to the
                    // correctly-cased version from metadata so that if the user clicks Finish, the correct one will be saved. This
                    // allows some flexibility the designer without preserving an incorrectly-cased value that could cause errors at runtime.
                    if (String.Equals(containerName, containerNameItem.EntityContainerName, StringComparison.OrdinalIgnoreCase))
                    {
                        containerToSelect = containerNameItem;
                    }
                }

                // didn't find a matching container, so just create a placeholder for one using the specified name and add it to the list
                if (containerToSelect == null && addIfNotFound)
                {
                    containerToSelect = new EntityDataSourceContainerNameItem(containerName);
                    _containerNames.Add(containerToSelect);                    
                }

                Debug.Assert(addIfNotFound == false || containerToSelect != null, "expected a non-null EntityDataSourceContainerNameItem");
                return containerToSelect;
            }

            return null;
        }

        // Set the container name in temporary storage, update the connection string, and fire the event so the EntitySet will know there has been a change
        internal void SelectContainerName(EntityDataSourceContainerNameItem selectedContainer)
        {
            _selectedContainerName = selectedContainer;

            UpdateWizardState();
            OnContainerNameChanged(_selectedContainerName);
        }

        private void SaveContainerName()
        {
            Debug.Assert(_selectedContainerName != null, "wizard data should not be saved if container name is empty");
            _entityDataSourceState.DefaultContainerName = _selectedContainerName.EntityContainerName;
        }
        #endregion

        #region ConnectionString
        // Populates the NamedConnection ComboBox with all of the EntityClient connections from the web.config.
        // If the specified connectionString is a named connection (if it contains "name=ConnectionName"), it is added to the list and selected.        
        // If the specified connectionString is not a named connection, the plain connection string option is selected and populated with the specified value.
        private void LoadConnectionStrings()
        {
            // Get a list of all named EntityClient connections in the web.config
            _namedConnections = _helper.GetNamedEntityClientConnections(false /*sortResults*/);

            EntityConnectionStringBuilderItem connStrBuilderItem = _helper.GetEntityConnectionStringBuilderItem(_entityDataSourceState.ConnectionString);
            Debug.Assert(connStrBuilderItem != null, "expected GetEntityConnectionStringBuilder to always return non-null");

            if (connStrBuilderItem.IsNamedConnection)
            {
                // Try to find the specified connection in the list or add it
                connStrBuilderItem = FindCurrentNamedConnection(connStrBuilderItem);
                Debug.Assert(connStrBuilderItem != null, "expected a non-null connStrBuilderItem for the named connection because it should have added it if it didn't exist");
            }

            // Sort results now, after we may have added a new item above
            _namedConnections.Sort();

            SelectConnectionStringBuilder(connStrBuilderItem, false /*resetContainer*/);
            
            // Update the controls
            _panel.SetNamedConnections(_namedConnections);
            _panel.SetConnectionString(_selectedConnectionStringBuilder);
        }

        // Find the current named connection in the list of connections
        // The returned item may refer to the same connection as the specified item, but it will be the actual reference from the list
        private EntityConnectionStringBuilderItem FindCurrentNamedConnection(EntityConnectionStringBuilderItem newBuilderItem)
        {
            Debug.Assert(_namedConnections != null, "_namedConnections should have already been initialized and should not be null");
            Debug.Assert(newBuilderItem != null && newBuilderItem.IsNamedConnection, "expected non-null newBuilderItem");

            foreach (EntityConnectionStringBuilderItem namedConnectionItem in _namedConnections)
            {
                if (((IComparable<EntityConnectionStringBuilderItem>)newBuilderItem).CompareTo(namedConnectionItem) == 0)
                {
                    // returning the one that was actually in the list, so we can select it in the control
                    return namedConnectionItem;
                }
            }

            // didn't find it in the list, so add it
            _namedConnections.Add(newBuilderItem);
            return newBuilderItem;            
        }

        internal EntityConnectionStringBuilderItem GetEntityConnectionStringBuilderItem(string connectionString)
        {
            return _helper.GetEntityConnectionStringBuilderItem(connectionString);
        }

        // Set the connection string in temporary storage
        // Returns true if the metadata was successfully loaded for the specified connections
        internal bool SelectConnectionStringBuilder(EntityConnectionStringBuilderItem selectedConnection, bool resetContainer)
        {
            _selectedConnectionStringBuilder = selectedConnection;

            bool metadataLoaded = false;
            if (selectedConnection != null)
            {
                if (selectedConnection.EntityConnectionStringBuilder != null)
                {
                    metadataLoaded = _helper.LoadMetadata(selectedConnection.EntityConnectionStringBuilder);                    
                }
                else
                {
                    // Since we don't have a valid connection string builder, we don't have enough information to load metadata.
                    // Clear any existing metadata so we don't see an old item collection on any subsequent calls that access it.
                    // Don't need to display an error here because that was handled by the caller who created the builder item
                    _helper.ClearMetadata();                    
                }
            }

            // Reset the list of containers if requested and set the ComboBox to have no selection.
            // In some cases the containers do not need to be reset because the caller wants to delay that until a later event or wants to preserve a specific value
            if (resetContainer)
            {
                string defaultContainerName = _selectedConnectionStringBuilder.EntityConnectionStringBuilder == null ? null : _selectedConnectionStringBuilder.EntityConnectionStringBuilder.Name;
                LoadContainerNames(defaultContainerName, false /*initialLoad*/);
            }
            
            // Update the controls
            UpdateWizardState();

            return metadataLoaded;
        }

        // Set a flag indicating that the connection string textbox or named connection dropdown has a value
        // This value has not be verified at this point, or may not even be complete, so we don't want to validate it yet and turn it into a builder
        internal void SelectConnectionStringHasValue(bool connectionStringHasValue)
        {
            _connectionStringHasValue = connectionStringHasValue;

            // Update the controls
            UpdateWizardState();
        }

        private void SaveConnectionString()
        {
            Debug.Assert(_selectedConnectionStringBuilder != null, "wizard data should not be saved if connection string is empty");
            _entityDataSourceState.ConnectionString = _selectedConnectionStringBuilder.ConnectionString;
        }

        
        #endregion        
        #endregion

        #region Wizard button state management
        // Update the state of the wizard buttons
        internal void UpdateWizardState()
        {
            _wizardForm.SetCanNext(this.CanEnableNext);

            // Finish button should never be enabled at this stage
            _wizardForm.SetCanFinish(false);
        }

        // Next button can only be enabled when the following are true:
        // (1) DefaultContainerName has a value             
        // (2) Either a named connection is selected or the connection string textbox has a value            
        internal bool CanEnableNext
        { 
            get
            {
                return _selectedContainerName != null && _connectionStringHasValue;
            }
        }
        #endregion
    }
}
