// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.UnitTesting;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Factories;
using System.ComponentModel.Composition.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.Composition.Primitives;

namespace System.ComponentModel.Composition
{
    [TestClass]
    public class ImportEngineTests
    {
        [TestMethod]
        public void PreviewImports_Successful_NoAtomicComposition_ShouldBlockChanges()
        {
            var exportProvider = ExportProviderFactory.CreateRecomposable();
            var engine = new ImportEngine(exportProvider);

            var import = ImportDefinitionFactory.Create("Value");
            var importer = PartFactory.CreateImporter(import);

            exportProvider.AddExport("Value", 21);

            engine.PreviewImports(importer, null);

            ExceptionAssert.Throws<ChangeRejectedException>(() =>
                exportProvider.AddExport("Value", 22));

            ExceptionAssert.Throws<ChangeRejectedException>(() =>
                exportProvider.RemoveExport("Value"));

            GC.KeepAlive(importer);
        }

        [TestMethod]
        public void PreviewImports_Unsuccessful_NoAtomicComposition_ShouldNotBlockChanges()
        {
            var exportProvider = ExportProviderFactory.CreateRecomposable();
            var engine = new ImportEngine(exportProvider);

            var import = ImportDefinitionFactory.Create("Value");
            var importer = PartFactory.CreateImporter(import);

            ExceptionAssert.Throws<CompositionException>(() =>
                engine.PreviewImports(importer, null));

            exportProvider.AddExport("Value", 22);
            exportProvider.AddExport("Value", 23);
            exportProvider.RemoveExport("Value");

            GC.KeepAlive(importer);
        }

        [TestMethod]
        public void PreviewImports_Successful_AtomicComposition_Completeted_ShouldBlockChanges()
        {
            var exportProvider = ExportProviderFactory.CreateRecomposable();
            var engine = new ImportEngine(exportProvider);

            var import = ImportDefinitionFactory.Create("Value");
            var importer = PartFactory.CreateImporter(import);

            exportProvider.AddExport("Value", 21);

            using (var atomicComposition = new AtomicComposition())
            {
                engine.PreviewImports(importer, atomicComposition);
                atomicComposition.Complete();
            }

            ExceptionAssert.Throws<ChangeRejectedException>(() =>
                exportProvider.AddExport("Value", 22));

            ExceptionAssert.Throws<ChangeRejectedException>(() =>
                exportProvider.RemoveExport("Value"));

            GC.KeepAlive(importer);
        }

        [TestMethod]
        public void PreviewImports_Successful_AtomicComposition_RolledBack_ShouldNotBlockChanges()
        {
            var exportProvider = ExportProviderFactory.CreateRecomposable();
            var engine = new ImportEngine(exportProvider);

            var import = ImportDefinitionFactory.Create("Value");
            var importer = PartFactory.CreateImporter(import);

            exportProvider.AddExport("Value", 21);

            using (var atomicComposition = new AtomicComposition())
            {
                engine.PreviewImports(importer, atomicComposition);

                // Let atomicComposition get disposed thus rolledback
            }

            exportProvider.AddExport("Value", 22);
            exportProvider.RemoveExport("Value");

            GC.KeepAlive(importer);
        }

        [TestMethod]
        public void PreviewImports_Unsuccessful_AtomicComposition_ShouldNotBlockChanges()
        {
            var exportProvider = ExportProviderFactory.CreateRecomposable();
            var engine = new ImportEngine(exportProvider);

            var import = ImportDefinitionFactory.Create("Value");
            var importer = PartFactory.CreateImporter(import);

            using (var atomicComposition = new AtomicComposition())
            {
                ExceptionAssert.Throws<ChangeRejectedException>(() =>
                    engine.PreviewImports(importer, atomicComposition));
            }

            exportProvider.AddExport("Value", 22);
            exportProvider.AddExport("Value", 23);
            exportProvider.RemoveExport("Value");

            GC.KeepAlive(importer);
        }

        [TestMethod]
        public void PreviewImports_ReleaseImports_ShouldNotBlockChanges()
        {
            var exportProvider = ExportProviderFactory.CreateRecomposable();
            var engine = new ImportEngine(exportProvider);

            var import = ImportDefinitionFactory.Create("Value");
            var importer = PartFactory.CreateImporter(import);

            exportProvider.AddExport("Value", 21);

            engine.PreviewImports(importer, null);

            ExceptionAssert.Throws<ChangeRejectedException>(() =>
                exportProvider.AddExport("Value", 22));

            ExceptionAssert.Throws<ChangeRejectedException>(() =>
                exportProvider.RemoveExport("Value"));

            engine.ReleaseImports(importer, null);

            exportProvider.AddExport("Value", 22);
            exportProvider.RemoveExport("Value");

            GC.KeepAlive(importer);
        }

