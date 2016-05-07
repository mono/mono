//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Selection 
{
    using System.ComponentModel;
    using System.Diagnostics;

    // <summary>
    // A class we use to describe what visual element is currently selected within
    // the CategoryList class (property, category, category editor, ...)
    // </summary>
    internal class SelectionPath 
    {
        private string _pathTypeId;
        private string _path;

        // <summary>
        // Creates a new instance of SelectionPath with the specified
        // path ID and path.
        // </summary>
        // <param name="pathTypeId">Token that identifies the ISelectionPathInterpreter
        // that knows how to resolve the specified path</param>
        // <param name="path">Path to the selected object</param>
        public SelectionPath(string pathTypeId, string path) 
        {
            _pathTypeId = pathTypeId;
            _path = path;
        }

        // <summary>
        // Gets the token that identifies the ISelectionPathInterpreter that knows how
        // to resolve the contained path.
        // </summary>
        public string PathTypeId 
        { get { return _pathTypeId; } }

        // <summary>
        // Gets the path itself.
        // </summary>
        public string Path 
        { get { return _path; } }

        // <summary>
        // Packages this instance into a serializable object
        // </summary>
        public object State 
        {
            get {
                return new string[] { _pathTypeId, _path };
            }
        }

        // <summary>
        // Converts the serializable object returned by the State property
        // back into an instance of SelectionPath
        // </summary>
        // <param name="state">State to convert</param>
        // <returns>Instance of SelectionPath represented by the given state object</returns>
        public static SelectionPath FromState(object state) 
        {
            string[] values = state as string[];
            if (values == null || values.Length != 2) 
            {
                Debug.Fail("Invalid SelectionPath State object");
                return null;
            }

            return new SelectionPath(values[0], values[1]);
        }
    }
}
