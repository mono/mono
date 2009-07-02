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


#if !SILVERLIGHT

using System.Collections.ObjectModel;
using System.Diagnostics;
#if CODEPLEX_40
using System.Linq.Expressions;
#else
using Microsoft.Linq.Expressions;
#endif
using System.Runtime.CompilerServices;
#if !CODEPLEX_40
using Microsoft.Runtime.CompilerServices;
#endif


#if CODEPLEX_40
namespace System.Dynamic {
#else
namespace Microsoft.Scripting {
#endif
    /// <summary>
    /// Invokes the object. If it falls back, just produce an error.
    /// </summary>
    internal sealed class ComInvokeAction : InvokeBinder {
        internal ComInvokeAction(CallInfo callInfo)
            : base(callInfo) {
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public override bool Equals(object obj) {
            return base.Equals(obj as ComInvokeAction);
        }

        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion) {
            return errorSuggestion ?? new DynamicMetaObject(
                Expression.Throw(
                    Expression.New(
                        typeof(NotSupportedException).GetConstructor(new[] { typeof(string) }),
                        Expression.Constant(Strings.CannotCall)
                    )
                ),
                target.Restrictions.Merge(BindingRestrictions.Combine(args))
            );
        }
    }

    /// <summary>
    /// Splats the arguments to another nested dynamic site, which does the
    /// real invocation of the IDynamicMetaObjectProvider. 
    /// </summary>
    internal sealed class SplatInvokeBinder : CallSiteBinder {
        internal readonly static SplatInvokeBinder Instance = new SplatInvokeBinder();

        // Just splat the args and dispatch through a nested site
        public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel) {
            Debug.Assert(args.Length == 2);

            int count = ((object[])args[1]).Length;
            ParameterExpression array = parameters[1];

            var nestedArgs = new ReadOnlyCollectionBuilder<Expression>(count + 1);
            var delegateArgs = new Type[count + 3]; // args + target + returnType + CallSite
            nestedArgs.Add(parameters[0]);
            delegateArgs[0] = typeof(CallSite);
            delegateArgs[1] = typeof(object);
            for (int i = 0; i < count; i++) {
                nestedArgs.Add(Expression.ArrayAccess(array, Expression.Constant(i)));
                delegateArgs[i + 2] = typeof(object).MakeByRefType();
            }
            delegateArgs[delegateArgs.Length - 1] = typeof(object);

            return Expression.IfThen(
                Expression.Equal(Expression.ArrayLength(array), Expression.Constant(count)),
                Expression.Return(
                    returnLabel,
                    Expression.MakeDynamic(
                        Expression.GetDelegateType(delegateArgs),
                        new ComInvokeAction(new CallInfo(count)),
                        nestedArgs
                    )
                )
            );
        }
    }
}

#endif
