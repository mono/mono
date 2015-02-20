//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.State 
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Runtime;

    // <summary>
    // Helper class we use to contain a bunch of PersistedState objects.  Basically, the common functionality
    // of CategoryStateContainer and PropertyStateContainer lives here.
    // </summary>
    internal abstract class PersistedStateContainer : IStateContainer 
    {

        private readonly Dictionary<object, PersistedState> _persistedStates = new Dictionary<object, PersistedState>();

        // <summary>
        // Gets the state for the specified key.  If not found, a default state object is
        // created and returned.
        // </summary>
        // <param name="key">Key to look up</param>
        // <returns>Guaranteed non-null result</returns>
        protected PersistedState GetState(object key) 
        {
            PersistedState state = null;
            if (!_persistedStates.TryGetValue(key, out state)) 
            {
                state = CreateDefaultState(key);
                Fx.Assert(state != null, "CreateDefaultState() should always return a value");

                _persistedStates[key] = state;
            }

            return state;
        }

        // <summary>
        // Creates a default state object based on the specified key
        // </summary>
        // <param name="key">Key of the state object</param>
        // <returns>Default state object</returns>
        protected abstract PersistedState CreateDefaultState(object key);

        // <summary>
        // Deserializes the specified string value into a state object
        // </summary>
        // <param name="serializedValue">Serialized value of the state object</param>
        // <returns>Deserialized instance of the state object</returns>
        protected abstract PersistedState DeserializeState(string serializedValue);

        // IStateContainer Members

        // <summary>
        // Merges the content of the state object with the content of this class
        // </summary>
        // <param name="state">State object to apply</param>
        public void RestoreState(object state) 
        {
            string serializedStates = state as string;
            if (string.IsNullOrEmpty(serializedStates))
            {
                return;
            }

            string[] stateArray = serializedStates.Split(';');
            if (stateArray == null || stateArray.Length == 0)
            {
                return;
            }

            foreach (string stateString in stateArray) 
            {
                PersistedState deserializedState = DeserializeState(stateString);
                if (deserializedState == null) 
                {
                    Debug.Fail("Invalid state.  Deserialization failed: " + stateString);
                    continue;
                }

                _persistedStates[deserializedState.Key] = deserializedState;
            }
        }

        // <summary>
        // Serializes the content of this class into a state object.
        // </summary>
        // <returns>Serialized version of this class</returns>
        public object RetrieveState() 
        {
            if (_persistedStates.Count == 0)
            {
                return null;
            }

            StringBuilder serializedState = new StringBuilder();
            bool addSeparator = false;
            foreach (PersistedState state in _persistedStates.Values) 
            {

                string stateString = state.Serialize();
                if (stateString == null)
                {
                    continue;
                }

                if (addSeparator)
                {
                    serializedState.Append(';');
                }

                serializedState.Append(stateString);
                addSeparator = true;
            }

            return serializedState.ToString();
        }

    }
}
