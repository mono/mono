//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.State 
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;

    // <summary>
    // Simple state object that knows how to remember state for a single property.
    // The only thing it currently remembers is whether the property's sub-properties
    // are expanded or collapsed.
    // </summary>
    internal class PropertyState : PersistedState 
    {

        private const bool DefaultSubPropertiesExpanded = false;

        private string _propertyName;
        private bool _subPropertiesExpanded = DefaultSubPropertiesExpanded;

        // <summary>
        // Basic ctor
        // </summary>
        // <param name="propertyName">Property to wrap around</param>
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public PropertyState(string propertyName) 
        {
            Fx.Assert(!string.IsNullOrEmpty(propertyName), "Expected a full property name");
            _propertyName = propertyName;
        }

        // <summary>
        // We key these state objects by their property names
        // </summary>
        public override object Key 
        {
            get { return _propertyName; }
        }

        // <summary>
        // Returns true if any of the contained values differ from the default
        // </summary>
        public override bool IsSignificant 
        {
            get { return _subPropertiesExpanded != DefaultSubPropertiesExpanded; }
        }

        // <summary>
        // Gets or sets a flag indicating whether the sub-properties of the contained
        // property have been expanded or collapsed.
        // </summary>
        public bool SubPropertiesExpanded 
        {
            get { return _subPropertiesExpanded; }
            set { _subPropertiesExpanded = value; }
        }

        // <summary>
        // Serializes this object into a simple string (AppDomains like strings).
        //
        // Format: PropertyName,SubPropertiesExpanded;NextPropertyName,SubPropertiesExpanded;...
        // Where bools are recorded as 0 = false and 1 = true
        // </summary>
        // <returns>Serialized version of this state object (may be null)</returns>
        protected override string SerializeCore() 
        {
            return string.Concat(
                PersistedStateUtilities.Escape(_propertyName),
                ',',
                PersistedStateUtilities.BoolToDigit(_subPropertiesExpanded));
        }

        // <summary>
        // Attempts to deserialize a string into a PropertyState object
        // </summary>
        // <param name="propertyStateString">String to deserialize</param>
        // <returns>Instance of PropertyState if the serialized string was valid, null otherwise.</returns>
        public static PropertyState Deserialize(string propertyStateString) 
        {
            string[] args = propertyStateString.Split(',');
            if (args == null || args.Length != 2)
            {
                return null;
            }

            bool? subPropertiesExpanded = PersistedStateUtilities.DigitToBool(args[1]);
            if (subPropertiesExpanded == null)
            {
                return null;
            }

            string propertyName = PersistedStateUtilities.Unescape(args[0]);
            if (string.IsNullOrEmpty(propertyName))
            {
                return null;
            }

            PropertyState propertyState = new PropertyState(propertyName);
            propertyState.SubPropertiesExpanded = (bool)subPropertiesExpanded;
            return propertyState;
        }
    }
}
