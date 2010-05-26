/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc {
    using System;
    using System.ComponentModel;
    using System.Reflection;

    internal static class ParameterInfoUtil {

        public static bool TryGetDefaultValue(ParameterInfo parameterInfo, out object value) {
            // this will get the default value as seen by the VB / C# compilers
            // if no value was baked in, RawDefaultValue returns DBNull.Value
            object rawDefaultValue = parameterInfo.RawDefaultValue;
            if (rawDefaultValue != DBNull.Value) {
                value = rawDefaultValue;
                return true;
            }

            // if the compiler did not bake in a default value, check the [DefaultValue] attribute
            DefaultValueAttribute[] attrs = (DefaultValueAttribute[])parameterInfo.GetCustomAttributes(typeof(DefaultValueAttribute), false);
            if (attrs == null || attrs.Length == 0) {
                value = default(object);
                return false;
            }
            else {
                value = attrs[0].Value;
                return true;
            }
        }

    }
}
