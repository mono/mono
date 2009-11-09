// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Factories;
using System.Linq;
using System.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ComponentModel.Composition
{
    [TestClass]
    public class ComposablePartTests
    {
        [TestMethod]
        public void Constructor1_ShouldSetMetadataPropertyToEmptyDictionary()
        {
            var part = PartFactory.Create();

            EnumerableAssert.IsEmpty(part.Metadata);
        }

        [TestMethod]
        public void Constructor1_ShouldSetMetadataPropertyToReadOnlyDictionary()
        {
            var part = PartFactory.Create();

            ExceptionAssert.Throws<NotSupportedException>(() =>
            {
                part.Metadata["Value"] = "Value";
            });
        }

        [TestMethod]
        public void OnComposed_DoesNotThrow()
        {
            var part = PartFactory.Create();
            part.Activate();
        }

    }
}