        [TestMethod]
        public void PreviewImports_MissingOptionalImport_ShouldSucceed()
        {
            var exportProvider = ExportProviderFactory.CreateRecomposable();
            var engine = new ImportEngine(exportProvider);

            var import = ImportDefinitionFactory.Create("Value", ImportCardinality.ZeroOrOne);
            var importer = PartFactory.CreateImporter(import);

            engine.PreviewImports(importer, null);

            GC.KeepAlive(importer);
        }

        [TestMethod]
        public void PreviewImports_ZeroCollectionImport_ShouldSucceed()
        {
            var exportProvider = ExportProviderFactory.CreateRecomposable();
            var engine = new ImportEngine(exportProvider);

            var import = ImportDefinitionFactory.Create("Value", ImportCardinality.ZeroOrMore);
            var importer = PartFactory.CreateImporter(import);

            engine.PreviewImports(importer, null);

            GC.KeepAlive(importer);
        }

        [TestMethod]
        public void PreviewImports_MissingOptionalImport_NonRecomposable_ShouldNotBlockChanges()
        {
            var exportProvider = ExportProviderFactory.CreateRecomposable();
            var engine = new ImportEngine(exportProvider);

            var import = ImportDefinitionFactory.Create("Value", ImportCardinality.ZeroOrOne, false, false);
            var importer = PartFactory.CreateImporter(import);

            engine.PreviewImports(importer, null);

            exportProvider.AddExport("Value", 21);
            exportProvider.AddExport("Value", 22);
            exportProvider.RemoveExport("Value");

            GC.KeepAlive(importer);
        }

        [TestMethod]
        public void PreviewImports_ZeroCollectionImport_NonRecomposable_ShouldNotBlockChanges()
        {
            var exportProvider = ExportProviderFactory.CreateRecomposable();
            var engine = new ImportEngine(exportProvider);

            var import = ImportDefinitionFactory.Create("Value", ImportCardinality.ZeroOrMore, false, false);
            var importer = PartFactory.CreateImporter(import);

            engine.PreviewImports(importer, null);

            exportProvider.AddExport("Value", 21);
            exportProvider.AddExport("Value", 22);
            exportProvider.RemoveExport("Value");

            GC.KeepAlive(importer);
        }

        [TestMethod]
        public void SatisfyImports_NonRecomposable_ValueShouldNotChange()
        {
            var exportProvider = ExportProviderFactory.CreateRecomposable();
            var engine = new ImportEngine(exportProvider);

            exportProvider.AddExport("Value", 21);

            var import = ImportDefinitionFactory.Create("Value", false);
            var importer = PartFactory.CreateImporter(import);

            engine.SatisfyImports(importer);

            Assert.AreEqual(21, importer.GetImport(import));

            // After rejection batch failures throw ChangeRejectedException to indicate that
            // the failure did not affect the container
            ExceptionAssert.Throws<ChangeRejectedException>(() =>
                exportProvider.ReplaceExportValue("Value", 42));

            Assert.AreEqual(21, importer.GetImport(import), "Value should not change!");

            GC.KeepAlive(importer);
        }

        [TestMethod]
        public void SatisfyImports_Recomposable_ValueShouldChange()
        {
            var exportProvider = ExportProviderFactory.CreateRecomposable();
            var engine = new ImportEngine(exportProvider);

            exportProvider.AddExport("Value", 21);

            var import = ImportDefinitionFactory.Create("Value", true);
            var importer = PartFactory.CreateImporter(import);

            engine.SatisfyImports(importer);

            Assert.AreEqual(21, importer.GetImport(import));

            exportProvider.ReplaceExportValue("Value", 42);

            Assert.AreEqual(42, importer.GetImport(import), "Value should change!");

            GC.KeepAlive(importer);
        }

        [TestMethod]
        public void SatisfyImports_NonRecomposable_Prerequisite_ValueShouldNotChange()
        {
            var exportProvider = ExportProviderFactory.CreateRecomposable();
            var engine = new ImportEngine(exportProvider);

            var import = ImportDefinitionFactory.Create("Value", false, true);
            var importer = PartFactory.CreateImporter(import);

            exportProvider.AddExport("Value", 21);

            engine.SatisfyImports(importer);

            Assert.AreEqual(21, importer.GetImport(import));

            ExceptionAssert.Throws<ChangeRejectedException>(() =>
                exportProvider.ReplaceExportValue("Value", 42));

            Assert.AreEqual(21, importer.GetImport(import), "Value should NOT change!");

            GC.KeepAlive(importer);
        }

