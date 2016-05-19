//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Selection 
{
    using System.Diagnostics;
    using System.Text;
    using System.Windows;

    using System.Activities.Presentation.PropertyEditing;

    using System.Activities.Presentation.Internal.PropertyEditing.Editors;
    using System.Activities.Presentation.Internal.PropertyEditing.Model;
    using System.Activities.Presentation.Internal.PropertyEditing.State;

    // <summary>
    // Helper class that knows how to construct and interpret SelectionPaths leading
    // to properties.
    // </summary>
    internal class PropertySelectionPathInterpreter : ISelectionPathInterpreter 
    {
        internal const string PropertyPathTypeId = "Cider_PropertyPath";
        private static PropertySelectionPathInterpreter _instance = new PropertySelectionPathInterpreter();
        private PropertySelectionPathInterpreter() 
        {
        }
        public static PropertySelectionPathInterpreter Instance 
        { get { return _instance; } }

        public string PathTypeId
        { get { return PropertyPathTypeId; } }

        // <summary>
        // Creates an instance of SelectionPath to the specified property that
        // this class knows how to interpret.
        // </summary>
        // <param name="property">Property to create a path to</param>
        // <returns>A new instance of SelectionPath to the specified property</returns>
        public SelectionPath ConstructSelectionPath(PropertyEntry property) 
        {
            StringBuilder path = new StringBuilder();
            path.Append(ModelUtilities.GetCachedSubPropertyHierarchyPath(property));
            return new SelectionPath(PathTypeId, path.ToString());
        }

        // ISelectionPathInterpreter Members

        public DependencyObject ResolveSelectionPath(CategoryList root, SelectionPath path, out bool pendingGeneration) 
        {
            pendingGeneration = false;
            if (path == null || !string.Equals(PathTypeId, path.PathTypeId)) 
            {
                Debug.Fail("Invalid SelectionPath specified.");
                return null;
            }

            if (root == null) 
            {
                Debug.Fail("No CategoryList specified.");
                return null;
            }

            string[] pathValues = path.Path.Split(',');
            if (pathValues.Length < 1) 
            {
                Debug.Fail("Invalid SelectionPath specified.");
                return null;
            }

            //
            // Note: By the time this method gets called, all the visuals should have been expanded
            // and rendered.  Hence, if we can't find a visual in the visual tree, it doesn't exist
            // and we shouldn't worry about trying to expand some parent visual and waiting for it
            // to render.
            //

            ModelCategoryEntry parentCategory;

            PropertyEntry currentProperty = root.FindPropertyEntry(PersistedStateUtilities.Unescape(pathValues[0]), out parentCategory);
            PropertyContainer currentContainer = root.FindPropertyEntryVisual(currentProperty, parentCategory, out pendingGeneration);
            DependencyObject lastFoundContainer = currentContainer;
            int pathIndex = 1;

            while (currentContainer != null && pathIndex < pathValues.Length) 
            {

                SubPropertyEditor subPropertyEditor = VisualTreeUtils.GetTemplateChild<SubPropertyEditor>(currentContainer);
                if (subPropertyEditor == null)
                {
                    break;
                }

                // If the subpropertyEditor is not expanded and is expandable, we won't be able to get the target property's visual
                // element, Expand it, set pendingGeneration to True, and return null. Expect the caller to call again.
                if (subPropertyEditor.IsExpandable && !subPropertyEditor.IsExpanded)
                {
                    subPropertyEditor.IsExpanded = true;
                    pendingGeneration = true;
                    return null;
                }

                PropertyEntry property = subPropertyEditor.FindSubPropertyEntry(PersistedStateUtilities.Unescape(pathValues[pathIndex]));
                if (property == null)
                {
                    break;
                }

                currentContainer = subPropertyEditor.FindSubPropertyEntryVisual(property);
                lastFoundContainer = currentContainer ?? lastFoundContainer;
                pathIndex++;
            }

            if (lastFoundContainer == null)
            {
                return null;
            }

            return lastFoundContainer;
        }

    }
}
