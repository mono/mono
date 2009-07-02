/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
using System; using Microsoft;


#if CODEPLEX_40
using System.Linq.Expressions;
#else
using Microsoft.Linq.Expressions;
#endif

#if CODEPLEX_40
namespace System.Dynamic {
#else
namespace Microsoft.Scripting {
#endif
    /// <summary>
    /// Represents a dynamic object, that can have its operations bound at runtime.
    /// </summary>
    /// <remarks>
    /// Objects that want to participate in the binding process should implement an IDynamicMetaObjectProvider interface,
    /// and implement <see cref="IDynamicMetaObjectProvider.GetMetaObject" /> to return a <see cref="DynamicMetaObject" />.
    /// </remarks>
    public interface IDynamicMetaObjectProvider {
        /// <summary>
        /// Returns the <see cref="DynamicMetaObject" /> responsible for binding operations performed on this object.
        /// </summary>
        /// <param name="parameter">The expression tree representation of the runtime value.</param>
        /// <returns>The <see cref="DynamicMetaObject" /> to bind this object.</returns>
        DynamicMetaObject GetMetaObject(Expression parameter);
    }
}
