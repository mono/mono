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


#if !SILVERLIGHT // ComObject

using System.Diagnostics;
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

    internal sealed class IDispatchMetaObject : ComFallbackMetaObject {
        private readonly IDispatchComObject _self;

        internal IDispatchMetaObject(Expression expression, IDispatchComObject self)
            : base(expression, BindingRestrictions.Empty, self) {
            _self = self;
        }

        public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args) {
            ContractUtils.RequiresNotNull(binder, "binder");

            ComMethodDesc method;
            if (_self.TryGetMemberMethod(binder.Name, out method) ||
                _self.TryGetMemberMethodExplicit(binder.Name, out method)) {

                bool[] isByRef = ComBinderHelpers.ProcessArgumentsForCom(ref args);
                return BindComInvoke(args, method, binder.CallInfo, isByRef);
            }

            return base.BindInvokeMember(binder, args);
        }

        public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args) {
            ContractUtils.RequiresNotNull(binder, "binder");

            ComMethodDesc method;
            if (_self.TryGetGetItem(out method)) {

                bool[] isByRef = ComBinderHelpers.ProcessArgumentsForCom(ref args);
                return BindComInvoke(args, method, binder.CallInfo, isByRef);
            }

            return base.BindInvoke(binder, args);
        }

        private DynamicMetaObject BindComInvoke(DynamicMetaObject[] args, ComMethodDesc method, CallInfo callInfo, bool[] isByRef) {
            return new ComInvokeBinder(
                callInfo,
                args,
                isByRef,
                IDispatchRestriction(),
                Expression.Constant(method),
                Expression.Property(
                    Helpers.Convert(Expression, typeof(IDispatchComObject)),
                    typeof(IDispatchComObject).GetProperty("DispatchObject")
                ),
                method
            ).Invoke();
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder) {
            ComBinder.ComGetMemberBinder comBinder = binder as ComBinder.ComGetMemberBinder;
            bool canReturnCallables = comBinder == null ? false : comBinder._CanReturnCallables;

            ContractUtils.RequiresNotNull(binder, "binder");

            ComMethodDesc method;
            ComEventDesc @event;

            // 1. Try methods
            if (_self.TryGetMemberMethod(binder.Name, out method)) {
                return BindGetMember(method, canReturnCallables);
            }

            // 2. Try events
            if (_self.TryGetMemberEvent(binder.Name, out @event)) {
                return BindEvent(@event);
            }

            // 3. Try methods explicitly by name
            if (_self.TryGetMemberMethodExplicit(binder.Name, out method)) {
                return BindGetMember(method, canReturnCallables);

            }

            // 4. Fallback
            return base.BindGetMember(binder);
        }

        private DynamicMetaObject BindGetMember(ComMethodDesc method, bool canReturnCallables) {
            if (method.IsDataMember) {
                if (method.ParamCount == 0) {
                    return BindComInvoke(DynamicMetaObject.EmptyMetaObjects, method, new CallInfo(0) , new bool[]{});
                }
            }

            // ComGetMemberBinder does not expect callables. Try to call always.
            if (!canReturnCallables) {
                return BindComInvoke(DynamicMetaObject.EmptyMetaObjects, method, new CallInfo(0), new bool[0]);
            }

            return new DynamicMetaObject(
                Expression.Call(
                    typeof(ComRuntimeHelpers).GetMethod("CreateDispCallable"),
                    Helpers.Convert(Expression, typeof(IDispatchComObject)),
                    Expression.Constant(method)
                ),
                IDispatchRestriction()
            );
        }

        private DynamicMetaObject BindEvent(ComEventDesc @event) {
            // BoundDispEvent CreateComEvent(object rcw, Guid sourceIid, int dispid)
            Expression result =
                Expression.Call(
                    typeof(ComRuntimeHelpers).GetMethod("CreateComEvent"),
                    ComObject.RcwFromComObject(Expression),
                    Expression.Constant(@event.sourceIID),
                    Expression.Constant(@event.dispid)
                );

            return new DynamicMetaObject(
                result,
                IDispatchRestriction()
            );
        }

        public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes) {
            ContractUtils.RequiresNotNull(binder, "binder");

            ComMethodDesc getItem;
            if (_self.TryGetGetItem(out getItem)) {

                bool[] isByRef = ComBinderHelpers.ProcessArgumentsForCom(ref indexes);
                return BindComInvoke(indexes, getItem, binder.CallInfo , isByRef);
            }

            return base.BindGetIndex(binder, indexes);
        }

        public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value) {
            ContractUtils.RequiresNotNull(binder, "binder");

            ComMethodDesc setItem;
            if (_self.TryGetSetItem(out setItem)) {

                bool[] isByRef = ComBinderHelpers.ProcessArgumentsForCom(ref indexes);
                isByRef = isByRef.AddLast(false);

                var result = BindComInvoke(indexes.AddLast(value), setItem, binder.CallInfo, isByRef);

                // Make sure to return the value; some languages need it.
                return new DynamicMetaObject(
                    Expression.Block(result.Expression, Expression.Convert(value.Expression, typeof(object))),
                    result.Restrictions
                );
            }

            return base.BindSetIndex(binder, indexes, value);
        }

        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value) {
            ContractUtils.RequiresNotNull(binder, "binder");

            return
                // 1. Check for simple property put
                TryPropertyPut(binder, value) ??

                // 2. Check for event handler hookup where the put is dropped
                TryEventHandlerNoop(binder, value) ??

                // 3. Fallback
                base.BindSetMember(binder, value);
        }

        private DynamicMetaObject TryPropertyPut(SetMemberBinder binder, DynamicMetaObject value) {
            ComMethodDesc method;
            bool holdsNull = value.Value == null && value.HasValue;
            if (_self.TryGetPropertySetter(binder.Name, out method, value.LimitType, holdsNull) ||
                _self.TryGetPropertySetterExplicit(binder.Name, out method, value.LimitType, holdsNull)) {
                BindingRestrictions restrictions = IDispatchRestriction();
                Expression dispatch =
                    Expression.Property(
                        Helpers.Convert(Expression, typeof(IDispatchComObject)),
                        typeof(IDispatchComObject).GetProperty("DispatchObject")
                    );

                var result = new ComInvokeBinder(
                    new CallInfo(1),
                    new[] { value },
                    new bool[] { false },
                    restrictions,
                    Expression.Constant(method),
                    dispatch,
                    method
                ).Invoke();

                // Make sure to return the value; some languages need it.
                return new DynamicMetaObject(
                    Expression.Block(result.Expression, Expression.Convert(value.Expression, typeof(object))),
                    result.Restrictions
                );
            }

            return null;
        }

        private DynamicMetaObject TryEventHandlerNoop(SetMemberBinder binder, DynamicMetaObject value) {
            ComEventDesc @event;
            if (_self.TryGetMemberEvent(binder.Name, out @event) && value.LimitType == typeof(BoundDispEvent)) {
                // Drop the event property set.
                return new DynamicMetaObject(
                    Expression.Constant(null),
                    value.Restrictions.Merge(IDispatchRestriction()).Merge(BindingRestrictions.GetTypeRestriction(value.Expression, typeof(BoundDispEvent)))
                );
            }

            return null;
        }

        private BindingRestrictions IDispatchRestriction() {
            return IDispatchRestriction(Expression, _self.ComTypeDesc);
        }

        internal static BindingRestrictions IDispatchRestriction(Expression expr, ComTypeDesc typeDesc) {
            return BindingRestrictions.GetTypeRestriction(
                expr, typeof(IDispatchComObject)
            ).Merge(
                BindingRestrictions.GetExpressionRestriction(
                    Expression.Equal(
                        Expression.Property(
                            Helpers.Convert(expr, typeof(IDispatchComObject)),
                            typeof(IDispatchComObject).GetProperty("ComTypeDesc")
                        ),
                        Expression.Constant(typeDesc)
                    )
                )
            );
        }

        protected override ComUnwrappedMetaObject UnwrapSelf() {
            return new ComUnwrappedMetaObject(
                ComObject.RcwFromComObject(Expression),
                IDispatchRestriction(),
                _self.RuntimeCallableWrapper
            );
        }
    }
}

#endif
