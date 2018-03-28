//------------------------------------------------------------------------------
// <copyright file="TargetControlTypeCache.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.UI {
    using System;
    using System.Collections;

    // Cache TargetControlTypeAttributes to improve performance
    internal static class TargetControlTypeCache {
        // Maps Type (extender control) to Type[] (valid target control types)
        private static readonly Hashtable _targetControlTypeCache = Hashtable.Synchronized(new Hashtable());

        public static Type[] GetTargetControlTypes(Type extenderControlType) {
            Type[] types = (Type[])_targetControlTypeCache[extenderControlType];
            if (types == null) {
                types = GetTargetControlTypesInternal(extenderControlType);
                _targetControlTypeCache[extenderControlType] = types;
            }
            return types;
        }

        private static Type[] GetTargetControlTypesInternal(Type extenderControlType) {
            object[] attrs = extenderControlType.GetCustomAttributes(typeof(TargetControlTypeAttribute), true);
            Type[] types = new Type[attrs.Length];
            for (int i = 0; i < attrs.Length; i++) {
                types[i] = ((TargetControlTypeAttribute)attrs[i]).TargetControlType;
            }
            return types;
        }
    }
}
