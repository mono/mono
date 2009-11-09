// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ComponentModel.Composition
{
    [TestClass]
    public class MetadataAttributeAttributeTests
    {
        [TestMethod]
        public void Constructor_ShouldNotThrow()
        {
            var attribute = new MetadataAttributeAttribute();

            Assert.IsNotNull(attribute);
        }
    }
}
