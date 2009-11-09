// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Globalization;
using System.Reflection;
using System.ComponentModel.Composition;
using System.Text;

namespace Microsoft.Internal
{
    internal static class Requires
    {
        [DebuggerStepThrough]
        public static void NotNull<T>(T value, string parameterName) 
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        [DebuggerStepThrough]
        public static void NotNullOrEmpty(string value, string parameterName)
        {
            NotNull(value, parameterName);

            if (value.Length == 0)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Strings.ArgumentException_EmptyString, parameterName), parameterName);
            }
        }

        [DebuggerStepThrough]
        public static void NotNullOrNullElements<T>(IEnumerable<T> values, string parameterName)
            where T : class
        {
            NotNull(values, parameterName);
            NotNullElements(values, parameterName);
        }

        [DebuggerStepThrough]
        public static void NullOrNotNullElements<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> values, string parameterName)
            where TKey : class
            where TValue : class
        {
            if (values != null)
            {
                NotNullElements(values, parameterName);
            }
        }

        [DebuggerStepThrough]
        public static void NullOrNotNullElements<T>(IEnumerable<T> values, string parameterName)
            where T : class
        {
            if (values != null)
            {
                NotNullElements(values, parameterName);
            }
        }

        private static void NotNullElements<T>(IEnumerable<T> values, string parameterName)
            where T : class
        {
            foreach (T value in values)
            {
                if (value == null)
                {
                    throw ExceptionBuilder.CreateContainsNullElement(parameterName);
                }
            }
        }

        private static void NotNullElements<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> values, string parameterName)
            where TKey : class
            where TValue : class
        {
            foreach (KeyValuePair<TKey, TValue> value in values)
            {
                if ((value.Key == null) || (value.Value == null))
                {
                    throw ExceptionBuilder.CreateContainsNullElement(parameterName);
                }
            }
        }
        [DebuggerStepThrough]
        public static void IsInMembertypeSet(MemberTypes value, string parameterName, MemberTypes enumFlagSet)
        {
            if ((value & enumFlagSet) != value || // Ensure the member is in the set
                (value & (value - 1)) != 0) // Ensure that there is only one flag in the value (i.e. value is a power of 2).
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Strings.ArgumentOutOfRange_InvalidEnumInSet, parameterName, value, enumFlagSet.ToString()), parameterName);
            }
        }
    }
}
