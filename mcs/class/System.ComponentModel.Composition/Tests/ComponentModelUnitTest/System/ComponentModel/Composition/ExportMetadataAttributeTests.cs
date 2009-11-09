// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.UnitTesting;

namespace System.ComponentModel.Composition
{
    [TestClass]
    public class ExportMetadataAttributeTests
    {
        [TestMethod]
        public void Constructor_NullAsNameArgument_ShouldSetNamePropertyToEmptyString()
        {
            var attribute = new ExportMetadataAttribute((string)null, "Value");

            Assert.AreEqual(string.Empty, attribute.Name);
        }

        [TestMethod]
        public void Constructor_ShouldSetIsMultiplePropertyToFalse()
        {
            var attribute = new ExportMetadataAttribute("Name", "Value");

            Assert.IsFalse(attribute.IsMultiple);
        }

        [TestMethod]
        public void Constructor_ValueAsNameArgument_ShouldSetNameProperty()
        {
            var expectations = Expectations.GetMetadataNames();
            
            foreach (var e in expectations)
            {
                var attribute = new ExportMetadataAttribute(e, "Value");

                Assert.AreEqual(e, attribute.Name);                
            }
        }

        [TestMethod]
        public void Constructor_ValueAsValueArgument_ShouldSetValueProperty()
        {
            var expectations = Expectations.GetMetadataValues();
            
            foreach (var e in expectations)
            {
                var attribute = new ExportMetadataAttribute("Name", e);

                Assert.AreEqual(e, attribute.Value);
            }
        }

        [TestMethod]
        public void IsMultiple_ValueAsValueArgument_ShouldSetPropert()
        {
            var expectations = Expectations.GetBooleans();

            var attribute = new ExportMetadataAttribute("Name", "Value");

            foreach (var e in expectations)
            {
                attribute.IsMultiple = e;
                Assert.AreEqual(e, attribute.IsMultiple);
            }
        }
    }
}
