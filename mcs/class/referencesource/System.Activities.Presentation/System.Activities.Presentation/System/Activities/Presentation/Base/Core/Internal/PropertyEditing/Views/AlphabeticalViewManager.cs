//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Views 
{
    using System;
    using System.Collections.Generic;

    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.PropertyEditing;

    using System.Activities.Presentation.Internal.PropertyEditing.Model;
    using System.Activities.Presentation.Internal.PropertyEditing.Selection;

    // <summary>
    // IPropertyViewManager we use to display an alphabetical list of properties
    // </summary>
    internal class AlphabeticalViewManager : IPropertyViewManager 
    {

        public static readonly AlphabeticalViewManager Instance = new AlphabeticalViewManager();
        private AlphabeticalViewManager() 
        {
        }

        // <summary>
        // AlphabeticalViewManager doesn't show category headers
        // </summary>
        public bool ShowCategoryHeaders 
        { get { return false; } }

        // <summary>
        // AlphabeticalViewManager always uses PropertyEntryNameComparer instance
        // </summary>
        private IComparer<PropertyEntry> PropertyComparer 
        {
            get {
                return PropertyEntryNameComparer.Instance;
            }
        }

        // IPropertyViewManager Members

        // <summary>
        // Add a property into the correct category within the specified CategoryList.
        // </summary>
        // <param name="propertySet">Specified property (passed in as a set for multi-select scenarios)</param>
        // <param name="propertyName">Name of the current property (perf optimization)</param>
        // <param name="categoryList">CategoryList instance to populate</param>
        // <returns>Wrapped ModelPropertyEntry for the specified propertySet</returns>
        public ModelPropertyEntry AddProperty(IEnumerable<ModelProperty> propertySet, string propertyName, CategoryList categoryList) 
        {
            string categoryName = System.Activities.Presentation.Internal.Properties.Resources.PropertyCategoryAllProperties;
            ModelCategoryEntry category = categoryList.FindCategory(categoryName) as ModelCategoryEntry;

            bool reuseEntries = ExtensibilityAccessor.IsEditorReusable(propertySet);
            if (reuseEntries && category != null) 
            {
                ModelPropertyEntry foundProperty;
                if ((foundProperty = (ModelPropertyEntry)category[propertyName]) != null) {
                    if (foundProperty.PropertyType != null && foundProperty.PropertyType.Equals(System.Activities.Presentation.Internal.PropertyEditing.Model.ModelUtilities.GetPropertyType(propertySet)))
                    {
                        // Found a match for the property, so reuse it

                        bool oldIsBrowsable = foundProperty.IsBrowsable;

                        foundProperty.SetUnderlyingModelProperty(propertySet);

                        // If the IsBrowsable or IsAdvanced value of the property changed,
                        // refresh the property within the category, because how and whether
                        // this property should be rendered may have changed.
                        // Note that refreshing a selected property also nullifies its stickiness
                        // (ie. it resets CategoryList.PropertySelectionMode)
                        if (oldIsBrowsable != foundProperty.IsBrowsable) 
                        {
                            category.Refresh(foundProperty, category.BasicProperties, this.PropertyComparer);
                        }

                        return foundProperty;
                    }
                }
            }

            if (category == null) 
            {
                category = new ModelCategoryEntry(categoryName);
                categoryList.InsertAlphabetically(category);
            }

            ModelPropertyEntry property = new ModelPropertyEntry(propertySet, null);
            category.Add(property, category.BasicProperties, this.PropertyComparer);
            return property;
        }

        // <summary>
        // Scans the list of categories in the specified CategoryList and returns a set of
        // CategoryEditor types that should be present in the list based on the properties
        // in it.
        // </summary>
        // <param name="ownerType">Type of the currently displayed item</param>
        // <param name="categoryList">CategoryList to examine</param>
        // <returns>Set of expected CategoryEditor types</returns>
        public Dictionary<Type, object> GetCategoryEditors(Type ownerType, CategoryList categoryList) 
        {
            // No category editors in alpha-view
            return new Dictionary<Type, object>();
        }

        // <summary>
        // Returns a SelectionPath pointing to the first visible property
        // in the list or null if no such property exists.
        // </summary>
        // <param name="categoryList">CategoryList for reference</param>
        // <returns>SelectionPath pointing to the first visible property
        // in the list or null if no such property exists.</returns>
        public SelectionPath GetDefaultSelectionPath(CategoryList categoryList) 
        {
            if (categoryList.Count > 0) 
            {
                CategoryEntry firstCategory = categoryList[0];

                if (firstCategory != null) 
                {
                    foreach (ModelPropertyEntry firstProperty in firstCategory.Properties) 
                    {
                        if (firstProperty != null && firstProperty.IsBrowsable && firstProperty.MatchesFilter)
                        {
                            return PropertySelectionPathInterpreter.Instance.ConstructSelectionPath(firstProperty);
                        }
                    }
                }
            }

            return null;
        }

        // Blend's CollectionEditor compatibility APIs

        //
        // Since Blend's API uses PropertyEntries instead of ModelProperties, we need
        // to provide methods that consume those instead.  Ideally, with the two code
        // bases merged, we wouldn't need these at all.
        //

        // <summary>
        // Gets the category name of the specified property
        // </summary>
        // <param name="property">Property to examine</param>
        // <returns>Category name the property belongs to.</returns>
        public string GetCategoryName(PropertyEntry property) 
        {
            return System.Activities.Presentation.Internal.Properties.Resources.PropertyCategoryAllProperties;
        }

        // <summary>
        // Adds the specified property into the specified category.
        // </summary>
        // <param name="property">Property to add</param>
        // <param name="category">Category to populate</param>
        public void AddProperty(PropertyEntry property, ModelCategoryEntry category) 
        {
            category.Add(property, category.BasicProperties, this.PropertyComparer);
        }

    }
}
