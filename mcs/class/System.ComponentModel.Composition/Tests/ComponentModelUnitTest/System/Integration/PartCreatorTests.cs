// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.AttributedModel;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.Factories;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.UnitTesting;
using System.Linq;
using System.Reflection;
using System.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.ComponentModel.Composition.ReflectionModel;

#if SILVERLIGHT

namespace Tests.Integration
{
    [TestClass]
    public class PartCreatorTests
    {
        public interface IId
        {
            int Id { get; }
        }

        public interface IIdTypeMetadata
        {
            string IdType { get; }
            string ExportTypeIdentity { get; }
        }


        [Export(typeof(IId))]
        [ExportMetadata("IdType", "PostiveIncrement")]
        public class UniqueExport : IId, IDisposable
        {
            private static int lastId = 0;

            public UniqueExport()
            {
                Id = lastId++;
            }

            public int Id { get; private set; }

            public void Dispose()
            {
                Id = -1;
            }
        }

        [Export]
        [CLSCompliant(false)]
        public class PartCreatorImporter
        {
            [ImportingConstructor]
            public PartCreatorImporter(
                PartCreator<IId> idCreatorTCtor, 
                PartCreator<IId, IIdTypeMetadata> idCreatorTMCtor)
            {
                this._idCreatorTCtor = idCreatorTCtor;
                this._idCreatorTMCtor = idCreatorTMCtor;
            }

            private PartCreator<IId> _idCreatorTCtor;
            private PartCreator<IId, IIdTypeMetadata> _idCreatorTMCtor;

            [Import(typeof(IId))]
            public PartCreator<IId> _idCreatorTField = null; // public so these can work on SL

            [Import]
            public PartCreator<IId, IIdTypeMetadata> _idCreatorTMField = null; // public so these can work on SL

            [Import]
            public PartCreator<IId> IdCreatorTProperty { get; set; }

            [Import(typeof(IId))]
            public PartCreator<IId, IIdTypeMetadata> IdCreatorTMProperty { get; set; }

            [ImportMany]
            public PartCreator<IId>[] IdCreatorsTProperty { get; set; }

            [ImportMany]
            public PartCreator<IId, IIdTypeMetadata>[] IdCreatorsTMProperty { get; set; }

            public void AssertValid()
            {
                var ids = new int[] 
                {
                    VerifyPartCreator(this._idCreatorTCtor),
                    VerifyPartCreator(this._idCreatorTMCtor),
                    VerifyPartCreator(this._idCreatorTField),
                    VerifyPartCreator(this._idCreatorTMField),
                    VerifyPartCreator(this.IdCreatorTProperty),
                    VerifyPartCreator(this.IdCreatorTMProperty),
                    VerifyPartCreator(this.IdCreatorsTProperty[0]),
                    VerifyPartCreator(this.IdCreatorsTMProperty[0])
                };

                Assert.AreEqual(1, this.IdCreatorsTProperty.Length, "Should only be one PartCreator");
                Assert.AreEqual(1, this.IdCreatorsTMProperty.Length, "Should only be one PartCreator");

                CollectionAssert.AllItemsAreUnique(ids, "There should be no duplicate ids");
            }

            private int VerifyPartCreator(PartCreator<IId> creator)
            {
                var val1 = creator.CreatePart();
                var val2 = creator.CreatePart();

                Assert.AreNotEqual(val1.ExportedValue, val2.ExportedValue, "Values should not be the same");
                Assert.AreNotEqual(val1.ExportedValue.Id, val2.ExportedValue.Id, "Value Ids should not be the same");

                Assert.IsTrue(val1.ExportedValue.Id >= 0, "Id should be positive");

                val1.Dispose();

                Assert.IsTrue(val1.ExportedValue.Id < 0, "Disposal of the value should set the id to negative");

                return creator.CreatePart().ExportedValue.Id;
            }

            private int VerifyPartCreator(PartCreator<IId, IIdTypeMetadata> creator)
            {
                var val = VerifyPartCreator((PartCreator<IId>)creator);

                Assert.AreEqual("PostiveIncrement", creator.Metadata.IdType, "IdType should be PositiveIncrement");
                Assert.AreEqual(AttributedModelServices.GetTypeIdentity(typeof(ComposablePartDefinition)), creator.Metadata.ExportTypeIdentity);

                return val;
            }
        }

