// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Collections;

namespace System.Xaml.Schema
{
    internal class ReferenceEqualityComparer<T> : EqualityComparer<T> where T : class
    {
        internal static ReferenceEqualityComparer<T> Singleton = new ReferenceEqualityComparer<T>();

        public override bool Equals(T x, T y)
        {
            return object.ReferenceEquals(x, y);
        }

        public override int GetHashCode(T obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }

    internal class ReferenceEqualityTuple<T1, T2> : Tuple<T1, T2>
    {
        public ReferenceEqualityTuple(T1 item1, T2 item2)
            : base(item1, item2)
        {
        }

        public override bool Equals(object obj)
        {
            return ((IStructuralEquatable)this).Equals(obj, ReferenceEqualityComparer<object>.Singleton);
        }

        public override int GetHashCode()
        {
            return ((IStructuralEquatable)this).GetHashCode(ReferenceEqualityComparer<object>.Singleton);
        }
    }

    internal class ReferenceEqualityTuple<T1, T2, T3> : Tuple<T1, T2, T3>
    {
        public ReferenceEqualityTuple(T1 item1, T2 item2, T3 item3)
            : base(item1, item2, item3)
        {
        }

        public override bool Equals(object obj)
        {
            return ((IStructuralEquatable)this).Equals(obj, ReferenceEqualityComparer<object>.Singleton);
        }

        public override int GetHashCode()
        {
            return ((IStructuralEquatable)this).GetHashCode(ReferenceEqualityComparer<object>.Singleton);
        }
    }
}
