// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.Composition.Factories;
using System.UnitTesting;
using System.Linq;
using System.ComponentModel.Composition.Primitives;

namespace Tests.Integration
{
    [TestClass]
    public class RecompositionTests
    {
        public class Class_OptIn_AllowRecompositionImports
        {
            [Import("Value", AllowRecomposition = true)]
            public int Value { get; set; }
        }

        [TestMethod]
        public void Import_OptIn_AllowRecomposition()
        {
            var container = new CompositionContainer();
            var importer = new Class_OptIn_AllowRecompositionImports();

            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(importer);
            var valueKey = batch.AddExportedValue("Value", 21);
            container.Compose(batch);

            // Initial compose Value should be 21
            Assert.AreEqual(21, importer.Value);

            // Recompose Value to be 42
            batch = new CompositionBatch();
            batch.RemovePart(valueKey);
            batch.AddExportedValue("Value", 42);
            container.Compose(batch);

            Assert.AreEqual(42, importer.Value, "Value should have changed!");
        }

        public class Class_OptOut_AllowRecompositionImports
        {
            [Import("Value", AllowRecomposition = false)]
            public int Value { get; set; }
        }

        [TestMethod]
        public void Import_OptOut_AllowRecomposition()
        {
            var container = new CompositionContainer();
            var importer = new Class_OptOut_AllowRecompositionImports();

            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(importer);
            var valueKey = batch.AddExportedValue("Value", 21);
            container.Compose(batch);

            // Initial compose Value should be 21
            Assert.AreEqual(21, importer.Value);

            // Reset value to ensure it doesn't get set to same value again
            importer.Value = -21; 

            // Recompose Value to be 42
            batch = new CompositionBatch();
            batch.RemovePart(valueKey);
            batch.AddExportedValue("Value", 42);
            // After rejection batch failures throw ChangeRejectedException to indicate that
            // the failure did not affect the container
            CompositionAssert.ThrowsChangeRejectedError(ErrorId.ImportEngine_PreventedByExistingImport, () =>
            {
                container.Compose(batch);
            });

            Assert.AreEqual(-21, importer.Value, "Value should NOT have changed!");
        }

        public class Class_Default_AllowRecompositionImports
        {
            [Import("Value")]
            public int Value { get; set; }
        }

        [TestMethod]
        public void Import_Default_AllowRecomposition()
        {
            var container = new CompositionContainer();
            var importer = new Class_Default_AllowRecompositionImports();

            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(importer);
            var valueKey = batch.AddExportedValue("Value", 21);
            container.Compose(batch);

            // Initial compose Value should be 21
            Assert.AreEqual(21, importer.Value);

            // Reset value to ensure it doesn't get set to same value again
            importer.Value = -21; 

            // Recompose Value to be 42
            batch = new CompositionBatch();
            batch.RemovePart(valueKey);
            batch.AddExportedValue("Value", 42);
            // After rejection batch failures throw ChangeRejectedException to indicate that
            // the failure did not affect the container
            CompositionAssert.ThrowsChangeRejectedError(ErrorId.ImportEngine_PreventedByExistingImport, () =>
            {
                container.Compose(batch);
            });

            Assert.AreEqual(-21, importer.Value, "Value should NOT have changed!");
        }

        public class Class_BothOptInAndOptOutRecompositionImports
        {
            [Import("Value", AllowRecomposition = true)]
            public int RecomposableValue { get; set; }

            [Import("Value", AllowRecomposition = false)]
            public int NonRecomposableValue { get; set; }
        }

