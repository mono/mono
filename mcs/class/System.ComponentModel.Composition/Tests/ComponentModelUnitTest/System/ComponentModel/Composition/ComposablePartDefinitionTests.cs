// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.UnitTesting;
using System.ComponentModel.Composition.Factories;

namespace System.ComponentModel.Composition
{
    [TestClass]
    public class ComposablePartDefinitionTests
    {
        [TestMethod]
        public void Constructor1_ShouldNotThrow()
        {
            PartDefinitionFactory.Create();
        }

        [TestMethod]
        public void Constructor1_ShouldSetMetadataPropertyToEmptyDictionary()
        {
            var definition = PartDefinitionFactory.Create();

            EnumerableAssert.IsEmpty(definition.Metadata);
        }

        [TestMethod]
        public void Constructor1_ShouldSetMetadataPropertyToReadOnlyDictionary()
        {
            var definition = PartDefinitionFactory.Create();

            ExceptionAssert.Throws<NotSupportedException>(() =>
            {
                definition.Metadata["Value"] = "Value";
            });
        }
    }
}

