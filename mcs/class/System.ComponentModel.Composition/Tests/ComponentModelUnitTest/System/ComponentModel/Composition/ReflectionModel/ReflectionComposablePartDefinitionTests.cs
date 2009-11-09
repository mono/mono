// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition.Factories;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Reflection;
using Microsoft.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.UnitTesting;
using System.Threading;

namespace System.ComponentModel.Composition.ReflectionModel
{
    [TestClass]
    public class ReflectionComposablePartDefinitionTests
    {
        private ReflectionComposablePartDefinition CreateReflectionPartDefinition(
            Lazy<Type> partType,
            bool requiresDisposal,
            Func<IEnumerable<ImportDefinition>> imports,
            Func<IEnumerable<ExportDefinition>>exports,
            IDictionary<string, object> metadata,
            ICompositionElement origin)
        {
            return (ReflectionComposablePartDefinition)ReflectionModelServices.CreatePartDefinition(partType, requiresDisposal, 
                new Lazy<IEnumerable<ImportDefinition>>(imports, false), 
                new Lazy<IEnumerable<ExportDefinition>>(exports, false), 
                metadata.AsLazy(), origin);
        }

        [TestMethod]
        public void Constructor()
        {
            Type expectedType = typeof(TestPart);
            Lazy<Type> expectedLazyType = expectedType.AsLazy();
            IDictionary<string, object> expectedMetadata = new Dictionary<string, object>();
            expectedMetadata["Key1"] = 1;
            expectedMetadata["Key2"] = "Value2";

            IEnumerable<ImportDefinition> expectedImports = CreateImports(expectedType);
            IEnumerable<ExportDefinition> expectedExports = CreateExports(expectedType);

            ICompositionElement expectedOrigin = new MockOrigin();

            ReflectionComposablePartDefinition definition = CreateReflectionPartDefinition(
                expectedLazyType,
                false,
                () => expectedImports,
                () => expectedExports,
                expectedMetadata,
                expectedOrigin);

            Assert.AreSame(expectedType, definition.GetPartType());
            Assert.IsTrue(definition.Metadata.Keys.SequenceEqual(expectedMetadata.Keys));
            Assert.IsTrue(definition.Metadata.Values.SequenceEqual(expectedMetadata.Values));
            Assert.IsTrue(definition.ExportDefinitions.SequenceEqual(expectedExports.Cast<ExportDefinition>()));
            Assert.IsTrue(definition.ImportDefinitions.SequenceEqual(expectedImports.Cast<ImportDefinition>()));
            Assert.AreSame(expectedOrigin, ((ICompositionElement)definition).Origin);
            Assert.IsNotNull(((ICompositionElement)definition).DisplayName);
            Assert.IsFalse(definition.IsDisposalRequired);
        }

        [TestMethod]
        public void Constructor_DisposablePart()
        {
            Type expectedType = typeof(TestPart);
            Lazy<Type> expectedLazyType = expectedType.AsLazy();
            IDictionary<string, object> expectedMetadata = new Dictionary<string, object>();
            expectedMetadata["Key1"] = 1;
            expectedMetadata["Key2"] = "Value2";

            IEnumerable<ImportDefinition> expectedImports = CreateImports(expectedType);
            IEnumerable<ExportDefinition> expectedExports = CreateExports(expectedType);

            ICompositionElement expectedOrigin = new MockOrigin();

            ReflectionComposablePartDefinition definition = CreateReflectionPartDefinition(
                expectedLazyType,
                true,
                () => expectedImports,
                () => expectedExports,
                expectedMetadata,
                expectedOrigin);

            Assert.AreSame(expectedType, definition.GetPartType());
            Assert.IsTrue(definition.Metadata.Keys.SequenceEqual(expectedMetadata.Keys));
            Assert.IsTrue(definition.Metadata.Values.SequenceEqual(expectedMetadata.Values));
            Assert.IsTrue(definition.ExportDefinitions.SequenceEqual(expectedExports.Cast<ExportDefinition>()));
            Assert.IsTrue(definition.ImportDefinitions.SequenceEqual(expectedImports.Cast<ImportDefinition>()));
            Assert.AreSame(expectedOrigin, ((ICompositionElement)definition).Origin);
            Assert.IsNotNull(((ICompositionElement)definition).DisplayName);
            Assert.IsTrue(definition.IsDisposalRequired);
        }

