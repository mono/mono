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
    public class ImportAttributeTests
    {
        [TestMethod]
        public void Constructor1_ShouldSetContractNamePropertyToEmptyString()
        {
            var attribute = new ImportAttribute();

            Assert.IsNull(attribute.ContractName);
            Assert.IsNull(attribute.ContractType);
        }

        [TestMethod]
        public void Constructor2_NullAsContractNameArgument_ShouldSetContractNamePropertyToEmptyString()
        {
            var attribute = new ImportAttribute((string)null);

            Assert.IsNull(attribute.ContractName);
            Assert.IsNull(attribute.ContractType);
        }

        [TestMethod]
        public void Constructor3_NullAsContractTypeArgument_ShouldSetContractNamePropertyToEmptyString()
        {
            var attribute = new ImportAttribute((Type)null);

            Assert.IsNull(attribute.ContractName);
            Assert.IsNull(attribute.ContractType);
        }

        [TestMethod]
        public void Constructor4_NullAsContractTypeArgument_ShouldSetContractNamePropertyToEmptyString()
        {
            var attribute = new ImportAttribute((string)null, (Type)null);

            Assert.IsNull(attribute.ContractName);
            Assert.IsNull(attribute.ContractType);
        }

        [TestMethod]
        public void Constructor2_ValueAsContractNameArgument_ShouldSetContractNameProperty()
        {
            var expectations = Expectations.GetContractNamesWithEmpty();

            foreach (var e in expectations)
            {
                var attribute = new ImportAttribute(e);

                Assert.AreEqual(e, attribute.ContractName);
            }
        }

        [TestMethod]
        public void Constructor1_ShouldSetAllowDefaultPropertyToFalse()
        {
            var attribute = new ImportAttribute();

            Assert.IsFalse(attribute.AllowDefault);
        }

        [TestMethod]
        public void Constructor2_ShouldSetAllowDefaultPropertyToFalse()
        {
            var attribute = new ImportAttribute("ContractName");

            Assert.IsFalse(attribute.AllowDefault);
        }

        [TestMethod]
        public void Constructor3_ShouldSetAllowDefaultPropertyToFalse()
        {
            var attribute = new ImportAttribute(typeof(String));

            Assert.IsFalse(attribute.AllowDefault);
        }

        [TestMethod]
        public void Constructor1_ShouldSetAllowRecompositionPropertyToFalse()
        {
            var attribute = new ImportAttribute();

            Assert.IsFalse(attribute.AllowRecomposition);
        }

        [TestMethod]
        public void Constructor2_ShouldSetAllowRecompositionPropertyToFalse()
        {
            var attribute = new ImportAttribute("ContractName");

            Assert.IsFalse(attribute.AllowRecomposition);
        }

        [TestMethod]
        public void Constructor3_ShouldSetAllowRecompositionPropertyToFalse()
        {
            var attribute = new ImportAttribute(typeof(String));

            Assert.IsFalse(attribute.AllowRecomposition);
        }

        [TestMethod]
        public void AllowDefault_ValueAsValueArgument_ShouldSetProperty()
        {
            var expectations = Expectations.GetBooleans();

            var attribute = new ImportAttribute();

            foreach (var e in expectations)
            {
                attribute.AllowDefault = e;
                Assert.AreEqual(e, attribute.AllowDefault);
            }
        }

        [TestMethod]
        public void AllowRecomposition_ValueAsValueArgument_ShouldSetProperty()
        {
            var expectations = Expectations.GetBooleans();

            var attribute = new ImportAttribute();

            foreach (var e in expectations)
            {
                attribute.AllowRecomposition = e;
                Assert.AreEqual(e, attribute.AllowRecomposition);
            }
        }
    }
}