        [TestMethod]
        public void PartCreatorStandardImports_ShouldWorkProperly()
        {
            var container = ContainerFactory.CreateWithAttributedCatalog(typeof(UniqueExport), typeof(PartCreatorImporter));
            var partCreatorImporter = container.GetExportedValue<PartCreatorImporter>();

            partCreatorImporter.AssertValid();
        }

        [Export]
        public class Foo : IDisposable
        {
            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                this.IsDisposed = true;
            }
        }

        [Export]
        public class SimplePartCreatorImporter
        {
            [Import]
            public PartCreator<Foo> FooFactory { get; set; }
        }

        [TestMethod]
        public void PartCreatorOfT_RecompositionSingle_ShouldBlockChanges()
        {
            var aggCat = new AggregateCatalog();
            var typeCat = new TypeCatalog(typeof(Foo));
            aggCat.Catalogs.Add(new TypeCatalog(typeof(SimplePartCreatorImporter)));
            aggCat.Catalogs.Add(typeCat);

            var container = new CompositionContainer(aggCat);

            var fooFactory = container.GetExportedValue<SimplePartCreatorImporter>();

            ExceptionAssert.Throws<ChangeRejectedException>(() =>
                aggCat.Catalogs.Remove(typeCat));

            ExceptionAssert.Throws<ChangeRejectedException>(() =>
                aggCat.Catalogs.Add(new TypeCatalog(typeof(Foo))));
        }

        [Export]
        public class ManyPartCreatorImporter
        {
            [ImportMany(AllowRecomposition = true)]
            public PartCreator<Foo>[] FooFactories { get; set; }
        }

        [TestMethod]
        public void FactoryOfT_RecompositionImportMany_ShouldSucceed()
        {
            var aggCat = new AggregateCatalog();
            var typeCat = new TypeCatalog(typeof(Foo));
            aggCat.Catalogs.Add(new TypeCatalog(typeof(ManyPartCreatorImporter)));
            aggCat.Catalogs.Add(typeCat);

            var container = new CompositionContainer(aggCat);

            var fooFactories = container.GetExportedValue<ManyPartCreatorImporter>();

            Assert.AreEqual(1, fooFactories.FooFactories.Length);

            aggCat.Catalogs.Add(new TypeCatalog(typeof(Foo)));

            Assert.AreEqual(2, fooFactories.FooFactories.Length);
        }

        public class PartCreatorExplicitCP
        {
            [Import(RequiredCreationPolicy = CreationPolicy.Any)]
            public PartCreator<Foo> FooCreatorAny { get; set; }

            [Import(RequiredCreationPolicy = CreationPolicy.NonShared)]
            public PartCreator<Foo> FooCreatorNonShared { get; set; }

            [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
            public PartCreator<Foo> FooCreatorShared { get; set; }

            [ImportMany(RequiredCreationPolicy = CreationPolicy.Any)]
            public PartCreator<Foo>[] FooCreatorManyAny { get; set; }

            [ImportMany(RequiredCreationPolicy = CreationPolicy.NonShared)]
            public PartCreator<Foo>[] FooCreatorManyNonShared { get; set; }

            [ImportMany(RequiredCreationPolicy = CreationPolicy.Shared)]
            public PartCreator<Foo>[] FooCreatorManyShared { get; set; }
        }

        [TestMethod]
        public void PartCreator_ExplicitCreationPolicy_CPShouldBeIgnored()
        {
            var container = ContainerFactory.CreateWithAttributedCatalog(typeof(Foo));

            var part = new PartCreatorExplicitCP();

            container.SatisfyImportsOnce(part);

            // specifying the required creation policy explicit on the import 
            // of a PartCreator will be ignored because the PartCreator requires
            // the part it wraps to be either Any or NonShared to work properly.
            Assert.IsNotNull(part.FooCreatorAny);
            Assert.IsNotNull(part.FooCreatorNonShared);
            Assert.IsNotNull(part.FooCreatorShared);

            Assert.AreEqual(1, part.FooCreatorManyAny.Length);
            Assert.AreEqual(1, part.FooCreatorManyNonShared.Length);
            Assert.AreEqual(1, part.FooCreatorManyShared.Length);
        }

