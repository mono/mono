/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

#if CLR2
namespace Microsoft.Scripting.Ast {
    using Microsoft.Scripting.Utils;
#else
namespace System.Linq.Expressions {
#endif
    /// <summary>
    /// Provides an internal interface for accessing the arguments that multiple tree
    /// nodes (DynamicExpression, ElementInit, MethodCallExpression, InvocationExpression, NewExpression,
    /// and InexExpression).
    /// 
    /// This enables two optimizations which reduce the size of the trees.  The first is it enables
    /// the nodes to hold onto an IList of T instead of a ReadOnlyCollection.  This saves the cost
    /// of allocating the ReadOnlyCollection for each node.  The second is that it enables specialized
    /// subclasses to be created which hold onto a specific number of arguments.  For example Block2,
    /// Block3, Block4.  These nodes can therefore avoid allocating both a ReadOnlyCollection and an
    /// array for storing their elements saving 32 bytes per node.
    /// 
    /// Meanwhile the nodes can continue to expose the original LINQ properties of ReadOnlyCollections.  They
    /// do this by re-using 1 field for storing both the array or an element that would normally be stored
    /// in the array.  
    /// 
    /// For the array case the collection is typed to IList of T instead of ReadOnlyCollection of T.
    /// When the node is initially constructed it is an array.  When the compiler accesses the members it
    /// uses this interface.  If a user accesses the members the array is promoted to a ReadOnlyCollection.
    /// 
    /// For the object case we store the 1st argument in a field typed to object and when the node is initially
    /// constructed this holds directly onto the Expression.  When the compiler accesses the members
    /// it again uses this interface and the accessor for the 1st argument uses Expression.ReturnObject to
    /// return the object which handles the Expression or ReadOnlyCollection case.  When the user accesses
    /// the ReadOnlyCollection then the object field is updated to hold directly onto the ReadOnlyCollection.
    /// 
    /// It is important that the Expressions consistently return the same ReadOnlyCollection otherwise the
    /// re-writer will be broken and it would be a breaking change from LINQ v1.  The problem is that currently
    /// users can rely on object identity to tell if the node has changed.  Storing the readonly collection in 
    /// an overloaded field enables us to both reduce memory usage as well as maintain compatibility and an 
    /// easy to use external API.
    /// </summary>
    public interface IArgumentProvider {
        Expression GetArgument(int index);
        int ArgumentCount {
            get;
        }
    }

    static class ArgumentProviderOps {
        internal static T[] Map<T>(this IArgumentProvider collection, Func<Expression, T> select) {
            int count = collection.ArgumentCount;
            T[] result = new T[count];
            count = 0;
            for (int i = 0; i < count; i++) {            
                result[i] = select(collection.GetArgument(i));
            }
            return result;
        }

    }
}
