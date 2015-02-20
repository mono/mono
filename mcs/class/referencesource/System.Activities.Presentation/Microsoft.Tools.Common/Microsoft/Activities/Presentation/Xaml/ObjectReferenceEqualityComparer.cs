//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace Microsoft.Activities.Presentation.Xaml
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal sealed class ObjectReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
    {
        private static ObjectReferenceEqualityComparer<T> defaultComparer;

        private ObjectReferenceEqualityComparer()
        {
        }

        public static ObjectReferenceEqualityComparer<T> Default
        {
            get
            {
                if (defaultComparer == null)
                {
                    defaultComparer = new ObjectReferenceEqualityComparer<T>();
                }

                return defaultComparer;
            }
        }

        public bool Equals(T x, T y)
        {
            return object.ReferenceEquals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}
