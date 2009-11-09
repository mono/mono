// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Factories;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System
{
    [TestClass]
    public class LazyOfTMetadataTests
    {
        public class MetadataView
        {
        }

        [TestMethod]
        public void Constructor1_MetadataViewSet()
        {
            MetadataView metadataView = new MetadataView();
            var export = new Lazy<string, MetadataView>(() => "Value", metadataView, false);
            Assert.AreEqual(metadataView, export.Metadata);
        }

        [TestMethod]
        public void Constructor1_MetadataViewSetToNull()
        {
            MetadataView metadataView = new MetadataView();
            var export = new Lazy<string, MetadataView>(() => "Value", null, false);
            Assert.IsNull(export.Metadata);
        }


        [TestMethod]
        public void Constructor1_NullAsExportedValueGetterArgument_ShouldThrowArgumentNull()
        {
            ExceptionAssert.ThrowsArgument<ArgumentNullException>("valueFactory", () =>
            {
                new Lazy<string, MetadataView>((Func<string>)null, new MetadataView(), false);
            });
        }

        [TestMethod]
        public void Constructor1_FuncReturningAStringAsExportedValueGetter_ShouldBeReturnedByGetExportedValue()
        {
            var export = new Lazy<string, MetadataView>(() => "Value", new MetadataView(), false);

            Assert.AreEqual("Value", export.Value);
        }

        [TestMethod]
        public void Constructor1_FuncReturningNullAsExportedValueGetter_ShouldBeReturnedByGetExportedValue()
        {
            var export = new Lazy<string, MetadataView>(() => null, new MetadataView(), false);

            Assert.IsNull(export.Value);
        }

        [TestMethod]
        public void Value_ShouldCacheExportedValueGetter()
        {
            int count = 0;

            var export = new Lazy<int, MetadataView>(() =>
            {
                count++;
                return count;
            }, new MetadataView(), false);

            Assert.AreEqual(1, export.Value);
            Assert.AreEqual(1, export.Value);
            Assert.AreEqual(1, export.Value);
        }
        [TestMethod]
        public void Constructor2_MetadataSet()
        {
            MetadataView metadataView = new MetadataView();
            var export = new Lazy<object, MetadataView>(metadataView, false);

            Assert.AreSame(metadataView, export.Metadata);
            Assert.IsNotNull(export.Value);
        }

#if CLR40
        [TestMethod]
        public void Constructor3_MetadataSet()
        {
            MetadataView metadataView = new MetadataView();
            var export = new Lazy<object, MetadataView>(metadataView, true);

            Assert.AreSame(metadataView, export.Metadata);
            Assert.IsNotNull(export.Value);
        }

        [TestMethod]
        public void Constructor4_MetadataSet()
        {
            MetadataView metadataView = new MetadataView();
            var export = new Lazy<string, MetadataView>(() => "Value",
                metadataView, true);

            Assert.AreSame(metadataView, export.Metadata);
            Assert.IsNotNull(export.Value);
        }
#endif
    }
}
