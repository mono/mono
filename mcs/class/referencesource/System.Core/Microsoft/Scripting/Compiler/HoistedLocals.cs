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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Dynamic.Utils;

#if CLR2
namespace Microsoft.Scripting.Ast.Compiler {
#else
namespace System.Linq.Expressions.Compiler {
#endif

    // Suppose we have something like:
    //
    //    (string s)=>()=>s.
    //
    // We wish to generate the outer as:
    // 
    //      Func<string> OuterMethod(Closure closure, string s)
    //      {
    //          object[] locals = new object[1];
    //          locals[0] = new StrongBox<string>();
    //          ((StrongBox<string>)locals[0]).Value = s;
    //          return ((DynamicMethod)closure.Constants[0]).CreateDelegate(typeof(Func<string>), new Closure(null, locals));
    //      }
    //      
    // ... and the inner as:
    // 
    //      string InnerMethod(Closure closure)
    //      {
    //          object[] locals = closure.Locals;
    //          return ((StrongBox<string>)locals[0]).Value;
    //      }
    //
    // This class tracks that "s" was hoisted into a closure, as the 0th
    // element in the array
    //
    /// <summary>
    /// Stores information about locals and arguments that are hoisted into
    /// the closure array because they're referenced in an inner lambda.
    /// 
    /// This class is sometimes emitted as a runtime constant for internal
    /// use to hoist variables/parameters in quoted expressions
    /// 
    /// Invariant: this class stores no mutable state
    /// </summary>
    internal sealed class HoistedLocals {

        // The parent locals, if any
        internal readonly HoistedLocals Parent;

        // A mapping of hoisted variables to their indexes in the array
        internal readonly ReadOnlyDictionary<Expression, int> Indexes;

        // The variables, in the order they appear in the array
        internal readonly ReadOnlyCollection<ParameterExpression> Variables;

        // A virtual variable for accessing this locals array
        internal readonly ParameterExpression SelfVariable;

        internal HoistedLocals(HoistedLocals parent, ReadOnlyCollection<ParameterExpression> vars) {

            if (parent != null) {
                // Add the parent locals array as the 0th element in the array
                vars = new TrueReadOnlyCollection<ParameterExpression>(vars.AddFirst(parent.SelfVariable));
            }

            Dictionary<Expression, int> indexes = new Dictionary<Expression, int>(vars.Count);
            for (int i = 0; i < vars.Count; i++) {
                indexes.Add(vars[i], i);
            }

            SelfVariable = Expression.Variable(typeof(object[]), null);
            Parent = parent;
            Variables = vars;
            Indexes = new ReadOnlyDictionary<Expression, int>(indexes);
        }

        internal ParameterExpression ParentVariable {
            get { return Parent != null ? Parent.SelfVariable : null; }
        }

        internal static object[] GetParent(object[] locals) {
            return ((StrongBox<object[]>)locals[0]).Value;
        }
    }
}
