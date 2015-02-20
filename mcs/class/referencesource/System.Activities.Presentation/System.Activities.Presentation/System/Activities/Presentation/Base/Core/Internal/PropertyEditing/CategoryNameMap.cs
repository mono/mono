//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing 
{
    using System;
    using System.Collections.Generic;

    // <summary>
    // Helper class that looks up (and keeps a map of) category names and their localized versions.
    // </summary>
    internal static class CategoryNameMap 
    {

        private static Dictionary<string, string> _cache = new Dictionary<string, string>();

        // <summary>
        // Gets the localized value of the specified category name.  If the input string
        // is already localized, it won't be found in Cider's resources and, hence, it
        // will be returned as is.
        //
        // Note that we pull category names from CategoryAttributes which already look up
        // the localized version for common categories, such as "Misc", "Layout" and "Appearance",
        // by default. This method just takes care of the few others that are WPF-specific and that
        // we want to be localized as well.
        // </summary>
        // <param name="categoryName">Category name to look up</param>
        // <returns>Translated version of the category name or the original name if not found.</returns>
        public static string GetLocalizedCategoryName(string categoryName) 
        {
            if (categoryName == null)
            {
                return null;
            }

            string localizedCategoryName;
            if (_cache.TryGetValue(categoryName, out localizedCategoryName))
            {
                return localizedCategoryName;
            }

            localizedCategoryName = GetLocalizedWPFCategoryName(categoryName) ?? categoryName;
            _cache[categoryName] = localizedCategoryName;
            return localizedCategoryName;
        }

        private static string GetLocalizedWPFCategoryName(string categoryName) 
        {
            return (string)System.Activities.Presentation.Internal.Properties.Resources.ResourceManager.GetString(string.Concat("PropertyCategory", categoryName.Replace(' ', '_')));
        }
    }
}
