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

#if CODEPLEX_40
using System.Linq.Expressions;
#else
using Microsoft.Linq.Expressions;
#endif
using System.Security;
using System.Security.Permissions;

#if CODEPLEX_40
namespace System.Dynamic {
#else
namespace Microsoft.Scripting {
#endif
    internal class DispCallableMetaObject : DynamicMetaObject {
        private readonly DispCallable _callable;

        internal DispCallableMetaObject(Expression expression, DispCallable callable)
            : base(expression, BindingRestrictions.Empty, callable) {
            _callable = callable;
        }

        public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes) {
            return BindGetOrInvoke(indexes, binder.CallInfo) ??
                base.BindGetIndex(binder, indexes);
        }

        public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args) {
            return BindGetOrInvoke(args, binder.CallInfo) ??
                base.BindInvoke(binder, args);
        }

#if CLR2
        [SecurityCritical, SecurityTreatAsSafe]
#else
        [SecuritySafeCritical]
#endif
        private DynamicMetaObject BindGetOrInvoke(DynamicMetaObject[] args, CallInfo callInfo) {
            //
            // Demand Full Trust to proceed with the binding.
            //

            new PermissionSet(PermissionState.Unrestricted).Demand();

            ComMethodDesc method;
            var target = _callable.DispatchComObject;
            var name = _callable.MemberName;

            if (target.TryGetMemberMethod(name, out method) ||
                target.TryGetMemberMethodExplicit(name, out method)) {

                bool[] isByRef = ComBinderHelpers.ProcessArgumentsForCom(ref args);
                return BindComInvoke(method, args, callInfo, isByRef);
            }
            return null;
        }

#if CLR2
        [SecurityCritical, SecurityTreatAsSafe]
#else
        [SecuritySafeCritical]
#endif
        public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value) {
            //
            // Demand Full Trust to proceed with the binding.
            //

            new PermissionSet(PermissionState.Unrestricted).Demand();

            ComMethodDesc method;
            var target = _callable.DispatchComObject;
            var name = _callable.MemberName;

            bool holdsNull = value.Value == null && value.HasValue;
            if (target.TryGetPropertySetter(name, out method, value.LimitType, holdsNull) ||
                target.TryGetPropertySetterExplicit(name, out method, value.LimitType, holdsNull)) {

                bool[] isByRef = ComBinderHelpers.ProcessArgumentsForCom(ref indexes);
                isByRef = isByRef.AddLast(false);
                var result = BindComInvoke(method, indexes.AddLast(value), binder.CallInfo, isByRef);

                // Make sure to return the value; some languages need it.
                return new DynamicMetaObject(
                    Expression.Block(result.Expression, Expression.Convert(value.Expression, typeof(object))),
                    result.Restrictions
                );
            }

            return base.BindSetIndex(binder, indexes, value);
        }

        [SecurityCritical]
        private DynamicMetaObject BindComInvoke(ComMethodDesc method, DynamicMetaObject[] indexes, CallInfo callInfo, bool[] isByRef) {
            var callable = Expression;
            var dispCall = Helpers.Convert(callable, typeof(DispCallable));

            return new ComInvokeBinder(
                callInfo,
                indexes,
                isByRef,
                DispCallableRestrictions(),
                Expression.Constant(method),
                Expression.Property(
                    dispCall,
                    typeof(DispCallable).GetProperty("DispatchObject")
                ),
                method
            ).Invoke();
        }

        [SecurityCritical]
        private BindingRestrictions DispCallableRestrictions() {
            var callable = Expression;

            var callableTypeRestrictions = BindingRestrictions.GetTypeRestriction(callable, typeof(DispCallable));
            var dispCall = Helpers.Convert(callable, typeof(DispCallable));
            var dispatch = Expression.Property(dispCall, typeof(DispCallable).GetProperty("DispatchComObject"));
            var dispId = Expression.Property(dispCall, typeof(DispCallable).GetProperty("DispId"));

            var dispatchRestriction = IDispatchMetaObject.IDispatchRestriction(dispatch, _callable.DispatchComObject.ComTypeDesc);
            var memberRestriction = BindingRestrictions.GetExpressionRestriction(
                Expression.Equal(dispId, Expression.Constant(_callable.DispId))
            );

            return callableTypeRestrictions.Merge(dispatchRestriction).Merge(memberRestriction);
        }
    }
}

#endif
