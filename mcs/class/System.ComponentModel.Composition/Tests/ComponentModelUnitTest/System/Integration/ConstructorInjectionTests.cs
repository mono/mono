// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.Composition.Factories;
using System.ComponentModel.Composition.UnitTesting;
using System.UnitTesting;
using System.ComponentModel.Composition.AttributedModel;
using System.ComponentModel.Composition.Hosting;

namespace Tests.Integration
{
    [TestClass]
    public class ConstructorInjectionTests
    {
        [TestMethod]
        public void SimpleConstructorInjection()
        {
            var container = ContainerFactory.Create();

            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(PartFactory.CreateAttributed(typeof(SimpleConstructorInjectedObject)));
            batch.AddExportedValue("CISimpleValue", 42);
            container.Compose(batch);

            SimpleConstructorInjectedObject simple = container.GetExportedValue<SimpleConstructorInjectedObject>();

            Assert.AreEqual(42, simple.CISimpleValue);
        }

        public interface IOptionalRef { }

        [Export]
        public class OptionalExportProvided { }

        [Export]
        public class AWithOptionalParameter
        {
            [ImportingConstructor]
            public AWithOptionalParameter([Import(AllowDefault = true)]IOptionalRef import,
                [Import("ContractThatShouldNotBeFound", AllowDefault = true)]int value,
                [Import(AllowDefault=true)]OptionalExportProvided provided)
            {
                Assert.IsNull(import);
                Assert.AreEqual(0, value);
                Assert.IsNotNull(provided);
            }
        }

        [TestMethod]
        public void OptionalConstructorArgument()
        {
            var container = GetContainerWithCatalog();
            var a = container.GetExportedValue<AWithOptionalParameter>();

            // A should verify that it receieved optional arugments properly
            Assert.IsNotNull(a);
        }

        [Export]
        public class AWithCollectionArgument
        {
            private IEnumerable<int> _values;

            [ImportingConstructor]
            public AWithCollectionArgument([ImportMany("MyConstructorCollectionItem")]IEnumerable<int> values)
            {
                this._values = values;
            }

            public IEnumerable<int> Values { get { return this._values; } }
        }

        [TestMethod]
        public void RebindingShouldNotHappenForConstructorArguments()
        {
            var container = GetContainerWithCatalog();
            CompositionBatch batch = new CompositionBatch();

            var p1 = batch.AddExportedValue("MyConstructorCollectionItem", 1);
            batch.AddExportedValue("MyConstructorCollectionItem", 2);
            batch.AddExportedValue("MyConstructorCollectionItem", 3);
            container.Compose(batch);

            var a = container.GetExportedValue<AWithCollectionArgument>();

            EnumerableAssert.AreEqual(a.Values, 1, 2, 3);

            batch = new CompositionBatch();
            batch.AddExportedValue("MyConstructorCollectionItem", 4);
            batch.AddExportedValue("MyConstructorCollectionItem", 5);
            batch.AddExportedValue("MyConstructorCollectionItem", 6);
            // After rejection changes that are incompatible with existing assumptions are no
            // longer silently ignored.  The batch attempting to make this change is rejected
            // with a ChangeRejectedException
            CompositionAssert.ThrowsChangeRejectedError(ErrorId.ImportEngine_PreventedByExistingImport,() =>
            {
                container.Compose(batch);
            });

            // The collection which is a constructor import should not be rebound
            EnumerableAssert.AreEqual(a.Values, 1, 2, 3);

            batch.RemovePart(p1);
            // After rejection changes that are incompatible with existing assumptions are no
            // longer silently ignored.  The batch attempting to make this change is rejected
            // with a ChangeRejectedException
            CompositionAssert.ThrowsChangeRejectedError(ErrorId.ImportEngine_PreventedByExistingImport, () =>
            {
                container.Compose(batch);
            });

            // The collection which is a constructor import should not be rebound
            EnumerableAssert.AreEqual(a.Values, 1, 2, 3);
        }

        [TestMethod]
        public void MissingConstructorArgsWithAlreadyCreatedInstance()
        {
            var container = GetContainerWithCatalog();
            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(new ClassWithNotFoundConstructorArgs(21));
            container.Compose(batch);
        }

        [TestMethod]
        public void MissingConstructorArgsWithTypeFromCatalogMissingArg()
        {
            var container = GetContainerWithCatalog();

            // After rejection part definitions in catalogs whose dependencies cannot be
            // satisfied are now silently ignored, turning this into a cardinality
            // exception for the GetExportedValue call
            ExceptionAssert.Throws<ImportCardinalityMismatchException>(() =>
            {
                container.GetExportedValue<ClassWithNotFoundConstructorArgs>();
            });
        }

        [TestMethod]
        public void MissingConstructorArgsWithWithTypeFromCatalogWithArg()
        {
            var container = GetContainerWithCatalog();
            CompositionBatch batch = new CompositionBatch();
            batch.AddExportedValue("ContractThatDoesntExist", 21);
            container.Compose(batch);

            Assert.IsTrue(container.IsPresent<ClassWithNotFoundConstructorArgs>());
        }

        [Export]
        public class ClassWithNotFoundConstructorArgs
        {
            [ImportingConstructor]
            public ClassWithNotFoundConstructorArgs([Import("ContractThatDoesntExist")]int i)
            {
            }
        }

        private CompositionContainer GetContainerWithCatalog()
        {
            var catalog = new AssemblyCatalog(typeof(ConstructorInjectionTests).Assembly);

            return new CompositionContainer(catalog);
        }

        [Export]
        public class InvalidImportManyCI
        {
            [ImportingConstructor]
            public InvalidImportManyCI(
                [ImportMany]List<MyExport> exports)
            {
            }
        }

        [TestMethod]
        public void ImportMany_ConstructorParameter_OnNonAssiganbleType_ShouldThrowCompositionException()
        {
            var container = ContainerFactory.CreateWithAttributedCatalog(typeof(InvalidImportManyCI));

            CompositionAssert.ThrowsError(ErrorId.ImportEngine_PartCannotGetExportedValue,
                ErrorId.ImportEngine_PartCannotActivate, 
                ErrorId.ReflectionModel_ImportManyOnParameterCanOnlyBeAssigned,
                () => container.GetExportedValue<InvalidImportManyCI>());
        }
    }
}