        [TestMethod]
        public void CreatePart()
        {
            Type expectedType = typeof(TestPart);
            Lazy<Type> expectedLazyType = expectedType.AsLazy();
            IDictionary<string, object> expectedMetadata = new Dictionary<string, object>();
            expectedMetadata["Key1"] = 1;
            expectedMetadata["Key2"] = "Value2";

            IEnumerable<ImportDefinition> expectedImports = CreateImports(expectedType);
            IEnumerable<ExportDefinition> expectedExports = CreateExports(expectedType);

            ICompositionElement expectedOrigin = new MockOrigin();

            ReflectionComposablePartDefinition definition = CreateReflectionPartDefinition(
                expectedLazyType,
                false,
                () => expectedImports,
                () => expectedExports,
                expectedMetadata,
                expectedOrigin);

            var part = definition.CreatePart();
            Assert.IsNotNull(part);
            Assert.IsFalse(part is IDisposable);
        }

        [TestMethod]
        public void CreatePart_Disposable()
        {
            Type expectedType = typeof(TestPart);
            Lazy<Type> expectedLazyType = expectedType.AsLazy();
            IDictionary<string, object> expectedMetadata = new Dictionary<string, object>();
            expectedMetadata["Key1"] = 1;
            expectedMetadata["Key2"] = "Value2";

            IEnumerable<ImportDefinition> expectedImports = CreateImports(expectedType);
            IEnumerable<ExportDefinition> expectedExports = CreateExports(expectedType);

            ICompositionElement expectedOrigin = new MockOrigin();

            ReflectionComposablePartDefinition definition = CreateReflectionPartDefinition(
                expectedLazyType,
                true,
                () => expectedImports,
                () => expectedExports,
                expectedMetadata,
                expectedOrigin);

            var part = definition.CreatePart();
            Assert.IsNotNull(part);
            Assert.IsTrue(part is IDisposable);
        }

        [TestMethod]
        public void CreatePart_DoesntLoadType()
        {
            Type expectedType = typeof(TestPart);
            Lazy<Type> expectedLazyType = new Lazy<Type>(() => { Assert.Fail("Part should not be loaded"); return null; });
            IDictionary<string, object> expectedMetadata = new Dictionary<string, object>();
            expectedMetadata["Key1"] = 1;
            expectedMetadata["Key2"] = "Value2";

            IEnumerable<ImportDefinition> expectedImports = CreateImports(expectedType);
            IEnumerable<ExportDefinition> expectedExports = CreateExports(expectedType);

            ICompositionElement expectedOrigin = new MockOrigin();

            ReflectionComposablePartDefinition definition = CreateReflectionPartDefinition(
                expectedLazyType,
                true,
                () => expectedImports,
                () => expectedExports,
                expectedMetadata,
                expectedOrigin);

            var part = definition.CreatePart();
            Assert.IsNotNull(part);
            Assert.IsTrue(part is IDisposable);
        }

        [TestMethod]
        public void Constructor_NullMetadata_ShouldSetMetadataPropertyToEmpty()
        {
            ReflectionComposablePartDefinition definition = CreateEmptyDefinition(typeof(object), typeof(object).GetConstructors().First(), null, new MockOrigin());
            Assert.IsNotNull(definition.Metadata);
            Assert.AreEqual(0, definition.Metadata.Count);
        }

        [TestMethod]
        public void Constructor_NullOrigin_ShouldSetOriginPropertyToNull()
        {
            ReflectionComposablePartDefinition definition = CreateEmptyDefinition(typeof(object), typeof(object).GetConstructors().First(), MetadataServices.EmptyMetadata, null);
            Assert.IsNotNull(((ICompositionElement)definition).DisplayName);
            Assert.IsNull(((ICompositionElement)definition).Origin);
        }

