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

using System.Diagnostics;
#if CODEPLEX_40
using System.Dynamic;
using System.Dynamic.Utils;
using System.Linq.Expressions;
using System.Linq.Expressions.Compiler;
#else
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using Microsoft.Linq.Expressions;
using Microsoft.Linq.Expressions.Compiler;
#endif
using System.Runtime.CompilerServices;
#if !CODEPLEX_40
using Microsoft.Runtime.CompilerServices;
#endif

using System.Runtime.InteropServices;
using System.Collections.Generic;

#if CODEPLEX_40
namespace System.Dynamic {
#else
namespace Microsoft.Scripting {
#endif
    internal static class ComBinderHelpers {

        internal static bool PreferPut(Type type, bool holdsNull) {
            Debug.Assert(type != null);

            if (type.IsValueType || type.IsArray) return true;

            if (type == typeof(String) ||
                type == typeof(DBNull) ||
                holdsNull ||
                type == typeof(System.Reflection.Missing) ||
                type == typeof(CurrencyWrapper)) {

                return true;
            } else {
                return false;
            }
        }

        internal static bool IsByRef(DynamicMetaObject mo) {
            ParameterExpression pe = mo.Expression as ParameterExpression;
            return pe != null && pe.IsByRef;
        }

        internal static bool IsStrongBoxArg(DynamicMetaObject o) {
            Type t = o.LimitType;
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(StrongBox<>);
        }

        // this helper prepares arguments for COM binding by transforming ByVal StongBox arguments
        // into ByRef expressions that represent the argument's Value fields.
        internal static bool[] ProcessArgumentsForCom(ref DynamicMetaObject[] args) {
            Debug.Assert(args != null);

            DynamicMetaObject[] newArgs = new DynamicMetaObject[args.Length];
            bool[] isByRefArg = new bool[args.Length];

            for (int i = 0; i < args.Length; i++) {
                DynamicMetaObject curArgument = args[i];

                // set new arg infos to their original values or set default ones
                // we will do this fixup early so that we can assume we always have
                // arginfos in COM binder.

                if (IsByRef(curArgument)) {
                    newArgs[i] = curArgument;
                    isByRefArg[i] = true;
                } else {
                    if (IsStrongBoxArg(curArgument)) {
                        var restrictions = curArgument.Restrictions.Merge(
                            GetTypeRestrictionForDynamicMetaObject(curArgument)
                        );

                        // we have restricted this argument to LimitType so we can convert and conversion will be trivial cast.
                        Expression boxedValueAccessor = Expression.Field(
                            Helpers.Convert(
                                curArgument.Expression,
                                curArgument.LimitType
                            ),
                            curArgument.LimitType.GetField("Value")
                        );

                        IStrongBox value = curArgument.Value as IStrongBox;
                        object boxedValue = value != null ? value.Value : null;

                        newArgs[i] = new DynamicMetaObject(
                            boxedValueAccessor,
                            restrictions,
                            boxedValue
                        );

                        isByRefArg[i] = true;
                    } else {
                        newArgs[i] = curArgument;
                        isByRefArg[i] = false;
                    }
                }
            }

            args = newArgs;
            return isByRefArg;
        }

        internal static BindingRestrictions GetTypeRestrictionForDynamicMetaObject(DynamicMetaObject obj) {
            if (obj.Value == null && obj.HasValue) {
                //If the meta object holds a null value, create an instance restriction for checking null
                return BindingRestrictions.GetInstanceRestriction(obj.Expression, null);
            }
            return BindingRestrictions.GetTypeRestriction(obj.Expression, obj.LimitType);
        }
    }
}

#endif