        [TestMethod]
        public void SatisfyImports_Recomposable_Prerequisite_ValueShouldChange()
        {
            var exportProvider = ExportProviderFactory.CreateRecomposable();
            var engine = new ImportEngine(exportProvider);

            var import = ImportDefinitionFactory.Create("Value", true, true);
            var importer = PartFactory.CreateImporter(import);

            exportProvider.AddExport("Value", 21);

            engine.SatisfyImports(importer);

            Assert.AreEqual(21, importer.GetImport(import));

            exportProvider.ReplaceExportValue("Value", 42);

            Assert.AreEqual(42, importer.GetImport(import), "Value should change!");

            GC.KeepAlive(importer);
        }

        [TestMethod]
        public void SatisfyImports_OneRecomposable_OneNotRecomposable()
        {
            var exportProvider = ExportProviderFactory.CreateRecomposable();
            var engine = new ImportEngine(exportProvider);

            var import1 = ImportDefinitionFactory.Create("Value", true);
            var import2 = ImportDefinitionFactory.Create("Value", false);
            var importer = PartFactory.CreateImporter(import1, import2);

            exportProvider.AddExport("Value", 21);

            engine.SatisfyImports(importer);

            // Initial compose values should be 21
            Assert.AreEqual(21, importer.GetImport(import1));
            Assert.AreEqual(21, importer.GetImport(import2));

            // Reset value to ensure it doesn't get set to same value again
            importer.ResetImport(import1);
            importer.ResetImport(import2);

            ExceptionAssert.Throws<ChangeRejectedException>(() =>
                exportProvider.ReplaceExportValue("Value", 42));

            Assert.AreEqual(null, importer.GetImport(import1), "Value should NOT have been set!");
            Assert.AreEqual(null, importer.GetImport(import2), "Value should NOT have been set!");

            GC.KeepAlive(importer);
        }

        [TestMethod]
        public void SatisfyImports_TwoRecomposables_SingleExportValueChanged()
        {
            var exportProvider = ExportProviderFactory.CreateRecomposable();
            var engine = new ImportEngine(exportProvider);

            var import1 = ImportDefinitionFactory.Create("Value1", true);
            var import2 = ImportDefinitionFactory.Create("Value2", true);
            var importer = PartFactory.CreateImporter(import1, import2);

            exportProvider.AddExport("Value1", 21);
            exportProvider.AddExport("Value2", 23);

            engine.SatisfyImports(importer);

            Assert.AreEqual(21, importer.GetImport(import1));
            Assert.AreEqual(23, importer.GetImport(import2));

            importer.ResetImport(import1);
            importer.ResetImport(import2);

            // Only change Value1 
            exportProvider.ReplaceExportValue("Value1", 42);

            Assert.AreEqual(42, importer.GetImport(import1), "Value should have been set!");

            Assert.AreEqual(null, importer.GetImport(import2), "Value should NOT have changed to the value in the container.");

            GC.KeepAlive(importer);
        }

        [TestMethod]
        public void SatisfyImports_Recomposable_Unregister_ValueShouldChangeOnce()
        {
            var exportProvider = ExportProviderFactory.CreateRecomposable();
            var engine = new ImportEngine(exportProvider);

            exportProvider.AddExport("Value", 21);

            var import = ImportDefinitionFactory.Create("Value", true);
            var importer = PartFactory.CreateImporter(import);

            engine.SatisfyImports(importer);

            Assert.AreEqual(21, importer.GetImport(import));

            exportProvider.ReplaceExportValue("Value", 42);

            Assert.AreEqual(42, importer.GetImport(import), "Value should change!");

            engine.ReleaseImports(importer, null);

            exportProvider.ReplaceExportValue("Value", 666);

            Assert.AreEqual(42, importer.GetImport(import), "Value should not change!");

            GC.KeepAlive(importer);
        }

        [TestMethod]
        public void SatisfyImports_MissingOptionalImport_NonRecomposable_ShouldNotBlockChanges()
        {
            var exportProvider = ExportProviderFactory.CreateRecomposable();
            var engine = new ImportEngine(exportProvider);

            var import = ImportDefinitionFactory.Create("Value", ImportCardinality.ZeroOrOne, false, false);
            var importer = PartFactory.CreateImporter(import);

            exportProvider.AddExport("Value", 20);

            engine.SatisfyImports(importer);

            ExceptionAssert.Throws<ChangeRejectedException>(() =>
                exportProvider.AddExport("Value", 21));

            ExceptionAssert.Throws<ChangeRejectedException>(() =>
                exportProvider.RemoveExport("Value"));

            GC.KeepAlive(importer);
        }

