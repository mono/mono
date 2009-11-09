// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Factories;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.UnitTesting;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ComponentModel.Composition
{
    [TestClass]
    public class AssemblyCatalogTests
    {
#if !SILVERLIGHT
        [TestMethod]
        public void Constructor1_ValueAsAssemblyArgument_ShouldSetAssemblyProperty()
        {
            var expectations = Expectations.GetAssemblies();

            foreach (var e in expectations)
            {
                var catalog = new AssemblyCatalog(e.CodeBase);

                Assert.AreSame(e, catalog.Assembly);
            }
        }
#endif

        [TestMethod]
        public void Constructor2_ValueAsAssemblyArgument_ShouldSetAssemblyProperty()
        {
            var expectations = Expectations.GetAssemblies();

            foreach (var e in expectations)
            {
                var catalog = new AssemblyCatalog(e);

                Assert.AreSame(e, catalog.Assembly);
            }
        }

#if !SILVERLIGHT
        
        [TestMethod]
        public void Constructor1_LockedFileAsCodeBaseArgument_ShouldThrowFileLoad()
        {
            using (TemporaryFile file = new TemporaryFile())
            {
                using (FileStream stream = new FileStream(file.FileName, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    ExceptionAssert.Throws<FileLoadException>(() =>
                    {
                        new AssemblyCatalog(file.FileName);
                    });
                }
            }
        }

        [TestMethod]
        public void Constructor1_NullFileNameAsCodeBaseArgument_ShouldThrowArgumentNull()
        {
            ExceptionAssert.ThrowsArgument<ArgumentNullException>("codeBase", () =>
            {
                new AssemblyCatalog((string)null);
            });
        }

        [TestMethod]
        public void Constructor1_EmptyFileNameAsCodeBaseArgument_ShouldThrowArgument()
        {
            ExceptionAssert.ThrowsArgument<ArgumentException>("codeBase", () =>
            {
                new AssemblyCatalog("");
            });
        }

        [TestMethod]
        public void Constructor1_InvalidFileNameAsCodeBaseArgument_ShouldThrowArgument()
        {
            ExceptionAssert.Throws<ArgumentException>(() =>
            {
                new AssemblyCatalog("??||>");
            });
        }

        [TestMethod]
        public void Constructor1_DirectoryAsCodeBaseArgument_ShouldThrowFileLoad()
        {
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.System);
            Assert.IsTrue(Directory.Exists(directory));

            ExceptionAssert.Throws<FileLoadException>(() =>
            {
                new AssemblyCatalog(directory);
            });
        }

        [TestMethod]
        public void Constructor1_TooLongFileNameAsCodeBaseArgument_ShouldThrowPathTooLong()
        {
            ExceptionAssert.Throws<PathTooLongException>(() =>
            {
                new AssemblyCatalog(@"c:\This is a very long path\And Just to make sure\We will continue to make it very long\This is a very long path\And Just to make sure\We will continue to make it very long\This is a very long path\And Just to make sure\We will continue to make it very long\myassembly.dll");
            });
        }

        [TestMethod]
        public void Constructor1_NonAssemblyFileNameAsCodeBaseArgument_ShouldThrowBadImageFormat()
        {
            using (TemporaryFile temporaryFile = new TemporaryFile())
            {
                ExceptionAssert.Throws<BadImageFormatException>(() =>
                {
                    new AssemblyCatalog(temporaryFile.FileName);
                });
            }
        }

        [TestMethod]
        public void Constructor1_NonExistentFileNameAsCodeBaseArgument_ShouldThrowFileNotFound()
        {
            ExceptionAssert.Throws<FileNotFoundException>(() =>
            {
                new AssemblyCatalog(@"FileThat should not ever exist");
            });
        }
#endif

#if !SILVERLIGHT

        [TestMethod]
        public void Constructor1_ShouldSetOriginToNull()
        {
            var catalog = (ICompositionElement)new AssemblyCatalog(GetAttributedAssemblyCodeBase());

            Assert.IsNull(catalog.Origin);
        }

#endif

        [TestMethod]
        public void Constructor2_ShouldSetOriginToNull()
        {
            var catalog = (ICompositionElement)new AssemblyCatalog(GetAttributedAssembly());

            Assert.IsNull(catalog.Origin);
        }

        [TestMethod]
        public void Assembly_WhenCatalogDisposed_ShouldNotThrow()
        {
            var catalog = CreateAssemblyCatalog();
            catalog.Dispose();

            Assert.IsNotNull(catalog.Assembly);
        }

        [TestMethod]
        public void ICompositionElementDisplayName_WhenCatalogDisposed_ShouldNotThrow()
        {
            var catalog = CreateAssemblyCatalog();
            catalog.Dispose();

            var displayName = ((ICompositionElement)catalog).DisplayName;
        }

        [TestMethod]
        public void ICompositionElementOrigin_WhenCatalogDisposed_ShouldNotThrow()
        {
            var catalog = CreateAssemblyCatalog();
            catalog.Dispose();

            var origin = ((ICompositionElement)catalog).Origin;
        }

        [TestMethod]
        public void ToString_WhenCatalogDisposed_ShouldNotThrow()
        {
            var catalog = CreateAssemblyCatalog();
            catalog.Dispose();

            catalog.ToString();
        }
        
        [TestMethod]
        public void Parts_WhenCatalogDisposed_ShouldThrowObjectDisposed()
        {
            var catalog = CreateAssemblyCatalog();
            catalog.Dispose();

            ExceptionAssert.ThrowsDisposed(catalog, () =>
            {
                var parts = catalog.Parts;
            });
        }

        [TestMethod]
        public void GetExports_WhenCatalogDisposed_ShouldThrowObjectDisposed()
        {
            var catalog = CreateAssemblyCatalog();
            catalog.Dispose();
			var definition = ImportDefinitionFactory.Create();

            ExceptionAssert.ThrowsDisposed(catalog, () =>
            {
                catalog.GetExports(definition);
            });
        }

        [TestMethod]
        public void GetExports_NullAsConstraintArgument_ShouldThrowArgumentNull()
        {
            var catalog = CreateAssemblyCatalog();

            ExceptionAssert.ThrowsArgument<ArgumentNullException>("definition", () =>
            {
                catalog.GetExports((ImportDefinition)null);
            });
        }


        [TestMethod]
        public void Dispose_ShouldNotThrow()
        {
            using (var catalog = CreateAssemblyCatalog())
            {
            }
        }

        [TestMethod]
        public void Dispose_CanBeCalledMultipleTimes()
        {
            var catalog = CreateAssemblyCatalog();
            catalog.Dispose();
            catalog.Dispose();
            catalog.Dispose();
        }

        [TestMethod]
        public void Parts()
        {
            var catalog = new AssemblyCatalog(typeof(AssemblyCatalogTests).Assembly);
            Assert.IsNotNull(catalog.Parts);
            Assert.IsTrue(catalog.Parts.Count()>0);
        }


        [TestMethod]
        public void Parts_ShouldSetDefinitionOriginToCatalogItself()
        {
            var catalog = CreateAssemblyCatalog();
            Assert.IsTrue(catalog.Parts.Count() > 0);

            foreach (ICompositionElement definition in catalog.Parts)
            {
                Assert.AreSame(catalog, definition.Origin);
            }
        }

        [TestMethod]
        public void GetExports()
        {
            var catalog = new AssemblyCatalog(typeof(AssemblyCatalogTests).Assembly);
            Expression<Func<ExportDefinition, bool>> constraint = (ExportDefinition exportDefinition) => exportDefinition.ContractName == AttributedModelServices.GetContractName(typeof(MyExport));
            IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> matchingExports = catalog.GetExports(constraint);
            Assert.IsNotNull(matchingExports);
            Assert.IsTrue(matchingExports.Count() >= 0);

            IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> expectedMatchingExports = catalog.Parts
                .SelectMany(part => part.ExportDefinitions, (part, export) => new Tuple<ComposablePartDefinition, ExportDefinition>(part, export))
                .Where(partAndExport => partAndExport.Item2.ContractName == AttributedModelServices.GetContractName(typeof(MyExport)));
            Assert.IsTrue(matchingExports.SequenceEqual(expectedMatchingExports));
        }



#if !SILVERLIGHT
        // Silverlight does not support loading assemblies via file path
        [TestMethod]
        public void AddAssemblyUsingFile()
        {
            var catalog = new AssemblyCatalog(typeof(AssemblyCatalogTests).Assembly.Location);
            var container = new CompositionContainer(catalog);

            Assert.IsNotNull(container.GetExportedValue<MyExport>());
        }
#endif

        [TestMethod]
        public void TwoTypesWithSameSimpleName()
        {
            var catalog = new AssemblyCatalog(typeof(AssemblyCatalogTests).Assembly);
            var container = new CompositionContainer(catalog);

            NotSoUniqueName unique1 = container.GetExportedValue<NotSoUniqueName>();
            Assert.IsNotNull(unique1);

            Assert.AreEqual(23, unique1.MyIntProperty);

            NotSoUniqueName2.NotSoUniqueName nestedUnique = container.GetExportedValue<NotSoUniqueName2.NotSoUniqueName>();

            Assert.IsNotNull(nestedUnique);
            Assert.AreEqual("MyStringProperty", nestedUnique.MyStringProperty);
        }

        [TestMethod]
        public void GettingFunctionExports()
        {
            var catalog = new AssemblyCatalog(typeof(AssemblyCatalogTests).Assembly);
            var container = new CompositionContainer(catalog);

            ImportDefaultFunctions import = container.GetExportedValue<ImportDefaultFunctions>("ImportDefaultFunctions");
            import.VerifyIsBound();
        }

        [TestMethod]
        public void AnExportOfAnInstanceThatFailsToCompose()
        {
            var catalog = new AssemblyCatalog(typeof(AssemblyCatalogTests).Assembly);
            var container = new CompositionContainer(catalog);

            // Rejection causes the part in the catalog whose imports cannot be
            // satisfied to be ignored, resulting in a cardinality mismatch instead of a
            // composition exception
            ExceptionAssert.Throws<ImportCardinalityMismatchException>(() =>
            {
                container.GetExportedValue<string>("ExportMyString");
            });
        }

        [TestMethod]
        public void SharedPartCreation()
        {
            var catalog = new AssemblyCatalog(typeof(AssemblyCatalogTests).Assembly);
            var container = new CompositionContainer(catalog);
            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(new Int32Exporter(41));
            container.Compose(batch);

            var sharedPart1 = container.GetExportedValue<MySharedPartExport>();
            Assert.AreEqual(41, sharedPart1.Value);
            var sharedPart2 = container.GetExportedValue<MySharedPartExport>();
            Assert.AreEqual(41, sharedPart2.Value);

            Assert.AreEqual(sharedPart1, sharedPart2, "These should be the same instances");
        }

        [TestMethod]
        public void NonSharedPartCreation()
        {
            var catalog = new AssemblyCatalog(typeof(AssemblyCatalogTests).Assembly);
            var container = new CompositionContainer(catalog);
            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(new Int32Exporter(41));
            container.Compose(batch);

            var nonSharedPart1 = container.GetExportedValue<MyNonSharedPartExport>();
            Assert.AreEqual(41, nonSharedPart1.Value);
            var nonSharedPart2 = container.GetExportedValue<MyNonSharedPartExport>();
            Assert.AreEqual(41, nonSharedPart2.Value);

            Assert.AreNotSame(nonSharedPart1, nonSharedPart2, "These should be different instances");
        }

        [TestMethod]
        public void RecursiveNonSharedPartCreation()
        {
            var catalog = new AssemblyCatalog(typeof(AssemblyCatalogTests).Assembly);
            var container = new CompositionContainer(catalog);

            CompositionAssert.ThrowsError(ErrorId.ImportEngine_PartCannotGetExportedValue, () =>
            {
                container.GetExportedValue<DirectCycleNonSharedPart>();
            });

            CompositionAssert.ThrowsError(ErrorId.ImportEngine_PartCannotGetExportedValue, () =>
            {
                container.GetExportedValue<CycleNonSharedPart>();
            });

            CompositionAssert.ThrowsError(ErrorId.ImportEngine_PartCannotGetExportedValue, () =>
            {
                container.GetExportedValue<CycleNonSharedPart1>();
            });

            CompositionAssert.ThrowsError(ErrorId.ImportEngine_PartCannotGetExportedValue, () =>
            {
                container.GetExportedValue<CycleNonSharedPart2>();
            });

            CompositionAssert.ThrowsError(ErrorId.ImportEngine_PartCannotGetExportedValue, () =>
            {
                container.GetExportedValue<CycleWithSharedPartAndNonSharedPart>();
            });

            Assert.IsNotNull(container.GetExportedValue<CycleSharedPart>());
            Assert.IsNotNull(container.GetExportedValue<CycleSharedPart1>());
            Assert.IsNotNull(container.GetExportedValue<CycleSharedPart2>());
            Assert.IsNotNull(container.GetExportedValue<NoCycleNonSharedPart>());
        }

        [TestMethod]
        public void TryToDiscoverExportWithGenericParameter()
        {
            var catalog = new AssemblyCatalog(typeof(AssemblyCatalogTests).Assembly);
            var container = new CompositionContainer(catalog);

            // Open generic should fail because we should discover that type
            Assert.IsNull(container.GetExportedValueOrDefault<object>(AttributedModelServices.GetContractName(typeof(ExportWithGenericParameter<>))));

            // This specific generic was not exported any where so it should fail
            Assert.IsNull(container.GetExportedValueOrDefault<object>(AttributedModelServices.GetContractName(typeof(ExportWithGenericParameter<double>))));

            // This specific generic was exported so it should succeed
            Assert.IsNotNull(container.GetExportedValueOrDefault<object>(AttributedModelServices.GetContractName(typeof(ExportWithGenericParameter<int>))));

            // Shouldn't discovoer static type with open generic
            Assert.IsNull(container.GetExportedValueOrDefault<object>(AttributedModelServices.GetContractName(typeof(StaticExportWithGenericParameter<>))));

            // Should find a type that inherits from an export
            Assert.IsNotNull(container.GetExportedValueOrDefault<object>(AttributedModelServices.GetContractName(typeof(ExportWhichInheritsFromGeneric))));

            // This should be exported because it is inherited by ExportWhichInheritsFromGeneric
            Assert.IsNotNull(container.GetExportedValueOrDefault<object>(AttributedModelServices.GetContractName(typeof(ExportWithGenericParameter<string>))));
        }

        [TestMethod]
        public void ICompositionElementDisplayName_ShouldIncludeCatalogTypeNameAndAssemblyFullName()
        {
            var expectations = Expectations.GetAssemblies();

            foreach (var e in expectations)
            {
                var catalog = (ICompositionElement)CreateAssemblyCatalog(e);

                string expected = string.Format("AssemblyCatalog (Assembly=\"{0}\")", e.FullName);

                Assert.AreEqual(expected, catalog.DisplayName);
            }
        }

        [TestMethod]
        public void ICompositionElementDisplayName_ShouldIncludeDerivedCatalogTypeNameAndAssemblyFullName()
        {
            var expectations = Expectations.GetAssemblies();

            foreach (var e in expectations)
            {
                var catalog = (ICompositionElement)new DerivedAssemblyCatalog(e);

                string expected = string.Format("DerivedAssemblyCatalog (Assembly=\"{0}\")", e.FullName);

                Assert.AreEqual(expected, catalog.DisplayName);
            }
        }

        [TestMethod]
        public void ToString_ShouldReturnICompositionElementDisplayName()
        {
            var expectations = Expectations.GetAssemblies();

            foreach (var e in expectations)
            {
                var catalog = (ICompositionElement)CreateAssemblyCatalog(e);

                Assert.AreEqual(catalog.DisplayName, catalog.ToString());
            }
        }



        private string GetAttributedAssemblyCodeBase()
        {
            return Assembly.GetExecutingAssembly().CodeBase;
        }

        private Assembly GetAttributedAssembly()
        {
            return Assembly.GetExecutingAssembly();
        }

        private AssemblyCatalog CreateAssemblyCatalog()
        {
            return CreateAssemblyCatalog(GetAttributedAssembly());
        }

        private AssemblyCatalog CreateAssemblyCatalog(Assembly assembly)
        {
            return new AssemblyCatalog(assembly);
        }

        private class DerivedAssemblyCatalog : AssemblyCatalog
        {
            public DerivedAssemblyCatalog(Assembly assembly)
                : base(assembly)
            {
            }
        }
    }
}
