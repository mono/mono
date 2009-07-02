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


using System.Diagnostics;
#if CODEPLEX_40
using System.Dynamic.Utils;
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Utils;
using Microsoft.Linq.Expressions;
#endif
using System.Reflection;

#if CODEPLEX_40
namespace System.Dynamic {
#else
namespace Microsoft.Scripting {
#endif
    /// <summary>
    /// Provides a simple class that can be inherited from to create an object with dynamic behavior
    /// at runtime.  Subclasses can override the various binder methods (GetMember, SetMember, Call, etc...)
    /// to provide custom behavior that will be invoked at runtime.  
    /// 
    /// If a method is not overridden then the DynamicObject does not directly support that behavior and 
    /// the call site will determine how the binding should be performed.
    /// </summary>
    public class DynamicObject : IDynamicMetaObjectProvider {

        /// <summary>
        /// Enables derived types to create a new instance of DynamicObject.  DynamicObject instances cannot be
        /// directly instantiated because they have no implementation of dynamic behavior.
        /// </summary>
        protected DynamicObject() {
        }

        #region Public Virtual APIs

        /// <summary>
        /// Provides the implementation of getting a member.  Derived classes can override
        /// this method to customize behavior.  When not overridden the call site requesting the
        /// binder determines the behavior.
        /// </summary>
        /// <param name="binder">The binder provided by the call site.</param>
        /// <param name="result">The result of the get operation.</param>
        /// <returns>true if the operation is complete, false if the call site should determine behavior.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public virtual bool TryGetMember(GetMemberBinder binder, out object result) {
            result = null;
            return false;
        }

        /// <summary>
        /// Provides the implementation of setting a member.  Derived classes can override
        /// this method to customize behavior.  When not overridden the call site requesting the
        /// binder determines the behavior.
        /// </summary>
        /// <param name="binder">The binder provided by the call site.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>true if the operation is complete, false if the call site should determine behavior.</returns>
        public virtual bool TrySetMember(SetMemberBinder binder, object value) {
            return false;
        }

        /// <summary>
        /// Provides the implementation of deleting a member.  Derived classes can override
        /// this method to customize behavior.  When not overridden the call site requesting the
        /// binder determines the behavior.
        /// </summary>
        /// <param name="binder">The binder provided by the call site.</param>
        /// <returns>true if the operation is complete, false if the call site should determine behavior.</returns>
        public virtual bool TryDeleteMember(DeleteMemberBinder binder) {
            return false;
        }

        /// <summary>
        /// Provides the implementation of calling a member.  Derived classes can override
        /// this method to customize behavior.  When not overridden the call site requesting the
        /// binder determines the behavior.
        /// </summary>
        /// <param name="binder">The binder provided by the call site.</param>
        /// <param name="args">The arguments to be used for the invocation.</param>
        /// <param name="result">The result of the invocation.</param>
        /// <returns>true if the operation is complete, false if the call site should determine behavior.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public virtual bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
            result = null;
            return false;
        }

