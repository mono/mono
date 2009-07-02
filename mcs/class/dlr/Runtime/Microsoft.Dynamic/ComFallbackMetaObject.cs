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
using System.Dynamic;
using System.Dynamic.Utils;
#else
using Microsoft.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
#endif
using System.Diagnostics;

#if CODEPLEX_40
namespace System.Dynamic {
#else
namespace Microsoft.Scripting {
#endif
    //
    // ComFallbackMetaObject just delegates everything to the binder.
    //
    // Note that before performing FallBack on a ComObject we need to unwrap it so that
    // binder would act upon the actual object (typically Rcw)
    //
    // Also: we don't need to implement these for any operations other than those
    // supported by ComBinder
    internal class ComFallbackMetaObject : DynamicMetaObject {
        internal ComFallbackMetaObject(Expression expression, BindingRestrictions restrictions, object arg)
            : base(expression, restrictions, arg) {
        }

        public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackGetIndex(UnwrapSelf(), indexes);
        }

        public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackSetIndex(UnwrapSelf(), indexes, value);
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackGetMember(UnwrapSelf());
        }

        public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackInvokeMember(UnwrapSelf(), args);
        }

        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackSetMember(UnwrapSelf(), value);
        }

        protected virtual ComUnwrappedMetaObject UnwrapSelf() {
            return new ComUnwrappedMetaObject(
                ComObject.RcwFromComObject(Expression),
                Restrictions.Merge(ComBinderHelpers.GetTypeRestrictionForDynamicMetaObject(this)),
                ((ComObject)Value).RuntimeCallableWrapper
            );
        }
    }

    // This type exists as a signal type, so ComBinder knows not to try to bind
    // again when we're trying to fall back
    internal sealed class ComUnwrappedMetaObject : DynamicMetaObject {
        internal ComUnwrappedMetaObject(Expression expression, BindingRestrictions restrictions, object value)
            : base(expression, restrictions, value) {
        }
    }
}

#endif