        [TestMethod]
        public void SatisfyImports_ZeroCollectionImport_NonRecomposable_ShouldNotBlockChanges()
        {
            var exportProvider = ExportProviderFactory.CreateRecomposable();
            var engine = new ImportEngine(exportProvider);

            var import = ImportDefinitionFactory.Create("Value", ImportCardinality.ZeroOrMore, false, false);
            var importer = PartFactory.CreateImporter(import);

            exportProvider.AddExport("Value", 20);

            engine.SatisfyImports(importer);

            ExceptionAssert.Throws<ChangeRejectedException>(() =>
                exportProvider.AddExport("Value", 21));

            ExceptionAssert.Throws<ChangeRejectedException>(() =>
                exportProvider.RemoveExport("Value"));

            GC.KeepAlive(importer);
        }

        [TestMethod]
        public void SatisfyImports_MissingOptionalImport_Recomposable_ShouldNotBlockChanges()
        {
            var exportProvider = ExportProviderFactory.CreateRecomposable();
            var engine = new ImportEngine(exportProvider);

            var import = ImportDefinitionFactory.Create("Value", ImportCardinality.ZeroOrOne, true, false);
            var importer = PartFactory.CreateImporter(import);

            exportProvider.AddExport("Value", 20);

            engine.SatisfyImports(importer);

            exportProvider.AddExport("Value", 21);
            exportProvider.RemoveExport("Value");

            GC.KeepAlive(importer);
        }

        [TestMethod]
        public void SatisfyImports_ZeroCollectionImport_Recomposable_ShouldNotBlockChanges()
        {
            var exportProvider = ExportProviderFactory.CreateRecomposable();
            var engine = new ImportEngine(exportProvider);

            var import = ImportDefinitionFactory.Create("Value", ImportCardinality.ZeroOrMore, true, false);
            var importer = PartFactory.CreateImporter(import);

            exportProvider.AddExport("Value", 20);

            engine.SatisfyImports(importer);

            exportProvider.AddExport("Value", 21);
            exportProvider.RemoveExport("Value");

            GC.KeepAlive(importer);
        }

        [TestMethod]
        public void SatisfyImportsOnce_Recomposable_ValueShouldNotChange_NoRecompositionRequested()
        {
            var exportProvider = ExportProviderFactory.CreateRecomposable();
            var engine = new ImportEngine(exportProvider);

            exportProvider.AddExport("Value", 21);

            var import = ImportDefinitionFactory.Create("Value", true);
            var importer = PartFactory.CreateImporter(import);

            engine.SatisfyImportsOnce(importer);

            Assert.AreEqual(21, importer.GetImport(import));

            exportProvider.ReplaceExportValue("Value", 42);

            Assert.AreEqual(21, importer.GetImport(import), "Value should not change!");

            GC.KeepAlive(importer);
        }

        [TestMethod]
        public void SatisifyImportsOnce_Recomposable_ValueShouldNotChange_NoRecompositionRequested_ViaNonArgumentSignature()
        {
            var exportProvider = ExportProviderFactory.CreateRecomposable();
            var engine = new ImportEngine(exportProvider);

            exportProvider.AddExport("Value", 21);

            var import = ImportDefinitionFactory.Create("Value", true);
            var importer = PartFactory.CreateImporter(import);

            engine.SatisfyImportsOnce(importer);

            Assert.AreEqual(21, importer.GetImport(import));

            exportProvider.ReplaceExportValue("Value", 42);

            Assert.AreEqual(21, importer.GetImport(import), "Value should not change!");

            GC.KeepAlive(importer);
        }

        [TestMethod]
        public void SatisfyImportsOnce_Successful_ShouldNotBlockChanges()
        {
            var exportProvider = ExportProviderFactory.CreateRecomposable();
            var engine = new ImportEngine(exportProvider);

            var import = ImportDefinitionFactory.Create("Value");
            var importer = PartFactory.CreateImporter(import);

            exportProvider.AddExport("Value", 21);

            engine.SatisfyImportsOnce(importer);

            exportProvider.AddExport("Value", 22);
            exportProvider.RemoveExport("Value");

            GC.KeepAlive(importer);
        }

        [TestMethod]
        public void SatisfyImportsOnce_Unsuccessful_ShouldNotBlockChanges()
        {
            var exportProvider = ExportProviderFactory.CreateRecomposable();
            var engine = new ImportEngine(exportProvider);

            var import = ImportDefinitionFactory.Create("Value");
            var importer = PartFactory.CreateImporter(import);

            ExceptionAssert.Throws<CompositionException>(() =>
                engine.SatisfyImportsOnce(importer));

            exportProvider.AddExport("Value", 22);
            exportProvider.AddExport("Value", 23);
            exportProvider.RemoveExport("Value");

            GC.KeepAlive(importer);
        }
    }
}