        [TestMethod]
        public void ImportaAndExports_CreatorsShouldBeCalledLazilyAndOnce()
        {
            Type expectedType = typeof(TestPart);

            IEnumerable<ImportDefinition> expectedImports = CreateImports(expectedType);
            IEnumerable<ExportDefinition> expectedExports = CreateExports(expectedType);

            bool importsCreatorCalled = false;
            Func<IEnumerable<ImportDefinition>> importsCreator = () =>
            {
                Assert.IsFalse(importsCreatorCalled);
                importsCreatorCalled = true;
                return expectedImports.Cast<ImportDefinition>();
            };

            bool exportsCreatorCalled = false;
            Func<IEnumerable<ExportDefinition>> exportsCreator = () =>
            {
                Assert.IsFalse(exportsCreatorCalled);
                exportsCreatorCalled = true;
                return expectedExports.Cast<ExportDefinition>();
            };

            ReflectionComposablePartDefinition definition = CreateReflectionPartDefinition(
                expectedType.AsLazy(),
                false,
                importsCreator,
                exportsCreator,
                null,
                null);

            IEnumerable<ExportDefinition> exports;
            Assert.IsFalse(exportsCreatorCalled);
            exports = definition.ExportDefinitions;
            Assert.IsTrue(exportsCreatorCalled);
            exports = definition.ExportDefinitions;


            IEnumerable<ImportDefinition> imports;
            Assert.IsFalse(importsCreatorCalled);
            imports = definition.ImportDefinitions;
            Assert.IsTrue(importsCreatorCalled);
            imports = definition.ImportDefinitions;
        }

        [TestMethod]
        public void ICompositionElementDisplayName_ShouldReturnTypeDisplayName()
        {
            var expectations = Expectations.GetAttributedTypes();
            foreach (var e in expectations)
            {
                var definition = (ICompositionElement)CreateEmptyDefinition(e, null, null, null);

                Assert.AreEqual(e.GetDisplayName(), definition.DisplayName);
            }
        }

        [TestMethod]
        public void ToString_ShouldReturnICompositionElementDisplayName()
        {
            var expectations = Expectations.GetAttributedTypes();
            foreach (var e in expectations)
            {
                var definition = (ICompositionElement)CreateEmptyDefinition(e, null, null, null);

                Assert.AreEqual(definition.DisplayName, definition.ToString());
            }
        }

        private ReflectionComposablePartDefinition CreateEmptyDefinition(Type type, ConstructorInfo constructor, IDictionary<string, object> metadata, ICompositionElement origin)
        {
            return (ReflectionComposablePartDefinition)ReflectionModelServices.CreatePartDefinition(
                (type != null) ? type.AsLazy() : null,
                false,
                Enumerable.Empty<ImportDefinition>().AsLazy(),
                Enumerable.Empty<ExportDefinition>().AsLazy(),
                metadata.AsLazy(),
                origin);
        }

        private static List<ImportDefinition> CreateImports(Type type)
        {
            List<ImportDefinition> imports = new List<ImportDefinition>();
            foreach (PropertyInfo property in type.GetProperties())
            {
                imports.Add(new ReflectionMemberImportDefinition(new LazyMemberInfo(property), "Contract", (string)null, new KeyValuePair<string, Type>[] { new KeyValuePair<string, Type>("Key1", typeof(object)) }, ImportCardinality.ZeroOrOne, true, CreationPolicy.Any, new TypeOrigin(type)));
            }

            return imports;
        }

        private static List<ExportDefinition> CreateExports(Type type)
        {
            List<ExportDefinition> exports = new List<ExportDefinition>();
            foreach (PropertyInfo property in type.GetProperties())
            {
                exports.Add(ReflectionModelServices.CreateExportDefinition(new LazyMemberInfo(property), "Contract", new Lazy<IDictionary<string, object>>(() => null, false), new TypeOrigin(type)));
            }

            return exports;
        }

        public class TestPart
        {
            public int field1;
            public string field2;
            public int Property1 { get; set; }
            public string Property2 { get; set; }
        }

        private class TypeOrigin : ICompositionElement
        {
            private readonly Type _type;
            private readonly ICompositionElement _orgin; 

            public TypeOrigin(Type type)
                : this(type, null)
            {
            }

            public TypeOrigin(Type type, ICompositionElement origin)
            {
                this._type = type;
                this._orgin = origin;
            }

            public string DisplayName
            {
                get
                {
                    return this._type.GetDisplayName();
                }
            }

            public ICompositionElement Origin
            {
                get
                {
                    return this._orgin;
                }
            }
        }

        private class MockOrigin : ICompositionElement
        {
            public string DisplayName
            {
                get { throw new NotImplementedException(); }
            }

            public ICompositionElement Origin
            {
                get { throw new NotImplementedException(); }
            }
        }
    }
}
