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
    // Interface we use to wrap logic that determines how a set of properties
    // will be categorized into categories.
    // </summary>
    internal interface IPropertyViewManager 
    {

        // <summary>
        // Gets a flag indicating whether the PropertyViewManager implementation
        // supports category headers.
        // </summary>
        bool ShowCategoryHeaders 
        { get; }

        // <summary>
        // Add a property into the correct category within the specified CategoryList.
        // </summary>
        // <param name="propertySet">Specified property (passed in as a set for multi-select scenarios)</param>
        // <param name="propertyName">Name of the current property (perf optimization)</param>
        // <param name="categoryList">CategoryList instance to populate</param>
        // <returns>Wrapped ModelPropertyEntry for the specified propertySet</returns>
        ModelPropertyEntry AddProperty(IEnumerable<ModelProperty> propertySet, string propertyName, CategoryList categoryList);

        // <summary>
        // Scans the list of categories in the specified CategoryList and returns a set of
        // CategoryEditor types that should be present in the list based on the properties
        // in it.
        // </summary>
        // <param name="ownerType">Type of the currently displayed item</param>
        // <param name="categoryList">CategoryList to examine</param>
        // <returns>Set of expected CategoryEditor types</returns>
        Dictionary<Type, object> GetCategoryEditors(Type ownerType, CategoryList categoryList);

        // <summary>
        // Figures out what property / category editor / category / ... we should select
        // if the currently selected item does not define a default property.
        // </summary>
        // <param name="categoryList">CategoryList for reference</param>
        // <returns>Thing to select (can be null) if no default property has been
        // specified and we are trying to select something by default.</returns>
        SelectionPath GetDefaultSelectionPath(CategoryList categoryList);

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
        string GetCategoryName(PropertyEntry property);

        // <summary>
        // Adds the specified property into the specified category.
        // </summary>
        // <param name="property">Property to add</param>
        // <param name="category">Category to populate</param>
        void AddProperty(PropertyEntry property, ModelCategoryEntry category);

    }
}
