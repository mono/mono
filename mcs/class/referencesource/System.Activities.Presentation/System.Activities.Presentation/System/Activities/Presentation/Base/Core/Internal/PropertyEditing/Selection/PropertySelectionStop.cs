//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Selection 
{
    using System;
    using System.Globalization;
    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Presentation;

    // <summary>
    // Helper class used to manage the selection stop behavior of a given PropertyContainer.
    // All it really does is expose the SelectionPath leading to its contained PropertyContainer.
    // </summary>
    internal class PropertySelectionStop : ISelectionStop 
    {

        private PropertyEntry _property;
        private SelectionPath _selectionPath;

        // <summary>
        // Creates a new PropertySelectionStop wrapping around the specified PropertyEntry
        // </summary>
        // <param name="property">PropertyEntry to wrap around</param>
        public PropertySelectionStop(PropertyEntry property) 
        {
            if (property == null) 
            {
                throw FxTrace.Exception.ArgumentNull("property");
            }
            _property = property;
        }

        // <summary>
        // Gets true, throws on set
        // </summary>
        public bool IsExpanded 
        {
            get { return true; }
            set { throw FxTrace.Exception.AsError(new InvalidOperationException()); }
        }

        // <summary>
        // Gets false
        // </summary>
        public bool IsExpandable 
        {
            get { return false; }
        }

        // <summary>
        // Gets a SelectionPath that leads to the contained PropertyEntry
        // </summary>
        public SelectionPath Path 
        {
            get {
                if (_selectionPath == null)
                {
                    _selectionPath = PropertySelectionPathInterpreter.Instance.ConstructSelectionPath(_property);
                }

                return _selectionPath;
            }
        }

        // <summary>
        // Gets a description of the contained property
        // to expose through automation
        // </summary>
        public string Description 
        {
            get {
                return string.Format(
                    CultureInfo.CurrentCulture,
                    Properties.Resources.PropertyEditing_SelectionStatus_Property,
                    _property.PropertyName);

            }
        }
    }
}
