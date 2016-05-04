//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Views 
{
    using System;
    using System.Collections.Generic;
    using System.Windows;

    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.PropertyEditing;

    using System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.PropertyInspector;
    using System.Activities.Presentation.Internal.PropertyEditing.Model;
    using System.Activities.Presentation.Internal.PropertyEditing.Selection;

    // <summary>
    // IPropertyViewManager we use to display properties grouped into categories
    // </summary>
    internal class ByCategoryViewManager : IPropertyViewManager 
    {

        public static readonly ByCategoryViewManager Instance = new ByCategoryViewManager();
        private ByCategoryViewManager() 
        {
        }

        // <summary>
        // ByCategoryViewManager shows category headers
        // </summary>
        public bool ShowCategoryHeaders 
        { get { return true; } }

        // <summary>
        // ByCategoryViewManager always uses PropertyEntryPropertyOrderComparer instance
        // </summary>
        private IComparer<PropertyEntry> PropertyComparer 
        {
            get {
                return PropertyEntryPropertyOrderComparer.Instance;
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

            string categoryName = GetCategoryName(propertySet);
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
                        bool oldIsAdvanced = foundProperty.IsAdvanced;

                        foundProperty.SetUnderlyingModelProperty(propertySet);

                        // If the IsBrowsable or IsAdvanced value of the property changed,
                        // refresh the property within the category, because how and whether
                        // this property should be rendered may have changed.
                        // Note that refreshing a selected property also nullifies its stickiness
                        // (ie. it resets CategoryList.PropertySelectionMode)
                        if (oldIsAdvanced != foundProperty.IsAdvanced ||
                            oldIsBrowsable != foundProperty.IsBrowsable) 
                        {
                            category.Refresh(foundProperty, category.GetBucket(foundProperty), this.PropertyComparer);
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
            category.Add(property, category.GetBucket(property), this.PropertyComparer);
            return property;
        }

        private static string GetCategoryName(IEnumerable<ModelProperty> propertySet) 
        {
            if (propertySet == null)
            {
                return null;
            }

            // Note: ExtensibilityAccessor uses CategoryAttribute to look up the category name.
            // CategoryAttribute logic tries to look up the localized names for standard category names
            // by default already.  CategoryNameMap.GetLocalizedCategoryName() takes care of the few,
            // special WPF categories that are not found by the existing mechanism.
            foreach (ModelProperty property in propertySet)
            {
                return CategoryNameMap.GetLocalizedCategoryName(ExtensibilityAccessor.GetCategoryName(property));
            }
            return null;
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
            Dictionary<Type, object> editorSet = new Dictionary<Type, object>();
            AddAttachedPropertiesCategoryEditors(editorSet, categoryList);
            AddTypeCategoryEditors(editorSet, ownerType);
            return editorSet;
        }

        // Scans through the properties of the specified categories, looking for CategoryEditors
        // associated with any attached properties.  If found, they will be added to the categoryEditorSet
        private static void AddAttachedPropertiesCategoryEditors(Dictionary<Type, object> categoryEditorSet, IEnumerable<CategoryBase> categories) 
        {
            if (categories != null) 
            {
                foreach (ModelCategoryEntry category in categories) 
                {
                    foreach (ModelPropertyEntry property in category.Properties) 
                    {

                        // Only attached-properties return any CategoryEditorTypes
                        IEnumerable<Type> editorTypes = property.CategoryEditorTypes;
                        if (editorTypes == null)
                        {
                            continue;
                        }

                        foreach (Type editorType in editorTypes)
                        {
                            categoryEditorSet[editorType] = null;
                        }
                    }
                }
            }
        }

        // Adds any CategoryEditors associated with the specified type
        private static void AddTypeCategoryEditors(Dictionary<Type, object> categoryEditorSet, Type type) 
        {
            if (type != null) 
            {
                IEnumerable<Type> editorTypes = ExtensibilityAccessor.GetCategoryEditorTypes(type);
                if (editorTypes != null) 
                {
                    foreach (Type editorType in editorTypes)
                    {
                        categoryEditorSet[editorType] = null;
                    }
                }
            }
        }

        // <summary>
        // Returns a SelectionPath pointing to the first visible category in the list
        // or null if no such category exists.
        // </summary>
        // <param name="categoryList">CategoryList for reference</param>
        // <returns>SelectionPath pointing to the first visible category in the list
        // or null if no such category exists.</returns>
        public SelectionPath GetDefaultSelectionPath(CategoryList categoryList) 
        {
            foreach (CategoryEntry categoryEntry in categoryList) 
            {
                CategoryContainer container = categoryList.FindCategoryEntryVisual(categoryEntry);
                if (container != null && container.Visibility == Visibility.Visible) 
                {
                    return CategoryContainerSelectionPathInterpreter.Instance.ConstructSelectionPath(categoryEntry.CategoryName, false);
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
            return property.CategoryName;
        }

        // <summary>
        // Adds the specified property into the specified category.
        // </summary>
        // <param name="property">Property to add</param>
        // <param name="category">Category to populate</param>
        public void AddProperty(PropertyEntry property, ModelCategoryEntry category) 
        {
            category.Add(property as ModelPropertyEntry, category.GetBucket(property), this.PropertyComparer);
        }

    }
}
