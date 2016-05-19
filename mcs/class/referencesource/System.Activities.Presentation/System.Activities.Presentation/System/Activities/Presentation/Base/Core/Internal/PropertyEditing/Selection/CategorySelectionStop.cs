//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Selection 
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Activities.Presentation;

    using Blend = System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.PropertyInspector;

    // <summary>
    // Helper class used to manage the selection stop behavior of a given CategoryContainer's basic
    // and advanced sections.  It deals with both expanding and collapsing of the specified section
    // as well as knowing how to get a SelectionPath leading to its heading.
    // </summary>
    internal class CategorySelectionStop : ISelectionStop 
    {

        private CiderCategoryContainer _parent;
        private DependencyProperty _expansionProperty;
        private SelectionPath _selectionPath;
        private string _description;

        // <summary>
        // Creates a new selection stop logic for the specified CiderCategoryContainer.
        // </summary>
        // <param name="parent">CategoryContainer to wrap around</param>
        // <param name="isAdvanced">True if this selection stop wraps around the
        // advanced set of properties, false otherwise.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        public CategorySelectionStop(CiderCategoryContainer parent, bool isAdvanced) 
        {

            if (parent == null) 
            {
                throw FxTrace.Exception.ArgumentNull("parent");
            }
            if (parent.Category == null) 
            {
                throw FxTrace.Exception.ArgumentNull("parent.Category");
            }

            _parent = parent;
            _expansionProperty = isAdvanced ? Blend.CategoryContainer.AdvancedSectionPinnedProperty : Blend.CategoryContainer.ExpandedProperty;
            _selectionPath = CategoryContainerSelectionPathInterpreter.Instance.ConstructSelectionPath(parent.Category.CategoryName, isAdvanced);
            _description = isAdvanced ?
                string.Format(
                CultureInfo.CurrentCulture,
                System.Activities.Presentation.Internal.Properties.Resources.PropertyEditing_SelectionStatus_AdvancedCategory,
                parent.Category.CategoryName) :
                string.Format(
                CultureInfo.CurrentCulture,
                System.Activities.Presentation.Internal.Properties.Resources.PropertyEditing_SelectionStatus_Category,
                parent.Category.CategoryName);
        }

        // <summary>
        // Gets or sets a flag indicating whether the basic/advanced section is expanded
        // </summary>
        public bool IsExpanded 
        {
            get { return (bool)_parent.GetValue(_expansionProperty); }
            set { _parent.SetValue(_expansionProperty, value); }
        }

        // <summary>
        // Returns true
        // </summary>
        public bool IsExpandable 
        {
            get { return true; }
        }

        // <summary>
        // Gets the SelectionPath to the contained CategoryContainer section
        // </summary>
        public SelectionPath Path 
        {
            get { return _selectionPath; }
        }

        // <summary>
        // Gets a description of the contained category container
        // to expose through automation
        // </summary>
        public string Description 
        {
            get { return _description; }
        }
    }
}
