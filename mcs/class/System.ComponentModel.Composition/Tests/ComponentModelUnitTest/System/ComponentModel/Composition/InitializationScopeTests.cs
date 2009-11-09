// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Factories;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ComponentModel.Composition
{
    [TestClass]
    public class InitializationScopeTests
    {
        [TestMethod]
        public void SingleContainerSimpleCompose()
        {
            var container = ContainerFactory.Create();
            ImportingComposablePart importPart;
            CompositionBatch batch = new CompositionBatch();

            batch.AddExportedValue("value1", "Hello");
            batch.AddExportedValue("value2", "World");
            batch.AddPart(importPart = PartFactory.CreateImporter("value1", "value2"));
            container.Compose(batch);

            Assert.AreEqual(2, importPart.ImportSatisfiedCount);
            Assert.AreEqual("Hello", importPart.GetImport("value1"));
            Assert.AreEqual("World", importPart.GetImport("value2"));
        }

        [TestMethod]
        public void ParentedContainerSimpleCompose()
        {
            var container = ContainerFactory.Create();
            var importPart = PartFactory.CreateImporter("value1", "value2");

            CompositionBatch batch = new CompositionBatch();
            batch.AddExportedValue("value1", "Parent");

            var childContainer = new CompositionContainer(container);
            CompositionBatch childBatch = new CompositionBatch();
            childBatch.AddExportedValue("value2", "Child");
            childBatch.AddPart(importPart);

            Assert.AreEqual(0, importPart.ImportSatisfiedCount, "Import should not happen until outer scope is disposed");

            container.Compose(batch);
            childContainer.Compose(childBatch);

            Assert.AreEqual(2, importPart.ImportSatisfiedCount);
            Assert.AreEqual("Parent", importPart.GetImport("value1"));
            Assert.AreEqual("Child", importPart.GetImport("value2"));
        }

        [TestMethod]
        public void SingleContainerPartReplacement()
        {
            var container = ContainerFactory.Create();
            var importPart = PartFactory.CreateImporter(true, "value1", "value2");

            CompositionBatch batch = new CompositionBatch();
            var export1Key = batch.AddExportedValue("value1", "Hello");
            batch.AddExportedValue("value2", "World");
            batch.AddPart(importPart);
            container.Compose(batch);

            Assert.AreEqual(2, importPart.ImportSatisfiedCount);
            Assert.AreEqual("Hello", importPart.GetImport("value1"));
            Assert.AreEqual("World", importPart.GetImport("value2"));

            importPart.ResetImportSatisfiedCount();

            batch = new CompositionBatch();
            batch.RemovePart(export1Key);
            batch.AddExportedValue("value1", "Goodbye");
            container.Compose(batch);

            Assert.AreEqual(1, importPart.ImportSatisfiedCount);
            Assert.AreEqual("Goodbye", importPart.GetImport("value1"));
            Assert.AreEqual("World", importPart.GetImport("value2"));
        }

        [TestMethod]
        public void ParentedContainerPartReplacement()
        {
            var container = ContainerFactory.Create();
            CompositionBatch batch = new CompositionBatch();
            var importPart = PartFactory.CreateImporter(true, "value1", "value2");
            var exportKey = batch.AddExportedValue("value1", "Parent");

            var childContainer = new CompositionContainer(container);
            CompositionBatch childBatch = new CompositionBatch();
            childBatch.AddExportedValue("value2", "Child");
            childBatch.AddPart(importPart);

            Assert.AreEqual(0, importPart.ImportSatisfiedCount, "Should not import until outer scope is disposed");
            container.Compose(batch);
            childContainer.Compose(childBatch);

            Assert.AreEqual(2, importPart.ImportSatisfiedCount);
            Assert.AreEqual("Parent", importPart.GetImport("value1"));
            Assert.AreEqual("Child", importPart.GetImport("value2"));

            importPart.ResetImportSatisfiedCount();
            batch = new CompositionBatch();
            batch.RemovePart(exportKey);
            batch.AddExportedValue("value1", "New Parent");
            container.Compose(batch);

            Assert.AreEqual(1, importPart.ImportSatisfiedCount);
            Assert.AreEqual("New Parent", importPart.GetImport("value1"));
            Assert.AreEqual("Child", importPart.GetImport("value2"));
        }

        [TestMethod]
        public void SelectiveRecompose()
        {
            var container = ContainerFactory.Create();
            var stableImporter = PartFactory.CreateImporter("stable");
            var dynamicImporter = PartFactory.CreateImporter("dynamic", true);
            CompositionBatch batch = new CompositionBatch();

            batch.AddPart(stableImporter);
            batch.AddPart(dynamicImporter);
            var exportKey = batch.AddExportedValue("dynamic", 1);
            batch.AddExportedValue("stable", 42);
            container.Compose(batch);

            Assert.AreEqual(1, stableImporter.ImportSatisfiedCount);
            Assert.AreEqual(stableImporter.GetImport("stable"), 42);
            Assert.AreEqual(1, dynamicImporter.ImportSatisfiedCount);
            Assert.AreEqual(dynamicImporter.GetImport("dynamic"), 1);

            batch = new CompositionBatch();
            stableImporter.ResetImportSatisfiedCount();
            dynamicImporter.ResetImportSatisfiedCount();
            batch.RemovePart(exportKey);
            batch.AddExportedValue("dynamic", 2);
            container.Compose(batch);

            Assert.AreEqual(0, stableImporter.ImportSatisfiedCount, "Should not have imported the stable import part");
            Assert.AreEqual(stableImporter.GetImport("stable"), 42);
            Assert.AreEqual(1, dynamicImporter.ImportSatisfiedCount);
            Assert.AreEqual(dynamicImporter.GetImport("dynamic"), 2);
        }
    }
}