        [TestMethod]
        public void Import_BothOptInAndOptOutRecomposition()
        {
            var container = new CompositionContainer();
            var importer = new Class_BothOptInAndOptOutRecompositionImports();

            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(importer);
            var valueKey = batch.AddExportedValue("Value", 21);
            container.Compose(batch);

            // Initial compose values should be 21
            Assert.AreEqual(21, importer.RecomposableValue);
            Assert.AreEqual(21, importer.NonRecomposableValue);

            // Reset value to ensure it doesn't get set to same value again
            importer.NonRecomposableValue = -21;
            importer.RecomposableValue = -21;

            // Recompose Value to be 42
            batch = new CompositionBatch();
            batch.RemovePart(valueKey);
            batch.AddExportedValue("Value", 42);
            // After rejection batch failures throw ChangeRejectedException to indicate that
            // the failure did not affect the container
            CompositionAssert.ThrowsChangeRejectedError(ErrorId.ImportEngine_PreventedByExistingImport, () =>
            {
                container.Compose(batch);
            });

            Assert.AreEqual(-21, importer.NonRecomposableValue, "Value should NOT have changed!");
            // The batch rejection means that the recomposable value shouldn't change either
            Assert.AreEqual(-21, importer.RecomposableValue, "Value should NOT have changed!");
        }

        public class Class_MultipleOptInRecompositionImportsWithDifferentContracts
        {
            [Import("Value1", AllowRecomposition = true)]
            public int Value1 { get; set; }

            [Import("Value2", AllowRecomposition = true)]
            public int Value2 { get; set; }
        }

        [TestMethod]
        public void Import_OptInRecomposition_Multlple()
        {
            var container = new CompositionContainer();
            var importer = new Class_MultipleOptInRecompositionImportsWithDifferentContracts();

            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(importer);
            var value1Key = batch.AddExportedValue("Value1", 21);
            var value2Key = batch.AddExportedValue("Value2", 23);
            container.Compose(batch);

            Assert.AreEqual(21, importer.Value1);
            Assert.AreEqual(23, importer.Value2);

            // Reset value to ensure it doesn't get set to same value again
            importer.Value1 = -21;
            importer.Value2 = -23;

            // Recompose Value to be 42
            batch = new CompositionBatch();
            batch.RemovePart(value1Key);
            batch.AddExportedValue("Value1", 42);
            container.Compose(batch);

            Assert.AreEqual(42, importer.Value1, "Value should have changed!");
            Assert.AreEqual(-23, importer.Value2, "Value should NOT have changed because Value2 contract should not be updated.");
        }

        [PartNotDiscoverable]
        public class MyName
        {
            public MyName(string name)
            {
                this.Name = name;
            }

            [Export("Name")]
            public string Name { get; private set; }
        }

        [PartNotDiscoverable]
        public class Spouse
        {
            public Spouse(string name)
            {
                this.Name = name;
            }

            [Export("Friend")]
            [ExportMetadata("Relationship", "Wife")]
            public string Name { get; private set; }
        }

        [PartNotDiscoverable]
        public class Child
        {
            public Child(string name)
            {
                this.Name = name;
            }

            [Export("Child")]
            public string Name { get; private set; }
        }

        [PartNotDiscoverable]
        public class Job
        {
            public Job(string name)
            {
                this.Name = name;
            }

            [Export("Job")]
            public string Name { get; private set; }
        }

        [PartNotDiscoverable]
        public class Friend
        {
            public Friend(string name)
            {
                this.Name = name;
            }

            [Export("Friend")]
            public string Name { get; private set; }
        }

        public interface IRelationshipView
        {
            string Relationship { get; }
        }

        [PartNotDiscoverable]
        public class Me
        {
            [Import("Name", AllowRecomposition = true)]
            public string Name { get; set; }

            [Import("Job", AllowDefault = true, AllowRecomposition = true)]
            public string Job { get; set; }

            [ImportMany("Child")]
            public string[] Children { get; set; }

            [ImportMany("Friend")]
            public Lazy<string, IRelationshipView>[] Relatives { get; set; }

            [ImportMany("Friend", AllowRecomposition = true)]
            public string[] Friends { get; set; }
        }

