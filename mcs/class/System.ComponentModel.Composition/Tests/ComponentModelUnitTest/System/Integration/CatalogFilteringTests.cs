// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Factories;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ComponentModel.Composition
{
    [TestClass]
    public class CatalogFilteringTests
    {
        [TestMethod]
        public void FilteredCatalog_ScopeA()
        {
            var cat = GetCatalog();
            var contA = new CompositionContainer(ScopeCatalog(cat, "A"));

            Assert.IsTrue(contA.IsPresent<ScopeAComponent1>());
            Assert.IsTrue(contA.IsPresent<ScopeAComponent2>());
            Assert.IsFalse(contA.IsPresent<ScopeBComponent>());
            Assert.IsFalse(contA.IsPresent<ScopeCComponent>());
        }

        [TestMethod]
        public void FilteredCatalog_ScopeB()
        {
            var cat = GetCatalog();
            var contA = new CompositionContainer(ScopeCatalog(cat, "A"));
            var contB = new CompositionContainer(ScopeCatalog(cat, "B"), contA);

            Assert.IsTrue(contB.IsPresent<ScopeAComponent1>());
            Assert.IsTrue(contB.IsPresent<ScopeAComponent2>());
            Assert.IsTrue(contB.IsPresent<ScopeBComponent>());
            Assert.IsFalse(contB.IsPresent<ScopeCComponent>());
        }

        [TestMethod]
        public void FilteredCatalog_ScopeC()
        {
            var cat = GetCatalog();
            var contA = new CompositionContainer(ScopeCatalog(cat, "A"));
            var contB = new CompositionContainer(ScopeCatalog(cat, "B"), contA);
            var contC = new CompositionContainer(ScopeCatalog(cat, "C"), contB);

            Assert.IsTrue(contC.IsPresent<ScopeAComponent1>());
            Assert.IsTrue(contC.IsPresent<ScopeAComponent2>());
            Assert.IsTrue(contC.IsPresent<ScopeBComponent>());
            Assert.IsTrue(contC.IsPresent<ScopeCComponent>());
        }

        [TestMethod]
        [Ignore]
        [WorkItem(812029)]
        public void FilteredCatalog_EventsFired()
        {
            var aggCatalog = CatalogFactory.CreateAggregateCatalog();
            var cat1 = CatalogFactory.CreateAttributed(typeof(ScopeAComponent1), typeof(ScopeBComponent));

            var filteredCatalog = CatalogFactory.CreateFiltered(aggCatalog, 
                partDef => partDef.Metadata.ContainsKey("Scope") &&
                                    partDef.Metadata["Scope"].ToString() == "A");

            var container = ContainerFactory.Create(filteredCatalog);

            Assert.IsFalse(container.IsPresent<ScopeAComponent1>(), "sa before add");
            Assert.IsFalse(container.IsPresent<ScopeBComponent>(), "sb before add");

            aggCatalog.Catalogs.Add(cat1);

            Assert.IsTrue(container.IsPresent<ScopeAComponent1>(), "sa after add");
            Assert.IsFalse(container.IsPresent<ScopeBComponent>(), "sb after add");

            aggCatalog.Catalogs.Remove(cat1);

            Assert.IsFalse(container.IsPresent<ScopeAComponent1>(), "sa after remove");
            Assert.IsFalse(container.IsPresent<ScopeBComponent>(), "sb after remove");
        }

        private ComposablePartCatalog GetCatalog()
        {
            return CatalogFactory.CreateAttributed(
                typeof(ScopeAComponent1),
                typeof(ScopeAComponent2),
                typeof(ScopeBComponent),
                typeof(ScopeCComponent));
        }

        private ComposablePartCatalog ScopeCatalog(ComposablePartCatalog catalog, string scope)
        {
            return CatalogFactory.CreateFiltered(catalog,
                         partDef => partDef.Metadata.ContainsKey("Scope") &&
                                    partDef.Metadata["Scope"].ToString() == scope);
        }

        [Export]
        [PartMetadata("Scope", "A")]
        public class ScopeAComponent1
        {
        }

        [Export]
        [PartMetadata("Scope", "A")]
        public class ScopeAComponent2
        {
            [Import]
            public ScopeAComponent1 ScopeA { get; set; }
        }

        [Export]
        [PartMetadata("Scope", "B")]
        public class ScopeBComponent
        {
            [Import]
            public ScopeAComponent1 ScopeA { get; set; }
        }

        [Export]
        [PartMetadata("Scope", "C")]
        public class ScopeCComponent
        {
            [Import]
            public ScopeBComponent ScopeB { get; set; }
        }
    }
}
