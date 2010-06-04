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
    }
}
