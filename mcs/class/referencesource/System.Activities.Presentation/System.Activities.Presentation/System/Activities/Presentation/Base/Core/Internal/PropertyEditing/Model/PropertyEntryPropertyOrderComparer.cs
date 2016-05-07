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
    // Compares PropertyEntry instances based on their PropertyOrder tokens
    // as well as their DisplayNames
    // </summary>
    internal class PropertyEntryPropertyOrderComparer : IComparer, IComparer<PropertyEntry> 
    {

        private static readonly PropertyOrder DefaultOrder = PropertyOrder.Default;
        private static PropertyEntryPropertyOrderComparer _instance;

        // <summary>
        // Gets a singleton instance of this class
        // </summary>
        public static PropertyEntryPropertyOrderComparer Instance 
        {
            get {
                if (_instance == null)
                {
                    _instance = new PropertyEntryPropertyOrderComparer();
                }

                return _instance;
            }
        }

        // <summary>
        // Compares two instances of PropertyEntry class, using both
        // PropertyOrder and DisplayName to cast its vote.
        // </summary>
        // <param name="x">Left side</param>
        // <param name="y">Right side</param>
        // <returns>Comparison result</returns>
        public int Compare(object x, object y) 
        {
            return CompareCore(x, y);
        }

        // <summary>
        // Compares two instances of PropertyEntry class, using both
        // PropertyOrder and DisplayName to cast its vote.
        // Same method, different signature.
        // </summary>
        // <param name="x">Left</param>
        // <param name="y">Right</param>
        // <param name="x">Left side</param>
        // <param name="y">Right side</param>
        // <returns>Comparison result</returns>
        public int Compare(PropertyEntry x, PropertyEntry y) 
        {
            return CompareCore(x, y);
        }

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

            PropertyOrder a = j.PropertyOrder ?? DefaultOrder;
            PropertyOrder b = k.PropertyOrder ?? DefaultOrder;

            int result = a.CompareTo(b);

            return result != 0 ? result : string.Compare(j.DisplayName, k.DisplayName, StringComparison.CurrentCulture);
        }
    }
}
