//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Activities.Presentation.Model;

    internal static class DynamicActivityPropertyUtilities
    {
        public static DynamicActivityProperty Find(ModelItemCollection properties, string propertyName)
        {
            foreach (ModelItem entry in properties)
            {
                DynamicActivityProperty property = (DynamicActivityProperty)entry.GetCurrentValue();

                if (StringComparer.Ordinal.Equals(property.Name, propertyName))
                {
                    return property;
                }
            }

            return null;
        }
    }
}
