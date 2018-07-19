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
namespace System.Linq.Expressions
{
#endif

    public interface IDynamicExpression : IArgumentProvider
    {
        /// <summary>
        /// Gets the type of the delegate used by the CallSite />.
        /// </summary>
        Type DelegateType { get; }

        /// <summary>
        /// Rewrite this node replacing the args with the provided values.  The 
        /// number of the args needs to match the number of the current block.
        /// 
        /// This helper is provided to allow re-writing of nodes to not depend on the specific 
        /// class of DynamicExpression which is being used. 
        /// </summary>
        Expression Rewrite(Expression[] args);

        /// <summary>
        /// Creates a CallSite for the node.
        /// </summary>
        object CreateCallSite();
    }
}