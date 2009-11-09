// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Factories;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.ComponentModel.Composition.Primitives;

namespace System.ComponentModel.Composition
{
    [TestClass]
    public class MetadataAttributeTests
    {
        [TestMethod]
        [TestProperty("Type", "Integration")]
        public void UntypedStructureTest()
        {
            var container = ContainerFactory.Create();
            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(AttributedModelServices.CreatePart(new BasicTestComponent()));
            container.Compose(batch);
            var export = container.GetExport<BasicTestComponent, IDictionary<string, object>>();
            
            
            Assert.IsNotNull(export.Metadata, "It should have metadata");
            Assert.AreEqual("One", export.Metadata["String1"], "Property attribute should copy straight across");
            Assert.AreEqual("Two", export.Metadata["String2"], "Property attribute should copy straight across");
            var e = export.Metadata["Numbers"] as IList<int>;
            Assert.IsNotNull(e, "Should get a collection of numbers");
            Assert.IsTrue(e.Contains(1), "Should have 1 in the list");
            Assert.IsTrue(e.Contains(2), "Should have 2 in the list");
            Assert.IsTrue(e.Contains(3), "Should have 3 in the list");
            Assert.AreEqual(3, e.Count, "Should be three numbers total");
        }

#if !SILVERLIGHT
		// Silverlight doesn't support strongly typed metadata
        [TestMethod]
        [TestProperty("Type", "Integration")]
        public void StronglyTypedStructureTest()
        {
            var container = ContainerFactory.Create();
            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(AttributedModelServices.CreatePart(new BasicTestComponent()));
            container.Compose(batch);

            var export = container.GetExport<BasicTestComponent, IStronglyTypedStructure>();

            Assert.IsNotNull(export.Metadata, "It should have metadata");
            Assert.AreEqual("One", export.Metadata.String1, "Property should copy straight across");
            Assert.AreEqual("Two", export.Metadata.String2, "Property should copy straight across");
            Assert.IsTrue(export.Metadata.Numbers.Contains(1), "Should have 1 in the list");
            Assert.IsTrue(export.Metadata.Numbers.Contains(2), "Should have 2 in the list");
            Assert.IsTrue(export.Metadata.Numbers.Contains(3), "Should have 3 in the list");
            Assert.AreEqual(3, export.Metadata.Numbers.Length, "Should be three numbers total");
        }
#endif //!SILVERLIGHT

        [Export]
        // Should cause a conflict with the multiple nature of Name.Bar because 
        // it isn't marked with IsMultiple=true
        [ExportMetadata("Bar", "Blah")]
        [Name("MEF")]
        [Name("MEF2")]
        [PartNotDiscoverable]
        public class BasicTestComponentWithInvalidMetadata
        {
        }

        [TestMethod]
        [TestProperty("Type", "Integration")]
        public void InvalidMetadataAttributeTest()
        {
            ComposablePart part = AttributedModelServices.CreatePart(new BasicTestComponentWithInvalidMetadata());
            ExportDefinition export = part.ExportDefinitions.First();

            var ex = ExceptionAssert.Throws<InvalidOperationException>(RetryMode.DoNotRetry, () => 
            {
                var metadata = export.Metadata;
            });
            
            Assert.IsTrue(ex.Message.Contains("Bar"));
        }

        [AttributeUsage(AttributeTargets.All)]
        [MetadataAttribute]
        public class MetadataWithInvalidCustomAttributeType : Attribute
        {
            public PersonClass Person { get { return new PersonClass(); } }

            public class PersonClass
            {
                public string First { get { return "George"; } }
                public string Last { get { return "Washington"; } }
            }
        }

        [Export]
        [MetadataWithInvalidCustomAttributeType]
        [PartNotDiscoverable]
        public class ClassWithInvalidCustomAttributeType
        {

        }

        [TestMethod]
        public void InvalidAttributType_CustomType_ShouldThrow()
        {
            ComposablePart part = AttributedModelServices.CreatePart(new ClassWithInvalidCustomAttributeType());
            ExportDefinition export = part.ExportDefinitions.First();
            
            // Should throw InvalidOperationException during discovery because
            // the person class is an invalid metadata type
            ExceptionAssert.Throws<InvalidOperationException>(RetryMode.DoNotRetry, () =>
            {
                var metadata = export.Metadata;
            });
        }

        [AttributeUsage(AttributeTargets.All)]
        [MetadataAttribute]
        public class MetadataWithInvalidVersionPropertyAttributeType : Attribute
        {
            public MetadataWithInvalidVersionPropertyAttributeType()
            {
                this.Version = new Version(1, 1);
            }
            public Version Version { get; set; }
        }

        [Export]
        [MetadataWithInvalidVersionPropertyAttributeType]
        [PartNotDiscoverable]
        public class ClassWithInvalidVersionPropertyAttributeType
        {

        }

        [TestMethod]
        public void InvalidAttributType_VersionPropertyType_ShouldThrow()
        {
            ComposablePart part = AttributedModelServices.CreatePart(new ClassWithInvalidVersionPropertyAttributeType());
            ExportDefinition export = part.ExportDefinitions.First();
            
            // Should throw InvalidOperationException during discovery because
            // the person class is an invalid metadata type
            ExceptionAssert.Throws<InvalidOperationException>(RetryMode.DoNotRetry, () =>
            {
                var metadata = export.Metadata;
            });
        }

        [MetadataAttribute]
        public class BaseMetadataAttribute : Attribute
        {
            public string BaseKey { get { return "BaseValue"; } }
        }

        public class DerivedMetadataAttribute : BaseMetadataAttribute
        {
            public string DerivedKey { get { return "DerivedValue"; } }
        }

        [Export]
        [DerivedMetadata]
        public class ExportWithDerivedMetadataAttribute { }

        [TestMethod]
        public void DerivedMetadataAttributeAttribute_ShouldSupplyMetadata()
        {
            ComposablePart part = AttributedModelServices.CreatePart(new ExportWithDerivedMetadataAttribute());
            ExportDefinition export = part.ExportDefinitions.Single();

            Assert.AreEqual("BaseValue", export.Metadata["BaseKey"]);
            Assert.AreEqual("DerivedValue", export.Metadata["DerivedKey"]);
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    [MetadataAttribute]
    public class BasicMetadataAttribute : Attribute
    {
        public string String1 { get { return "One"; } }

        public string String2 { get { return "Two"; } }

        public int[] Numbers { get { return new int[] { 1, 2, 3 }; } }

        public CreationPolicy Policy { get { return CreationPolicy.NonShared; } }

        public Type Type { get { return typeof(BasicMetadataAttribute); } }
    }

    public interface IStronglyTypedStructure
    {
        string String1 { get; }
        string String2 { get; }
        int[] Numbers { get; }
        CreationPolicy Policy { get; }
        Type Type { get; }
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    [MetadataAttribute]
    public class Name : Attribute
    {
        public Name(string name) { Bar = name; }

        public string Bar { set; get; }
    }

    [PartNotDiscoverable]
    [Export]
    [BasicMetadata]
    public class BasicTestComponent
    {
    }
}
