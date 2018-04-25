// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Debugger
{
    using System;
    using System.Runtime;

    internal static class UnitTestUtility
    {
        internal static Func<string, Exception> AssertionExceptionFactory
        {
            get;
            set;
        }

        internal static void TestInitialize(Func<string, Exception> createAssertionException)
        {
            UnitTestUtility.AssertionExceptionFactory = createAssertionException;
        }

        internal static void TestCleanup()
        {
            UnitTestUtility.AssertionExceptionFactory = null;
        }

        internal static void Assert(bool condition, string assertionMessage)
        {
            if (UnitTestUtility.AssertionExceptionFactory != null)
            {
                if (!condition)
                {
                    throw FxTrace.Exception.AsError(UnitTestUtility.AssertionExceptionFactory(assertionMessage));
                }
            }
            else
            {
                Fx.Assert(condition, assertionMessage);
            }
        }
    }
}
