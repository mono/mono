// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition.Factories;
using System.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;

namespace System.ComponentModel.Composition
{
    [TestClass]
    public class ExportProviderTests
    {
        [TestMethod]
        public void GetExports2_NullAsDefinitionArgument_ShouldThrowArgumentNull()
        {
            var provider = ExportProviderFactory.Create();

            ExceptionAssert.Throws<ArgumentNullException>(() =>
            {
                provider.GetExports((ImportDefinition)null);                
            });
        }

        [TestMethod]
        public void TryGetExports_NullAsDefinitionArgument_ShouldThrowArgumentNull()
        {
            var provider = ExportProviderFactory.Create();

            ExceptionAssert.Throws<ArgumentNullException>(() =>
            {
                IEnumerable<Export> exports;
                provider.TryGetExports((ImportDefinition)null, null, out exports);
            });
        }

        [TestMethod]
        public void TryGetExports_NullAsDefinitionArgument_ShouldNotSetExportsArgument()
        {
            var provider = ExportProviderFactory.Create();

            IEnumerable<Export> exports = new Export[0];
            IEnumerable<Export> results = exports;
            
            ExceptionAssert.Throws<ArgumentNullException>(() =>
            {
                provider.TryGetExports((ImportDefinition)null, null, out results);
            });

            Assert.AreSame(exports, results);
        }
    }
}
