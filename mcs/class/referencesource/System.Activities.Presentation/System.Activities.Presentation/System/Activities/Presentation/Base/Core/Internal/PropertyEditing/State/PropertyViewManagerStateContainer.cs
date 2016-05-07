//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.State 
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Activities.Presentation.Internal.PropertyEditing.Views;
    using System.Activities.Presentation;

    // <summary>
    // StateContainer for current selection of IPropertyViewManager
    // </summary>
    internal class PropertyViewManagerStateContainer : IStateContainer 
    {

        public static readonly PropertyViewManagerStateContainer Instance = new PropertyViewManagerStateContainer();

        public const string RootPropertyInspectorPersistenceId = "RootPropertyInspector";

        private Dictionary<string, string> _persistenceIdToManagerTypeNameMap;
        private PropertyViewManagerStateContainer() 
        {
        }

        // <summary>
        // Event fired when the internal state is scrapped and restored from
        // some saved value
        // </summary>
        public event EventHandler ContentRestored;

        // <summary>
        // Gets the instance of IPropertyViewManager stored under the specified
        // persistence ID.
        // </summary>
        // <param name="persistenceId">ID to look up</param>
        // <returns>Instance of IPropertyViewManager stored under the specified
        // persistence ID.  If not found, an instance of the default IPropertyViewManager
        // is returned.</returns>
        public IPropertyViewManager GetPropertyViewManager(string persistenceId) 
        {
            string propertyViewManagerTypeName;
            if (_persistenceIdToManagerTypeNameMap == null ||
                !_persistenceIdToManagerTypeNameMap.TryGetValue(persistenceId, out propertyViewManagerTypeName)) 
            {

                // If we don't have any state stored, default to the value of the 
                // root PropertyInspector, unless that state is not stored either
                // in which case default to category view.
                //
                if (persistenceId != RootPropertyInspectorPersistenceId)
                {
                    return GetPropertyViewManager(RootPropertyInspectorPersistenceId);
                }
                else
                {
                    return ByCategoryViewManager.Instance;
                }
            }

            if (string.Equals(propertyViewManagerTypeName, typeof(ByCategoryViewManager).Name))
            {
                return ByCategoryViewManager.Instance;
            }
            else if (string.Equals(propertyViewManagerTypeName, typeof(AlphabeticalViewManager).Name))
            {
                return AlphabeticalViewManager.Instance;
            }

            Debug.Fail("Unknown IPropertyViewManager type: " + (propertyViewManagerTypeName ?? "null"));
            return ByCategoryViewManager.Instance;
        }

        // <summary>
        // Stores the specified IPropertyViewManager under the specified persistenceId.
        // </summary>
        // <param name="persistenceId">ID to store under</param>
        // <param name="manager">IPropertyViewManager to store</param>
        public void StorePropertyViewManager(string persistenceId, IPropertyViewManager manager) 
        {
            if (persistenceId == null) 
            {
                throw FxTrace.Exception.ArgumentNull("persistenceId");
            }

            if (manager == null && _persistenceIdToManagerTypeNameMap == null)
            {
                return;
            }

            if (manager == null) 
            {
                _persistenceIdToManagerTypeNameMap.Remove(persistenceId);
            }
            else 
            {
                if (_persistenceIdToManagerTypeNameMap == null)
                {
                    _persistenceIdToManagerTypeNameMap = new Dictionary<string, string>();
                }

                _persistenceIdToManagerTypeNameMap[persistenceId] = manager.GetType().Name;
            }
        }

        // IStateContainer Members

        // <summary>
        // Retrieves all stored IPropertyViewManager types under all persistence IDs
        // </summary>
        // <returns>All stored IPropertyViewManager types under all persistence IDs</returns>
        public object RetrieveState() 
        {
            if (_persistenceIdToManagerTypeNameMap == null || _persistenceIdToManagerTypeNameMap.Count == 0) 
            {
                return null;
            }

            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> pair in _persistenceIdToManagerTypeNameMap) 
            {
                if (sb.Length > 0)
                {
                    sb.Append(';');
                }

                sb.Append(PersistedStateUtilities.Escape(pair.Key));
                sb.Append(';');
                sb.Append(pair.Value);
            }

            return sb.ToString();
        }

        // <summary>
        // Attempts to restore the persisted state
        // </summary>
        // <param name="state"></param>
        public void RestoreState(object state) 
        {
            if (state == null) 
            {
                return;
            }

            string serializedState = state as string;
            if (serializedState == null) 
            {
                Debug.Fail("Invalid PropertyViewManager state: " + state.ToString());
                return;
            }

            string[] items = serializedState.Split(';');
            if ((items.Length % 2) != 0) 
            {
                Debug.Fail("Invalid PropertyViewManager state: " + state.ToString());
                return;
            }

            if (_persistenceIdToManagerTypeNameMap == null)
            {
                _persistenceIdToManagerTypeNameMap = new Dictionary<string, string>();
            }

            for (int i = 0; i < items.Length;)
            {
                _persistenceIdToManagerTypeNameMap[items[i++]] = items[i++];
            }

            if (ContentRestored != null)
            {
                ContentRestored(this, EventArgs.Empty);
            }
        }

    }
}
