//------------------------------------------------------------------------------
// <copyright file="WebBaseEventKeyComparer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration
{
    using System;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;

    internal class WebBaseEventKeyComparer : IEqualityComparer {
        public new bool Equals(object x, object y) {
            CustomWebEventKey   xKey = (CustomWebEventKey)x;
            CustomWebEventKey   yKey = (CustomWebEventKey)y;
            
            if (xKey._eventCode == yKey._eventCode && xKey._type.Equals(yKey._type)) {
                return true;
            }

            return false;
        }

        [SuppressMessage("Microsoft.Usage", "CA2303:FlagTypeGetHashCode", Justification = "CustomWebEventKey is internal - okay")]
        public int GetHashCode(object obj)
        {
            return ((CustomWebEventKey)obj)._eventCode ^ ((CustomWebEventKey)obj)._type.GetHashCode();
        }

        public int Compare(object x, object y) {
            CustomWebEventKey   xKey = (CustomWebEventKey)x;
            CustomWebEventKey   yKey = (CustomWebEventKey)y;

            int     xEventCode = xKey._eventCode;
            int     yEventCode = yKey._eventCode;

            if (xEventCode == yEventCode) {
                Type            xType = xKey._type;
                Type            yType = yKey._type;

                if (xType.Equals(yType)) {
                    return 0;
                }
                else {
                    return Comparer.Default.Compare(xType.ToString(), yType.ToString());
                }
            }
            else {
                if (xEventCode > yEventCode) {
                    return 1;
                } 
                else {
                    return -1;
                }
            }
            
        }

    }
}
