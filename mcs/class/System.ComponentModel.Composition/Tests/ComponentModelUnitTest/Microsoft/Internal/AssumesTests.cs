// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#if !SILVERLIGHT
using System.Runtime.Serialization;
#endif

namespace Microsoft.Internal
{
    [TestClass]
    public class AssumesTests
    {
        [TestMethod]
        public void NotNullOfT_NullAsValueArgument_ShouldThrowInternalErrorException()
        {
            Throws(() =>
            {
                Assumes.NotNull<string>((string)null);
            });
        }

        [TestMethod]
        public void NotNullOfT1T2_NullAsValue1Argument_ShouldThrowInternalErrorException()
        {
            Throws(() =>
            {
                Assumes.NotNull<string, string>((string)null, "Value");
            });
        }

        [TestMethod]
        public void NotNullOfT1T2_NullAsValue2Argument_ShouldThrowInternalErrorException()
        {
            Throws(() =>
            {
                Assumes.NotNull<string, string>("Value", (string)null);
            });
        }

        [TestMethod]
        public void NotNullOfT1T2_NullAsValue1ArgumentAndValue2Argument_ShouldThrowInternalErrorException()
        {
            Throws(() =>
            {
                Assumes.NotNull<string, string>((string)null, (string)null);
            });
        }

        [TestMethod]
        public void NotNullOfT1T2T3_NullAsValue1Argument_ShouldThrowInternalErrorException()
        {
            Throws(() =>
            {
                Assumes.NotNull<string, string, string>((string)null, "Value", "Value");
            });
        }

        [TestMethod]
        public void NotNullOfT1T2T3_NullAsValue2Argument_ShouldThrowInternalErrorException()
        {
            Throws(() =>
            {
                Assumes.NotNull<string, string, string>("Value", (string)null, "Value");
            });
        }

        [TestMethod]
        public void NotNullOfT1T2T3_NullAsValue3Argument_ShouldThrowInternalErrorException()
        {
            Throws(() =>
            {
                Assumes.NotNull<string, string, string>("Value", "Value", (string)null);
            });
        }

        [TestMethod]
        public void NotNullOfT1T2T3_NullAsValue1ArgumentAndValue2Argument_ShouldThrowInternalErrorException()
        {
            Throws(() =>
            {
                Assumes.NotNull<string, string, string>((string)null, (string)null, "Value");
            });
        }

        [TestMethod]
        public void NotNullOfT1T2T3_NullAsValue1ArgumentAnd3_ShouldThrowInternalErrorException()
        {
            Throws(() =>
            {
                Assumes.NotNull<string, string, string>((string)null, "Value", (string)null);
            });
        }

        [TestMethod]
        public void NotNullOfT1T2T3_NullAsValue2ArgumentAndValue3Argument_ShouldThrowInternalErrorException()
        {
            Throws(() =>
            {
                Assumes.NotNull<string, string, string>("Value", (string)null, (string)null);
            });
        }

        [TestMethod]
        public void NotNullOfT_ValueAsValueArgument_ShouldNotThrow()
        {
            Assumes.NotNull<string>("Value");
        }

        [TestMethod]
        public void NotNullOfT1T2_ValueAsValue1ArgumentAndValue2Argument_ShouldNotThrow()
        {
            Assumes.NotNull<string, string>("Value", "Value");
        }

        [TestMethod]
        public void NotNullOfT1T2T3_ValueAsValue1ArgumentAndValue2ArgumentAndValue3Argument_ShouldNotThrow()
        {
            Assumes.NotNull<string, string, string>("Value", "Value", "Value");
        }

        [TestMethod]
        public void NullOfT_NullAsValueArgument_ShouldNotThrow()
        {
            Assumes.Null<string>((string)null);
        }

        [TestMethod]
        public void NullOfT_ValueAsValueArgument_ShouldThrowInternalErrorException()
        {
            Throws(() =>
            {
                Assumes.Null<string>("Value");
            });
        }

        [TestMethod]
        public void NotNullOrEmpty_NullAsValueArgument_ShouldThrowInternalErrorException()
        {
            Throws(() =>
            {
                Assumes.NotNullOrEmpty((string)null);
            });
        }

        [TestMethod]
        public void NotNullOrEmpty_EmptyAsValueArgument_ShouldThrowInternalErrorException()
        {
            Throws(() =>
            {
                Assumes.NotNullOrEmpty("");
            });
        }

        [TestMethod]
        public void NotNullOrEmpty_ValueAsValueArgument_ShouldNotThrow()
        {
            var expectations = new List<string>();
            expectations.Add(" ");
            expectations.Add("  ");
            expectations.Add("   ");
            expectations.Add("Value");

            foreach (var e in expectations)
            {
                Assumes.NotNullOrEmpty(e);
            }
        }

        [TestMethod]
        public void IsTrue1_FalseAsConditionArgument_ShouldThrowInternalErrorException()
        {
            Throws(() =>
            {
                Assumes.IsTrue(false);
            });
        }

        [TestMethod]
        public void IsTrue2_FalseAsConditionArgument_ShouldThrowInternalErrorException()
        {
            Throws(() =>
            {
                Assumes.IsTrue(false, "Message");
            });
        }

        [TestMethod]
        public void IsTrue1_TrueAsConditionArgument_ShouldNotThrow()
        {
            Assumes.IsTrue(true);
        }

        [TestMethod]
        public void IsTrue2_TrueAsConditionArgument_ShouldNotThrow()
        {
            Assumes.IsTrue(true, "Message");
        }

        [TestMethod]
        public void IsFalse1_TrueAsConditionArgument_ShouldThrowInternalErrorException()
        {
            Throws(() =>
            {
                Assumes.IsFalse(true);
            });
        }

        [TestMethod]
        public void IsFalse1_FalseAsConditionArgument_ShouldNotThrow()
        {
            Assumes.IsFalse(false);
        }

        [TestMethod]
        public void NotReachable_ShouldAlwaysThrow()
        {
            Throws(() =>
            {
                Assumes.NotReachable<object>();
            });
        }

        [TestMethod]
        public void Fail_ShouldThrowInternalErrorException()
        {
            var expectations = Expectations.GetExceptionMessages();
            
            foreach (var e in expectations)
            {
                Throws(() =>
                {
                    Assumes.Fail(e);
                });
            }
        }

        private static void Throws(Action action)
        {
            try
            {
                action();
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Type exceptionType = ex.GetType();

                // The exception should not be a 
                // publicily catchable exception
                Assert.IsFalse(exceptionType.IsVisible);
            }
        }

#if !SILVERLIGHT

        [TestMethod]
        public void Message_CanBeSerialized()
        {
            var expectations = Expectations.GetExceptionMessages();

            foreach (var e in expectations)
            {
                var exception = CreateInternalErrorException(e);

                var result = SerializationTestServices.RoundTrip(exception);

                Assert.AreEqual(exception.Message, result.Message);
            }
        }

#endif

        private static Exception CreateInternalErrorException()
        {
            return CreateInternalErrorException((string)null);
        }

        private static Exception CreateInternalErrorException(string message)
        {
            Exception exception = null;

            try
            {
                Assumes.Fail(message);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            Assert.IsNotNull(exception);
            return exception;
        }
    }
}
