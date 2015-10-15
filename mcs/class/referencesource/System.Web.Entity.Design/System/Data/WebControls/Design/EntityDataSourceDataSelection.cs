//------------------------------------------------------------------------------
// <copyright file="EntityDataSourceDataSelection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//
// Manages the properties that can be set on the second page of the wizard
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.WebControls
{
    using System.Collections.Generic;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;

    internal class EntityDataSourceDataSelection
    {
        #region Private static fields
        // iterator prefix used in building and parsing Select property value
        private static readonly string s_itKeyword = "it.";
        // Placeholder item to indicate (None) on the EntityTypeFilter ComboBox
        private static readonly EntityDataSourceEntityTypeFilterItem s_entityTypeFilterNoneItem =
                new EntityDataSourceEntityTypeFilterItem(Strings.Wizard_DataSelectionPanel_NoEntityTypeFilter);
        #endregion

        #region Private readonly fields
        private readonly EntityDataSourceDataSelectionPanel _panel;
        private readonly EntityDataSourceDesignerHelper _helper;
        #endregion

        #region Private fields for temporary storage of property values
        private readonly EntityDataSourceState _entityDataSourceState;
        private List<EntityDataSourceEntitySetNameItem> _entitySetNames;
        private EntityDataSourceEntitySetNameItem _selectedEntitySetName;
        private List<EntityDataSourceEntityTypeFilterItem> _entityTypeFilters;
        private EntityDataSourceEntityTypeFilterItem _selectedEntityTypeFilter;

        #region Select views
        // The Data Selection wizard panel can display two kinds of views of the Select property:
        //     (1) Simple Select View: CheckedListBox with a list of available entity type properties
        //     (2) Advanced Select View: TextBox that allows any statement to be entered (no validation)
        //
        // When either view is visible to the user, the fields shown below for that view should be non-null, and the fields
        // for the other view should be null.

        // Simple Select View
        // _selectedEntityTypeProperties contains a set of indexes of properties in _entityTypeProperties
        private List<string> _entityTypeProperties; 
        private List<int> _selectedEntityTypeProperties;

        // Advanced Select View
        private string _select;
        #endregion

        private bool _enableInsert;
        private bool _enableUpdate;
        private bool _enableDelete;
        private readonly EntityDataSourceWizardForm _wizardForm;        
        #endregion

        #region Constructors
        internal EntityDataSourceDataSelection(EntityDataSourceDataSelectionPanel panel, EntityDataSourceWizardForm wizard, EntityDataSourceDesignerHelper designerHelper, EntityDataSourceState entityDataSourceState)
        {
            _panel = panel;
            _panel.Register(this);
            _helper = designerHelper;

            _entityDataSourceState = entityDataSourceState;
            _wizardForm = wizard;
        }
        #endregion

        #region Events
        // Event handler to process notifications when a DefaultContainerName is selected on the ObjectContext configuration panel
        internal void ContainerNameChangedHandler(object sender, EntityDataSourceContainerNameItem newContainerName)
        {
            // Load the entity sets for this container, don't select anything initially in the list
            LoadEntitySetNames(newContainerName, null);
            
            // Reset the other controls that depend on the value of EntitySet
            LoadEntityTypeFilters(null, null);
            LoadSelect(String.Empty);
        }
        #endregion        

        #region Methods to manage temporary state and wizard contents
        // Used when the wizard is launched, to load existing property values from data source control
        internal void LoadState()
        {
            LoadEntitySetNames(_helper.GetEntityContainerItem(_entityDataSourceState.DefaultContainerName), _entityDataSourceState.EntitySetName);
            LoadEntityTypeFilters(_selectedEntitySetName, _entityDataSourceState.EntityTypeFilter);
            LoadSelect(_entityDataSourceState.Select);
            LoadInsertUpdateDelete();
        }

        // Save current wizard settings back to the EntityDataSourceState
        internal void SaveState()
        {
            SaveEntitySetName();
            SaveEntityTypeFilter();
            SaveSelect();
            SaveInsertUpdateDelete();
            SaveEnableFlattening();
        }

        #region EntitySetName
        // Find the specified entitySetName in the list or add it if it's not there
        private EntityDataSourceEntitySetNameItem FindEntitySetName(string entitySetName)
        {
            if (!String.IsNullOrEmpty(entitySetName))
            {
                EntityDataSourceEntitySetNameItem entitySetToSelect = null;
                foreach (EntityDataSourceEntitySetNameItem entitySetNameItem in _entitySetNames)
                {
                    // Ignore case here when searching the list for a matching item, but set the temporary state property to the
                    // correctly-cased version from metadata so that if the user clicks Finish, the correct one will be saved. This
                    // allows some flexibility the designer without preserving an incorrectly-cased value that could cause errors at runtime.                    
                    if (String.Equals(entitySetNameItem.EntitySetName, entitySetName, StringComparison.OrdinalIgnoreCase))
                    {
                        entitySetToSelect = entitySetNameItem;
                    }
                }

                // didn't find a matching entityset, so just create a placeholder for one using the specified name and add it to the list
                if (entitySetToSelect == null)
                {
                    entitySetToSelect = new EntityDataSourceEntitySetNameItem(entitySetName);
                    _entitySetNames.Add(entitySetToSelect);                    
                }

                Debug.Assert(entitySetToSelect != null, "expected a non-null EntityDataSourceEntitySetNameItem");
                return entitySetToSelect;
            }

            return null;
        }

        // Populates the EntitySetName combobox with all of the discoverable EntitySets for the specified container.
        // If the specified entitySetName is not empty, it is added to the list and selected as the initial value
        // containerNameItem may not be backed by a real EntityContainer, in which case there is no way to look up the EntitySet in metadata
        // devnote: This method should not automatically reset EntityTypeFilter and Select because it can be used to load the initial state
        //          for the form, in which case we need to preserve any values that are already set on the data source control.
        private void LoadEntitySetNames(EntityDataSourceContainerNameItem containerNameItem, string entitySetName)
        {
            // If this is a container that we found in the project's metadata, get a list of EntitySets for that container
            if (containerNameItem != null && containerNameItem.EntityContainer != null)
            {
                _entitySetNames = _helper.GetEntitySets(containerNameItem.EntityContainer, false /*sortResults*/);

                // Try to find the specified entityset in list and add it if it isn't there
                _selectedEntitySetName = FindEntitySetName(entitySetName);
            }
            else
            {
                // if this is an unknown container, there is no way to find a list of entitysets from metadata
                // so just create a new list and placeholder for the specified entityset
                _entitySetNames = new List<EntityDataSourceEntitySetNameItem>();
                if (!String.IsNullOrEmpty(entitySetName))
                {
                    _selectedEntitySetName = new EntityDataSourceEntitySetNameItem(entitySetName);
                    _entitySetNames.Add(_selectedEntitySetName);
                }
                else
                {
                    _selectedEntitySetName = null;
                }
            }

            // Sort the list now, after we may have added one above
            _entitySetNames.Sort();

            // Update the controls
            _panel.SetEntitySetNames(_entitySetNames);
            _panel.SetSelectedEntitySetName(_selectedEntitySetName);
        }

        // Set EntitySetName in temporary storage
        internal void SelectEntitySetName(EntityDataSourceEntitySetNameItem selectedEntitySet)
        {
            _selectedEntitySetName = selectedEntitySet;
            // Load the types for the selected EntitySet, don't select one initially
            LoadEntityTypeFilters(selectedEntitySet, null);
            // Reinitialize the Select control with a list of properties, don't preserve any existing Select value
            LoadSelect(String.Empty);
        }

        private void SaveEntitySetName()
        {
            if (_selectedEntitySetName != null)
            {
                _entityDataSourceState.EntitySetName = _selectedEntitySetName.EntitySetName;
            }
            else
            {
                _entityDataSourceState.EntitySetName = String.Empty;
            }
        }
        #endregion

        #region EntityTypeFilter
        // Populate a list with the base type for the EntitySet plus all derived types, and a special entry to indicate no filter
        // devnote: This method should not automatically reset Select because it can be used to load the initial state
        //          for the form, in which case we need to preserve any values that are already set on the data source control.
        private void LoadEntityTypeFilters(EntityDataSourceEntitySetNameItem entitySetItem, string entityTypeFilter)
        {
            // If this is an EntitySet that we found in the project's metadata, get the type information
            if (entitySetItem != null && entitySetItem.EntitySet != null)
            {
                _entityTypeFilters = _helper.GetEntityTypeFilters(entitySetItem.EntitySet.ElementType, false /*sortResults*/);
                // add (None) to the beginning of the list
                _entityTypeFilters.Insert(0, s_entityTypeFilterNoneItem);

                // Try to find the specified type in list and add it if it isn't there
                _selectedEntityTypeFilter = FindEntityTypeFilter(entityTypeFilter);
            }
            else
            {
                // if this is an unknown EntitySet, there is no way to find a list of types from metadata
                // so just create a new list and placeholder for the specified type
                _entityTypeFilters = new List<EntityDataSourceEntityTypeFilterItem>();
                _entityTypeFilters.Add(s_entityTypeFilterNoneItem);

                if (!String.IsNullOrEmpty(entityTypeFilter))
                {
                    _selectedEntityTypeFilter = new EntityDataSourceEntityTypeFilterItem(entityTypeFilter);
                    _entityTypeFilters.Add(_selectedEntityTypeFilter);
                }
                else
                {
                    _selectedEntityTypeFilter = s_entityTypeFilterNoneItem;
                }
            }

            // Sort now after we might have added items above
            _entityTypeFilters.Sort();

            // Update the controls
            _panel.SetEntityTypeFilters(_entityTypeFilters);
            _panel.SetSelectedEntityTypeFilter(_selectedEntityTypeFilter);
        }

        // Find the specified entityTypeFilter in the list and add it if it's not there
        private EntityDataSourceEntityTypeFilterItem FindEntityTypeFilter(string entityTypeFilter)
        {
            if (!String.IsNullOrEmpty(entityTypeFilter))
            {
                EntityDataSourceEntityTypeFilterItem typeToSelect = null;
                foreach (EntityDataSourceEntityTypeFilterItem entityTypeFilterItem in _entityTypeFilters)
                {
                    // Ignore case here when searching the list for a matching item, but set the temporary state property to the
                    // correctly-cased version from metadata so that if the user clicks Finish, the correct one will be saved. This
                    // allows some flexibility the designer without preserving an incorrectly-cased value that could cause errors at runtime.                    
                    if (String.Equals(entityTypeFilterItem.EntityTypeName, entityTypeFilter, StringComparison.OrdinalIgnoreCase))
                    {
                        typeToSelect = entityTypeFilterItem;
                    }
                }

                // didn't find a matching type, so just create a placeholder item and add it to the list
                if (typeToSelect == null)
                {
                    typeToSelect = new EntityDataSourceEntityTypeFilterItem(entityTypeFilter);
                    _entityTypeFilters.Add(typeToSelect);

                }

                Debug.Assert(typeToSelect != null, "expected a non-null string for EntityTypeFilter");
                return typeToSelect;
            }

            return s_entityTypeFilterNoneItem;
        }

        // Set EntityTypeFilter in temporary storage and load the Select property
        internal void SelectEntityTypeFilter(EntityDataSourceEntityTypeFilterItem selectedEntityTypeFilter)
        {
            _selectedEntityTypeFilter = selectedEntityTypeFilter;
            // Reinitialize the Select control with a list of properties, don't preserve any existing Select value
            LoadSelect(String.Empty);
        }

        private void SaveEntityTypeFilter()
        {
            // If (None) is selected, it is the same as an empty string on the data source control
            if (Object.ReferenceEquals(_selectedEntityTypeFilter, s_entityTypeFilterNoneItem))
            {
                _entityDataSourceState.EntityTypeFilter = String.Empty;
            }
            else
            {
                _entityDataSourceState.EntityTypeFilter = _selectedEntityTypeFilter.EntityTypeName;
            }
        }

        #endregion

        #region Select
        // Load and parse the Select property
        private void LoadSelect(string select)
        {
            Debug.Assert(_selectedEntityTypeFilter != null, "_selectedEntityTypeFilter should never be null");

            EntityType entityType = GetSelectedEntityType();

            if (entityType != null)
            {
                // this is a real type from metadata, load its properties
                _entityTypeProperties = _helper.GetEntityTypeProperties(entityType);
                // add the 'Select All (Entity Value)' placeholder at the beginning of the list
                _entityTypeProperties.Insert(0, Strings.Wizard_DataSelectionPanel_SelectAllProperties);

                // parse the current value for the Select property to see if it can be displayed in the simple CheckedListBox view                
                if (TryParseSelect(select))
                {
                    _select = null;

                    // Update the controls
                    _panel.SetEntityTypeProperties(_entityTypeProperties, _selectedEntityTypeProperties);
                    UpdateInsertUpdateDeleteState();
                    return;
                }
                // else we failed to parse the select into entity type properties on the specified type, so just use the advanced select view
            } // else can't get a list of properties unless we have a known EntityType


            // if we don't have a valid entity type or couldn't parse the incoming Select value, just display the advanced TextBox view
            _entityTypeProperties = null;
            _selectedEntityTypeProperties = null;
            _select = select;

            // Update the controls
            _panel.SetSelect(_select);
            UpdateInsertUpdateDeleteState();
        }

        // Build a value for the Select property from the selected values in the CheckedListBox
        // Value will be in the from "it.Property1, it.Property2, it.Property3"
        private string BuildSelect()
        {
            Debug.Assert(_selectedEntityTypeProperties != null && _selectedEntityTypeProperties.Count > 0, "expected non-null _selectedEntityTypeProperties with at least one value");

            // 'Select All (Entity Value)' is the same thing as an empty string for the property
            if (_selectedEntityTypeProperties[0] == 0)
            {
                Debug.Assert(_selectedEntityTypeProperties.Count == 1, "'Select All (Entity Value)' should be the only property selected");
                return String.Empty;
            }

            StringBuilder selectProperties = new StringBuilder();
            bool addComma = false;
            foreach (int propertyIndex in _selectedEntityTypeProperties)
            {
                if (addComma)
                {
                    selectProperties.Append(", ");
                }
                else
                {
                    addComma = true;
                }

                selectProperties.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}", s_itKeyword, EscapePropertyName(_entityTypeProperties[propertyIndex]));

            }
            return selectProperties.ToString();
        }

        private static string EscapePropertyName(string propertyName)
        {
            return "[" + propertyName.Replace("]", "]]") + "]";
        }

        static string UnescapePropertyName(string name)
        {
            if (name[0] == '[' && name[name.Length - 1] == ']')
            {
                return name.Substring(1, name.Length - 2).Replace("]]", "]");
            }
            else
            {
                // else the property is not escaped at all or is not properly escaped. We can't parse it so just return.
                return name;
            }            
        }

        // Parses the current Select property on the data source to see if it matches a specific format that we can use to display the properties
        // in the CheckedListBox in the simple select wizard view
        private bool TryParseSelect(string currentSelect)
        {
            bool parseSuccess = false; // gets set to true after the statement has been successfully parsed
            if (!String.IsNullOrEmpty(currentSelect))
            {
                // first try to split the string up into pieces divided by commas
                // expects a format like the following: (extra spaces around the commas should work as well)
                //     "it.KnownPropertyName1, it.KnownPropertyName2, it.KnownPropertyName3"                
                string[] tokenizedSelect = currentSelect.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                bool foundUnknownProperty = false;
                List<int> selectedProperties = new List<int>();
                foreach (string token in tokenizedSelect)
                {
                    string propertyName = token.Trim();

                    // Does the current property token start with "it."?
                    if (ReadItKeyword(propertyName))
                    {
                        // Does the rest of the property token match a known property name for the selected EntityTypeFilter?
                        int propertyIndex = ReadPropertyName(propertyName.Substring(s_itKeyword.Length));
                        if (propertyIndex == -1)
                        {
                            // the property was not known, so we can just stop looking
                            foundUnknownProperty = true;
                            break;
                        }
                        else
                        {
                            // this is a known property, so add its index to the list
                            selectedProperties.Add(propertyIndex);
                        }
                    }
                    else
                    {
                        // the property was not known, so we can just stop looking
                        foundUnknownProperty = true;
                        break;
                    }
                }
                if (!foundUnknownProperty)
                {
                    // if we never found anything unknown, the current list of properties is what we'll use to fill in the CheckedListBox
                    _selectedEntityTypeProperties = selectedProperties;
                    parseSuccess = true;
                }
                else
                {
                    _selectedEntityTypeProperties = null;
                }
            }
            else
            {
                // if Select is empty, we just want to add 'Select All (Entity Value)' to the list
                _selectedEntityTypeProperties = new List<int>();
                _selectedEntityTypeProperties.Add(0);
                parseSuccess = true;
            }

            return parseSuccess;
        }

        // Determines if the specified propertyName starts with "it." (case-insensitive)
        private bool ReadItKeyword(string propertyName)
        {
            // will accept any casing of "it." here, although when the value is saved back to the property, it will be correctly lower-cased
            return propertyName.StartsWith(s_itKeyword, StringComparison.OrdinalIgnoreCase);
        }

        // Determines if the specified propertyName matches one of the known properties for the selected type
        private int ReadPropertyName(string propertyName)
        {
            for (int propIndex = 0; propIndex < _entityTypeProperties.Count; propIndex++)
            {
                // Ignore case here when searching the list for a matching item, but set the temporary state property to the
                // correctly-cased version from metadata so that if the user clicks Finish, the correct one will be saved. This
                // allows some flexibility the designer without preserving an incorrectly-cased value that could cause errors at runtime.

                // Does the specified property name exactly match any of the properties for the selected EntityTypeFilter?
                if (String.Equals(UnescapePropertyName(propertyName), _entityTypeProperties[propIndex], StringComparison.OrdinalIgnoreCase))
                {
                    return propIndex;
                }
            }

            return -1;
        }

        // Add the specified property to the list of selected entity properties used to build up the Select property
        internal void SelectEntityProperty(int propertyIndex)
        {
            _selectedEntityTypeProperties.Add(propertyIndex);
        }

        internal void ClearAllSelectedProperties()
        {
            _selectedEntityTypeProperties.Clear();
        }

        // Remove specified entity property index from the selected list
        internal void DeselectEntityProperty(int propertyIndex)
        {
            _selectedEntityTypeProperties.Remove(propertyIndex);
        }

        // Set Select property to the specified string (used with advanced select view)
        internal void SelectAdvancedSelect(string select)
        {
            _select = select;
        }

        private void SaveSelect()
        {
            if (_select != null)
            {
                _entityDataSourceState.Select = _select;
            }
            else
            {
                Debug.Assert(_selectedEntityTypeProperties != null, "expected _entityTypeProperties to be non-null if _select is null");
                _entityDataSourceState.Select = BuildSelect();
            }
        }
        #endregion

        #region EnableInsertUpdateDelete
        // Load the initial values for EnableInsert/EnableUpdate/EnableDelete CheckBoxes
        private void LoadInsertUpdateDelete()
        {
            SelectEnableInsert(_entityDataSourceState.EnableInsert);
            SelectEnableUpdate(_entityDataSourceState.EnableUpdate);
            SelectEnableDelete(_entityDataSourceState.EnableDelete);

            UpdateInsertUpdateDeleteState();
        }

        // Set EnableDelete in temporary storage
        internal void SelectEnableDelete(bool enableDelete)
        {
            _enableDelete = enableDelete;
        }

        // Set EnableInsert in temporary storage
        internal void SelectEnableInsert(bool enableInsert)
        {
            _enableInsert = enableInsert;
        }

        // Set EnableUpdate in temporary storage
        internal void SelectEnableUpdate(bool enableUpdate)
        {
            _enableUpdate = enableUpdate;
        }

        private void SaveInsertUpdateDelete()
        {
            _entityDataSourceState.EnableInsert = _enableInsert;
            _entityDataSourceState.EnableUpdate = _enableUpdate;
            _entityDataSourceState.EnableDelete = _enableDelete;
        }

        /// <summary>
        /// Update the panel control state based on the valued of enableInsert,
        /// enableUpdate, enableDelete, and the selectedEntityTypeProperties
        /// </summary>
        internal void UpdateInsertUpdateDeleteState()
        {
            // Set the checkbox state for the panel controls
            _panel.SetEnableInsertUpdateDelete(_enableInsert, _enableUpdate, _enableDelete);

            // The InsertUpdateDelete panel should be enabled if:
            // 1. Insert, Update, or Delete is selected -OR-
            // 2. The EntitySelection has SelectAll checked
            bool enablePanel = (_enableInsert || _enableUpdate || _enableDelete || 
                        (_selectedEntityTypeProperties != null && 
                         _selectedEntityTypeProperties.Count == 1 && 
                         _selectedEntityTypeProperties[0] == 0));

            _panel.SetEnableInsertUpdateDeletePanel(enablePanel);
        }
        #endregion        

        #region  EnableFlattening

        private EntityType GetSelectedEntityType()
        {
            EntityType entityType = null;

            // determine which EntityType to load properties for, based on the value selected for EntityTypeFilter
            if (Object.ReferenceEquals(_selectedEntityTypeFilter, s_entityTypeFilterNoneItem))
            {
                // If (None) is selected, use the base type for the EntitySet if available
                if (_selectedEntitySetName != null && _selectedEntitySetName.EntitySet != null)
                {
                    entityType = _selectedEntitySetName.EntitySet.ElementType;
                }
                // else the EntitySet base type is not known
            }
            else
            {
                entityType = _selectedEntityTypeFilter.EntityType; // could still be null if the type if not known in metadata
            }

            return entityType;
        }

        private void SaveEnableFlattening()
        {
            bool enableFlattening = false;

            EntityType entityType = GetSelectedEntityType();

            if (entityType != null)
            {
                foreach (EdmMember member in entityType.Members)
                {
                    // If there is a complex member, enable flattening
                    if (member.TypeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.ComplexType)
                    {
                        enableFlattening = true;
                        break;
                    }
                    else if (member.BuiltInTypeKind == BuiltInTypeKind.NavigationProperty)
                    {
                        NavigationProperty navProp = (NavigationProperty)member;
                        if (navProp.ToEndMember.RelationshipMultiplicity != RelationshipMultiplicity.Many)
                        {
                            AssociationType associationType = navProp.ToEndMember.DeclaringType as AssociationType;
                            if (!associationType.IsForeignKey)
                            {
                                // If there is an independent association, enable flattening
                                enableFlattening = true;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                // Projection
                enableFlattening = true;
            }

            _entityDataSourceState.EnableFlattening = enableFlattening;
        }

        #endregion

        #endregion

        #region Wizard button state management
        internal void UpdateWizardState()
        {
            // EntitySetName must be selected and a Select must be configured or must be the empty string
            _wizardForm.SetCanFinish(_selectedEntitySetName != null && (_select != null || _selectedEntityTypeProperties.Count > 0));
        }
        #endregion
    }
}
