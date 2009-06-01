using System;
using System.Collections.Generic;

using NUnit.Framework;

namespace MonoTests.Common
{
    delegate void AssertThrowsDelegate ();

    static class AssertExtensions
    {
        public static void Throws<ET> (AssertThrowsDelegate code, string message)
        {
            Throws (typeof (ET), code, message);
        }

        public static void Throws (Type exceptionType, AssertThrowsDelegate code, string message)
        {
            if (code == null)
                Assert.Fail ("No code provided for the test.");
            if (exceptionType == null)
                Assert.Fail ("Exception type missing for the test.");

            Exception exception = null;
            try {
                code ();
            } catch (Exception ex) {
                exception = ex;
            }

            if (exception == null || exception.GetType () != exceptionType)
                Assert.Fail ("{0}{1}Expected: {2}{1}But was: {3}{1}",
                    message,
                    Environment.NewLine,
                    exceptionType,
                    exception == null ? "null" : exception.GetType ().ToString ());
        }
    }
}
