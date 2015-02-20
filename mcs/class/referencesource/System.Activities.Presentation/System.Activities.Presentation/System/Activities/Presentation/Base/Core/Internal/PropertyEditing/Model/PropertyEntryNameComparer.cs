//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Model 
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using System.Activities.Presentation.PropertyEditing;

    // <summary>
    // Compares PropertyEntry instances solely on their DisplayName
    // </summary>
    internal class PropertyEntryNameComparer : IComparer, IComparer<PropertyEntry> 
    {

        public static readonly PropertyEntryNameComparer Instance = new PropertyEntryNameComparer();

        private static int CompareCore(object x, object y) 
        {
            ModelPropertyEntry j = x as ModelPropertyEntry;
            ModelPropertyEntry k = y as ModelPropertyEntry;

            if (j == null && k == null) 
            {
                return 0;
            }
            if (j == null) 
            {
                return -1;
            }
            if (k == null) 
            {
                return 1;
            }

            return string.Compare(j.DisplayName, k.DisplayName, StringComparison.CurrentCulture);
        }

        // IComparer<PropertyEntry> Members

        public int Compare(PropertyEntry x, PropertyEntry y) 
        {
            return CompareCore(x, y);
        }


        // IComparer Members

        public int Compare(object x, object y) 
        {
            return CompareCore(x, y);
        }

    }
}
