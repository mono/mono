//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Resources 
{
    using System;
    using System.Windows;
    using System.Activities.Presentation;

    // <summary>
    // Helper utilities for accessing values in ResourceDictionaries of controls
    // </summary>
    internal static class ResourceUtilities 
    {

        private const string TypeIconWidthKey = "TypeIconWidth";
        private const string TypeIconHeightKey = "TypeIconHeight";

        // <summary>
        // Looks up a double based on the specified key, returning specified fallback value if not found
        // </summary>
        // <param name="element">Element to use as the starting point</param>
        // <param name="key">Key to look up</param>
        // <param name="fallbackValue">Fallback value to return if key is not found</param>
        // <returns>Double from the resource or fallback value if not found</returns>
        public static double GetDouble(FrameworkElement element, string key, double fallbackValue) 
        {
            if (element == null) 
            {
                throw FxTrace.Exception.ArgumentNull("element");
            }
            if (string.IsNullOrEmpty(key)) 
            {
                throw FxTrace.Exception.ArgumentNull("key");
            }
            return (double)(element.FindResource(key) ?? fallbackValue);
        }

        public static Size GetDesiredTypeIconSize(FrameworkElement queryRoot) 
        {
            return new Size(ResourceUtilities.GetDouble(queryRoot, TypeIconWidthKey, 16),
                ResourceUtilities.GetDouble(queryRoot, TypeIconHeightKey, 16));
        }
    }
}
