//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Selection 
{
    using System.Diagnostics;
    using System.Text;
    using System.Windows;

    using System.Activities.Presentation.PropertyEditing;

    using System.Activities.Presentation.Internal.PropertyEditing.State;

    // <summary>
    // Helper class that knows how to construct and interpret SelectionPaths leading
    // to CategoryContainers.
    // </summary>
    internal class CategoryContainerSelectionPathInterpreter : ISelectionPathInterpreter 
    {

        private static CategoryContainerSelectionPathInterpreter _instance = new CategoryContainerSelectionPathInterpreter();
        private CategoryContainerSelectionPathInterpreter() 
        {
        }
        public static CategoryContainerSelectionPathInterpreter Instance 
        { get { return _instance; } }

        public string PathTypeId 
        { get { return "Cider_CategoryPath"; } }

        // <summary>
        // Creates an instance of SelectionPath to the specified category container's
        // basic or advanced sections that this class knows how to interpret.
        // </summary>
        // <param name="categoryName">Name of the category</param>
        // <param name="isAdvanced">True if the path should lead to the advanced section,
        // false otherwise.</param>
        // <returns>A new instance of SelectionPath to the specified section of the specified
        // category container.</returns>
        public SelectionPath ConstructSelectionPath(string categoryName, bool isAdvanced) 
        {
            StringBuilder path = new StringBuilder(PersistedStateUtilities.Escape(categoryName));
            if (isAdvanced)
            {
                path.Append(",Advanced");
            }

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
            string categoryName = PersistedStateUtilities.Unescape(pathValues[0]);
            bool isAdvanced = pathValues.Length == 2;

            CategoryEntry category = root.FindCategory(categoryName);
            if (category == null)
            {
                return null;
            }

            DependencyObject categoryVisual = root.FindCategoryEntryVisual(category);
            if (categoryVisual == null)
            {
                return null;
            }

            DependencyObject searchStart;

            // For basic section, start at the root.
            // For advanced section, start at the advanced expander.
            // The next SelectionStop in both cases will be the section header SelectionStop.
            if (!isAdvanced)
            {
                searchStart = categoryVisual;
            }
            else
            {
                searchStart = VisualTreeUtils.GetNamedChild<FrameworkElement>(categoryVisual, "PART_AdvancedExpander");
            }

            return PropertySelection.FindNeighborSelectionStop<DependencyObject>(searchStart, SearchDirection.Next);
        }

    }
}
