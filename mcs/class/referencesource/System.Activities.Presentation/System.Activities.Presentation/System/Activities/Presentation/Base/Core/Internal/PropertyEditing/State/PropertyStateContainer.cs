//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing 
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;

    using System.Activities.Presentation.Internal.PropertyEditing.State;

    // <summary>
    // Simple wrapper around a dictionary of PropertyStates keyed by the property names.
    // </summary>
    internal class PropertyStateContainer : PersistedStateContainer 
    {

        private static PropertyStateContainer _instance;

        // The ctor is private because we use this class as a singleton
        private PropertyStateContainer() 
        {
        }

        // <summary>
        // Gets a static instance of this class
        // </summary>
        public static PropertyStateContainer Instance 
        {
            get {
                if (_instance == null)
                {
                    _instance = new PropertyStateContainer();
                }

                return _instance;
            }
        }

        // <summary>
        // Gets the PropertyState for the specified category.  If one does not exist
        // yet, it will be created automatically, guaranteeing a non-null return value.
        // </summary>
        // <param name="propertyName">Name of the property itself</param>
        // <returns>A non-null instance of PropertyState</returns>
        public PropertyState GetPropertyState(string propertyName) 
        {
            return (PropertyState)this.GetState(propertyName);
        }

        // <summary>
        // Creates a default state object based on the specified key
        // </summary>
        // <param name="key">Key of the state object</param>
        // <returns>Default state object</returns>
        protected override PersistedState CreateDefaultState(object key) 
        {
            return new PropertyState(key as string);
        }

        // <summary>
        // Deserializes the specified string value into a state object
        // </summary>
        // <param name="serializedValue">Serialized value of the state object</param>
        // <returns>Deserialized instance of the state object</returns>
        protected override PersistedState DeserializeState(string serializedValue) 
        {
            return PropertyState.Deserialize(serializedValue);
        }
    }
}
