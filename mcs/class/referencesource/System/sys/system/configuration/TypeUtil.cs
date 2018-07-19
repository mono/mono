//------------------------------------------------------------------------------
// <copyright file="TypeUtil.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace System.Configuration {

    internal static class TypeUtil {
        [ReflectionPermission(SecurityAction.Assert, Flags=ReflectionPermissionFlag.MemberAccess)]
        static internal object CreateInstanceWithReflectionPermission(string typeString) {
            Type type = Type.GetType(typeString, true);           // catch the errors and report them
            object result = Activator.CreateInstance(type, true); // create non-public types
            return result;
        }
    }
}
