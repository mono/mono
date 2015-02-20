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
    // Simple category state object that knows how to remember
    // two boolean flags: the expansion state of the category itself and the
    // expansion state of its advanced section.
    // </summary>
    internal class CategoryState : PersistedState 
    {

        private const bool DefaultCategoryExpanded = true;
        private const bool DefaultAdvancedSectionExpanded = true;

        private string _categoryName;

        private bool _categoryExpanded = DefaultCategoryExpanded;
        private bool _advancedSectionExpanded = DefaultAdvancedSectionExpanded;

        // <summary>
        // Basic ctor
        // </summary>
        // <param name="categoryName">Name of the contained category</param>
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public CategoryState(string categoryName) 
        {
            Fx.Assert(!string.IsNullOrEmpty(categoryName), "Expected a full category name");
            _categoryName = categoryName;
        }

        // <summary>
        // We key these state objects by the category names
        // </summary>
        public override object Key 
        {
            get { return _categoryName; }
        }

        // <summary>
        // Returns true if any of the contained values differ from the default
        // </summary>
        public override bool IsSignificant 
        {
            get { return _categoryExpanded != DefaultCategoryExpanded || _advancedSectionExpanded != DefaultAdvancedSectionExpanded; }
        }

        // <summary>
        // Gets or sets a flag indicating whether this category should be expanded or collapsed
        // </summary>
        public bool CategoryExpanded 
        {
            get { return _categoryExpanded; }
            set { _categoryExpanded = value; }
        }

        // <summary>
        // Gets or sets a flag indicating whether this category should have its advanced section
        // expanded or collapsed
        // </summary>
        public bool AdvancedSectionExpanded 
        {
            get { return _advancedSectionExpanded; }
            set { _advancedSectionExpanded = value; }
        }

        // <summary>
        // Serializes this object into a simple string (AppDomains like strings).
        //
        // Format: CategoryName,CategoryExpanded,AdvancedExpanded;NextCategoryName,CategoryExpanded,AdvancedExpanded;...
        // Where bools are recorded as 0 = false and 1 = true and ';' and ',' are escaped
        // </summary>
        // <returns>Serialized version of this state object (may be null)</returns>
        protected override string SerializeCore() 
        {
            return string.Concat(
                PersistedStateUtilities.Escape(_categoryName),
                ',',
                PersistedStateUtilities.BoolToDigit(_categoryExpanded),
                ',',
                PersistedStateUtilities.BoolToDigit(_advancedSectionExpanded));
        }

        // <summary>
        // Attempts to deserialize a string into a CategoryState object
        // </summary>
        // <param name="categoryStateString">String to deserialize</param>
        // <returns>Instance of CategoryState if the serialized string was valid, null otherwise.</returns>
        public static CategoryState Deserialize(string categoryStateString) 
        {
            string[] args = categoryStateString.Split(',');
            if (args == null || args.Length != 3)
            {
                return null;
            }

            bool? categoryExpanded = PersistedStateUtilities.DigitToBool(args[1]);
            bool? advancedSectionExpanded = PersistedStateUtilities.DigitToBool(args[2]);
            if (categoryExpanded == null || advancedSectionExpanded == null)
            {
                return null;
            }

            string categoryName = PersistedStateUtilities.Unescape(args[0]);
            if (string.IsNullOrEmpty(categoryName))
            {
                return null;
            }

            CategoryState categoryState = new CategoryState(categoryName);
            categoryState.CategoryExpanded = (bool)categoryExpanded;
            categoryState.AdvancedSectionExpanded = (bool)advancedSectionExpanded;
            return categoryState;
        }
    }
}
