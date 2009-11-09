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
    public class ExportAttributeTests
    {
        [TestMethod]
        public void Constructor1_ShouldSetContractNamePropertyToEmptyString()
        {
            var attribute = new ExportAttribute();

            Assert.IsNull(attribute.ContractName);
            Assert.IsNull(attribute.ContractType);
        }

        [TestMethod]
        public void Constructor2_NullAsContractNameArgument_ShouldSetContractNamePropertyToEmptyString()
        {
            var attribute = new ExportAttribute((string)null);

            Assert.IsNull(attribute.ContractName);
            Assert.IsNull(attribute.ContractType);
        }

        [TestMethod]
        public void Constructor3_NullAsContractTypeArgument_ShouldSetContractNamePropertyToEmptyString()
        {
            var attribute = new ExportAttribute((Type)null);

            Assert.IsNull(attribute.ContractName);
            Assert.IsNull(attribute.ContractType);
        }

        [TestMethod]
        public void Constructor4_NullAsContractTypeArgument_ShouldSetContractNamePropertyToEmptyString()
        {
            var attribute = new ExportAttribute((string)null, (Type)null);

            Assert.IsNull(attribute.ContractName);
            Assert.IsNull(attribute.ContractType);
        }

        [TestMethod]
        public void Constructor2_ValueAsContractNameArgument_ShouldSetContractNameProperty()
        {
            var expectations = Expectations.GetContractNamesWithEmpty();
            
            foreach (var e in expectations)
            {
                var attribute = new ExportAttribute(e);

                Assert.AreEqual(e, attribute.ContractName);
            }
        }
    }
}