        /// <summary>
        /// Provides the implementation of converting the DynamicObject to another type.  Derived classes
        /// can override this method to customize behavior.  When not overridden the call site
        /// requesting the binder determines the behavior.
        /// </summary>
        /// <param name="binder">The binder provided by the call site.</param>
        /// <param name="result">The result of the conversion.</param>
        /// <returns>true if the operation is complete, false if the call site should determine behavior.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public virtual bool TryConvert(ConvertBinder binder, out object result) {
            result = null;
            return false;
        }

        /// <summary>
        /// Provides the implementation of creating an instance of the DynamicObject.  Derived classes
        /// can override this method to customize behavior.  When not overridden the call site requesting
        /// the binder determines the behavior.
        /// </summary>
        /// <param name="binder">The binder provided by the call site.</param>
        /// <param name="args">The arguments used for creation.</param>
        /// <param name="result">The created instance.</param>
        /// <returns>true if the operation is complete, false if the call site should determine behavior.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public virtual bool TryCreateInstance(CreateInstanceBinder binder, object[] args, out object result) {
            result = null;
            return false;
        }

        /// <summary>
        /// Provides the implementation of invoking the DynamicObject.  Derived classes can
        /// override this method to customize behavior.  When not overridden the call site requesting
        /// the binder determines the behavior.
        /// </summary>
        /// <param name="binder">The binder provided by the call site.</param>
        /// <param name="args">The arguments to be used for the invocation.</param>
        /// <param name="result">The result of the invocation.</param>
        /// <returns>true if the operation is complete, false if the call site should determine behavior.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public virtual bool TryInvoke(InvokeBinder binder, object[] args, out object result) {
            result = null;
            return false;
        }

        /// <summary>
        /// Provides the implementation of performing a binary operation.  Derived classes can
        /// override this method to customize behavior.  When not overridden the call site requesting
        /// the binder determines the behavior.
        /// </summary>
        /// <param name="binder">The binder provided by the call site.</param>
        /// <param name="arg">The right operand for the operation.</param>
        /// <param name="result">The result of the operation.</param>
        /// <returns>true if the operation is complete, false if the call site should determine behavior.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public virtual bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result) {
            result = null;
            return false;
        }

        /// <summary>
        /// Provides the implementation of performing a unary operation.  Derived classes can
        /// override this method to customize behavior.  When not overridden the call site requesting
        /// the binder determines the behavior.
        /// </summary>
        /// <param name="binder">The binder provided by the call site.</param>
        /// <param name="result">The result of the operation.</param>
        /// <returns>true if the operation is complete, false if the call site should determine behavior.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public virtual bool TryUnaryOperation(UnaryOperationBinder binder, out object result) {
            result = null;
            return false;
        }

        /// <summary>
        /// Provides the implementation of performing a get index operation.  Derived classes can
        /// override this method to customize behavior.  When not overridden the call site requesting
        /// the binder determines the behavior.
        /// </summary>
        /// <param name="binder">The binder provided by the call site.</param>
        /// <param name="indexes">The indexes to be used.</param>
        /// <param name="result">The result of the operation.</param>
        /// <returns>true if the operation is complete, false if the call site should determine behavior.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public virtual bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result) {
            result = null;
            return false;
        }

        /// <summary>
        /// Provides the implementation of performing a set index operation.  Derived classes can
        /// override this method to custmize behavior.  When not overridden the call site requesting
        /// the binder determines the behavior.
        /// </summary>
        /// <param name="binder">The binder provided by the call site.</param>
        /// <param name="indexes">The indexes to be used.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>true if the operation is complete, false if the call site should determine behavior.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public virtual bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value) {
            return false;
        }

        /// <summary>
        /// Provides the implementation of performing a delete index operation.  Derived classes
        /// can override this method to custmize behavior.  When not overridden the call site
        /// requesting the binder determines the behavior.
        /// </summary>
        /// <param name="binder">The binder provided by the call site.</param>
        /// <param name="indexes">The indexes to be deleted.</param>
        /// <returns>true if the operation is complete, false if the call site should determine behavior.</returns>
        public virtual bool TryDeleteIndex(DeleteIndexBinder binder, object[] indexes) {
            return false;
        }

        /// <summary>
        /// Returns the enumeration of all dynamic member names.
        /// </summary>
        /// <returns>The list of dynamic member names.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public virtual System.Collections.Generic.IEnumerable<string> GetDynamicMemberNames() {
            return new string[0];
        }
        #endregion

        #region MetaDynamic

        private sealed class MetaDynamic : DynamicMetaObject {

            internal MetaDynamic(Expression expression, DynamicObject value)
                : base(expression, BindingRestrictions.Empty, value) {
            }

            public override System.Collections.Generic.IEnumerable<string> GetDynamicMemberNames()
            {
                return Value.GetDynamicMemberNames();
            }

            public override DynamicMetaObject BindGetMember(GetMemberBinder binder) {
                if (IsOverridden("TryGetMember")) {
                    return CallMethodWithResult("TryGetMember", binder, NoArgs, (e) => binder.FallbackGetMember(this, e));
                }

                return base.BindGetMember(binder);
            }

            public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value) {
                if (IsOverridden("TrySetMember")) {
                    return CallMethodReturnLast("TrySetMember", binder, GetArgs(value), (e) => binder.FallbackSetMember(this, value, e));
                }

                return base.BindSetMember(binder, value);
            }

            public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder) {
                if (IsOverridden("TryDeleteMember")) {
                    return CallMethodNoResult("TryDeleteMember", binder, NoArgs, (e) => binder.FallbackDeleteMember(this, e));
                }

                return base.BindDeleteMember(binder);
            }

            public override DynamicMetaObject BindConvert(ConvertBinder binder) {
                if (IsOverridden("TryConvert")) {
                    return CallMethodWithResult("TryConvert", binder, NoArgs, (e) => binder.FallbackConvert(this, e));
                }

                return base.BindConvert(binder);
            }

            public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args) {
                if (IsOverridden("TryInvokeMember")) {
                    return CallMethodWithResult("TryInvokeMember", binder, GetArgArray(args), (e) => binder.FallbackInvokeMember(this, args, e));
                } else if (IsOverridden("TryGetMember")) {
                    // Generate a tree like:
                    //
                    // {
                    //   object result;
                    //   TryGetMember(payload, out result) ? FallbackInvoke(result) : fallbackResult
                    // }
                    //
                    // Then it calls FallbackInvokeMember with this tree as the
                    // "error", giving the language the option of using this
                    // tree or doing .NET binding.
                    //
                    return CallMethodWithResult(
                        "TryGetMember", new GetBinderAdapter(binder), NoArgs,
                        (e) => binder.FallbackInvokeMember(this, args, e),
                        (e) => binder.FallbackInvoke(e, args, null)
                    );
                }

                return base.BindInvokeMember(binder, args);
            }


            public override DynamicMetaObject BindCreateInstance(CreateInstanceBinder binder, DynamicMetaObject[] args) {
                if (IsOverridden("TryCreateInstance")) {
                    return CallMethodWithResult("TryCreateInstance", binder, GetArgArray(args), (e) => binder.FallbackCreateInstance(this, args, e));
                }

                return base.BindCreateInstance(binder, args);
            }

            public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args) {
                if (IsOverridden("TryInvoke")) {
                    return CallMethodWithResult("TryInvoke", binder, GetArgArray(args), (e) => binder.FallbackInvoke(this, args, e));
                }

                return base.BindInvoke(binder, args);
            }

            public override DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg) {
                if (IsOverridden("TryBinaryOperation")) {
                    return CallMethodWithResult("TryBinaryOperation", binder, GetArgs(arg), (e) => binder.FallbackBinaryOperation(this, arg, e));
                }

                return base.BindBinaryOperation(binder, arg);
            }

            public override DynamicMetaObject BindUnaryOperation(UnaryOperationBinder binder) {
                if (IsOverridden("TryUnaryOperation")) {
                    return CallMethodWithResult("TryUnaryOperation", binder, NoArgs, (e) => binder.FallbackUnaryOperation(this, e));
                }

                return base.BindUnaryOperation(binder);
            }

            public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes) {
                if (IsOverridden("TryGetIndex")) {
                    return CallMethodWithResult("TryGetIndex", binder, GetArgArray(indexes), (e) => binder.FallbackGetIndex(this, indexes, e));
                }

                return base.BindGetIndex(binder, indexes);
            }

            public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value) {
                if (IsOverridden("TrySetIndex")) {
                    return CallMethodReturnLast("TrySetIndex", binder, GetArgArray(indexes, value), (e) => binder.FallbackSetIndex(this, indexes, value, e));
                }

                return base.BindSetIndex(binder, indexes, value);
            }

            public override DynamicMetaObject BindDeleteIndex(DeleteIndexBinder binder, DynamicMetaObject[] indexes) {
                if (IsOverridden("TryDeleteIndex")) {
                    return CallMethodNoResult("TryDeleteIndex", binder, GetArgArray(indexes), (e) => binder.FallbackDeleteIndex(this, indexes, e));
                }

                return base.BindDeleteIndex(binder, indexes);
            }

            private delegate DynamicMetaObject Fallback(DynamicMetaObject errorSuggestion);

            private readonly static Expression[] NoArgs = new Expression[0];

            private static Expression[] GetArgs(params DynamicMetaObject[] args) {
                Expression[] paramArgs = DynamicMetaObject.GetExpressions(args);

                for (int i = 0; i < paramArgs.Length; i++) {
                    paramArgs[i] = Expression.Convert(args[i].Expression, typeof(object));
                }

                return paramArgs;
            }

            private static Expression[] GetArgArray(DynamicMetaObject[] args) {
                return new[] { Expression.NewArrayInit(typeof(object), GetArgs(args)) };
            }

            private static Expression[] GetArgArray(DynamicMetaObject[] args, DynamicMetaObject value) {
                return new Expression[] {
                    Expression.NewArrayInit(typeof(object), GetArgs(args)),
                    Expression.Convert(value.Expression, typeof(object))
                };
            }

            private static ConstantExpression Constant(DynamicMetaObjectBinder binder) {
                Type t = binder.GetType();
                while (!t.IsVisible) {
                    t = t.BaseType;
                }
                return Expression.Constant(binder, t);
            }

            /// <summary>
            /// Helper method for generating a MetaObject which calls a
            /// specific method on Dynamic that returns a result
            /// </summary>
            private DynamicMetaObject CallMethodWithResult(string methodName, DynamicMetaObjectBinder binder, Expression[] args, Fallback fallback) {
                return CallMethodWithResult(methodName, binder, args, fallback, null);
            }

            /// <summary>
            /// Helper method for generating a MetaObject which calls a
            /// specific method on Dynamic that returns a result
            /// </summary>
            private DynamicMetaObject CallMethodWithResult(string methodName, DynamicMetaObjectBinder binder, Expression[] args, Fallback fallback, Fallback fallbackInvoke) {
                //
                // First, call fallback to do default binding
                // This produces either an error or a call to a .NET member
                //
                DynamicMetaObject fallbackResult = fallback(null);

                //
                // Build a new expression like:
                // {
                //   object result;
                //   TryGetMember(payload, out result) ? fallbackInvoke(result) : fallbackResult
                // }
                //
                var result = Expression.Parameter(typeof(object), null);

                var callArgs = new Expression[args.Length + 2];
                Array.Copy(args, 0, callArgs, 1, args.Length);
                callArgs[0] = Constant(binder);
                callArgs[callArgs.Length - 1] = result;

                var resultMO = new DynamicMetaObject(result, BindingRestrictions.Empty);

                // Need to add a conversion if calling TryConvert
                if (binder.ReturnType != typeof(object)) {
                    Debug.Assert(binder is ConvertBinder && fallbackInvoke == null);

                    var convert = Expression.Convert(resultMO.Expression, binder.ReturnType);
                    // will always be a cast or unbox
                    Debug.Assert(convert.Method == null);

                    resultMO = new DynamicMetaObject(convert, resultMO.Restrictions);
                }

                if (fallbackInvoke != null) {
                    resultMO = fallbackInvoke(resultMO);
                }

                var callDynamic = new DynamicMetaObject(
                    Expression.Block(
                        new[] { result },
                        Expression.Condition(
                            Expression.Call(
                                GetLimitedSelf(),
                                typeof(DynamicObject).GetMethod(methodName),
                                callArgs
                            ),
                            resultMO.Expression,
                            fallbackResult.Expression,
                            binder.ReturnType
                        )
                    ),
                    GetRestrictions().Merge(resultMO.Restrictions).Merge(fallbackResult.Restrictions)
                );
                
                //
                // Now, call fallback again using our new MO as the error
                // When we do this, one of two things can happen:
                //   1. Binding will succeed, and it will ignore our call to
                //      the dynamic method, OR
                //   2. Binding will fail, and it will use the MO we created
                //      above.
                //
                return fallback(callDynamic);
            }


            /// <summary>
            /// Helper method for generating a MetaObject which calls a
            /// specific method on Dynamic, but uses one of the arguments for
            /// the result.
            /// </summary>
            private DynamicMetaObject CallMethodReturnLast(string methodName, DynamicMetaObjectBinder binder, Expression[] args, Fallback fallback) {
                //
                // First, call fallback to do default binding
                // This produces either an error or a call to a .NET member
                //
                DynamicMetaObject fallbackResult = fallback(null);

                //
                // Build a new expression like:
                // {
                //   object result;
                //   TrySetMember(payload, result = value) ? result : fallbackResult
                // }
                //

                var result = Expression.Parameter(typeof(object), null);
                var callArgs = args.AddFirst(Constant(binder));
                callArgs[args.Length] = Expression.Assign(result, callArgs[args.Length]);

                var callDynamic = new DynamicMetaObject(
                    Expression.Block(
                        new[] { result },
                        Expression.Condition(
                            Expression.Call(
                                GetLimitedSelf(),
                                typeof(DynamicObject).GetMethod(methodName),
                                callArgs
                            ),
                            result,
                            fallbackResult.Expression,
                            typeof(object)
                        )
                    ),
                    GetRestrictions().Merge(fallbackResult.Restrictions)
                );

                //
                // Now, call fallback again using our new MO as the error
                // When we do this, one of two things can happen:
                //   1. Binding will succeed, and it will ignore our call to
                //      the dynamic method, OR
                //   2. Binding will fail, and it will use the MO we created
                //      above.
                //
                return fallback(callDynamic);
            }


            /// <summary>
            /// Helper method for generating a MetaObject which calls a
            /// specific method on Dynamic, but uses one of the arguments for
            /// the result.
            /// </summary>
            private DynamicMetaObject CallMethodNoResult(string methodName, DynamicMetaObjectBinder binder, Expression[] args, Fallback fallback) {
                //
                // First, call fallback to do default binding
                // This produces either an error or a call to a .NET member
                //
                DynamicMetaObject fallbackResult = fallback(null);

                //
                // Build a new expression like:
                //   if (TryDeleteMember(payload)) { } else { fallbackResult }
                //
                var callDynamic = new DynamicMetaObject(
                    Expression.Condition(
                        Expression.Call(
                            GetLimitedSelf(),
                            typeof(DynamicObject).GetMethod(methodName),
                            args.AddFirst(Constant(binder))
                        ),
                        Expression.Empty(),
                        fallbackResult.Expression,
                        typeof(void)
                    ),
                    GetRestrictions().Merge(fallbackResult.Restrictions)
                );

                //
                // Now, call fallback again using our new MO as the error
                // When we do this, one of two things can happen:
                //   1. Binding will succeed, and it will ignore our call to
                //      the dynamic method, OR
                //   2. Binding will fail, and it will use the MO we created
                //      above.
                //
                return fallback(callDynamic);
            }

            /// <summary>
            /// Checks if the derived type has overridden the specified method.  If there is no
            /// implementation for the method provided then Dynamic falls back to the base class
            /// behavior which lets the call site determine how the binder is performed.
            /// </summary>
            private bool IsOverridden(string method) {
                var methods = Value.GetType().GetMember(method, MemberTypes.Method, BindingFlags.Public | BindingFlags.Instance);

                foreach (MethodInfo mi in methods) {
                    if (mi.DeclaringType != typeof(DynamicObject) && mi.GetBaseDefinition().DeclaringType == typeof(DynamicObject)) {
                        return true;
                    }
                }

                return false;
            }

            /// <summary>
            /// Returns a Restrictions object which includes our current restrictions merged
            /// with a restriction limiting our type
            /// </summary>
            private BindingRestrictions GetRestrictions() {
                Debug.Assert(Restrictions == BindingRestrictions.Empty, "We don't merge, restrictions are always empty");

                return BindingRestrictions.GetTypeRestriction(this);
            }

            /// <summary>
            /// Returns our Expression converted to our known LimitType
            /// </summary>
            private Expression GetLimitedSelf() {
                if (TypeUtils.AreEquivalent(Expression.Type, LimitType)) {
                    return Expression;
                }
                return Expression.Convert(Expression, LimitType);
            }

            private new DynamicObject Value {
                get {
                    return (DynamicObject)base.Value;
                }
            }

            // It is okay to throw NotSupported from this binder. This object
            // is only used by DynamicObject.GetMember--it is not expected to
            // (and cannot) implement binding semantics. It is just so the DO
            // can use the Name and IgnoreCase properties.
            private sealed class GetBinderAdapter : GetMemberBinder {
                internal GetBinderAdapter(InvokeMemberBinder binder)
                    : base(binder.Name, binder.IgnoreCase) {
                }

                public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion) {
                    throw new NotSupportedException();
                }
            }
        }

        #endregion

        #region IDynamicMetaObjectProvider Members

        /// <summary>
        /// The provided MetaObject will dispatch to the Dynamic virtual methods.
        /// The object can be encapsulated inside of another MetaObject to
        /// provide custom behavior for individual actions.
        /// </summary>
        public virtual DynamicMetaObject GetMetaObject(Expression parameter) {
            return new MetaDynamic(parameter, this);
        }

        #endregion
    }
}
