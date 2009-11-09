// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ComponentModel.Composition
{
    [TestClass]
    public class ImportingConstructorAttributeTests
    {
        [TestMethod]
        public void Constructor_ShouldNotThrow()
        {
            var attribute = new ImportingConstructorAttribute();

            Assert.IsNotNull(attribute);
        }
    }
}
