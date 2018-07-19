//------------------------------------------------------------------------------
// <copyright file="ExecutionContextUtil.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Util {
    using System;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Threading;

    // This class contains utility methods for dealing with security contexts when crossing AppDomain boundaries.

    [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
    internal static class ExecutionContextUtil {

        private static readonly ContextCallback s_actionToActionObjShunt = obj => ((Action)obj)();
        private static readonly ExecutionContext s_dummyDefaultEC = GetDummyDefaultEC();

        [ReflectionPermission(SecurityAction.Assert, MemberAccess = true)]
        private static ExecutionContext GetDummyDefaultEC() {
            // The ExecutionContext.PreAllocatedDefault property is special-cased by ExecutionContext to be a blank context and to allow multiple invocation.
            PropertyInfo propInfo = typeof(ExecutionContext).GetProperty("PreAllocatedDefault", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

            if (propInfo == null) {
                throw new Exception(SR.GetString(SR.Type_doesnt_have_property, typeof(ExecutionContext).FullName, "PreAllocatedDefault"));
            }

            return (ExecutionContext) propInfo.GetValue(null, null);
        }

        // Removes all context associated with the current thread (call context, IPrincipal, etc.) and invokes the provided callback.
        // The intent of this method is to get any user-provided context off the current thread before crossing an AppDomain boundary,
        // as the existence of that context could lead to behavioral problems (see DevDiv #205764) or security problems (see DevDiv #206598).
        internal static void RunInNullExecutionContext(Action callback) {
            ExecutionContext.Run(s_dummyDefaultEC, s_actionToActionObjShunt, callback);
        }

    }
}
