//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.State 
{
    using System.Collections;
    using System.Text;
    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Presentation.Internal.PropertyEditing.Model;

    // <summary>
    // Container storing the names of properties the value editors of which
    // are in ExtendedPinned ActiveEditMode.  We persist this information
    // across domain reloads and view changes.
    // </summary>
    internal class PropertyActiveEditModeStateContainer : IStateContainer 
    {

        public static readonly PropertyActiveEditModeStateContainer Instance = new PropertyActiveEditModeStateContainer();

        private Hashtable _expandedPropertyEditors = new Hashtable();
        private PropertyActiveEditModeStateContainer() 
        {
        }

        // <summary>
        // Gets the last stored ActiveEditMode for the specified property
        // </summary>
        // <param name="property">PropertyEntry to look up</param>
        // <returns>Last stored ActiveEditMode for the specified property</returns>
        public PropertyContainerEditMode GetActiveEditMode(PropertyEntry property) 
        {
            return _expandedPropertyEditors.ContainsKey(ModelUtilities.GetCachedSubPropertyHierarchyPath(property)) ?
                PropertyContainerEditMode.ExtendedPinned :
                PropertyContainerEditMode.Inline;
        }

        // <summary>
        // Stores the current ActiveEditMode for the specified property
        // </summary>
        // <param name="property">Property to key off of</param>
        // <param name="editMode">ActiveEditMode value to store</param>
        public void StoreActiveEditMode(PropertyEntry property, PropertyContainerEditMode editMode) 
        {
            string path = ModelUtilities.GetCachedSubPropertyHierarchyPath(property);

            // We only care about storing the ExtendedPinned state.  Everything
            // else is transitory and shouldn't be persisted.
            //
            if (editMode == PropertyContainerEditMode.ExtendedPinned)
            {
                _expandedPropertyEditors[path] = null;
            }
            else
            {
                _expandedPropertyEditors.Remove(path);
            }
        }

        // IStateContainer

        public object RetrieveState() 
        {
            if (_expandedPropertyEditors.Count == 0)
            {
                return null;
            }

            bool firstTime = true;
            StringBuilder sb = new StringBuilder();
            foreach (string propertyPath in _expandedPropertyEditors) 
            {
                sb.Append(propertyPath);
                if (!firstTime) 
                {
                    sb.Append(';');
                }
                firstTime = false;
            }

            return sb.ToString();
        }

        public void RestoreState(object state) 
        {
            string stateString = state as string;
            if (stateString == null) 
            {
                return;
            }

            string[] paths = stateString.Split(';');
            foreach (string path in paths) 
            {
                if (string.IsNullOrEmpty(path)) 
                {
                    continue;
                }
                _expandedPropertyEditors[path] = null;
            }
        }

    }
}
