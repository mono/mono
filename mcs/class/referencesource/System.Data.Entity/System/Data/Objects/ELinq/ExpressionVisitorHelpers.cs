//---------------------------------------------------------------------
// <copyright file="ExpressionVisitorHelper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
//---------------------------------------------------------------------

namespace System.Linq.Expressions.Internal
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;

    // Because we are using the source file for ExpressionVistor from System.Core
    // we need to add code to facilitate some external calls that ExpressionVisitor makes.
    // The classes in this file do that.

    internal static class Error
    {
        internal static Exception UnhandledExpressionType(ExpressionType expressionType)
        {
            return EntityUtil.NotSupported(System.Data.Entity.Strings.ELinq_UnhandledExpressionType(expressionType));
        }
        
        internal static Exception UnhandledBindingType(MemberBindingType memberBindingType)
        {
            return EntityUtil.NotSupported(System.Data.Entity.Strings.ELinq_UnhandledBindingType(memberBindingType));
        }
    }

    internal static class ReadOnlyCollectionExtensions
    {
        internal static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> sequence)
        {
            if (sequence == null)
                return DefaultReadOnlyCollection<T>.Empty;
            ReadOnlyCollection<T> col = sequence as ReadOnlyCollection<T>;
            if (col != null)
                return col;
            return new ReadOnlyCollection<T>(sequence.ToArray());
        }
        private static class DefaultReadOnlyCollection<T>
        {
            private static ReadOnlyCollection<T> _defaultCollection;
            internal static ReadOnlyCollection<T> Empty
            {
                get
                {
                    if (_defaultCollection == null)
                        _defaultCollection = new ReadOnlyCollection<T>(new T[] { });
                    return _defaultCollection;
                }
            }
        }
    }
}
