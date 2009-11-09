// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition;
using System.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ComponentModel.Composition
{
    [TestClass]
    public class PartCreationPolicyAttributeTests
    {
        [TestMethod]
        public void Constructor_ShouldSetCreationPolicyToGivenValue()
        {
            var expectations = Expectations.GetEnumValues<CreationPolicy>();

            foreach (var e in expectations)
            {
                var attribute = new PartCreationPolicyAttribute(e);

                Assert.AreEqual(e, attribute.CreationPolicy);
            }
        }

        [TestMethod]
        public void Constructor_OutOfRangeValueAsCreationPolicyArgument_ShouldSetCreationPolicy()
        {   // Attributes should not throw exceptions

            var expectations = Expectations.GetInvalidEnumValues<CreationPolicy>();

            foreach (var e in expectations)
            {
                var attribute = new PartCreationPolicyAttribute(e);

                Assert.AreEqual(e, attribute.CreationPolicy);
            }
        }
    }
}
