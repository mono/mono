// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Factories;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.Composition.Hosting;

namespace System.ComponentModel.Composition
{
    [TestClass]
    public class AggregateExportProviderTests
    {
        [TestMethod]
        public void Constructor1_NullAsProvidersArgument_ShouldSetProvidersPropertyToEmpty()
        {
            var provider = new AggregateExportProvider((ExportProvider[])null);

            EnumerableAssert.IsEmpty(provider.Providers);
        }

        [TestMethod]
        public void Constructor2_NullAsProvidersArgument_ShouldSetProvidersPropertyToEmpty()
        {
            var provider = new AggregateExportProvider((IEnumerable<ExportProvider>)null);

            EnumerableAssert.IsEmpty(provider.Providers);
        }

        [TestMethod]
        public void Constructor1_EmptyArrayAsProvidersArgument_ShouldSetProvidersPropertyToEmpty()
        {
            var provider = new AggregateExportProvider(new ExportProvider[0]);

            EnumerableAssert.IsEmpty(provider.Providers);
        }

        [TestMethod]
        public void Constructor2_EmptyArrayAsProvidersArgument_ShouldSetProvidersPropertyToEmpty()
        {
            var provider = new AggregateExportProvider((IEnumerable<ExportProvider>)new ExportProvider[0]);

            EnumerableAssert.IsEmpty(provider.Providers);
        }

        [TestMethod]
        public void Constructor2_EmptyEnumerableAsProvidersArgument_ShouldSetProvidersPropertyToEmpty()
        {
            var provider = new AggregateExportProvider(Enumerable.Empty<ExportProvider>());

            EnumerableAssert.IsEmpty(provider.Providers);
        }

        [TestMethod]
        public void Constructor1_ArrayAsProvidersArgument_ShouldNotAllowModificationAfterConstruction()
        {
            var providers = new ExportProvider[] { ExportProviderFactory.Create() };
            var provider = new AggregateExportProvider(providers);

            providers[0] = null;

            Assert.IsNotNull(provider.Providers[0]);
        }

        [TestMethod]
        public void Constructor2_ArrayAsProvidersArgument_ShouldNotAllowModificationAfterConstruction()
        {
            var providers = new ExportProvider[] { ExportProviderFactory.Create() };
            var provider = new AggregateExportProvider((IEnumerable<ExportProvider>)providers);

            providers[0] = null;

            Assert.IsNotNull(provider.Providers[0]);
        }

        [TestMethod]
        public void Providers_WhenDisposed_ShouldThrowObjectDisposed()
        {
            var provider = CreateAggregateExportProvider();
            provider.Dispose();

            ExceptionAssert.ThrowsDisposed(provider, () =>
            {
                var providers = provider.Providers;
            });
        }

        private AggregateExportProvider CreateAggregateExportProvider()
        {
            return new AggregateExportProvider(Enumerable.Empty<ExportProvider>());
        }
    }
}