        [TestMethod]
        public void Recomposition_IntegrationTest()
        {
            var container = new CompositionContainer();
            var batch = new CompositionBatch();

            var me = new Me();
            batch.AddPart(me);
            var namePart = batch.AddPart(new MyName("Blake"));
            batch.AddPart(new Spouse("Barbara"));
            batch.AddPart(new Friend("Steve"));
            batch.AddPart(new Friend("Joyce"));
            container.Compose(batch);
            Assert.AreEqual(me.Name, "Blake", "Name in initial composition incorrect");
            Assert.AreEqual(me.Job, null, "Job should have the default value");
            Assert.AreEqual(me.Friends.Length, 3, "Number of friends in initial composition incorrect");
            Assert.AreEqual(me.Relatives.Length, 1, "Number of relatives in initial composition incorrect");
            Assert.AreEqual(me.Children.Length, 0, "Number of children in initial composition incorrect");

            // Can only have one name
            ExceptionAssert.Throws<ChangeRejectedException>(() =>
                container.ComposeParts(new MyName("Blayke")));

            batch = new CompositionBatch();
            batch.AddPart(new MyName("Blayke"));
            batch.RemovePart(namePart);
            container.Compose(batch);
            Assert.AreEqual(me.Name, "Blayke", "Name after recomposition incorrect");

            batch = new CompositionBatch();
            var jobPart = batch.AddPart(new Job("Architect"));
            container.Compose(batch);
            Assert.AreEqual(me.Job, "Architect", "Job after recomposition incorrect");

            batch = new CompositionBatch();
            batch.AddPart(new Job("Chimney Sweep"));
            container.Compose(batch);
            Assert.IsTrue(me.Job == null, "More than one of an optional import should result in the default value");

            batch = new CompositionBatch();
            batch.RemovePart(jobPart);
            container.Compose(batch);
            Assert.AreEqual(me.Job, "Chimney Sweep", "Job after re-recomposition incorrect");

            batch = new CompositionBatch();

            // Can only have one spouse because they aren't recomposable
            ExceptionAssert.Throws<ChangeRejectedException>(() =>
                container.ComposeParts(new Spouse("Cameron")));

            Assert.AreEqual(me.Relatives.Length, 1, "Number of relatives shouldn't be affected by rolled back composition");

            batch = new CompositionBatch();
            batch.AddPart(new Friend("Graham"));
            container.Compose(batch);
            Assert.AreEqual(me.Friends.Length, 4, "Number of friends after recomposition incorrect");
            Assert.AreEqual(me.Relatives.Length, 1, "Number of relatives shouldn't be affected by rolled back composition");
        }

        public class FooWithOptionalImport
        {
            private FooWithSimpleImport _optionalImport;

            [Import(AllowDefault=true, AllowRecomposition=true)]
            public FooWithSimpleImport OptionalImport
            {
                get
                {
                    return this._optionalImport;
                }
                set
                {
                    if (value != null)
                    {
                        this._optionalImport = value;

                        Assert.IsTrue(!string.IsNullOrEmpty(this._optionalImport.SimpleValue), "Value should have it's imports satisfied");
                    }
                }
            }
        }

        [Export]
        public class FooWithSimpleImport
        {
            [Import("FooSimpleImport")]
            public string SimpleValue { get; set; }
        }

        [TestMethod]
        public void PartsShouldHaveImportsSatisfiedBeforeBeingUsedToSatisfyRecomposableImports()
        {
            var container = new CompositionContainer();
            var fooOptional = new FooWithOptionalImport();
            
            container.ComposeParts(fooOptional);
            container.ComposeExportedValue<string>("FooSimpleImport", "NotNullOrEmpty");
            container.ComposeParts(new FooWithSimpleImport());

            Assert.IsTrue(!string.IsNullOrEmpty(fooOptional.OptionalImport.SimpleValue));
        }


        [Export]
        public class RootImportRecomposable
        {
            [Import(AllowDefault = true, AllowRecomposition = true)]
            public NonSharedImporter Importer { get; set; }
        }

        [Export]
        [PartCreationPolicy(CreationPolicy.NonShared)]
        public class NonSharedImporter
        {
            [Import]
            public SimpleImport Import { get; set; }
        }

        [Export]
        public class RootImporter
        {
            [Import]
            public SimpleImport Import { get; set; }
        }

        [Export]
        public class SimpleImport
        {
            public int Property { get { return 42; } }
        }