        public class PartCreatorImportRequiredMetadata
        {
            [ImportMany]
            public PartCreator<Foo>[] FooCreator { get; set; }

            [ImportMany]
            public PartCreator<Foo, IIdTypeMetadata>[] FooCreatorWithMetadata { get; set; }
        }

        [TestMethod]
        public void PartCreator_ImportRequiredMetadata_MissingMetadataShouldCauseImportToBeExcluded()
        {
            var container = ContainerFactory.CreateWithAttributedCatalog(typeof(Foo));

            var part = new PartCreatorImportRequiredMetadata();

            container.SatisfyImportsOnce(part);

            Assert.AreEqual(1, part.FooCreator.Length, "Should contain the one Foo");
            Assert.AreEqual(0, part.FooCreatorWithMetadata.Length, "Should NOT contain Foo because it is missing the required Id metadata property");
        }

        [Export(typeof(Foo))]
        [PartCreationPolicy(CreationPolicy.Shared)]
        public class SharedFoo : Foo
        {
        }

        [TestMethod]
        public void PartCreator_ImportShouldNotImportSharedPart()
        {
            var container = ContainerFactory.CreateWithAttributedCatalog(typeof(SharedFoo));

            var foo = container.GetExportedValue<Foo>();
            Assert.IsNotNull(foo, "Ensure that a Foo actually exists in the container");

            var part = new PartCreatorImportRequiredMetadata();

            container.SatisfyImportsOnce(part);

            Assert.AreEqual(0, part.FooCreator.Length, "Should not contain the SharedFoo because the PartCreator should only wrap Any/NonShared parts");
        }


        [TestMethod]
        public void PartCreator_QueryContainerDirectly_ShouldWork()
        {
            var container = ContainerFactory.CreateWithAttributedCatalog(typeof(Foo));

            var importDef = ReflectionModelServices.CreateImportDefinition(
                new LazyMemberInfo(MemberTypes.Field, () => new MemberInfo[] { typeof(PartCreatorTests) }), // Give it a bogus member
                AttributedModelServices.GetContractName(typeof(Foo)),
                AttributedModelServices.GetTypeIdentity(typeof(Foo)),
                Enumerable.Empty<KeyValuePair<string, Type>>(),
                ImportCardinality.ZeroOrMore,
                true,
                CreationPolicy.Any,
                true, // isPartCreator
                null);

            var exports = container.GetExports(importDef);

            var partCreator = exports.Single();

            // Manually walk the steps of using a raw part creator which is modeled as a PartDefinition with
            // a single ExportDefinition.
            var partDef = (ComposablePartDefinition)partCreator.Value;
            var part = partDef.CreatePart();
            var foo = (Foo)part.GetExportedValue(partDef.ExportDefinitions.Single());

            Assert.IsNotNull(foo);

            var foo1 = (Foo)part.GetExportedValue(partDef.ExportDefinitions.Single());
            Assert.AreEqual(foo, foo1, "Retrieving the exported value from the same part should return the same value");

            // creating a new part should result in getting a new exported value
            var part2 = partDef.CreatePart();
            var foo2 = (Foo)part2.GetExportedValue(partDef.ExportDefinitions.Single());

            Assert.AreNotEqual(foo, foo2, "New part should equate to a new exported value");

            // Disposing of part should cause foo to be disposed
            ((IDisposable)part).Dispose();
            Assert.IsTrue(foo.IsDisposed);
        }

        [Export]
        public class PartImporter<PartType>
        {
            [Import]
            public PartCreator<PartType> Creator { get; set; }
        }

        [Export]
        public class SimpleExport
        {
        }

        [TestMethod]
        public void PartCreator_SimpleRejectionRecurrection_ShouldWork()
        {
            var importTypeCat = new TypeCatalog(typeof(PartImporter<SimpleExport>));
            var aggCatalog = new AggregateCatalog(importTypeCat);
            var container = ContainerFactory.Create(aggCatalog);
            var exports = container.GetExports<PartImporter<SimpleExport>>();
            Assert.AreEqual(0, exports.Count());

            aggCatalog.Catalogs.Add(new TypeCatalog(typeof(SimpleExport)));

            exports = container.GetExports<PartImporter<SimpleExport>>();
            Assert.AreEqual(1, exports.Count());
        }
    }
}

#endif
