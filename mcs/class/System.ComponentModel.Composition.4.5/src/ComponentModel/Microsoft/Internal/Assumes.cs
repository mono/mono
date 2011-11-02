// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Internal
{
    internal static partial class Assumes
    {
        [DebuggerStepThrough]
        internal static void NotNull<T>(T value)
            where T : class
        {
            IsTrue(value != null);
        }

        [DebuggerStepThrough]
        internal static void NotNull<T1, T2>(T1 value1, T2 value2)
            where T1 : class
            where T2 : class
        {
            NotNull(value1);
            NotNull(value2);
        }

        [DebuggerStepThrough]
        internal static void NotNull<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
            where T1 : class
            where T2 : class
            where T3 : class
        {
            NotNull(value1);
            NotNull(value2);
            NotNull(value3);
        }

        [DebuggerStepThrough]
        internal static void NotNullOrEmpty(string value)
        {
            NotNull(value);
            IsTrue(value.Length > 0);
        }

        [DebuggerStepThrough]
        internal static void IsTrue(bool condition)
        {
            if (!condition)
            {
                throw UncatchableException(null);
            }
        }

        [DebuggerStepThrough]
        internal static void IsTrue(bool condition, [Localizable(false)]string message)
        {
            if (!condition)
            {
                throw UncatchableException(message);
            }
        }

        [DebuggerStepThrough]
        internal static T NotReachable<T>()
        {
            throw UncatchableException("Code path should never be reached!");
        }

        [DebuggerStepThrough]
        private static Exception UncatchableException([Localizable(false)]string message)
        {
            return new InternalErrorException(message);
        }
    } 
}