        [TestMethod]
        [Ignore]
        [WorkItem(733533)]
        public void RemoveCatalogWithNonSharedPartWithRequiredImport()
        {
            var typeCatalog = new TypeCatalog(typeof(NonSharedImporter), typeof(SimpleImport));
            var aggCatalog = new AggregateCatalog();
            var container = new CompositionContainer(aggCatalog);

            aggCatalog.Catalogs.Add(typeCatalog);
            aggCatalog.Catalogs.Add(new TypeCatalog(typeof(RootImportRecomposable)));

            var rootExport = container.GetExport<RootImportRecomposable>();
            var root = rootExport.Value;

            Assert.AreEqual(42, root.Importer.Import.Property);

            aggCatalog.Catalogs.Remove(typeCatalog);

            Assert.IsNull(root.Importer);            
        }

        [TestMethod]
        [Ignore]
        [WorkItem(734123)]
        public void GetExportResultShouldBePromise()
        {
            var typeCatalog = new TypeCatalog(typeof(RootImporter), typeof(SimpleImport));
            var aggCatalog = new AggregateCatalog();
            var container = new CompositionContainer(aggCatalog);

            aggCatalog.Catalogs.Add(typeCatalog);

            var root = container.GetExport<RootImporter>();

            ExceptionAssert.Throws<ChangeRejectedException>(() =>
                aggCatalog.Catalogs.Remove(typeCatalog)
            );

            var value = root.Value;
            Assert.AreEqual(42, value.Import.Property);
        }

        [TestMethod]
        [WorkItem(789269)]
        public void TestRemovingAndReAddingMultipleDefinitionsFromCatalog()
        {
            var fixedParts = new TypeCatalog(typeof(RootMultipleImporter), typeof(ExportedService));
            var changingParts = new TypeCatalog(typeof(Exporter1), typeof(Exporter2));
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(fixedParts);
            catalog.Catalogs.Add(changingParts);

            var container = new CompositionContainer(catalog);

            var root = container.GetExport<RootMultipleImporter>().Value;

            Assert.AreEqual(2, root.Imports.Length);

            catalog.Catalogs.Remove(changingParts);

            Assert.AreEqual(0, root.Imports.Length);

            catalog.Catalogs.Add(changingParts);

            Assert.AreEqual(2, root.Imports.Length);
        }

        [Export]
        public class RootMultipleImporter
        {
            [ImportMany(AllowRecomposition=true)]
            public IExportedInterface[] Imports { get; set; }
        }

        public interface IExportedInterface
        {

        }

        [Export(typeof(IExportedInterface))]
        public class Exporter1 : IExportedInterface
        {
            [Import]
            public ExportedService Service { get; set; }
        }

        [Export(typeof(IExportedInterface))]
        public class Exporter2 : IExportedInterface
        {
            [Import]
            public ExportedService Service { get; set; }
        }

        [Export]
        public class ExportedService
        {

        }

        [TestMethod]
        [WorkItem(762215)]
        [Ignore]
        public void TestPartCreatorResurrection()
        {
            var container = new CompositionContainer(new TypeCatalog(typeof(NonDisposableImportsDisposable), typeof(PartImporter<NonDisposableImportsDisposable>)));
            var exports = container.GetExports<PartImporter<NonDisposableImportsDisposable>>();
            Assert.AreEqual(0, exports.Count());
            container.ComposeParts(new DisposablePart());
            exports = container.GetExports<PartImporter<NonDisposableImportsDisposable>>();
            Assert.AreEqual(1, exports.Count());
        }

        [Export]
        public class PartImporter<PartType>
        {
            [Import]
            public PartType Creator { get; set; }
        }

        [Export]
        public class NonDisposableImportsDisposable
        {
            [Import]
            public DisposablePart Part { get; set; }
        }

        [Export]
        public class Part
        {

        }
        [Export]
        [PartCreationPolicy(CreationPolicy.NonShared)]
        public class DisposablePart : Part, IDisposable
        {
            public bool Disposed { get; private set; }
            public void Dispose()
            {
                Disposed = true;
            }
        }

    }
}
