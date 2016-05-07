//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.State 
{
    using System;

    // <summary>
    // Simple base class for persisted state objects so that we can reuse a chunk of the logic
    // from one persistent state storage to another.
    // </summary>
    internal abstract class PersistedState 
    {

        // <summary>
        // Gets a value indicating whether the content of the state object is significant
        // enough that it warrants serialization
        // </summary>
        public abstract bool IsSignificant 
        { get; }

        // <summary>
        // Gets an object that we use as a key to key off this state instance
        // </summary>
        public abstract object Key 
        { get; }

        // <summary>
        // Serializes this state into a string that can be persisted across app domains.
        // If the content of this state is not significant enough to persist, null is returned.
        // </summary>
        // <returns>Serialization of this state object, or null if not significant enough.</returns>
        public string Serialize() 
        {
            return IsSignificant ? SerializeCore() : null;
        }

        // <summary>
        // Serializes this object into a string.
        // </summary>
        // <returns>String representation of this object.</returns>
        protected abstract string SerializeCore();
    }
}
